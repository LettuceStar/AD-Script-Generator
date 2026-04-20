using AdScript.Core.Models;
using AdScript.Core.Services.Path;
using Microsoft.Extensions.Options;
using Xunit;

namespace AdScript.Core.Tests;

public class OuPathBuilderTests
{
    private static OuPathBuilder CreateBuilder()
    {
        var options = Options.Create(new AdScriptOptions
        {
            StaffOu = "Staff",
            StudentsOu = "Students",
            DomainDn = "DC=cats,DC=local"
        });

        return new OuPathBuilder(options);
    }

    [Fact]
    public void Build_ShouldReturnStaffPath_WhenAccountTypeIsStaff()
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            AccountType = AdUserInput.AdAccountType.Staff,
            Team = "ICT",
            Campus = "Hobart"
        };

        var result = builder.Build(input);

        Assert.Equal("OU=ICT,OU=Hobart,OU=Staff,DC=cats,DC=local", result);
    }

    [Fact]
    public void Build_ShouldReturnStudentPath_WhenAccountTypeIsStudent()
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            AccountType = AdUserInput.AdAccountType.Student,
            Campus = "Launceston"
        };

        var result = builder.Build(input);

        Assert.Equal("OU=Launceston,OU=Students,DC=cats,DC=local", result);
    }

    [Fact]
    public void Build_ShouldSkipEmptyTeam_WhenStaffTeamIsBlank()
    {
        var builder = CreateBuilder();

        var input = new AdUserInput
        {
            AccountType = AdUserInput.AdAccountType.Staff,
            Team = "",
            Campus = "Burnie"
        };

        var result = builder.Build(input);

        Assert.Equal("OU=Burnie,OU=Staff,DC=cats,DC=local", result);
    }
}