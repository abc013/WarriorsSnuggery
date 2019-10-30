using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class PhysicsLayer
	{
		public const float SectorSize = 2;
		public PhysicsSector[,] Sectors;
		public MPos Size;

		public PhysicsLayer()
		{
			Sectors = new PhysicsSector[0, 0];
		}

		public void SetMapDimensions(MPos size)
		{
			Size = new MPos((int)Math.Ceiling(size.X / SectorSize), (int)Math.Ceiling(size.Y / SectorSize));
			Sectors = new PhysicsSector[Size.X, Size.Y];
			for (int x = 0; x < Size.X; x++)
			{
				for (int y = 0; y < Size.Y; y++)
				{
					Sectors[x, y] = new PhysicsSector(new MPos(x, y));
				}
			}
		}

		public void UpdateSectors(PhysicsObject obj, bool @new = false)
		{
			if (obj.Physics == null || obj.Physics.Shape == Physics.Shape.NONE)
				return;

			if (!@new)
			{
				foreach (var sector in obj.PhysicsSectors)
				{
					sector.Leave(obj);
				}
			}

			var position = obj.Position;
			// Add margin to be sure.
			var radiusX = obj.Physics.RadiusX + 10;
			var radiusY = obj.Physics.RadiusY + 10;
			var shape = obj.Physics.Shape;
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

				var x = point.X / (SectorSize * 1024);
				if (x < 0) x = 0;
				if (x >= Size.X) x = Size.X - 1;

				var y = point.Y / (SectorSize * 1024);
				if (y < 0) y = 0;
				if (y >= Size.Y) y = Size.Y - 1;

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

					sector.Enter(obj);
				}
			}

			obj.PhysicsSectors = sectors.ToArray();
		}
	}

	public class PhysicsSector
	{
		public readonly MPos Position;

		readonly List<PhysicsObject> Objects = new List<PhysicsObject>();

		public PhysicsSector(MPos position)
		{
			Position = position;
		}

		public void Enter(PhysicsObject obj)
		{
			if (!Objects.Contains(obj))
				Objects.Add(obj);
		}

		public void Leave(PhysicsObject obj)
		{
			Objects.Remove(obj);
		}

		public bool Check(PhysicsObject obj, bool ignoreHeight = false, Type[] ignoreTypes = null, PhysicsObject[] ignoreObjects = null)
		{
			if (!Objects.Contains(obj))
				return true;

			return Objects.Any((o) => o.Physics != obj.Physics && o.Physics.Intersects(obj.Physics, ignoreHeight) && (ignoreObjects == null || !ignoreObjects.Contains(o)) && (ignoreTypes == null || !ignoreTypes.Contains(o.GetType())));
		}

		public PhysicsObject[] CheckRay(Physics.RayPhysics physics, Type[] ignoreTypes = null, PhysicsObject[] ignoreObjects = null)
		{
			return Objects.Where((o) => (ignoreObjects == null || !ignoreObjects.Contains(o)) && (ignoreTypes == null || !ignoreTypes.Contains(o.GetType()))).ToArray();
		}
	}
}
