using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services.Excel
{
    public sealed record ExcelRowError(
     int RowNumber,
     string Field,
     string Message
    );

}
