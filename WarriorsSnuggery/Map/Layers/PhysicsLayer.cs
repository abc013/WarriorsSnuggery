using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery
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

		public void UpdateSectors(PhysicsObject obj, bool @new = false, bool updateSectors = true)
		{
			if (obj.Physics.Shape == Shape.NONE)
				return;

			var oldSectors = obj.PhysicsSectors;
			obj.PhysicsSectors = GetSectors(obj.Physics);

			if (updateSectors)
			{
				if (!@new)
				{
					foreach (var sector in oldSectors)
					{
						if (!obj.PhysicsSectors.Contains(sector))
							sector.Leave(obj);
					}
				}
				foreach (var sector in obj.PhysicsSectors)
					sector.Enter(obj);
			}
		}

		public PhysicsSector[] GetSectors(SimplePhysics physics)
		{
			if (physics == null || physics.Shape == Shape.NONE)
				return new PhysicsSector[0];

			var position = physics.Position - Map.Offset;
			// Add margin to be sure.
			var radiusX = physics.RadiusX + 10;
			var radiusY = physics.RadiusY + 10;
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

				var x = point.X / (SectorSize * 1024f);
				if (x < 0) x = 0;
				if (x >= Bounds.X) x = Bounds.X - 1;

				var y = point.Y / (SectorSize * 1024f);
				if (y < 0) y = 0;
				if (y >= Bounds.Y) y = Bounds.Y - 1;

				sectorPositions[i] = new MPos((int)Math.Floor(x), (int)Math.Floor(y));
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

		readonly List<PhysicsObject> objects = new List<PhysicsObject>();

		public PhysicsSector(MPos position)
		{
			Position = position;
		}

		public void Enter(PhysicsObject obj)
		{
			if (!objects.Contains(obj))
				objects.Add(obj);
		}

		public void Leave(PhysicsObject obj)
		{
			objects.Remove(obj);
		}

		public bool Check(PhysicsObject obj, Actor toIgnore)
		{
			return objects.Any((o) => o.Physics != obj.Physics && o != toIgnore && o.Physics.Intersects(obj.Physics));
		}

		public IEnumerable<PhysicsObject> GetObjects()
		{
			return objects;
		}

		public IEnumerable<PhysicsObject> GetObjects(PhysicsObject[] ignoreObjects)
		{
			return objects.Where((o) => ignoreObjects == null || !ignoreObjects.Contains(o));
		}

		public void RenderDebug()
		{
			var pos = (Position * new MPos(2, 2)).ToCPos() + new CPos(512, 512, 0);
			ColorManager.DrawLine(pos - new CPos(1000, 1000, 0), pos + new CPos(-1000, 1000, 0), new Color(0, 0, 1f, 0.2f));
			ColorManager.DrawLine(pos - new CPos(1000, 1000, 0), pos + new CPos(1000, -1000, 0), new Color(0, 0, 1f, 0.2f));
			ColorManager.DrawLine(pos - new CPos(-1000, 1000, 0), pos + new CPos(1000, 1000, 0), new Color(0, 0, 1f, 0.2f));
			ColorManager.DrawLine(pos - new CPos(1000, -1000, 0), pos + new CPos(1000, 1000, 0), new Color(0, 0, 1f, 0.2f));
			foreach (var obj in objects)
				obj.Physics.RenderDebug();
		}
	}
}
