using System.Text;
using System.Text.RegularExpressions;

namespace AdScript.Core.Services.Excel;

public static class HeaderAliases
{
    // Canonical keys (internal column identifiers)
    public const string ID = "ID";
    public const string FirstName = "FirstName";
    public const string LastName = "LastName";
    public const string Campus = "Campus";
    public const string Team = "Team";
    public const string UpnSuffix = "UpnSuffix";

    // Aliases are compared after normalization (see NormalizeHeader).
    public static readonly Dictionary<string, string[]> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        [ID] = new[]
        {
            "id",
            "employeeid",
            "employee id",
            "employee number",
            "staff id",
            "staff number",
            "student unique number",
            "student unique number sun",
            "sun",
            "unique number"
        },

        [FirstName] = new[]
        {
            "firstname",
            "first name",
            "givenname",
            "given name",
            "fn"
        },

        [LastName] = new[]
        {
            "lastname",
            "last name",
            "surname",
            "familyname",
            "family name",
            "ln"
        },

        [Campus] = new[]
        {
            "campus",
            "site",
            "location",
            "campus hbt lau"
        },

        [Team] = new[]
        {
            "team",
            "department",
            "dept",
            "division",
            "group"
        },

        [UpnSuffix] = new[]
        {
            "upnsuffix",
            "upn suffix",
            "upn",
            "domain",
            "email domain"
        }
    };

    // Normalize header text so "First Name (FN)" matches "first name".
    public static string NormalizeHeader(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;

        // Normalize whitespace & case
        var s = raw.Trim().ToLowerInvariant();

        // Replace line breaks/tabs with space
        s = s.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

        // Remove anything inside parentheses: "(FN)", "(HBT / LAU)" etc.
        s = Regex.Replace(s, @"\([^)]*\)", " ");

        // Keep only letters/numbers/spaces (drop punctuation like "/" "-" etc.)
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) || char.IsWhiteSpace(ch))
                sb.Append(ch);
            else
                sb.Append(' ');
        }

        // Collapse multiple spaces
        s = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();

        return s;
    }
}