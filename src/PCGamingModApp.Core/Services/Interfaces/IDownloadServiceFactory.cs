using PCGamingModApp.Data.Entities;

namespace PCGamingModApp.Core.Services.Interfaces;

public interface IDownloadServiceFactory
{
    ISingleDownloadService Create(
        DownloadDataModel download,
        Func<long> getSpeedLimit,
        Action<long> reportBandwidthUsage);
}