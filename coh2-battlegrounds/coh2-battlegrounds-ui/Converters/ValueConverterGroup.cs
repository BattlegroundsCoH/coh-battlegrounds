﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Battlegrounds.UI.Converters;

public sealed class ValueConverterGroup : List<IValueConverter>, IValueConverter {
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }

}
