using System.Text;
using AdScript.Core.Models;
using System;
using Microsoft.Extensions.Options;
using static AdScript.Core.Models.AdUserInput;

namespace AdScript.Core.Services.Script;

public class PowerShellScriptGenerator : IScriptGenerator
{
    private readonly AdScriptOptions _options;

    public PowerShellScriptGenerator(IOptions<AdScriptOptions> options)
    {
        _options = options.Value;
    }
    public string GenerateNewAdUserCommand(
        AdUserInput input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        // Basic sanitisation
        var firstName = (input.FirstName ?? string.Empty).Trim();
        var lastNameRaw = (input.LastName ?? string.Empty).Trim();

        // For display fields: remove "-" and "'", but keep normal spacing
        var lastNameDisplay = RemoveChars(lastNameRaw, "-", "'");

        var name = $"{firstName} {lastNameDisplay}".Trim();
        var displayName = name;

        // Account naming (samAccountName <= 20 chars)
        var lastNameAccount = RemoveChars(lastNameRaw, "-", "'", " ");
        var sam = $"{firstName}.{lastNameAccount}".ToLowerInvariant();
        sam = Truncate(sam, 20);

        // UPN
        var upnSuffix = (input.UpnSuffix ?? string.Empty).Trim().TrimStart('@');
        var upn = $"{sam}@{upnSuffix}";

        // Build OU path dynamically, skipping empty segments
        var ous = new List<string>();

        // OU Path (Team -> Campus -> Staff -> Domain)
        var teamOu = (input.Team ?? string.Empty).Trim();
        var campusOu = (input.Campus ?? string.Empty).Trim();

        var path = BuildOuPath(input);

        // var path = $"OU={teamOu},OU={campusOu},OU={_options.StaffOu},{_options.DomainDn}";
        //if (!string.IsNullOrWhiteSpace(teamOu))
        //    ous.Add($"OU={teamOu}");

        //if (!string.IsNullOrWhiteSpace(campusOu))
        //    ous.Add($"OU={campusOu}");

        //if (!string.IsNullOrWhiteSpace(_options.StaffOu))
        //    ous.Add($"OU={_options.StaffOu}");

        // Combine all OU segments with domain DN
        //var path = string.Join(",", ous) + "," + _options.DomainDn;

        // Flag rule: PasswordNeverExpires=true => ChangePasswordAtLogon must be false
        var changePasswordAtLogon = _options.ChangePasswordAtLogon;
        var passwordNeverExpires = _options.PasswordNeverExpires;
        if (passwordNeverExpires)
            changePasswordAtLogon = false;

        // Build command (single line, easiest for copy/paste + batch)
        var sb = new StringBuilder();
        sb.Append("New-ADUser");
        sb.Append($" -Name \"{EscapePs(name)}\"");
        sb.Append($" -GivenName \"{EscapePs(firstName)}\"");
        sb.Append($" -Surname \"{EscapePs(lastNameDisplay)}\"");
        sb.Append($" -DisplayName \"{EscapePs(displayName)}\"");
        sb.Append($" -SamAccountName \"{EscapePs(sam)}\"");
        sb.Append($" -UserPrincipalName \"{EscapePs(upn)}\"");
        sb.Append($" -Path \"{EscapePs(path)}\"");
        sb.Append($" -AccountPassword (ConvertTo-SecureString \"{EscapePs(_options.DefaultPassword)}\" -AsPlainText -Force)");
        sb.Append($" -Enabled {(_options.Enabled ? "$true" : "$false")}");
        sb.Append($" -ChangePasswordAtLogon {(changePasswordAtLogon ? "$true" : "$false")}");
        sb.Append($" -PasswordNeverExpires {(passwordNeverExpires ? "$true" : "$false")}");

        return sb.ToString();
    }

    private static string Truncate(string value, int maxLen)
        => value.Length <= maxLen ? value : value.Substring(0, maxLen);

    private static string RemoveChars(string value, params string[] charsToRemove)
    {
        var result = value;
        foreach (var c in charsToRemove)
        {
            result = result.Replace(c, string.Empty);
        }
        return result;
    }

    private static string EscapePs(string value)
        => value.Replace("\"", "`\""); // Escape double quotes for PowerShell strings

    // Build distinguished name path safely
    private string BuildOuPath(AdUserInput input)
    {
        var segments = new List<string>();

        void AddOu(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                segments.Add($"OU={value.Trim()}");
        }

        if (input.AccountType == AdAccountType.Student)
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
