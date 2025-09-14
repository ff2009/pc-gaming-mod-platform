using System;
using PCGamingModApp.Models.Enums;

namespace PCGamingModApp.Models;

public class GameItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public eStoreType Store { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsFavorite { get; set; }
    public string? InstallPath { get; set; }
    public string IconKey { get; set; } = string.Empty; // key for the image cache
}