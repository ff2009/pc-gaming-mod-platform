using CommunityToolkit.Mvvm.Messaging;
using PCGamingModApp.Core.Services.Interfaces;
using PCGamingModApp.Data.Entities;
using PCGamingModApp.Data.Repositories;

namespace PCGamingModApp.Core.Services.Implementations;

internal sealed class DownloadServiceFactory(
    IHttpClientFactory httpClientFactory,
    IDownloadRepository downloadRepository,
    IMessenger messenger)
    : IDownloadServiceFactory
{
    public ISingleDownloadService Create(
        DownloadDataModel download,
        Func<long> getSpeedLimit,
        Action<long> reportBandwidthUsage)
    {
        return new SingleDownloadService(
            download,
            httpClientFactory,
            downloadRepository,
            messenger,
            getSpeedLimit,
            reportBandwidthUsage);
    }
}