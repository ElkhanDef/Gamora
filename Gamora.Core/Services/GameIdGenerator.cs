namespace Gamora.Core.Services;

// Yeni oyun eklenirken catalog.json'daki "id" alanı buradan üretilir (ör. "cs2", "valorant").
// TurkishSearch.Normalize aksanları/Türkçe harfleri zaten sadeleştirdiği için slug üretiminde
// de onu kullanıyoruz — iki ayrı normalizasyon mantığı olmasın diye.
public static class GameIdGenerator
{
    public static string GenerateUniqueId(string name, IEnumerable<string> existingIds)
    {
        var baseSlug = Slugify(name);
        if (baseSlug.Length == 0)
        {
            baseSlug = "oyun";
        }

        var existing = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(baseSlug))
        {
            return baseSlug;
        }

        var suffix = 2;
        string candidate;
        do
        {
            candidate = $"{baseSlug}-{suffix}";
            suffix++;
        } while (existing.Contains(candidate));

        return candidate;
    }

    private static string Slugify(string name)
    {
        var normalized = TurkishSearch.Normalize(name);
        var buffer = new char[normalized.Length];
        var length = 0;
        var lastWasDash = false;

        foreach (var c in normalized)
        {
            if (char.IsLetterOrDigit(c))
            {
                buffer[length++] = c;
                lastWasDash = false;
            }
            else if (!lastWasDash && length > 0)
            {
                buffer[length++] = '-';
                lastWasDash = true;
            }
        }

        return new string(buffer, 0, length).TrimEnd('-');
    }
}
