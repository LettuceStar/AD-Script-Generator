using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Excel;

public sealed class ExcelReadResult<T>
{
    public required List<T> Rows { get; init; }
    public List<string> Errors { get; init; } = [];

}