using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Helpers;

public static class ConversionHelper
{
    public static double ConvertBytesToUnit(long bytes, SizeUnit unit)
    {
        return unit switch
        {
            SizeUnit.KB => bytes / 1024d,
            SizeUnit.MB => bytes / (1024d * 1024),
            SizeUnit.GB => bytes / (1024d * 1024 * 1024),
            SizeUnit.TB => bytes / (1024d * 1024 * 1024 * 1024),
            SizeUnit.PB => bytes / (1024d * 1024 * 1024 * 1024d * 1024),
            SizeUnit.EB => bytes / (1024d * 1024 * 1024 * 1024 * 1024 * 1024),
            _ => bytes
        };
    }
}