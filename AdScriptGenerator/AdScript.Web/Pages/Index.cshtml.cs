using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using AdScript.Core.Services;

namespace AdScript.Web.Pages;

public class IndexModel : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    public void OnGet()
    {
    }

    public void OnPostGenerate()
    {
        // Test input
        var input = new AdUserInput
        {
            FirstName = "Simone",
            LastName = "Emma-Peartree",
            EmployeeId = "205",
            Campus = "HBT",
            Team = "Chief_Executive_Officer"
        };
                
        var generator = new PowerShellScriptGenerator();

        GeneratedCommand = generator.GenerateNewAdUserCommand(
            input,
            upnSuffix: "cats.local",
            domainDn: "DC=cats,DC=local",
            staffOu: "Staff",
            defaultPassword: "Password1",
            enabled: true,
            changePasswordAtLogon: false,
            passwordNeverExpires: true
        );
    }
}
