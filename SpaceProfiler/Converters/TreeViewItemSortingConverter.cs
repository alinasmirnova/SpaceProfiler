using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace SpaceProfiler.Converters;

public class TreeViewItemSortingConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var collection = value as IList;
        var view = new ListCollectionView(collection ?? throw new InvalidOperationException());
        var sort = new SortDescription(parameter?.ToString() ?? string.Empty, ListSortDirection.Descending);
        view.SortDescriptions.Add(sort);
        view.LiveSortingProperties.Add(parameter?.ToString());
        view.IsLiveSorting = true;
        return view;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}