using AdScript.Core.Models;

public interface IAdCommandBuilder
{
    string Build(AdUserInput input, string path);
}
