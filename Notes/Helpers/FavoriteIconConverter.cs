using System.Globalization;

namespace Notes.Helpers;

public class FavoriteIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isFavorite = (bool)value;
        return isFavorite ? "star_filled.png" : "star_empty.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}