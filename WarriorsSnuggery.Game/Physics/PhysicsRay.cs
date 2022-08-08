using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public class PhysicsRay
	{
		static readonly CPos invalid = new CPos(int.MaxValue, int.MaxValue, 0);

		public CPos Start;
		public CPos End { get; private set; }
		public CPos Target;

		readonly CPos[][] mapBounds;
		readonly List<MPos> positions = new List<MPos>();
		readonly World world;

		public PhysicsRay(World world)
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

		public void CalculateEnd(SimplePhysics[] ignore = null, bool ignoreActors = false, int maxSteps = int.MaxValue, bool onlyToTarget = false)
		{
			CPos noIntersect = new CPos(0, 0, int.MaxValue);

			positions.Clear();

			var diff = Target - Start;

			if (diff.Flat() == CPos.Zero)
			{
				End = Start;
				return;
			}

			var stepLimit = maxSteps;
			if (onlyToTarget)
				stepLimit = (int)Math.Ceiling((Math.Abs(diff.X) + Math.Abs(diff.Y)) / 1024f) + 2; // MaxSteps are calculated with the Manhattan distance and a margin of 2

			var bounds = world.Map.Bounds;

			var sx = Math.Sign(diff.X);
			var sy = Math.Sign(diff.Y);

			var closestIntersect = noIntersect;
			var closestT1 = double.MaxValue;

			bool checkHeight(SimplePhysics physics, CPos end, double t1)
			{
				var height = calculateHeight(end);

				// Smart as we are, we use Z Boundaries to add them onto the height.
				if (height <= physics.Height + physics.Boundaries.Z * 2 && height >= physics.Height)
				{
					// HACK for damage: Don't hit the wall directly
					closestIntersect = end.Flat() + new CPos(0, 0, height) - new CPos(sx * 4, sy * 4, 0);
					closestT1 = t1;
					return true;
				}

				return true;
			}

			bool checkLinesIntersect(SimplePhysics physics)
			{
				bool hit = false;
				foreach (var line in physics.GetLines())
				{
					var end = getIntersection(line.start, line.end, out var t1);
					if (end != invalid && t1 < closestT1)
						hit |= checkHeight(physics, end, t1);
				}

				return hit;
			}

			var x0 = (int)Math.Round(Start.X / 1024.0);
			var y0 = (int)Math.Round(Start.Y / 1024.0);
			MPos currentSector = new MPos(x0 / PhysicsLayer.SectorSize, y0 / PhysicsLayer.SectorSize);

			if (x0 >= 0 && y0 >= 0 && x0 < bounds.X && y0 < bounds.Y)
			{
				var tMaxX = Math.Abs((x0 * 1024.0 - Start.X + 512 * sx) / diff.X);
				var tMaxY = Math.Abs((y0 * 1024.0 - Start.Y + 512 * sy) / diff.Y);
				var tDeltaX = Math.Abs(1024.0 / diff.X);
				var tDeltaY = Math.Abs(1024.0 / diff.Y);

				while (stepLimit-- > 0)
				{
					positions.Add(new MPos(x0, y0));

					var hit = false;

					var wall1 = world.WallLayer.Walls[x0 * 2, y0];
					if (wall1 != null)
						hit |= checkLinesIntersect(wall1.Physics);

					var wall2 = world.WallLayer.Walls[x0 * 2 + 1, y0];
					if (wall2 != null)
						hit |= checkLinesIntersect(wall2.Physics);

					// Check sectors with actors (even if wall was hit, because actor could be in front of it)
					var newSector = new MPos(x0 / PhysicsLayer.SectorSize, y0 / PhysicsLayer.SectorSize);
					if (!ignoreActors && (hit || currentSector != newSector))
					{
						if (!currentSector.InRange(MPos.Zero, new MPos(bounds.X / PhysicsLayer.SectorSize, bounds.Y / PhysicsLayer.SectorSize)))
							continue;

						var sector = world.PhysicsLayer.Sectors[currentSector.X, currentSector.Y];

						var actorHit = false;
						foreach (var physics in sector.GetObjects(ignore))
						{
							if (physics.Shape != Shape.CIRCLE)
								actorHit |= checkLinesIntersect(physics);
							else
							{
								var end = getIntersection(physics.Position, physics.Boundaries.X, out var t1, out var t2, out var end2);
								if (end != invalid && t1 < closestT1)
								{
									var firstCheck = checkHeight(physics, end, t1);
									actorHit |= firstCheck;

									if (!firstCheck && t2 < closestT1)
										actorHit |= checkHeight(physics, end2, t2);
								}
							}
						}

						// We hit something, we won't need to check anything behind
						if (actorHit) break;

						currentSector = newSector;
					}

					// We hit something, therefore we can ignore the rest which is further away
					if (hit) break;

					if (double.IsInfinity(tDeltaX))
						y0 += sy;
					else if (double.IsInfinity(tDeltaY))
						x0 += sx;
					else if (tMaxX < tMaxY)
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
				}
			}

			// Collision at map bounds, if nothing was hit
			if (closestIntersect == noIntersect)
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
			var diff = Target - Start;

			if (diff.Flat() == CPos.Zero)
				return 1f;

			var walls = new List<Wall>();
			var bounds = world.Map.Bounds;

			var x0 = (int)Math.Round(Start.X / 1024.0);
			var y0 = (int)Math.Round(Start.Y / 1024.0);

			if (x0 < 0 || y0 < 0 || x0 > world.Map.Bounds.X || y0 > world.Map.Bounds.Y)
				return 1f;

			var sx = Math.Sign(diff.X);
			var sy = Math.Sign(diff.Y);

			var tMaxX = Math.Abs((x0 * 1024.0 - Start.X + 512 * sx) / diff.X);
			var tMaxY = Math.Abs((y0 * 1024.0 - Start.Y + 512 * sy) / diff.Y);
			var tDeltaX = Math.Abs(1024.0 / diff.X);
			var tDeltaY = Math.Abs(1024.0 / diff.Y);

			bool run = true;
			while (run)
			{
				var walls2 = new Wall[2];
				walls2[0] = world.WallLayer.Walls[x0 * 2, y0];
				walls2[1] = world.WallLayer.Walls[x0 * 2 + 1, y0];

				foreach (var wall in walls2)
				{
					if (!run)
						break;

					if (wall == null)
						continue;

					foreach (var line in wall.Physics.GetLines())
					{
						var end = getIntersection(line.start, line.end, out var _);
						if (end != invalid && (Start - end).SquaredFlatDist <= (Start - Target).SquaredFlatDist)
						{
							walls.Add(wall);

							// Stop if damage does not get penetrated anyway
							if (wall.Type.DamagePenetration == 0f)
							{
								run = false;
								break;
							}
						}
					}
				}

				if (double.IsInfinity(tDeltaX))
					y0 += sy;
				else if (double.IsInfinity(tDeltaY))
					x0 += sx;
				else if (tMaxX < tMaxY)
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
			}

			var output = 1f;
			foreach (var wall in walls)
				output *= wall.Type.DamagePenetration;

			return output;
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

			// check signs and whether the vectors are opposing themselves
			if ((delta.X ^ dist.X) < 0 && (delta.Y ^ dist.Y) < 0 && dist.FlatDist - radius > 0)
				return invalid;

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
			var diff = (Start - pos).SquaredFlatDist / (double)(Start - Target).SquaredFlatDist;
			var hDiff = Start.Z - Target.Z;

			return Start.Z - (int)(diff * hDiff);
		}

		public void RenderDebug()
		{
			foreach (var pos in positions)
				ColorManager.DrawQuad(pos.ToCPos(), 1012, new Color(0, 255, 0, 64));
		}
	}
}
