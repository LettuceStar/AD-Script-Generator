using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using AdScript.Core.Services;
using Microsoft.Extensions.Options;

namespace AdScript.Web.Pages;


public class IndexModel : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    private readonly IScriptGenerator _generator;

        

    public IndexModel(
        IScriptGenerator generator)
    {
        _generator = generator;
    }


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

        GeneratedCommand = _generator.GenerateNewAdUserCommand(Input);

    }

}