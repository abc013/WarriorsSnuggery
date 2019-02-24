using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class PhysicsLayer
	{
		public PhysicsSector[,] Sectors;
		public MPos Size;

		public PhysicsLayer()
		{
			Sectors = new PhysicsSector[0,0];
		}

		public void SetMapSize(MPos size)
		{
			Size = new MPos((int) Math.Ceiling(size.X / 2f), (int) Math.Ceiling(size.Y / 2f));
			Sectors = new PhysicsSector[Size.X,Size.Y];
			for(int x = 0; x < Size.X; x++)
			{
				for(int y = 0; y < Size.Y; y++)
				{
					Sectors[x,y] = new PhysicsSector(new MPos(x,y));
				}
			}
		}

		public void UpdateSectors(GameObject obj, bool @new = false)
		{
			if (obj.Physics == null || obj.Physics.Shape == Shape.NONE)
				return;

			var position = obj.Position;
			var radius = obj.Physics.Radius;
			var shape = obj.Physics.Shape;
			var points = new MPos[4];

			points[0] = new MPos(position.X + radius, position.Y + radius);
			points[1] = new MPos(position.X - radius, position.Y - radius);
			points[2] = new MPos(position.X - radius, position.Y + radius);
			points[3] = new MPos(position.X + radius, position.Y - radius);

			var sectors = new List<PhysicsSector>();

			if (!@new)
			{
				foreach(var sector in obj.PhysicsSectors)
				{
					sector.Leave(obj);
				}
			}

			foreach(var point in points)
			{
				var x = point.X / 2048f;
				if (x < 0) x = 0;
				if (x >= Size.X) x = Size.X - 1;
				var y = point.Y / 2048f;
				if (y < 0) y = 0;
				if (y >= Size.Y) y = Size.Y - 1;

				var sector = Sectors[(int) Math.Floor(x), (int) Math.Floor(y)];
				if (!sectors.Contains(sector))
					sectors.Add(sector);
				sector.Enter(obj);
			}

			obj.PhysicsSectors = sectors.ToArray();
		}
	}

	public class PhysicsSector
	{
		public readonly MPos Position;

		readonly List<GameObject> Objects = new List<GameObject>();

		public PhysicsSector(MPos position)
		{
			Position = position;
		}

		public void Enter(GameObject obj)
		{
			if (!Objects.Contains(obj))
				Objects.Add(obj);
		}

		public void Leave(GameObject obj)
		{
			Objects.Remove(obj);
		}

		public bool Check(GameObject obj, Type[] ignoreTypes = null, GameObject[] ignoreObjects = null)
		{
			if(!Objects.Contains(obj))
				return true;

			return Objects.Any((o) => o.Physics != obj.Physics && o.Physics.Intersects(obj.Physics) && (ignoreObjects == null || !ignoreObjects.Contains(o)) && (ignoreTypes == null || !ignoreTypes.Contains(o.GetType())) );
		}
	}
}
