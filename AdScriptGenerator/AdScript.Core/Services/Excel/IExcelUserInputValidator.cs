using AdScript.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel
{
    public interface IExcelUserInputValidator
    {
        ExcelValidationResult<AdUserInput> Validate(List<(int RowNumber, AdUserInput Row)> rows);
    }
}
