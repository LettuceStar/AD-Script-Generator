using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AdScript.Core.Models;
using AdScript.Core.Services.Script;
using AdScript.Core.Services.Excel;
using System.Text;
using static AdScript.Core.Models.AdUserInput;

namespace AdScript.Web.Pages;


public class IndexModel(IScriptGenerator generator, IExcelUserInputReader reader,
    IExcelUserInputValidator validator) : PageModel
{
    [BindProperty]
    public string? GeneratedCommand { get; set; }

    [BindProperty]
    public AdUserInput Input { get; set; } = new();

    public List<string> Commands { get; set; } = new();

    [BindProperty]
    public string? CombinedScript { get; set; }

    [BindProperty]
    public string? DownloadFileName { get; set; }

    private readonly IScriptGenerator _generator = generator;

    private readonly IExcelUserInputReader _reader = reader;
    private readonly IExcelUserInputValidator _validator = validator;

    [BindProperty]
    public AdAccountType UploadAccountType { get; set; } = AdAccountType.Staff;


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
            ErrorMessage = "Please upload a .xlsx file";
            return Page();
        }

        var ext = Path.GetExtension(UploadFile.FileName);
        if (!string.Equals(ext, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Invalid file extension. Please upload a file ending with .xlsx.";
            return Page();
        }

        // 5MB limited
        const long maxBytes = 5 * 1024 * 1024; 
        if (UploadFile.Length > maxBytes)
        {
            ErrorMessage = "File size must not exceed 5 MB.";
            return Page();
        }

        // reanding: 1.open stream, 2.read with reader, 3.preview first 10 rows + error count (if any)
        await using var stream = UploadFile.OpenReadStream();
        var read = await _reader.ReadAsync(stream, ct);

        if (read.Errors.Count > 0 && read.Rows.Count == 0)
        {
            ErrorMessage = string.Join("\n", read.Errors);
            return Page();
        }
        
        TotalRows = read.Rows.Count;


        var validated = _validator.Validate(read.Rows);


        foreach (var v in validated.ValidRows)
        {
            v.Row.AccountType = UploadAccountType;
        }

        ValidRowCount = validated.TotalValidRows;
        ErrorCount = validated.TotalErrors;
        RowErrors = validated.Errors;


        // Preview only valid rows
        PreviewRows = validated.ValidRows
            .Select(x => x.Row)
            .Take(10)
            .ToList();

        // Generate commands for valid rows (M3)
        Commands = validated.ValidRows
            .Select(v => _generator.GenerateNewAdUserCommand(v.Row))
            .ToList();

        // Combine as a .ps1 text block
        CombinedScript = string.Join(Environment.NewLine, Commands);

        // Provide a default file name for download
        DownloadFileName = $"new-adusers-{DateTime.Now:yyyyMMdd-HHmmss}.ps1";

        return Page();
    }

    public IActionResult OnPostDownload()
    {

        if (ErrorCount > 0)
        {
            ErrorMessage = "Fix validation errors before downloading.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(CombinedScript))
        {
            ErrorMessage = "Nothing to download. Please upload an Excel file and generate commands first.";
            return Page();
        }

        var fileName = string.IsNullOrWhiteSpace(DownloadFileName)
            ? $"new-adusers-{DateTime.Now:yyyyMMdd-HHmmss}.ps1"
            : DownloadFileName;

        // Ensure Windows-friendly line endings if you want:
        // var content = CombinedScript.Replace("\n", "\r\n");

        var bytes = Encoding.UTF8.GetBytes(CombinedScript);
        return File(bytes, "text/plain", fileName);
    }

}