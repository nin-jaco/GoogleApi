using System;

namespace Geocode.Code
{
    public struct GeoLoc
    {
        public double Lat;
        public double Lon;

        public GeoLoc(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public override string ToString()
        {
            return "Latitude: " + Lat.ToString() + " Longitude: " + Lon.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        public string ToQueryString()
        {
            return "+to:" + Lat + "%2B" + Lon;
        }

    }
}
