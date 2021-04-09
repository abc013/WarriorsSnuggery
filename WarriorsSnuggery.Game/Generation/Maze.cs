using System;

namespace WarriorsSnuggery
{
	class MazeField
	{
		public readonly MPos Position;
		public MazeField Parent;

		public byte Directions = 15;
		public bool IsWall = false;

		public MazeField(MPos position)
		{
			Position = position;
		}
	}

	public static class Maze
	{
		public static float[] GenerateMaze(MPos size, Random random, int additionalPathwayProbability = 0)
		{
			var start = new MPos(1, 1);

			var fields = new MazeField[size.X, size.Y];

			for (int x = 0; x < size.X; x++)
			{
				for (int y = 0; y < size.Y; y++)
				{
					var field = new MazeField(new MPos(x, y))
					{
						IsWall = (x * y) % 2 == 0
					};

					fields[x, y] = field;
				}
			}

			var startField = fields[start.X, start.Y];
			startField.Parent = startField;

			var last = startField;
			while ((last = link(fields, random, last, size)) != startField) { }

			var maze = new float[size.X * size.Y];

			for (int x = 0; x < size.X; x++)
			{
				for (int y = 0; y < size.Y; y++)
				{
					var isWall = fields[x, y].IsWall;

					if (isWall && (x % 2 != 0 || y % 2 != 0))
						isWall &= random.Next(100) >= additionalPathwayProbability;

					maze[x * size.Y + y] = isWall.GetHashCode();
				}
			}

			return maze;
		}

		static MazeField link(MazeField[,] fields, Random random, MazeField last, MPos size)
		{
			var pos = MPos.Zero;
			while (last.Directions != 0)
			{
				var dir = (byte)(1 << random.Next(4));

				if ((last.Directions & dir) == 0)
					continue;

				last.Directions &= (byte)(~dir & 0x000000FF);

				switch (dir)
				{
					case 1:
						if (last.Position.X + 2 < size.X)
							pos = new MPos(last.Position.X + 2, last.Position.Y);
						else
							continue;
						break;
					case 2:
						if (last.Position.Y + 2 < size.Y)
							pos = new MPos(last.Position.X, last.Position.Y + 2);
						else
							continue;
						break;
					case 4:
						if (last.Position.X - 2 >= 0)
							pos = new MPos(last.Position.X - 2, last.Position.Y);
						else
							continue;
						break;
					case 8:
						if (last.Position.Y - 2 >= 0)
							pos = new MPos(last.Position.X, last.Position.Y - 2);
						else
							continue;
						break;
				}
				var dest = fields[pos.X, pos.Y];
				if (!dest.IsWall)
				{
					if (dest.Parent != null)
						continue;

					dest.Parent = last;
					var diff = dest.Position - last.Position;
					fields[last.Position.X + diff.X / 2, last.Position.Y + diff.Y / 2].IsWall = false;

					return dest;
				}
			}
			return last.Parent;
		}
	}
}
