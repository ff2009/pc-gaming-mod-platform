using PCGamingModApp.Core.Services.Interfaces;

namespace PCGamingModApp.Core.Services.Implementations;

public class AppPaths : IAppPaths
{
    private string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    public string Root  => Path.Combine(LocalAppData, "PCGamingModPlatform");
    public string GameIcons => Path.Combine(Root, "game_icons");
    public string Database => Path.Combine(Root, "database", "pcgamingmod.db");
    public string Downloads => Path.Combine(Root, "downloads");
    //public string ModsArchive => Path.Combine(Root, "mods_archive");
    public string Temp => Path.Combine(Root, "temp");
    public string Logs => Path.Combine(Root, "logs");
    public string Config => Path.Combine(Root, "config");
    
    public string Assets => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets");
    public string StoreIcons => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "StoreIcons");

    public void EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(GameIcons);
        Directory.CreateDirectory(Database);
        Directory.CreateDirectory(Downloads);
        //Directory.CreateDirectory(ModsArchive);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Config);
    }

    public void Migrate()
    {
        /*string versionFile = Path.Combine(Root, "version.txt");
        if (!File.Exists(versionFile))
        {
            File.WriteAllText(versionFile, "1.0.0");
        }
        else
        {
            string currentVersion = File.ReadAllText(versionFile);
            if (currentVersion == "1.0.0")
            {
                string oldModsDir = Path.Combine(Root, "old_mods");
                if (Directory.Exists(oldModsDir))
                {
                    Directory.Move(oldModsDir, ModsArchive);
                    File.WriteAllText(versionFile, "1.1.0");
                }
            }
        }*/
    }
}