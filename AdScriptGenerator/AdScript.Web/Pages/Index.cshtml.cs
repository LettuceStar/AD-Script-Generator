using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using AdScript.Core.Services;

namespace AdScript.Web.Pages;


public class IndexModel : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public void OnPostGenerate()
    {
        if (!ModelState.IsValid)
        {
            GeneratedCommand = null; 
            return;
        }

        var generator = new PowerShellScriptGenerator();

        GeneratedCommand = generator.GenerateNewAdUserCommand(
            Input,
            upnSuffix: Input.UpnSuffix,
            domainDn: "DC=cats,DC=local",
            staffOu: "Staff",
            defaultPassword: "Password1",
            enabled: true,
            changePasswordAtLogon: false,
            passwordNeverExpires: true
        );
        }

}