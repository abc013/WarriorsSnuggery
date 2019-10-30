using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Physics
{
	public class RayPhysics
	{
		static readonly CPos invalid = new CPos(int.MaxValue, int.MaxValue, 0);

		public CPos Start;
		public CPos End { get; private set; }
		public CPos Target;

		readonly CPos[][] mapBounds;
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

		public void CalculateEnd(Actor shooter = null)
		{
			var closestIntersect = new CPos(0, 0, int.MaxValue);
			var closestT1 = double.MaxValue;

			// Collision at map Bounds.
			foreach(var line in mapBounds)
			{
				var end = getIntersection(line[0], line[1], out var t1);
				if (end != invalid && t1 < closestT1)
				{
					closestIntersect = end;
					closestT1 = t1;
				}
			}

			var layer = world.PhysicsLayer;

			// Calculate Sectors that the line intersects with
			var sector1 = new MPos((int)Math.Floor(Start.X / (PhysicsLayer.SectorSize * 1024)), (int)Math.Floor(Start.Y / (PhysicsLayer.SectorSize * 1024)));
			var sector2 = new MPos((int)Math.Floor(End.X / (PhysicsLayer.SectorSize * 1024)), (int)Math.Floor(End.Y / (PhysicsLayer.SectorSize * 1024)));

			MPos sectorMax;
			MPos sectorMin;
			if (sector1.X < sector2.X)
			{
				sectorMin = new MPos(sector1.X, 0);
				sectorMax = new MPos(sector2.X, 0);
			}
			else
			{
				sectorMin = new MPos(sector2.X, 0);
				sectorMax = new MPos(sector1.X, 0);
			}

			if (sector1.Y < sector2.Y)
			{
				sectorMin = new MPos(sectorMin.X, sector1.Y);
				sectorMax = new MPos(sectorMax.X, sector2.Y);
			}
			else
			{
				sectorMin = new MPos(sectorMin.X, sector2.Y);
				sectorMax = new MPos(sectorMax.X, sector1.Y);
			}

			// Collision at actors.
			foreach (var sector in layer.Sectors)
			{
				if (sector.Position.X - sectorMin.X < 0 || sector.Position.Y - sectorMin.Y < 0)
					continue;

				if (sector.Position.X - sectorMax.X > 0 || sector.Position.Y - sectorMax.Y > 0)
					continue;

				var objs = sector.CheckRay(this, new[] { typeof(Weapon), typeof(BeamWeapon), typeof(BulletWeapon), typeof(RocketWeapon) }, shooter == null ? null : new[] { shooter });
				foreach(var obj in objs)
				{
					foreach(var line in obj.Physics.GetLines())
					{
						var end = getIntersection(line.Start, line.End, out var t1);
						if (end != invalid && t1 < closestT1)
						{
							closestIntersect = end;
							closestT1 = t1;
						}
					}
				}
			}
			foreach(var wall in world.WallLayer.Walls)
			{
				if (wall == null)
					continue;

				var lines = wall.Physics.GetLines();
				foreach (var line in lines)
				{
					var end = getIntersection(line.Start, line.End, out var t1);
					if (end != invalid && t1 < closestT1)
					{
						closestIntersect = end;
						closestT1 = t1;
					}
				}
			}

			End = closestIntersect;
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

		public void RenderDebug()
		{
			ColorManager.LineWidth = 3f;
			ColorManager.DrawLine(Start, End, Color.Yellow);
			ColorManager.ResetLineWidth();
		}
	}
}
