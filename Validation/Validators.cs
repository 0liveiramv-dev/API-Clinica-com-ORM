using System.Text.RegularExpressions;

namespace ApiClinica.Validation;

public static class Validators
{
    // Aceita (47) 98888-7777 (celular, 9 dígitos) ou (47) 3333-4444 (fixo, 8 dígitos).
    private static readonly Regex TelefoneRegex = new(@"^\(\d{2}\) \d{4,5}-\d{4}$", RegexOptions.Compiled);

    public static bool IsValidTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return false;

        return TelefoneRegex.IsMatch(telefone);
    }

    // Validação numérica real de CPF (dígitos verificadores), não apenas formato.
    public static bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        var digitos = new string(cpf.Where(char.IsDigit).ToArray());

        if (digitos.Length != 11)
            return false;

        // Rejeita sequências com todos os dígitos iguais (ex: 111.111.111-11).
        if (digitos.Distinct().Count() == 1)
            return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += numeros[i] * (10 - i);

        int resto = soma % 11;
        int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[9] != digitoVerificador1)
            return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += numeros[i] * (11 - i);

        resto = soma % 11;
        int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

        return numeros[10] == digitoVerificador2;
    }
}
