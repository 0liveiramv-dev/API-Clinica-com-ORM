using ApiClinica.Models;
using ApiClinica.DTOs;

namespace ApiClinica.Mappers;

public static class PacienteMapper
{
    public static Paciente ToModel(PacienteCreateDTO dto)
    {
        return new Paciente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            DataNasc = dto.DataNasc,
            Cpf = dto.Cpf
        };
    }

    public static PacienteReadDTO ToDTO(Paciente paciente)
    {
        return new PacienteReadDTO
        {
            Id = paciente.Id,
            Nome = paciente.Nome,
            Email = paciente.Email,
            Telefone = paciente.Telefone,
            DataNasc = paciente.DataNasc
        };
    }

    // Aplica no Model apenas os campos informados (não nulos) no PATCH.
    // Cpf nunca é alterado aqui, pois PacienteUpdateDTO não possui esse campo.
    public static void ApplyUpdate(Paciente paciente, PacienteUpdateDTO dto)
    {
        if (dto.Nome != null)
            paciente.Nome = dto.Nome;

        if (dto.Email != null)
            paciente.Email = dto.Email;

        if (dto.Telefone != null)
            paciente.Telefone = dto.Telefone;

        if (dto.DataNasc != null)
            paciente.DataNasc = dto.DataNasc.Value;
    }
}
