using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel;

public sealed class ExcelReadResult<T>
{
    public List<(int RowNumber, T Row)> Rows { get; init; } = new();
    public List<string> Errors { get; init; } = new(); // file-level errors (missing headers, etc.)

}