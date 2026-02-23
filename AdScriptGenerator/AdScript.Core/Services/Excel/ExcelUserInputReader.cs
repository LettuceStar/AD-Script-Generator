using AdScript.Core.Models;
using ClosedXML.Excel;
using Microsoft.Extensions.Options;


namespace AdScript.Core.Services.Excel
{
    public sealed class ExcelUserInputReader : IExcelUserInputReader
    {
        private readonly AdScriptOptions _options;
        public ExcelUserInputReader(IOptions<AdScriptOptions> options)
        {
            _options = options.Value;
        }

        public Task<ExcelReadResult<AdUserInput>> ReadAsync(Stream xlsxStream, CancellationToken ct = default)
        {
            var result = new ExcelReadResult<AdUserInput>();

            using var wb = new XLWorkbook(xlsxStream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws is null)
            {
                result.Errors.Add("The Excel file does not contain any worksheet.");
                return Task.FromResult(result);
            }

            var used = ws.RangeUsed();
            if (used is null)
            {
                result.Errors.Add("Excel is empty!");
                return Task.FromResult(result);
            }


            // Find header row (do not assume it's the first row)
            var headerRow = HeaderRowDetector.FindHeaderRow(used);

            if (headerRow is null)
            {
                result.Errors.Add("Scanned first 50 rows, " +
                    "did not find a row containing required headers: Id, FirstName, LastName.");
                return Task.FromResult(result);
            }

            // Build canonical header map using aliases
            var headerMap = HeaderMapBuilder.BuildCanonicalHeaderMap(headerRow);

            // File-level required header validation (keep a single source of truth)
            HeaderRowDetector.ValidateRequiredHeaders(headerMap, result.Errors);
            if (result.Errors.Count > 0)
                return Task.FromResult(result);

            // read data from second line, until last used line
            var firstDataRowNumber = headerRow.RowNumber() + 1;
            var lastRowNumber = used.LastRow().RowNumber();

            for (int r = firstDataRowNumber; r <= lastRowNumber; r++)
            {
                ct.ThrowIfCancellationRequested();

                var row = ws.Row(r);

                // Skip completely empty rows
                if (IsRowEmpty(row)) continue;

                var input = new AdUserInput
                {
                    // Canonical reads
                    FirstName = ReadCell(row, headerMap, HeaderAliases.FirstName),
                    LastName = ReadCell(row, headerMap, HeaderAliases.LastName),
                    ID = ReadCell(row, headerMap, HeaderAliases.ID),
                    Campus = ReadCell(row, headerMap, HeaderAliases.Campus),
                    Team = ReadCell(row, headerMap, HeaderAliases.Team),
                    UpnSuffix = ReadCell(row, headerMap, HeaderAliases.UpnSuffix),

                };

                // Apply defaults from Options (B: UpnSuffix defaulted)
                if (string.IsNullOrWhiteSpace(input.UpnSuffix))
                    input.UpnSuffix = _options.DefaultUpnSuffix;

                if (string.IsNullOrWhiteSpace(input.Team))
                    input.Team = _options.DefaultTeam;

                if (string.IsNullOrWhiteSpace(input.Campus))
                    input.Campus = _options.DefaultCampus;

                // Basic required checks (M2 will do full DataAnnotations validation)
                if (string.IsNullOrWhiteSpace(input.ID) ||
                    string.IsNullOrWhiteSpace(input.FirstName) ||
                    string.IsNullOrWhiteSpace(input.LastName))
                {
                    result.Errors.Add($"Row {r}: ID / First Name / Last Name is required.");
                    continue;
                }


                result.Rows.Add((r, input));
            }

            return Task.FromResult(result);
        }


        private static string ReadCell(IXLRow row, Dictionary<string, int> map, string canonicalKey)
        {
            if (!map.TryGetValue(canonicalKey, out var col)) return string.Empty;
            return row.Cell(col).GetString().Trim();
        }


        private static bool IsRowEmpty(IXLRow row)
        {
            // Consider row empty if all used cells are empty
            foreach (var c in row.CellsUsed())
            {
                if (!c.IsEmpty() && !string.IsNullOrWhiteSpace(c.GetString()))
                    return false;
            }
            return true;
        }


    }
}
