using Microsoft.AspNetCore.Mvc;
using ApiClinica.Data;
using Microsoft.EntityFrameworkCore;
using ApiClinica.DTOs;
using ApiClinica.Mappers;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultasController : ControllerBase
{
    private readonly AppDbContext _context;
    private const int DuracaoConsultaMinutos = 30;

    public ConsultasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/consultas
    [HttpGet]
    public async Task<IActionResult> GetConsultas()
    {
        var consultas = await _context.Consultas.ToListAsync();

        var consultasDTO = consultas
            .Select(c => ConsultaMapper.ToDTO(c))
            .ToList();

        return Ok(consultasDTO);
    }

    // GET: api/consultas/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConsultaById(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound();

        return Ok(ConsultaMapper.ToDTO(consulta));
    }

    // POST: api/consultas
    [HttpPost]
    public async Task<IActionResult> CreateConsulta([FromBody] ConsultaCreateDTO dto)
    {
        var erro = await ValidarConsultaAsync(dto.PacienteId, dto.MedicoId, dto.DataHora, idIgnorado: null);
        if (erro != null)
            return BadRequest(new { mensagem = erro });

        var consulta = ConsultaMapper.ToModel(dto);

        _context.Consultas.Add(consulta);
        await _context.SaveChangesAsync();

        var consultaDTO = ConsultaMapper.ToDTO(consulta);

        return CreatedAtAction(nameof(GetConsultaById), new { id = consulta.Id }, consultaDTO);
    }

    // PATCH: api/consultas/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateConsulta(int id, [FromBody] ConsultaUpdateDTO dto)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound();

        // Calcula os valores efetivos (o que já existe + o que foi alterado)
        // para poder revalidar as regras de negócio com o estado final.
        var pacienteIdEfetivo = dto.PacienteId ?? consulta.PacienteId;
        var medicoIdEfetivo = dto.MedicoId ?? consulta.MedicoId;
        var dataHoraEfetiva = dto.DataHora ?? consulta.DataHora;

        var erro = await ValidarConsultaAsync(pacienteIdEfetivo, medicoIdEfetivo, dataHoraEfetiva, idIgnorado: id);
        if (erro != null)
            return BadRequest(new { mensagem = erro });

        ConsultaMapper.ApplyUpdate(consulta, dto);

        await _context.SaveChangesAsync();

        return Ok(ConsultaMapper.ToDTO(consulta));
    }

    // DELETE: api/consultas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConsulta(int id)
    {
        var consulta = await _context.Consultas.FindAsync(id);

        if (consulta == null)
            return NotFound();

        _context.Consultas.Remove(consulta);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Valida as regras de negócio da consulta:
    // - paciente e médico precisam existir;
    // - não pode ser agendada no passado;
    // - não pode sobrepor outra consulta do mesmo médico ou do mesmo paciente
    //   (duas consultas no mesmo horário exato sempre se sobrepõem, então essa
    //   mesma checagem já cobre a regra de "horários iguais").
    private async Task<string?> ValidarConsultaAsync(int pacienteId, int medicoId, DateTime dataHora, int? idIgnorado)
    {
        var pacienteExiste = await _context.Pacientes.AnyAsync(p => p.Id == pacienteId);
        if (!pacienteExiste)
            return $"Paciente com Id {pacienteId} não encontrado.";

        var medicoExiste = await _context.Medicos.AnyAsync(m => m.Id == medicoId);
        if (!medicoExiste)
            return $"Médico com Id {medicoId} não encontrado.";

        if (dataHora < DateTime.Now)
            return "Não é possível agendar consulta em uma data/hora no passado.";

        var fimNovaConsulta = dataHora.AddMinutes(DuracaoConsultaMinutos);

        var consultasDoMedico = await _context.Consultas
            .Where(c => c.Id != idIgnorado && c.MedicoId == medicoId)
            .ToListAsync();

        foreach (var c in consultasDoMedico)
        {
            var fimExistente = c.DataHora.AddMinutes(DuracaoConsultaMinutos);
            if (dataHora < fimExistente && c.DataHora < fimNovaConsulta)
                return "Já existe uma consulta marcada para este médico neste horário (ou com sobreposição).";
        }

        var consultasDoPaciente = await _context.Consultas
            .Where(c => c.Id != idIgnorado && c.PacienteId == pacienteId)
            .ToListAsync();

        foreach (var c in consultasDoPaciente)
        {
            var fimExistente = c.DataHora.AddMinutes(DuracaoConsultaMinutos);
            if (dataHora < fimExistente && c.DataHora < fimNovaConsulta)
                return "Já existe uma consulta marcada para este paciente neste horário (ou com sobreposição).";
        }

        return null;
    }
}
