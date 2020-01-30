using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public class RayPhysics
	{
		static readonly CPos invalid = new CPos(int.MaxValue, int.MaxValue, 0);

		public CPos Start;
		public int StartHeight;
		public CPos End { get; private set; }
		public int EndHeight;
		public CPos Target;
		public int TargetHeight;

		readonly CPos[][] mapBounds;
		MPos[] positions = new MPos[0];
		readonly World world;

		public RayPhysics(World world)
		{
			this.world = world;

			var map = world.Map;
			mapBounds = new[]
			{
				new [] { map.TopLeftCorner, map.TopRightCorner },
				new [] { map.TopLeftCorner, map.BottomLeftCorner },
				new [] { map.TopRightCorner, map.BottomRightCorner },
				new [] { map.BottomLeftCorner, map.BottomRightCorner }
			};
		}

		public void CalculateEnd(Actor[] ignore = null)
		{
			var closestIntersect = new CPos(0, 0, int.MaxValue);
			var closestT1 = double.MaxValue;

			positions = getPositions(Start, Target);
			var sectors = new List<MPos>();

			// Collision at walls
			foreach (var pos in positions)
			{
				if (pos.X < 0 || pos.Y < 0)
					continue;

				var walls = new Wall[2];
				walls[0] = world.WallLayer.Walls[pos.X * 2,pos.Y];
				walls[1] = world.WallLayer.Walls[pos.X * 2 + 1, pos.Y];

				var hit = false;
				foreach (var wall in walls)
				{
					if (wall == null)
						continue;

					var lines = wall.Physics.GetLines();
					foreach (var line in lines)
					{
						var end = getIntersection(line.Start, line.End, out var t1);
						if (end != invalid && t1 < closestT1)
						{
							var height = calculateHeight(end);
							if (height <= wall.Physics.Height + wall.Physics.HeightRadius || height >= wall.Physics.Height - wall.Physics.HeightRadius)
							{
								closestIntersect = end;
								closestT1 = t1;
								EndHeight = height;
								hit = true;
							}
						}
					}
				}

				// Add sectors that need to be checked for collision
				var sector = pos / new MPos(2, 2);
				if (!sectors.Contains(sector))
					sectors.Add(sector);

				// We hit something, therefore we can ignore the rest which is further away
				if (hit)
					break;
			}

			// Collision at actors
			foreach (var sectorPos in sectors)
			{
				var sector = world.PhysicsLayer.Sectors[sectorPos.X, sectorPos.Y];

				var objs = sector.GetObjects(ignore);

				var hit = false;
				foreach (var obj in objs)
				{
					if (obj.Physics.Shape == Shape.CIRCLE)
					{
						var end = getIntersection(obj.Physics.Position, obj.Physics.RadiusX, out var t1, out var t2, out var end2);
						if (end != invalid && t1 < closestT1)
						{
							var height = calculateHeight(end);
							if (height <= obj.Physics.Height + obj.Physics.HeightRadius && height >= obj.Physics.Height - obj.Physics.HeightRadius)
							{
								closestIntersect = end;
								closestT1 = t1;
								EndHeight = height;
								hit = true;
							}
							else if (t2 < closestT1)
							{
								height = calculateHeight(end2);
								if (height <= obj.Physics.Height + obj.Physics.HeightRadius && height >= obj.Physics.Height - obj.Physics.HeightRadius)
								{
									closestIntersect = end2;
									closestT1 = t2;
									EndHeight = height;
									hit = true;
								}
							}
						}
					}
					else
					{
						foreach (var line in obj.Physics.GetLines())
						{
							var end = getIntersection(line.Start, line.End, out var t1);
							if (end != invalid && t1 < closestT1)
							{
								var height = calculateHeight(end);
								if (height <= obj.Physics.Height + obj.Physics.HeightRadius && height >= obj.Physics.Height - obj.Physics.HeightRadius)
								{
									closestIntersect = end;
									closestT1 = t1;
									EndHeight = height;
									hit = true;
								}
							}
						}
					}
				}

				// If we hit something, we won't need to check any sectors behind
				if (hit)
					break;
			}

			// Collision at map bounds, if nothing was hit
			if (closestIntersect == new CPos(0, 0, int.MaxValue))
			{
				foreach (var line in mapBounds)
				{
					var end = getIntersection(line[0], line[1], out var t1);
					if (end != invalid && t1 < closestT1)
					{
						closestIntersect = end;
						closestT1 = t1;
					}
				}
			}

			End = closestIntersect;
		}

		public float GetWallPenetrationValue()
		{
			var walls = new List<Wall>();

			var diff = Target - Start;
			var bounds = world.Map.Bounds;
			var positions = new List<MPos>();

			var x0 = (int)Math.Round(Start.X / 1024.0);
			var y0 = (int)Math.Round(Start.Y / 1024.0);

			if (x0 < 0 || y0 < 0 || x0 > world.Map.Bounds.X || y0 > world.Map.Bounds.Y)
				return 1f;

			positions.Add(new MPos(x0, y0));

			var sx = Math.Sign(diff.X);
			var sy = Math.Sign(diff.Y);

			var tMaxX = Math.Abs((x0 * 1024.0 - Start.X + 512 * sx) / diff.X);
			var tMaxY = Math.Abs((y0 * 1024.0 - Start.Y + 512 * sy) / diff.Y);
			var tDeltaX = Math.Abs(1024.0 / diff.X);
			var tDeltaY = Math.Abs(1024.0 / diff.Y);

			bool run = true; ;
			while (run)
			{
				if (tMaxX < tMaxY)
				{
					tMaxX += tDeltaX;
					x0 += sx;
				}
				else
				{
					tMaxY += tDeltaY;
					y0 += sy;
				}

				// Map edges as exit conditions
				if (x0 < 0)
				{
					if (sx < 0) break;
					continue;
				}
				if (x0 >= bounds.X)
				{
					if (sx > 0) break;
					continue;
				}
				if (y0 < 0)
				{
					if (sy < 0) break;
					continue;
				}
				if (y0 >= bounds.Y)
				{
					if (sy > 0) break;
					continue;
				}

				var walls2 = new Wall[2];
				walls2[0] = world.WallLayer.Walls[x0 * 2, y0];
				walls2[1] = world.WallLayer.Walls[x0 * 2 + 1, y0];

				foreach (var wall in walls2)
				{
					if (wall == null)
						continue;

					var lines = wall.Physics.GetLines();
					foreach (var line in lines)
					{
						var end = getIntersection(line.Start, line.End, out var _);
						if (end != invalid && (Start - end).Dist <= (Start - Target).Dist)
						{
							walls.Add(wall);

							// Stop if damage does not get penetrated anyway
							if (wall.Type.DamagePenetration == 0f)
								run = false;
						}
					}
				}
			}

			var output = 1f;
			foreach (var wall in walls)
				output *= wall.Type.DamagePenetration;

			return output;
		}

		MPos[] getPositions(CPos start, CPos target)
		{
			var diff = target - start;
			var bounds = world.Map.Bounds;
			var positions = new List<MPos>();

			var x0 = (int)Math.Round(start.X / 1024.0);
			var y0 = (int)Math.Round(start.Y / 1024.0);

			if (x0 < 0 || y0 < 0 || x0 > world.Map.Bounds.X || y0 > world.Map.Bounds.Y)
				return new MPos[0];

			positions.Add(new MPos(x0, y0));

			var sx = Math.Sign(diff.X);
			var sy = Math.Sign(diff.Y);

			var tMaxX = Math.Abs((x0 * 1024.0 - start.X + 512 * sx) / diff.X);
			var tMaxY = Math.Abs((y0 * 1024.0 - start.Y + 512 * sy) / diff.Y);
			var tDeltaX = Math.Abs(1024.0 / diff.X);
			var tDeltaY = Math.Abs(1024.0 / diff.Y);

			while (true)
			{
				if (tMaxX < tMaxY)
				{
					tMaxX += tDeltaX;
					x0 += sx;
				}
				else
				{
					tMaxY += tDeltaY;
					y0 += sy;
				}

				// Map edges as exit conditions
				if (x0 < 0)
				{
					if (sx < 0) break;
					continue;
				}
				if (x0 >= bounds.X)
				{
					if (sx > 0) break;
					continue;
				}
				if (y0 < 0)
				{
					if (sy < 0) break;
					continue;
				}
				if (y0 >= bounds.Y)
				{
					if (sy > 0) break;
					continue;
				}

				positions.Add(new MPos(x0, y0));
			}

			return positions.ToArray();
		}

		CPos getIntersection(CPos a1, CPos a2, out double T1)
		{
			T1 = double.MaxValue;
			var pos1 = Start;
			var delta1 = Target - Start;

			var pos2 = a1;
			var delta2 = a2 - a1;

			var combined_dist = Math.Sqrt(delta1.X * (double)delta1.X + delta1.Y * (double)delta1.Y) * Math.Sqrt(delta2.X * (double)delta2.X + delta2.Y * (double)delta2.Y);
			var skalar_product = delta1.X * delta2.X + delta1.Y * delta2.Y;
			if (skalar_product / combined_dist == 1)
				return invalid;

			var T2 = (delta1.X * (pos2.Y - pos1.Y) + delta1.Y * (pos1.X - pos2.X)) / (double)(delta2.X * delta1.Y - delta2.Y * delta1.X);
			T1 = (pos2.X + delta2.X * T2 - pos1.X) / (delta1.X == 0 ? 1 : delta1.X);

			// Must be within parametic whatevers for RAY/SEGMENT
			if (T1 < 0 || double.IsNaN(T1))
				return invalid;
			if (T2 < 0 || T2 > 1)
				return invalid;

			// Return the POINT OF INTERSECTION
			return new CPos(pos1.X + (int)(delta1.X * T1), pos1.Y + (int)(delta1.Y * T1), 0);
		}

		CPos getIntersection(CPos center, int radius, out double T1, out double T2, out CPos intersect2)
		{
			T1 = double.MaxValue;
			T2 = double.MaxValue;
			intersect2 = invalid;
			var delta = Target - Start;
			var dist = center - Start;

			double a = (delta.X * delta.X) + (delta.Y * delta.Y);
			double b = 2 * ((delta.X * dist.X) + (delta.Y * dist.Y));
			double c = (dist.X * dist.X) + (dist.Y * dist.Y) - radius * radius;

			var discriminant = b * b - 4 * a * c;

			if (discriminant < 0)
				return invalid;

			// smallest one is the nearest
			T1 = Math.Abs(-b + Math.Sqrt(discriminant)) / (2 * a);
			T2 = Math.Abs(-b - Math.Sqrt(discriminant)) / (2 * a);

			// Return the POINT OF INTERSECTION
			intersect2 = new CPos(Start.X + (int)(delta.X * T2), Start.Y + (int)(delta.Y * T2), 0);
			return new CPos(Start.X + (int)(delta.X * T1), Start.Y + (int)(delta.Y * T1), 0);
		}

		int calculateHeight(CPos pos)
		{
			var diff = (Start - pos).FlatDist/(Start - Target).FlatDist;
			var hDiff = StartHeight - TargetHeight;

			return StartHeight - (int)(diff * hDiff);
		}

		public void RenderDebug()
		{
			foreach (var pos in positions)
				ColorManager.DrawQuad(pos.ToCPos(), 1012, new Color(0, 255, 0, 64));
		}
	}
}
