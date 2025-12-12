using PCGamingModApp.Core.ViewModels;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDialogProvider
{
    DialogViewModel Dialog { get; set; }
}