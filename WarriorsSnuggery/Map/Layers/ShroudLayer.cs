using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public sealed class ShroudLayer
	{
		public bool RevealAll;
		public MPos Bounds { get; private set; }

		readonly bool[,,] shroudRevealed; // First: Team Second: X Third: Y
		readonly byte[,] shroudAlpha;

		public ShroudLayer(MPos bounds)
		{
			Bounds = bounds * new MPos(2, 2);

			shroudRevealed = new bool[Settings.MaxTeams, bounds.X * 2, bounds.Y * 2];
			shroudAlpha = new byte[bounds.X * 2, bounds.Y * 2];
		}

		public bool ShroudRevealed(int team, int x, int y)
		{
			return RevealAll || shroudRevealed[team, x, y];
		}

		public bool ShroudRevealed(int team, MPos position)
		{
			return ShroudRevealed(team, position.X, position.Y);
		}

		public void RevealShroudList(int team, bool[] values)
		{
			if (values == null)
				return;

			for (int i = 0; i < values.Length; i++)
			{
				var x = (int)Math.Floor(i / (float)Bounds.X);
				var y = i % Bounds.X;

				shroudRevealed[team, x, y] = values[i];
			}
		}

		public void RevealShroudCircular(World world, int team, CPos position, int height, int radius, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			var isPlayerTeam = team == Objects.Actor.PlayerTeam;

			var shroudPos = (position * new CPos(2, 2, 0)).ToMPos();

			var radiusSquared = radius * radius;

			var triangles = getTriangles(world, position, height, shroudPos, radius);

			for (int x = shroudPos.X - radius; x < shroudPos.X + radius; x++)
			{
				if (x >= 0 && x < Bounds.X)
				{
					var dx = x - shroudPos.X;
					dx *= dx;

					for (int y = shroudPos.Y - radius; y < shroudPos.Y + radius; y++)
					{
						if (y >= 0 && y < Bounds.Y)
						{
							var dy = y - shroudPos.Y;
							dy *= dy;

							if (shroudRevealed[team, x, y])
								continue;

							bool isInTriangle = false;
							var p = new CPos(x * 512 - 256, y * 512 - 256, 0);
							foreach(var triangle in triangles)
							{
								if (triangle.IsInTriangle(p))
								{
									isInTriangle = true;
									break;
								}
							}

							shroudRevealed[team, x, y] = !isInTriangle && dx + dy <= radiusSquared;
							if (isPlayerTeam)
								VisibilitySolver.ShroudRevealed(x, y);
						}
					}
				}
			}

			// Camera automatically updates shroud, so we don't want to do that if we move anyways 
			if (!Camera.LockedToPlayer || ignoreLock)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		List<Triangle> getTriangles(World world, CPos position, int height, MPos shroudPos, int radius)
		{
			var outerRadius = (float)Math.Sqrt(2) * radius * 1024;

			var pos1 = (shroudPos - new MPos(radius, radius)) / new MPos(2, 2);
			// Why MPos(1, 1) here? -> when 7/2=3, but we want 4. Thus (7+1)/2=4. (8+1)/2=4 so works for this case as well.
			var pos2 = (shroudPos + new MPos(radius, radius) + new MPos(1, 1)) / new MPos(2, 2);

			var triangles = new List<Triangle>();

			var walls = world.WallLayer.GetRange(pos1, pos2);
			foreach (var wall in walls)
			{
				if (wall.Type.IsTransparent || wall.Type.IsOnFloor)
					continue;

				if (height > wall.Type.Height)
					continue;

				var angleA = (position - wall.EndPointA).FlatAngle;
				var angleB = (position - wall.EndPointB).FlatAngle;

				var pointC = new CPos(position.X + (int)(Math.Cos(angleA) * outerRadius), position.Y + (int)(Math.Sin(angleA) * outerRadius), 0);
				var pointD = new CPos(position.X + (int)(Math.Cos(angleB) * outerRadius), position.Y + (int)(Math.Sin(angleB) * outerRadius), 0);

				triangles.Add(new Triangle(wall.EndPointA, wall.EndPointB, pointC));
				triangles.Add(new Triangle(wall.EndPointB, pointC, pointD));
			}

			return triangles;
		}

		public float ShroudAlpha(MPos position, int team)
		{
			var alpha = shroudAlpha[position.X, position.Y];
			if (alpha < 4 && shroudRevealed[team, position.X, position.Y])
				shroudAlpha[position.X, position.Y]++;

			return 1 - alpha / 4f;
		}

		public string ToString(int team)
		{
			var shroud = team + "=";

			for (int x = 0; x < Bounds.X; x++)
				for (int y = 0; y < Bounds.Y; y++)
					shroud += ShroudRevealed(team, x, y).GetHashCode() + ",";

			shroud = shroud.TrimEnd(',');

			return shroud;
		}

		public void Clear()
		{
			Bounds = MPos.Zero;
		}

		class Triangle
		{
			public readonly CPos PointA;
			public readonly CPos PointB;
			public readonly CPos PointC;

			public Triangle(CPos a, CPos b, CPos c)
			{
				PointA = a;
				PointB = b;
				PointC = c;
			}

			public bool IsInTriangle(CPos p)
			{
				var x1 = PointA.X;
				var x2 = PointB.X;
				var x3 = PointC.X;
				var y1 = PointA.Y;
				var y2 = PointB.Y;
				var y3 = PointC.Y;

				var x = p.X;
				var y = p.Y;

				double a = ((y2 - y3) * (double)(x - x3) + (x3 - x2) * (double)(y - y3)) / ((y2 - y3) * (double)(x1 - x3) + (x3 - x2) * (double)(y1 - y3));
				double b = ((y3 - y1) * (double)(x - x3) + (x1 - x3) * (double)(y - y3)) / ((y2 - y3) * (double)(x1 - x3) + (x3 - x2) * (double)(y1 - y3));
				double c = 1 - a - b;

				return a >= 0 && a <= 1 && b >= 0 && b <= 1 && c >= 0 && c <= 1;
			}
		}
	}
}
