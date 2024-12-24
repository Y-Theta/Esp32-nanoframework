using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherDisplay.Utils
{
    public class Forecast
    {
        public string text_day { get; set; }
        public string text_night { get; set; }
        public int high { get; set; }
        public int low { get; set; }
        public string wc_day { get; set; }
        public string wd_day { get; set; }
        public string wc_night { get; set; }
        public string wd_night { get; set; }
        public string date { get; set; }
        public string week { get; set; }
    }

    public class Location
    {
        public string country { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Now
    {
        public string text { get; set; }
        public int temp { get; set; }
        public int feels_like { get; set; }
        public int rh { get; set; }
        public string wind_class { get; set; }
        public string wind_dir { get; set; }
        public string uptime { get; set; }
    }

    public class Result
    {
        public Location location { get; set; }
        public Now now { get; set; }
        public Forecast[] forecasts { get; set; }
    }

    public class Root
    {
        public int status { get; set; }
        public Result result { get; set; }
        public string message { get; set; }
    }
}
