using Microsoft.AspNetCore.Mvc;
using ApiClinica.Data;
using Microsoft.EntityFrameworkCore;
using ApiClinica.DTOs;
using ApiClinica.Mappers;
using ApiClinica.Validation;

namespace ApiClinica.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicosController : ControllerBase
{
    private readonly AppDbContext _context;

    public MedicosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/medicos
    [HttpGet]
    public async Task<IActionResult> GetMedicos()
    {
        var medicos = await _context.Medicos.ToListAsync();

        var medicosDTO = medicos
            .Select(m => MedicoMapper.ToDTO(m))
            .ToList();

        return Ok(medicosDTO);
    }

    // GET: api/medicos/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicoById(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);

        if (medico == null)
            return NotFound();

        return Ok(MedicoMapper.ToDTO(medico));
    }

    // POST: api/medicos
    [HttpPost]
    public async Task<IActionResult> CreateMedico([FromBody] MedicoCreateDTO dto)
    {
        if (!Validators.IsValidTelefone(dto.Telefone))
            return BadRequest(new { mensagem = "Telefone em formato inválido. Use o formato (47) 98888-7777." });

        var medico = MedicoMapper.ToModel(dto);

        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        var medicoDTO = MedicoMapper.ToDTO(medico);

        return CreatedAtAction(nameof(GetMedicoById), new { id = medico.Id }, medicoDTO);
    }

    // PATCH: api/medicos/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateMedico(int id, [FromBody] MedicoUpdateDTO dto)
    {
        var medico = await _context.Medicos.FindAsync(id);

        if (medico == null)
            return NotFound();

        if (dto.Telefone != null && !Validators.IsValidTelefone(dto.Telefone))
            return BadRequest(new { mensagem = "Telefone em formato inválido. Use o formato (47) 98888-7777." });

        MedicoMapper.ApplyUpdate(medico, dto);

        await _context.SaveChangesAsync();

        return Ok(MedicoMapper.ToDTO(medico));
    }

    // DELETE: api/medicos/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedico(int id)
    {
        var medico = await _context.Medicos.FindAsync(id);

        if (medico == null)
            return NotFound();

        var temConsultaFutura = await _context.Consultas
            .AnyAsync(c => c.MedicoId == id && c.DataHora > DateTime.Now);

        if (temConsultaFutura)
            return BadRequest(new { mensagem = "Não é possível remover um médico com consultas futuras agendadas." });

        _context.Medicos.Remove(medico);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
