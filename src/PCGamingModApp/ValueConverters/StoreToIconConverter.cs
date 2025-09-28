using System;
using System.Globalization;
using Avalonia.Data.Converters;
using PCGamingModApp.Core.Helpers;
using PCGamingModApp.Data.Enums;

namespace PCGamingModApp.ValueConverters;

public sealed class StoreToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is StoreType store)
        {
            return store switch
            {
                StoreType.EaApp => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/ea.png")),
                StoreType.Epic => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/epic.png")),
                StoreType.GoG => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/gog.png")),
                StoreType.Steam => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/steam.png")),
                StoreType.Ubisoft => ImageHelper.LoadFromResource(new Uri("avares://PCGamingModApp/Assets/Images/StoreIcons/ubisoft.png")),
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