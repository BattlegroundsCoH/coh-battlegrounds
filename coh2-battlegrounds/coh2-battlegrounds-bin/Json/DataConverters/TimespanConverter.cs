using System;
using System.Linq;

namespace Battlegrounds.Json.DataConverters {

    public class TimespanConverter : IJsonDataTypeConverter<TimeSpan> {
        
        public TimeSpan ConvertFromString(string stringValue) {
            if (stringValue.Count(x => x == ':') == 2) {
                string[] vals = stringValue.Split(':');
                if (int.TryParse(vals[0], out int hours) && int.TryParse(vals[1], out int minutes) && int.TryParse(vals[2], out int seconds)) {
                    return new TimeSpan(hours, minutes, seconds);
                }
            }
            throw new FormatException();
        }

        public string ConvertToString(TimeSpan value) => $"{value.Hours}:{value.Minutes}:{value.Seconds}";

    }

}
