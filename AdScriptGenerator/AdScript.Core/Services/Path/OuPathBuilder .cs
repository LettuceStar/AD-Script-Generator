using AdScript.Core.Models;
using AdScript.Core.Services.Path;
using Microsoft.Extensions.Options;

namespace AdScript.Core.Services.Path;

public class OuPathBuilder : IOuPathBuilder
{
    private readonly AdScriptOptions _options;

    public OuPathBuilder(IOptions<AdScriptOptions> options)
    {
        _options = options.Value;
    }

    private static string SanitizeOuValue(string value)
    {
        return value
            .Replace("\"", "")
            .Replace(",", "")
            .Trim();
    }

    // Build distinguished name path safely
    public string Build(AdUserInput input)
    {
        var segments = new List<string>();

        void AddOu(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                segments.Add($"OU={SanitizeOuValue(value)}");
        }

        if (input.AccountType == AdUserInput.AdAccountType.Student)
        {
            AddOu(input.Campus);
            AddOu(_options.StudentsOu);
        }
        else
        {
            AddOu(input.Team);
            AddOu(input.Campus);
            AddOu(_options.StaffOu);
        }


        // Domain DN is already like "DC=cats,DC=local"
        segments.Add(_options.DomainDn);

        return string.Join(",", segments);
    }




}

