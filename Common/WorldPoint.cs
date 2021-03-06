/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2020 Justin Shannon
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
using System.Drawing;

namespace Vatsim.Xpilot.Common
{
	public struct WorldPoint
	{
		public double Lon { set; get; }

		public double Lat { set; get; }

		public bool IsNonZero { get { return ((Lon != 0.0) || (Lat != 0.0)); } }

		public bool IsZero { get { return !IsNonZero; } }

		public static WorldPoint Zero { get { return new WorldPoint(); } }

		public WorldPoint(double lon, double lat)
			: this()
		{
			Lon = lon;
			Lat = lat;
		}

		public WorldPoint Offset(double lonOffset, double latOffset)
		{
			return new WorldPoint(Lon + lonOffset, Lat + latOffset);
		}

		public double DistanceTo(WorldPoint point)
		{
			return GeoCalc.CartesianDistance(this, point);
		}

		public double DistanceTo(WorldPoint point, double scaleFactor)
		{
			return GeoCalc.CartesianDistance(this, point, scaleFactor);
		}

		public double GreatCircleDistanceTo(WorldPoint point)
		{
			return GeoCalc.GreatCircleDistance(this, point);
		}

		public override string ToString()
		{
			return string.Format("{0}, {1}", Lon, Lat);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			return (((WorldPoint)obj).Lon == Lon) && (((WorldPoint)obj).Lat == Lat);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static WorldPoint FromPoint(Point pt)
		{
			return new WorldPoint(pt.X, pt.Y);
		}

		public static WorldPoint FromPoint(PointF pt)
		{
			return new WorldPoint(pt.X, pt.Y);
		}
	}
}
