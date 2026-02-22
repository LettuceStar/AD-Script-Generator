using AdScript.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdScript.Core.Services
{
    public interface IScriptGenerator
    {
        string GenerateNewAdUserCommand(AdUserInput input);
    }
}
