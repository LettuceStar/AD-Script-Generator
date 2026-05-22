using System.Text;
using AdScript.Core.Models;
using Microsoft.Extensions.Options;

namespace AdScript.Core.Services.Script;
public class AdCommandBuilder : IAdCommandBuilder
{
	private readonly AdScriptOptions _options;

	public AdCommandBuilder(IOptions<AdScriptOptions> options)
	{
		_options = options.Value;
	}


    public string Build(AdUserInput input, string path)
	{
		if (input is null) throw new ArgumentNullException(nameof(input));

        // Basic sanitisation
        //var firstName = (input.FirstName ?? string.Empty).Trim();
        //var lastNameRaw = (input.LastName ?? string.Empty).Trim();
        var firstName = SanitizeDisplayNamePart(input.FirstName);
        var lastNameRaw = SanitizeDisplayNamePart(input.LastName);

        // For display fields: remove "-" and "'", but keep normal spacing
        var lastNameDisplay = RemoveChars(lastNameRaw, "-", "'");

		var name = $"{firstName} {lastNameDisplay}".Trim();
		var displayName = name;

		// Account naming (samAccountName <= 20 chars)
		var lastNameAccount = RemoveChars(lastNameRaw, "-", "'", " ");
		var sam = $"{firstName}.{lastNameAccount}".ToLowerInvariant();
        sam = SanitizeSamAccountName(sam);
        sam = Truncate(sam, 20);

        // UPN
        //var upnSuffix = (input.UpnSuffix ?? string.Empty).Trim().TrimStart('@');
        var upnSuffix = SanitizeUpnSuffix(input.UpnSuffix);
        var upn = $"{sam}@{upnSuffix}";


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


        // Optional AD group memberships
        if (!string.IsNullOrWhiteSpace(input.Groups))
        {
            var groups = input.Groups
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var group in groups)
            {
                sb.AppendLine();

                sb.Append(
                    $"Add-ADGroupMember -Identity \"{EscapePs(group)}\" -Members \"{EscapePs(sam)}\"");
            }
        }


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

    // Keep only safe characters allowed in samAccountName
    private static string SanitizeSamAccountName(string value)
    {
        var allowed = value.Where(c =>
            char.IsLetterOrDigit(c) ||
            c == '.' ||
            c == '-' ||
            c == '_');

        return new string(allowed.ToArray());
    }


    // Clean UPN suffix before building userPrincipalName
    private static string SanitizeUpnSuffix(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .TrimStart('@')
            .Replace("\"", "")
            .Replace("'", "")
            .Replace(" ", "");
    }

    // Clean unsafe characters from name/display fields
    private static string SanitizeDisplayNamePart(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .Replace("$", "")
            .Replace("`", "")
            .Replace("\"", "")
            .Replace(";", "");
    }

    // Escape values before inserting them into PowerShell strings
    private static string EscapePs(string value)
    {
        return value
            .Replace("`", "``")
            .Replace("\"", "`\"")
            .Replace("$", "`$");
    }


}