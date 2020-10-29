using System;

namespace Dfc.GeoCoordinate
{
    public class GeoHelper
    {
        // Adapted from https://github.com/microsoft/referencesource/blob/bf498ea2b1a6270a2fe5cb122acf4b1c5b45c21d/System.Device/GeoCoordinate.cs#L190-L239
        public static double GetDistanceTo((double lat, double lng) from, (double lat, double lng) to)
        {
            //  The Haversine formula according to Dr. Math.
            //  http://mathforum.org/library/drmath/view/51879.html

            //  dlon = lon2 - lon1
            //  dlat = lat2 - lat1
            //  a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            //  c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            //  d = R * c

            //  Where
            //    * dlon is the change in longitude
            //    * dlat is the change in latitude
            //    * c is the great circle distance in Radians.
            //    * R is the radius of a spherical Earth.
            //    * The locations of the two points in 
            //        spherical coordinates (longitude and 
            //        latitude) are lon1,lat1 and lon2, lat2.

            if (double.IsNaN(from.lat) || double.IsNaN(from.lng) ||
                double.IsNaN(to.lat) || double.IsNaN(to.lng))
            {
                throw new ArgumentException("The coordinate's latitude or longitude is not a number.");
            }

            double dDistance = double.NaN;

            double dLat1 = from.lat * (Math.PI / 180.0);
            double dLon1 = from.lng * (Math.PI / 180.0);
            double dLat2 = to.lat * (Math.PI / 180.0);
            double dLon2 = to.lng * (Math.PI / 180.0);

            double dLon = dLon2 - dLon1;
            double dLat = dLat2 - dLat1;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLat / 2.0), 2.0) +
                       Math.Cos(dLat1) * Math.Cos(dLat2) *
                       Math.Pow(Math.Sin(dLon / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

            // Distance.
            const double kEarthRadiusMs = 6376500;
            dDistance = kEarthRadiusMs * c;

            return dDistance / 1000;
        }

        public static double MilesToKilometers(double miles) => miles * 1.609344;

        public static double KilometersToMiles(double km) => km / 1.609344;
    }
}
