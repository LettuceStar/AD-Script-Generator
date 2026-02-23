using AdScript.Core.Excel;
using AdScript.Core.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel
{
    public sealed class ExcelUserInputReader : IExcelUserInputReader
    {
        
        private static readonly Dictionary<string, Func<IXLRow, Dictionary<string, int>, string>> Getters =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["FirstName"] = (row, map) => GetCell(row, map, "FirstName", "First Name"),
                ["LastName"] = (row, map) => GetCell(row, map, "LastName", "Last Name"),

                // Excel header can be ID, but model property is EmployeeId (recommended for now)
                ["ID"] = (row, map) => GetCell(row, map, "EmployeeId", "ID", "Employee ID"),

                ["Campus"] = (row, map) => GetCell(row, map, "Campus"),

                // These columns may not exist in your current Excel — return empty string safely
                ["Team"] = (row, map) => GetCell(row, map, "Team"),
                ["UpnSuffix"] = (row, map) => GetCell(row, map, "UpnSuffix", "UPN Suffix", "UPNSuffix"),
                // you can also add Department / Title / Groups / Email 

            };

        public Task<ExcelReadResult<AdUserInput>> ReadAsync(Stream xlsxStream, CancellationToken ct = default)
        {
            var result = new ExcelReadResult<AdUserInput> { Rows = new List<AdUserInput>() };

            using var wb = new XLWorkbook(xlsxStream);
            var ws = wb.Worksheets.FirstOrDefault();
            if (ws is null)
            {
                result.Errors.Add("Excel don't have worksheet。");
                return Task.FromResult(result);
            }

            var used = ws.RangeUsed();
            if (used is null)
            {
                result.Errors.Add("Excel is empty!");
                return Task.FromResult(result);
            }

            // read from first line
            var headerRow = used.FirstRow();
            var headerMap = BuildHeaderMap(headerRow);

            // read data from second line, until last used line
            var firstDataRowNumber = headerRow.RowNumber() + 1;
            var lastRowNumber = used.LastRow().RowNumber();

            for (int r = firstDataRowNumber; r <= lastRowNumber; r++)
            {
                ct.ThrowIfCancellationRequested();

                var row = ws.Row(r);

                // skip empty rows (you can also choose to treat them as errors if you want)
                if (row.Cell(1).IsEmpty()) continue;

                var input = new AdUserInput
                {
                    // depend your AdUserInput properties, and also you can do some simple
                    // transformation here if needed (like trimming, case conversion, etc.)                    
                    FirstName = Getters["FirstName"](row, headerMap),
                    LastName = Getters["LastName"](row, headerMap),
                    ID = Getters["ID"](row, headerMap),
                    Campus = Getters["Campus"](row, headerMap),
                    Team = Getters["Team"](row, headerMap),
                    UpnSuffix = Getters["UpnSuffix"](row, headerMap),

                };

                // simple validation example: check required fields
                if (string.IsNullOrWhiteSpace(input.FirstName) ||
                    string.IsNullOrWhiteSpace(input.LastName) ||
                    string.IsNullOrWhiteSpace(input.ID))
                {
                    result.Errors.Add($"Row {r}: FirstName / LastName / ID is required.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(input.Team))
                    input.Team = "IT";

                if (string.IsNullOrWhiteSpace(input.UpnSuffix))
                    input.UpnSuffix = "@cats.local";

                result.Rows.Add(input);
            }

            return Task.FromResult(result);
        }

        private static Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in headerRow.CellsUsed())
            {
                var name = cell.GetString().Trim();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    map[name] = cell.Address.ColumnNumber;
                }
            }

            return map;
        }

        private static string GetCell(IXLRow row, Dictionary<string, int> map, params string[] headers)
        {
            foreach (var h in headers)
            {
                if (map.TryGetValue(h, out var col))
                    return row.Cell(col).GetString().Trim();
            }

            return string.Empty;
        }
    }
}
