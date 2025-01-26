namespace ManagedDoom;

public interface IConfigUtilities
{
    string GetExeDirectory();
    string GetConfigPath();
    string GetDefaultIwadPath();
    bool IsIwad(string path);
    string[] GetWadPaths(CommandLineArgs args);
}