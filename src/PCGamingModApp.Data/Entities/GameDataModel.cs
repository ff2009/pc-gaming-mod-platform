using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Data.Entities;

public class GameDataModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public StoreType Store { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsFavorite { get; set; }
    public string? InstallPath { get; set; }
    public string IconKey { get; set; } = string.Empty; // key for the image cache
    
    public int PCGamingWikiPageID { get; set; }
}