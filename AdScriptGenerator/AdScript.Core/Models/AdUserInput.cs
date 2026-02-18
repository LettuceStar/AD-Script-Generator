using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Models
{
    public class AdUserInput
    {
        //string.Empty is used to avoid null reference exceptions
        // when the properties are accessed before being set.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public string UpnSuffix { get; set; } = string.Empty;
    }
}
