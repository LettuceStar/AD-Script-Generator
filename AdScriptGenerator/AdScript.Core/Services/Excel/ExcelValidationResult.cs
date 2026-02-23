using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel
{
    public sealed class ExcelValidationResult<T>
    {
        public List<(int RowNumber, T Row)> ValidRows { get; } = new();
        public List<ExcelRowError> Errors { get; } = new();

        public int TotalValidRows => ValidRows.Count;
        public int TotalErrors => Errors.Count;
    }
}
