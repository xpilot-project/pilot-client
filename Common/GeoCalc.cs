/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2021 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vatsim.Xpilot.Common
{
	public enum DistanceUnits { Degrees, NauticalMiles, Feet }

	public static class GeoCalc
	{
		private const double EARTH_RADIUS_NM = 3437.670013352;
		private const double PI_OVER_ONE_EIGHTY = 0.0174532925;
		private const double ONE_EIGHTY_OVER_PI = 57.2957795;
		public const int FEET_PER_NM = 6076;
		private const double METERS_PER_FOOT = 0.3048;
		private const double METERS_PER_NM = FEET_PER_NM * METERS_PER_FOOT;
		public const double NM_PER_DEG = 60.0;
		private const double FEET_PER_DEG = NM_PER_DEG * FEET_PER_NM;
		private const double METERS_PER_DEG = FEET_PER_DEG * METERS_PER_FOOT;
		public const double DEG_PER_NM = 1.0 / NM_PER_DEG;

		public static double ConvertDistanceToDegrees(double dist, DistanceUnits units)
		{
			switch (units)
			{
				case DistanceUnits.Degrees: return dist;
				case DistanceUnits.NauticalMiles: return NauticalMilesToDegrees(dist);
				case DistanceUnits.Feet: return FeetToDegrees(dist);
				default: throw new ArgumentException(string.Format("Unsupported DistanceUnits value: {0}", units));
			}
		}

		public static double LongitudeScalingFactor(WorldPoint point)
		{
			return LongitudeScalingFactor(point.Lat);
		}

		public static double LongitudeScalingFactor(double lat)
		{
			return NauticalMilesPerDegreeLon(lat) / NM_PER_DEG;
		}

		public static double FeetToDegrees(int feet)
		{
			return FeetToDegrees((double)feet);
		}

		public static double FeetToDegrees(double feet)
		{
			return feet / FEET_PER_NM / NM_PER_DEG;
		}

		public static double MetersToDegrees(double meters)
		{
			return meters / METERS_PER_NM / NM_PER_DEG;
		}

		public static double FeetToNauticalMiles(int feet)
		{
			return FeetToNauticalMiles((double)feet);
		}

		public static double FeetToNauticalMiles(double feet)
		{
			return feet / FEET_PER_NM;
		}

		public static double DegreesToFeet(double degrees)
		{
			return degrees * FEET_PER_DEG;
		}

		public static double DegreesToMeters(double degrees)
		{
			return degrees * METERS_PER_DEG;
		}

		public static double DegreesToNauticalMiles(double degrees)
		{
			return degrees * NM_PER_DEG;
		}

		public static double NauticalMilesToDegrees(double nm)
		{
			return nm / NM_PER_DEG;
		}

		public static double NauticalMilesPerDegreeLon(WorldPoint loc)
		{
			return NauticalMilesPerDegreeLon(loc.Lat);
		}

		public static double NauticalMilesPerDegreeLon(double lat)
		{
			return PI_OVER_ONE_EIGHTY * EARTH_RADIUS_NM * Math.Cos(DegreesToRadians(lat));
		}

		public static double DegreesToRadians(double degrees)
		{
			return degrees * PI_OVER_ONE_EIGHTY;
		}

		public static double DegreesToRadians(int degrees)
		{
			return DegreesToRadians((double)degrees);
		}

		public static double RadiansToDegrees(double radians)
		{
			return radians * ONE_EIGHTY_OVER_PI;
		}

		public static double CartesianDistance(WorldPoint pointA, WorldPoint pointB)
		{
			return CartesianDistance(pointA.Lon, pointA.Lat, pointB.Lon, pointB.Lat, 1.0);
		}

		public static double CartesianDistance(WorldPoint pointA, WorldPoint pointB, double scaleFactor)
		{
			return CartesianDistance(pointA.Lon, pointA.Lat, pointB.Lon, pointB.Lat, scaleFactor);
		}

		public static double CartesianDistance(double lon1, double lat1, double lon2, double lat2)
		{
			return CartesianDistance(lon1, lat1, lon2, lat2, 1.0);
		}

		public static double CartesianDistance(double lon1, double lat1, double lon2, double lat2, double scaleFactor)
		{
			double deltaLon = Math.Abs(lon2 - lon1);
			double deltaLat = Math.Abs(lat2 - lat1);
			deltaLon *= scaleFactor;
			return Math.Sqrt((deltaLon * deltaLon) + (deltaLat * deltaLat));
		}

		public static double GreatCircleDistance(WorldPoint pointA, WorldPoint pointB)
		{
			return GreatCircleDistance(pointA.Lon, pointA.Lat, pointB.Lon, pointB.Lat);
		}

		public static double GreatCircleDistance(double lon1, double lat1, double lon2, double lat2)
		{
			/*
				The Haversine formula according to Dr. Math.
				http://mathforum.org/library/drmath/view/51879.html

				dlon = lon2 - lon1
				dlat = lat2 - lat1
				a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
				c = 2 * atan2(sqrt(a), sqrt(1-a)) 
				d = R * c

				Where
					* dlon is the change in longitude
					* dlat is the change in latitude
					* c is the great circle distance in Radians.
					* R is the radius of a spherical Earth.
					* The locations of the two points in 
						spherical coordinates (longitude and 
						latitude) are lon1,lat1 and lon2, lat2.
			*/
			double lat1Rad = DegreesToRadians(lat1);
			double lon1Rad = DegreesToRadians(lon1);
			double lat2Rad = DegreesToRadians(lat2);
			double lon2Rad = DegreesToRadians(lon2);

			double deltaLon = lon2Rad - lon1Rad;
			double deltaLat = lat2Rad - lat1Rad;

			// Intermediate result a.
			double a = Math.Pow(Math.Sin(deltaLat / 2.0), 2.0) +
				Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
				Math.Pow(Math.Sin(deltaLon / 2.0), 2.0);

			// Intermediate result c (great circle distance in Radians).
			double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));

			// Convert distance to NM.
			return EARTH_RADIUS_NM * c;
		}

		public static void FindIntersection(
			WorldPoint p1, WorldPoint p2, WorldPoint p3, WorldPoint p4,
			out bool linesIntersect, out bool segmentsIntersect,
			out WorldPoint intersection, out WorldPoint closestPoint1, out WorldPoint closestPoint2)
		{
			// Get the segments' parameters.
			double dx12 = p2.Lon - p1.Lon;
			double dy12 = p2.Lat - p1.Lat;
			double dx34 = p4.Lon - p3.Lon;
			double dy34 = p4.Lat - p3.Lat;

			// Solve for t1 and t2
			double denominator = (dy12 * dx34 - dx12 * dy34);

			double t1;
			try
			{
				t1 = ((p1.Lon - p3.Lon) * dy34 + (p3.Lat - p1.Lat) * dx34) / denominator;
			}
			catch
			{
				// The lines are parallel (or close enough to it).
				linesIntersect = false;
				segmentsIntersect = false;
				intersection = WorldPoint.Zero;
				closestPoint1 = WorldPoint.Zero;
				closestPoint2 = WorldPoint.Zero;
				return;
			}
			linesIntersect = true;

			double t2 = ((p3.Lon - p1.Lon) * dy12 + (p1.Lat - p3.Lat) * dx12) / -denominator;

			// Find the point of intersection.
			intersection = new WorldPoint(p1.Lon + dx12 * t1, p1.Lat + dy12 * t1);

			// The segments intersect if t1 and t2 are between 0 and 1.
			segmentsIntersect = ((t1 >= 0) && (t1 <= 1) && (t2 >= 0) && (t2 <= 1));

			// Find the closest points on the segments.
			if (t1 < 0)
			{
				t1 = 0;
			}
			else if (t1 > 1)
			{
				t1 = 1;
			}

			if (t2 < 0)
			{
				t2 = 0;
			}
			else if (t2 > 1)
			{
				t2 = 1;
			}

			closestPoint1 = new WorldPoint(p1.Lon + dx12 * t1, p1.Lat + dy12 * t1);
			closestPoint2 = new WorldPoint(p3.Lon + dx34 * t2, p3.Lat + dy34 * t2);
		}
	}
}
