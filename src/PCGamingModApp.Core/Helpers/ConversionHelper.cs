using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Helpers;

public static class ConversionHelper
{
    public static long ConvertBytesToUnit(long bytes, SizeUnit unit)
    {
        return unit switch
        {
            SizeUnit.KB => bytes / 1024L,
            SizeUnit.MB => bytes / (1024L * 1024L),
            SizeUnit.GB => bytes / (1024L * 1024L * 1024L),
            SizeUnit.TB => bytes / (1024L * 1024L * 1024L * 1024L),
            SizeUnit.PB => bytes / (1024L * 1024L * 1024L * 1024L * 1024L),
            SizeUnit.EB => bytes / (1024L * 1024L * 1024L * 1024L * 1024L * 1024L),
            _ => bytes
        };
    }
}