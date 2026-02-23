using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using Microsoft.Extensions.Options;
using AdScript.Core.Services.Script;
using AdScript.Core.Services.Excel;

namespace AdScript.Web.Pages;


public class IndexModel(IScriptGenerator generator, IExcelUserInputReader reader,
    IExcelUserInputValidator validator) : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    private readonly IScriptGenerator _generator = generator;

    private readonly IExcelUserInputReader _reader = reader;
    private readonly IExcelUserInputValidator _validator = validator;


    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public string? ErrorMessage { get; set; }

    public int TotalRows { get; set; }
    public List<AdUserInput> PreviewRows { get; set; } = new();

    public int ValidRowCount { get; set; }
    public int ErrorCount { get; set; }

    public List<ExcelRowError> RowErrors { get; set; } = new();

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
            ErrorMessage = string.Join("\n", read.Errors);
            return Page();
        }
        
        TotalRows = read.Rows.Count;

        var validated = _validator.Validate(read.Rows);

        ValidRowCount = validated.TotalValidRows;
        ErrorCount = validated.TotalErrors;

        RowErrors = validated.Errors;

        // Preview only valid rows
        PreviewRows = validated.ValidRows
            .Select(x => x.Row)
            .Take(10)
            .ToList();

        return Page();
    }

}