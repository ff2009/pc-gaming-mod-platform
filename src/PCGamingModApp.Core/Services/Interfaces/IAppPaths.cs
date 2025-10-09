namespace PCGamingModApp.Core.Services.Interfaces;


public interface IAppPaths
{
    string Root { get; }
    string GameIcons { get; }
    string Database { get; }
    string Downloads { get; }
    //string ModsArchiveDir { get; }
    string Temp { get; }
    string Logs { get; }
    string Config { get; }
    
    
    string Assets { get; }
    string StoreIcons { get; }

    void EnsureCreated();
    
    void Migrate();
}