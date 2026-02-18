using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using AdScript.Core.Services;

namespace AdScript.Web.Pages 
{

public class IndexModel : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public void OnPost()
    {
    }

    }
}