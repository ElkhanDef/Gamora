namespace Gamora.Core.Services;

// .NET'in Türkçe kültüründe ToUpper/ToLower "I/İ/ı/i" harflerini beklenmedik şekilde
// dönüştürür (ör. "FIFA".ToLower() Türkçe kültürde "fıfa" olur). Kültürden bağımsız,
// karakter karakter eşleme yaparak bu tuzağı tamamen bertaraf ediyoruz.
public static class TurkishSearch
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var buffer = new char[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            buffer[i] = value[i] switch
            {
                'I' or 'İ' or 'ı' or 'i' => 'i',
                'Ş' or 'ş' => 's',
                'Ğ' or 'ğ' => 'g',
                'Ü' or 'ü' => 'u',
                'Ö' or 'ö' => 'o',
                'Ç' or 'ç' => 'c',
                var c => char.ToLowerInvariant(c)
            };
        }

        return new string(buffer);
    }
}
