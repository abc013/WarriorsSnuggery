using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Maps.Layers
{
	public sealed class PhysicsLayer
	{
		public const int SectorSize = 2;
		public PhysicsSector[,] Sectors;
		public MPos Bounds { get; private set; }

		public PhysicsLayer(MPos bounds)
		{
			Bounds = new MPos((int)Math.Ceiling(bounds.X / (float)SectorSize), (int)Math.Ceiling(bounds.Y / (float)SectorSize));
			
			Sectors = new PhysicsSector[Bounds.X, Bounds.Y];
			for (int x = 0; x < Bounds.X; x++)
				for (int y = 0; y < Bounds.Y; y++)
					Sectors[x, y] = new PhysicsSector(new MPos(x, y));
		}

		public void UpdateSectors(SimplePhysics physics, bool @new = false)
		{
			if (physics.IsEmpty)
				return;

			var oldSectors = physics.Sectors;
			physics.Sectors = GetSectors(physics);

			if (!@new)
			{
				foreach (var sector in oldSectors)
				{
					if (!physics.Sectors.Contains(sector))
						sector.Remove(physics);
				}
			}
			foreach (var sector in physics.Sectors)
				sector.Add(physics);
		}

		public PhysicsSector[] GetSectors(SimplePhysics physics)
		{
			if (physics.IsEmpty)
				return new PhysicsSector[0];

			var position = physics.Position - Map.Offset;

			// Add margin to be sure.
			var radiusX = physics.Boundaries.X + 10;
			var radiusY = physics.Boundaries.Y + 10;
			var points = new MPos[4];

			// Corner points

			points[0] = new MPos(position.X + radiusX, position.Y + radiusY); // Sector 1 ( x| y)
			points[1] = new MPos(position.X + radiusX, position.Y - radiusY); // Sector 2 ( x|-y)
			points[2] = new MPos(position.X - radiusX, position.Y - radiusY); // Sector 3 (-x|-y)
			points[3] = new MPos(position.X - radiusX, position.Y + radiusY); // Sector 4 (-x| y)

			// Corner sectors

			var sectorPositions = new MPos[4];
			for (int i = 0; i < 4; i++)
			{
				var point = points[i];

				var x = Math.Clamp(point.X / (SectorSize * Constants.TileSize), 0, Bounds.X - 1);
				var y = Math.Clamp(point.Y / (SectorSize * Constants.TileSize), 0, Bounds.Y - 1);

				sectorPositions[i] = new MPos(x, y);
			}

			// Determine Size of the Sector field to enter and the sector with the smallest value (sector 3)
			var startPosition = sectorPositions[2];
			// Difference plus one to have the field (e.g. 1 and 2 -> diff. 1 + 1 = 2 fields)
			var xSize = (sectorPositions[1].X - sectorPositions[2].X) + 1;
			var ySize = (sectorPositions[3].Y - sectorPositions[2].Y) + 1;

			var sectors = new List<PhysicsSector>();
			for (int x = 0; x < xSize; x++)
			{
				for (int y = 0; y < ySize; y++)
				{
					var sector = Sectors[startPosition.X + x, startPosition.Y + y];
					if (!sectors.Contains(sector))
						sectors.Add(sector);
				}
			}

			return sectors.ToArray();
		}
	}

	public class PhysicsSector
	{
		public readonly MPos Position;

		readonly List<SimplePhysics> physicsList = new List<SimplePhysics>();

		public PhysicsSector(MPos position)
		{
			Position = position;
		}

		public void Add(SimplePhysics physics)
		{
			if (!physicsList.Contains(physics))
				physicsList.Add(physics);
		}

		public void Remove(SimplePhysics physics)
		{
			physicsList.Remove(physics);
		}

		public bool CheckIntersection(SimplePhysics physics, out Collision collision)
		{
			collision = null;
			foreach (var other in physicsList)
			{
				if (Collision.CheckCollision(other, physics, out collision))
					return true;
			}

			return false;
		}

		public IEnumerable<SimplePhysics> GetObjects(SimplePhysics[] ignore)
		{
			return physicsList.Where((o) => ignore == null || !ignore.Contains(o));
		}

		public void RenderDebug()
		{
			var pos = (Position * PhysicsLayer.SectorSize).ToCPos() - new CPos(512, 512, 0);
			ColorManager.DrawLine(pos, pos + new CPos(0, 512, 0), Color.White.WithAlpha(0.3f));
			ColorManager.DrawLine(pos, pos + new CPos(512, 0, 0), Color.White.WithAlpha(0.3f));
		}
	}
}
