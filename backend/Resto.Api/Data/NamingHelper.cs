using System.Text;

namespace Resto.Api.Data;

public static class NamingHelper
{
    /// <summary>
    /// Converts a C# PascalCase name to Oracle UPPER_SNAKE_CASE.
    /// CategoryId -> CATEGORY_ID, ImageUrl -> IMAGE_URL, DineIn -> DINE_IN
    /// </summary>
    public static string ToUpperSnake(string name)
    {
        var sb = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            var startsNewWord =
                i > 0 &&
                char.IsUpper(c) &&
                name[i - 1] != '_' &&
                (char.IsLower(name[i - 1]) ||
                 (i + 1 < name.Length && char.IsLower(name[i + 1])));

            if (startsNewWord)
                sb.Append('_');

            sb.Append(char.ToUpperInvariant(c));
        }

        return sb.ToString();
    }
}
