using AdScript.Core.Models;
using AdScript.Core.Services.Script;
using AdScript.Core.Services.Path;
using Microsoft.Extensions.Options;
using Xunit;

namespace AdScript.Core.Tests;

public class ScriptGeneratorTests
{
    [Fact]
    public void Generate_ShouldCombinePathAndCommandCorrectly()
    {
        var options = Options.Create(new AdScriptOptions
        {
            StaffOu = "Staff",
            StudentsOu = "Students",
            DomainDn = "DC=cats,DC=local",
            DefaultPassword = "P@ssword123",
            Enabled = true,
            ChangePasswordAtLogon = true,
            PasswordNeverExpires = false
        });

        var pathBuilder = new OuPathBuilder(options);
        var commandBuilder = new AdCommandBuilder(options);

        var generator = new PowerShellScriptGenerator(pathBuilder, commandBuilder);

        var input = new AdUserInput
        {
            FirstName = "John",
            LastName = "Smith",
            Campus = "Hobart",
            Team = "ICT",
            AccountType = AdUserInput.AdAccountType.Staff,
            UpnSuffix = "@cats.local"
        };

        var result = generator.GenerateNewAdUserCommand(input);

        Assert.Contains("New-ADUser", result);
        Assert.Contains("OU=ICT,OU=Hobart,OU=Staff,DC=cats,DC=local", result);
        Assert.Contains("john.smith@cats.local", result);
    }
}