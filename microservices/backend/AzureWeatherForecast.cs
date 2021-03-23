using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend
{
    public class AzureWeatherForecast
    {
        public class Rootobject
        {
            public Result[] results { get; set; }
        }

        public class Result
        {
            public DateTime dateTime { get; set; }
            public string phrase { get; set; }
            public int iconCode { get; set; }
            public bool hasPrecipitation { get; set; }
            public bool isDayTime { get; set; }
            public Temperature temperature { get; set; }
            public Realfeeltemperature realFeelTemperature { get; set; }
            public Realfeeltemperatureshade realFeelTemperatureShade { get; set; }
            public int relativeHumidity { get; set; }
            public Dewpoint dewPoint { get; set; }
            public Wind wind { get; set; }
            public Windgust windGust { get; set; }
            public int uvIndex { get; set; }
            public string uvIndexPhrase { get; set; }
            public Visibility visibility { get; set; }
            public string obstructionsToVisibility { get; set; }
            public int cloudCover { get; set; }
            public Ceiling ceiling { get; set; }
            public Pressure pressure { get; set; }
            public Pressuretendency pressureTendency { get; set; }
            public Past24hourtemperaturedeparture past24HourTemperatureDeparture { get; set; }
            public Apparenttemperature apparentTemperature { get; set; }
            public Windchilltemperature windChillTemperature { get; set; }
            public Wetbulbtemperature wetBulbTemperature { get; set; }
            public Precipitationsummary precipitationSummary { get; set; }
            public Temperaturesummary temperatureSummary { get; set; }
        }

        public class Temperature
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Realfeeltemperature
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Realfeeltemperatureshade
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Dewpoint
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Wind
        {
            public Direction direction { get; set; }
            public Speed speed { get; set; }
        }

        public class Direction
        {
            public float degrees { get; set; }
            public string localizedDescription { get; set; }
        }

        public class Speed
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Windgust
        {
            public Speed1 speed { get; set; }
        }

        public class Speed1
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Visibility
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Ceiling
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Pressure
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Pressuretendency
        {
            public string localizedDescription { get; set; }
            public string code { get; set; }
        }

        public class Past24hourtemperaturedeparture
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Apparenttemperature
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Windchilltemperature
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Wetbulbtemperature
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Precipitationsummary
        {
            public Pasthour pastHour { get; set; }
            public Past3hours past3Hours { get; set; }
            public Past6hours past6Hours { get; set; }
            public Past9hours past9Hours { get; set; }
            public Past12hours past12Hours { get; set; }
            public Past18hours past18Hours { get; set; }
            public Past24hours past24Hours { get; set; }
        }

        public class Pasthour
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past3hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past6hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past9hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past12hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past18hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past24hours
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Temperaturesummary
        {
            public Past6hours1 past6Hours { get; set; }
            public Past12hours1 past12Hours { get; set; }
            public Past24hours1 past24Hours { get; set; }
        }

        public class Past6hours1
        {
            public Minimum minimum { get; set; }
            public Maximum maximum { get; set; }
        }

        public class Minimum
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Maximum
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past12hours1
        {
            public Minimum1 minimum { get; set; }
            public Maximum1 maximum { get; set; }
        }

        public class Minimum1
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Maximum1
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Past24hours1
        {
            public Minimum2 minimum { get; set; }
            public Maximum2 maximum { get; set; }
        }

        public class Minimum2
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

        public class Maximum2
        {
            public float value { get; set; }
            public string unit { get; set; }
            public int unitType { get; set; }
        }

    }
}
