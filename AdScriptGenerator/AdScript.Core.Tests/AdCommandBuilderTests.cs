using AdScript.Core.Models;
using AdScript.Core.Services.Script;
using Microsoft.Extensions.Options;
using Xunit;

namespace AdScript.Core.Tests;

public class AdCommandBuilderTests
{
    private static AdCommandBuilder CreateBuilder(
        bool enabled = true,
        bool changePasswordAtLogon = true,
        bool passwordNeverExpires = false)
    {
        var options = Options.Create(new AdScriptOptions
        {
            DefaultPassword = "P@ssword123",
            Enabled = enabled,
            ChangePasswordAtLogon = changePasswordAtLogon,
            PasswordNeverExpires = passwordNeverExpires
        });

        return new AdCommandBuilder(options);
    }

    [Fact]
    public void Build_ShouldContainExpectedBasicFields()
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            FirstName = "John",
            LastName = "Smith",
            UpnSuffix = "@cats.local"
        };

        var result = builder.Build(input, "OU=ICT,OU=Hobart,OU=Staff,DC=cats,DC=local");

        Assert.Contains("New-ADUser", result);
        Assert.Contains("-Name \"John Smith\"", result);
        Assert.Contains("-GivenName \"John\"", result);
        Assert.Contains("-Surname \"Smith\"", result);
        Assert.Contains("-DisplayName \"John Smith\"", result);
        Assert.Contains("-SamAccountName \"john.smith\"", result);
        Assert.Contains("-UserPrincipalName \"john.smith@cats.local\"", result);
        Assert.Contains("-Path \"OU=ICT,OU=Hobart,OU=Staff,DC=cats,DC=local\"", result);
    }


    [Theory]
    [InlineData("Christopher", "VeryLongSurname", "christopher.verylong")]
    [InlineData("John", "Smith", "john.smith")]
    [InlineData("Anne", "O'Neil", "anne.oneil")]
    public void Build_ShouldGenerateCorrectSamAccountName(
    string firstName,
    string lastName,
    string expectedSam)
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            FirstName = firstName,
            LastName = lastName,
            UpnSuffix = "@cats.local"
        };

        var result = builder.Build(input, "OU=Test,DC=cats,DC=local");

        Assert.Contains($"-SamAccountName \"{expectedSam}\"", result);
    }

    [Theory]
    [InlineData("Christopher", "VeryLongSurname", "christopher.verylong")]
    [InlineData("Alex", "Short", "alex.short")] // no need to truncate
    [InlineData("VeryVeryLongFirstName", "Smith", "veryverylongfirstnam")] // long first name
    public void Build_ShouldHandleSamAccountNameLength(
      string firstName,
      string lastName,
      string expectedSam)
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            FirstName = firstName,
            LastName = lastName,
            UpnSuffix = "@cats.local"
        };

        var result = builder.Build(input, "OU=Test,DC=cats,DC=local");

        Assert.Contains($"-SamAccountName \"{expectedSam}\"", result);
    }

    [Theory]
    [InlineData("O'Neil", "ONeil")]
    [InlineData("Lee-Smith", "LeeSmith")]
    [InlineData("A-B'C", "ABC")]
    public void Build_ShouldRemoveSpecialCharacters(
        string lastName,
        string expected)
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            FirstName = "Test",
            LastName = lastName,
            UpnSuffix = "@cats.local"
        };

        var result = builder.Build(input, "OU=Test,DC=cats,DC=local");

        Assert.Contains($"-Surname \"{expected}\"", result);
    }

    [Fact]
    public void Build_ShouldForceChangePasswordAtLogonToFalse_WhenPasswordNeverExpiresIsTrue()
    {
        var builder = CreateBuilder(
            enabled: true,
            changePasswordAtLogon: true,
            passwordNeverExpires: true);

        var input = new AdUserInput
        {
            FirstName = "John",
            LastName = "Smith",
            UpnSuffix = "@cats.local"
        };

        var result = builder.Build(input, "OU=Test,DC=cats,DC=local");

        Assert.Contains("-ChangePasswordAtLogon $false", result);
        Assert.Contains("-PasswordNeverExpires $true", result);
    }
}