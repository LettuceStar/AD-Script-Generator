using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using Microsoft.Extensions.Options;
using AdScript.Core.Services.Script;
using AdScript.Core.Excel;

namespace AdScript.Web.Pages;


public class IndexModel(IScriptGenerator generator, IExcelUserInputReader reader) : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    private readonly IScriptGenerator _generator = generator;

    private readonly IExcelUserInputReader _reader = reader;


    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public string? ErrorMessage { get; set; }

    public int TotalRows { get; set; }
    public List<AdUserInput> PreviewRows { get; set; } = new();



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

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken ct)
    {
        // validation: 1) file presence, 2) extension, 3) size
        if (UploadFile is null || UploadFile.Length == 0)
        {
            ErrorMessage = "please upload a .xlsx file";
            return Page();
        }

        var ext = Path.GetExtension(UploadFile.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "wrong extension of file, please upload the file end with .xlsx";
            return Page();
        }

        // 5MB limited
        const long maxBytes = 5 * 1024 * 1024; 
        if (UploadFile.Length > maxBytes)
        {
            ErrorMessage = "the file's size no more than 5MB";
            return Page();
        }

        // reanding: 1) open stream, 2) read with reader, 3) preview first 10 rows + error count (if any)
        await using var stream = UploadFile.OpenReadStream();
        var read = await _reader.ReadAsync(stream, ct);

        if (read.Errors.Count > 0 && read.Rows.Count == 0)
        {
            ErrorMessage = "Read failed：\n" + string.Join("\n", read.Errors.Take(5));
            return Page();
        }
        
        TotalRows = read.Rows.Count;
        PreviewRows = read.Rows.Take(10).ToList();

        // simple error reporting: if there are errors but we still have some valid rows,
        // we show a message with error count and an example error
        if (read.Errors.Count > 0)
        {
            ErrorMessage = $"there is {read.Errors.Count} lines were skipped（example：{read.Errors[0]}）";
        }

        return Page();
    }

}