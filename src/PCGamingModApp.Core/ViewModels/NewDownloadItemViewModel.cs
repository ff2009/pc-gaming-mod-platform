using CommunityToolkit.Mvvm.ComponentModel;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.ViewModels;

public partial class NewDownloadItemViewModel : ViewModelBase
{
    [ObservableProperty] private Guid _id;
    
    [ObservableProperty] private string _url = string.Empty;

    [ObservableProperty] private string _fileName = string.Empty;
    
    [ObservableProperty] private string _savePath = string.Empty;

    [ObservableProperty] private long _fileSizeInBytes;
    
    [ObservableProperty] private DateTime _createdAt;
    
    [ObservableProperty] private string _unit = "MB"; // in bytes per second
    
    public double FileSize
    {
        get
        {
            double fileSize;
            switch (Unit)
            {
                case "KB":
                    fileSize = FileSizeInBytes / 1024d;
                    break;
                case "MB":
                    fileSize = FileSizeInBytes / (1024d * 1024d);
                    break;
                case "GB":
                    fileSize = FileSizeInBytes / (1024d * 1024d * 1024d);
                    break;
                default:
                    fileSize = FileSizeInBytes;
                    break;
            }

            return fileSize;
        }
    }
    
    
    /*public async Task GetMetadata()
    {
        // Simulate fetching metadata for the new download URL
        if (Uri.IsWellFormedUriString(NewDownloadUrl, UriKind.Absolute))
        {
            var datamodel = await _downloadService.GetDownloadMetadataAsync(NewDownloadUrl);
            NewDownloadFileSizeBytes = datamodel.FileSizeInBytes;
            NewDownloadDestinationPath = datamodel.SavePath;
            NewDownloadFilename = datamodel.FileName;
            return;
        }

        NewDownloadFileSizeBytes = 0;
    }*/
}

public static class NewDownloadItemViewModelExtensions
{
    /// <summary>
    /// Map the Game View Model to the Data Model (useful for persistence)
    /// </summary>
    /// <returns></returns>
    ///
    extension(NewDownloadItemViewModel viewModel)
    {
        public DownloadDataModel ToDataModel() => new()
        {
            Id = viewModel.Id,
            FileName = viewModel.FileName,
            Url = viewModel.Url,
            SavePath = viewModel.SavePath,
            FileSizeInBytes = viewModel.FileSizeInBytes,
            Status = DownloadStatus.NotStarted,
        };
    }
}