using System.ComponentModel.DataAnnotations;

namespace ApiClinica.DTOs;

public class PacienteUpdateDTO
{
    // Todos os campos são opcionais: no PATCH, um campo nulo significa
    // que ele não deve ser atualizado. Cpf não está aqui de propósito —
    // o enunciado proíbe alterar o Cpf no PATCH.
    public string? Nome { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public DateOnly? DataNasc { get; set; }
}
