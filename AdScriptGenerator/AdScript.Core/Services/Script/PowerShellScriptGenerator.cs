using AdScript.Core.Models;
using AdScript.Core.Services.Path;

namespace AdScript.Core.Services.Script;

public class PowerShellScriptGenerator : IScriptGenerator
{

    private readonly IOuPathBuilder _pathBuilder;

    private readonly IAdCommandBuilder _commandBuilder;

    public PowerShellScriptGenerator(
        IOuPathBuilder pathBuilder,
        IAdCommandBuilder commandBuilder)
    {
        _pathBuilder = pathBuilder;
        _commandBuilder = commandBuilder;
    }

    public string GenerateNewAdUserCommand(AdUserInput input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));

        var path = _pathBuilder.Build(input);
        return _commandBuilder.Build(input, path);
    }



}
