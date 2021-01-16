using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class ShroudLayer
	{
		public bool RevealAll;
		public MPos Bounds { get; private set; }

		readonly bool[,,] shroudRevealed; // First: Team Second: X Third: Y
		public readonly Shroud[,] Shroud;
		readonly Dictionary<MPos, List<Shroud>> listenerPositions = new Dictionary<MPos, List<Shroud>>();
		readonly List<Shroud> changingShroud = new List<Shroud>();

		public ShroudLayer(MPos bounds)
		{
			Bounds = bounds * new MPos(2, 2);

			shroudRevealed = new bool[Settings.MaxTeams, Bounds.X, Bounds.Y];

			Shroud = new Shroud[Bounds.X, Bounds.Y];
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					var shroud = new Shroud(new MPos(x, y));

					var list = new List<Shroud>()
					{
						shroud
					};

					Shroud[x, y] = shroud;

					listenerPositions.Add(new MPos(x, y), list);
				}
			}
		}

		public void SetWall(MPos pos, int height, bool exists)
		{
			if (pos.Y == 0)
				return;

			var shroudCount = height / 512;

			if (shroudCount == 0)
				return;

			var xPos = pos.X * 2;
			var yPos = pos.Y * 2;

			if (xPos >= Bounds.X || yPos >= Bounds.Y)
				return;

			if (yPos - 1 < shroudCount)
				shroudCount = yPos - 1;

			for (int x = xPos; x < xPos + 2; x++)
			{
				for (int y = yPos - shroudCount; y < yPos; y++)
				{
					var shroud = Shroud[x, y];

					listenerPositions[shroud.Listener].Remove(shroud);

					shroud.Listener = exists ? new MPos(x, yPos) : new MPos(x, y);

					var listener = shroud.Listener;
					listenerPositions[listener].Add(shroud);

					if (shroud.ChangeState(shroudRevealed[Actor.PlayerTeam, listener.X, listener.Y]))
						changingShroud.Add(shroud);
				}
			}
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

			var isPlayerTeam = team == Actor.PlayerTeam;

			for (int i = 0; i < values.Length; i++)
			{
				var x = (int)Math.Floor(i / (float)Bounds.X);
				var y = i % Bounds.X;

				if (isPlayerTeam)
					changeState(x, y, values[i]);

				shroudRevealed[team, x, y] = values[i];
			}
		}

		public void RevealShroudCircular(World world, int team, CPos position, int height, int radius, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			var isPlayerTeam = team == Actor.PlayerTeam;

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

							if (dx + dy > radiusSquared)
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

							var result = !isInTriangle;

							shroudRevealed[team, x, y] = result;

							if (isPlayerTeam)
								changeState(x, y, result);
						}
					}
				}
			}

			// Camera automatically updates shroud, so we don't want to do that if we move anyways 
			if (!Camera.LockedToPlayer || ignoreLock)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		void changeState(int x, int y, bool value)
		{
			foreach (var shroud in listenerPositions[new MPos(x, y)])
			{
				if (shroud.ChangeState(value))
					changingShroud.Add(shroud);
			}

			if (value)
				VisibilitySolver.ShroudRevealed(x, y);
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

		public void Tick()
		{
			foreach (var shroud in changingShroud)
				shroud.Tick();

			changingShroud.RemoveAll(s => s.StateAchieved);
		}

		public void Clear()
		{
			Bounds = MPos.Zero;
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
