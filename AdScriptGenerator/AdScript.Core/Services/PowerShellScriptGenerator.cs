using System.Text;
using AdScript.Core.Models;
using System;

namespace AdScript.Core.Services;

public class PowerShellScriptGenerator
{
    public string GenerateNewAdUserCommand(
        AdUserInput input,
        string upnSuffix = "cats.local",
        string domainDn = "DC=cats,DC=local",
        string staffOu = "Staff",
        string defaultPassword = "Password1",
        bool enabled = true,
        bool changePasswordAtLogon = false,
        bool passwordNeverExpires = true)
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
        upnSuffix = (upnSuffix ?? string.Empty).Trim().TrimStart('@');
        var upn = $"{sam}@{upnSuffix}";

        // OU Path (Team -> Campus -> Staff -> Domain)
        var teamOu = (input.Team ?? string.Empty).Trim();
        var campusOu = (input.Campus ?? string.Empty).Trim();
        var path = $"OU={teamOu},OU={campusOu},OU={staffOu},{domainDn}";

        // Flag rule: PasswordNeverExpires=true => ChangePasswordAtLogon must be false
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
        sb.Append($" -AccountPassword (ConvertTo-SecureString \"{EscapePs(defaultPassword)}\" -AsPlainText -Force)");
        sb.Append($" -Enabled {(enabled ? "$true" : "$false")}");
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
}
