using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.Core.Helpers;

public static class ConversionHelper
{
    public static double ConvertBytesToUnit(long bytes, SizeUnit unit)
    {
        return unit switch
        {
            SizeUnit.KB => bytes / 1024d,
            SizeUnit.MB => bytes / (1024d * 1024d),
            SizeUnit.GB => bytes / (1024d * 1024d * 1024d),
            SizeUnit.TB => bytes / (1024d * 1024d * 1024d * 1024d),
            _ => bytes
        };
    }
}