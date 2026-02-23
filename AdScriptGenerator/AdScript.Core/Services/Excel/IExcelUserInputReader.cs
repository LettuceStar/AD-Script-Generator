using AdScript.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel;

public interface IExcelUserInputReader
{
    Task<ExcelReadResult<AdUserInput>> ReadAsync(Stream xlsxStream, CancellationToken ct = default);
}
