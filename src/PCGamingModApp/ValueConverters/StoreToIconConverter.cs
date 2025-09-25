using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PCGamingModApp.Helpers;
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
                eStoreType.EaApp => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/ea.png")),
                eStoreType.Epic => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/epic.png")),
                eStoreType.GoG => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/gog.png")),
                eStoreType.Steam => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/steam.png")),
                eStoreType.Ubisoft => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/ubisoft.png")),
                _ => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/cd.png"))
            };
        }

        return ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/cd.png"));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}