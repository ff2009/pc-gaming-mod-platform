using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PCGamingModApp.Models.Enums;

namespace PCGamingModApp.ValueConverters;

public sealed class StoreToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is eStoreType store)
        {
            return store switch
            {
                eStoreType.EaApp => "ea.png",
                eStoreType.Epic => "epic.png",
                eStoreType.GoG => "gog.png",
                eStoreType.Steam => "steam.png",
                eStoreType.Ubisoft => "ubisoft.png",
                _ => "unknown.png"
            };
        }

        return "unknown.png";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}