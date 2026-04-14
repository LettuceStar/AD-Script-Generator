using System;

namespace AdScript.Core.Services.Path;

using AdScript.Core.Models;

public interface IOuPathBuilder
{
    string Build(AdUserInput input);

}
