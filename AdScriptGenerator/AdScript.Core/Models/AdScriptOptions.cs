using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Models
{
    public class AdScriptOptions
    {
        public string DomainDn { get; set; } = string.Empty;
        public string StaffOu { get; set; } = string.Empty;
        public string DefaultPassword { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public bool ChangePasswordAtLogon { get; set; }
        public bool PasswordNeverExpires { get; set; } = true;
    }
}
