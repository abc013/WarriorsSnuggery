﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class ShroudLayer : ISaveable
	{
		public bool RevealAll;
		public MPos Bounds { get; private set; }

		public readonly List<byte> UsedTeams = new List<byte>();
		readonly Dictionary<byte, bool[,]> shroudRevealed = new Dictionary<byte, bool[,]>();

		public readonly Shroud[,] Shroud;
		readonly Dictionary<MPos, List<Shroud>> listenerPositions = new Dictionary<MPos, List<Shroud>>();
		readonly List<Shroud> changingShroud = new List<Shroud>();

		public ShroudLayer(MPos bounds)
		{
			Bounds = bounds * 2;

			Shroud = new Shroud[Bounds.X, Bounds.Y];
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					var shroud = new Shroud(new MPos(x, y));

					var list = new List<Shroud>() { shroud };

					Shroud[x, y] = shroud;

					listenerPositions.Add(new MPos(x, y), list);
				}
			}
		}

		void initShroudLayer(byte team)
		{
			UsedTeams.Add(team);

			shroudRevealed.Add(team, new bool[Bounds.X, Bounds.Y]);
		}

		public void ClearWall(Wall wall)
		{
			setWall(wall.TerrainPosition, wall.Type.Height, false);
		}

		public void SetWall(Wall wall)
		{
			setWall(wall.TerrainPosition, wall.Type.Height, true);
		}

		void setWall(MPos pos, int height, bool remove)
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

					shroud.Listener = remove ? new MPos(x, yPos) : new MPos(x, y);

					var listener = shroud.Listener;
					listenerPositions[listener].Add(shroud);

					if (shroud.ChangeState(ShroudRevealed(Actor.PlayerTeam, listener.X, listener.Y)))
						changingShroud.Add(shroud);
				}
			}
		}

		public bool ShroudRevealed(byte team, int x, int y)
		{
			if (!UsedTeams.Contains(team))
				return false;

			return RevealAll || shroudRevealed[team][x, y];
		}

		public bool ShroudRevealedOnTile(byte team, MPos position)
		{
			return ShroudRevealedOnTile(team, position.X, position.Y);
		}

		public bool ShroudRevealedOnTile(byte team, int x, int y)
		{
			if (RevealAll)
				return true;

			// As shroud has a doubled tilesize, we have to check 4 tiles.
			return ShroudRevealed(team, x * 2, y * 2)
				|| ShroudRevealed(team, x * 2 + 1, y * 2)
				|| ShroudRevealed(team, x * 2 + 1, y * 2 + 1)
				|| ShroudRevealed(team, x * 2, y * 2 + 1);
		}

		public void RevealShroudList(byte team, bool[] values)
		{
			if (values == null)
				return;

			if (!UsedTeams.Contains(team))
				initShroudLayer(team);

			var isPlayerTeam = team == Actor.PlayerTeam;

			for (int i = 0; i < values.Length; i++)
			{
				var x = (int)Math.Floor(i / (float)Bounds.X);
				var y = i % Bounds.X;

				if (isPlayerTeam)
					changeState(x, y, values[i]);

				shroudRevealed[team][x, y] = values[i];
			}
		}

		public void RevealShroudRectangular(byte team, CPos topleft, CPos bottomright, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			if (!UsedTeams.Contains(team))
				initShroudLayer(team);

			var isPlayerTeam = team == Actor.PlayerTeam;

			var shroudTopLeft = (topleft * 2).ToMPos();
			var shroudBottomRight = (bottomright * 2).ToMPos();

			for (int x = shroudTopLeft.X ; x < shroudBottomRight.X; x++)
			{
				if (x >= 0 && x < Bounds.X)
				{
					for (int y = shroudTopLeft.Y; y < shroudBottomRight.Y; y++)
					{
						if (y >= 0 && y < Bounds.Y)
							shroudRevealed[team][x, y] = true;

						if (isPlayerTeam)
							changeState(x, y, true);
					}
				}
			}
		}

		public void RevealShroudCircular(World world, byte team, CPos position, int radius, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			if (!UsedTeams.Contains(team))
				initShroudLayer(team);

			var isPlayerTeam = team == Actor.PlayerTeam;

			var shroudPos = (position * 2).ToMPos();

			var radiusSquared = radius * radius;

			var triangles = getTriangles(world, position, shroudPos, radius);

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

							if (shroudRevealed[team][x, y])
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

							shroudRevealed[team][x, y] = result;

							if (isPlayerTeam)
								changeState(x, y, result);
						}
					}
				}
			}
		}

		void changeState(int x, int y, bool value)
		{
			foreach (var shroud in listenerPositions[new MPos(x, y)])
			{
				if (shroud.ChangeState(value))
					changingShroud.Add(shroud);
			}
		}

		List<Triangle> getTriangles(World world, CPos position, MPos shroudPos, int radius)
		{
			var outerRadius = MathF.Sqrt(2) * radius * 1024;

			var pos1 = (shroudPos - new MPos(radius, radius)) / 2;
			// Why MPos(1, 1) here? -> when 7/2=3, but we want 4. Thus (7+1)/2=4. (8+1)/2=4 so works for this case as well.
			var pos2 = (shroudPos + new MPos(radius, radius) + new MPos(1, 1)) / 2;

			var triangles = new List<Triangle>();

			foreach (var wall in world.WallLayer.GetRange(pos1, pos2))
			{
				if (wall.Physics.IsEmpty || wall.Type.IsTransparent)
					continue;

				if (position.Z > wall.Type.Height)
					continue;

				var angleA = (position - wall.EndPointA).FlatAngle;
				var angleB = (position - wall.EndPointB).FlatAngle;

				var pointC = position + CPos.FromFlatAngle(angleA, outerRadius);
				var pointD = position + CPos.FromFlatAngle(angleB, outerRadius);

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

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			foreach (var team in UsedTeams)
			{
				var shroud = string.Empty;

				for (int x = 0; x < Bounds.X; x++)
					for (int y = 0; y < Bounds.Y; y++)
						shroud += ShroudRevealed(team, x, y).GetHashCode() + ",";

				saver.Add(team.ToString(), shroud[0..^1]);
			}
			
			return saver;
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
