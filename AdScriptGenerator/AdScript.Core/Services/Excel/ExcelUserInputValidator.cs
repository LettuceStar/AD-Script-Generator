using AdScript.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel;

public sealed class ExcelUserInputValidator : IExcelUserInputValidator
{
    public ExcelValidationResult<AdUserInput> Validate(List<(int RowNumber, AdUserInput Row)> rows)
    {
        var result = new ExcelValidationResult<AdUserInput>();

        foreach (var (rowNum, row) in rows)
        {
            var errors = DataAnnotationRowValidator.ValidateRow(row, rowNum);

            if (errors.Count == 0)
                result.ValidRows.Add((rowNum, row));
            else
                result.Errors.AddRange(errors);
        }

        return result;
    }
}
