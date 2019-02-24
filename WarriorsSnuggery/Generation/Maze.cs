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
		public static bool[,] GenerateMaze(MPos size, Random random, MPos start, int multiplePossibilities = 0)
		{
			var fields = new MazeField[size.X, size.Y];
			var maze = new bool[size.X, size.Y];

			for (int x = 0; x < size.X; x++)
			{
				for (int y = 0; y < size.Y; y++)
				{
					var field = new MazeField(new MPos(x,y));
					field.IsWall = (x * y) % 2 == 0;

					fields[x,y] = field;
					maze[x,y] = field.IsWall;
				}
			}

			var startField = fields[start.X, start.Y];
			startField.Parent = startField;

			MazeField last = startField;
			while ((last = Link(fields, random, last, size)) != startField) { }

			for (int x = 0; x < size.X; x++)
			{
				for (int y = 0; y < size.Y; y++)
				{
					maze[x, y] = fields[x, y].IsWall && (multiplePossibilities == 0 || random.Next(multiplePossibilities) != 0);
				}
			}
			return maze;
		}

		static MazeField Link(MazeField[,] fields, Random random, MazeField last, MPos size)
		{
			var dir = (byte) 0;
			var pos = MPos.Zero;
			while (last.Directions != 0)
			{
				dir = (byte) (1 << random.Next(4));

				if ((last.Directions & dir) == 0)
					continue;

				last.Directions &= (byte) (~dir & 0x000000FF);

				switch(dir)
				{
					case 1:
						if (last.Position.X + 2 < size.X)
						{
							pos = new MPos(last.Position.X + 2, last.Position.Y);
						}
						else
							continue;
						break;
					case 2:
						if (last.Position.Y + 2 < size.Y)
						{
							pos = new MPos(last.Position.X, last.Position.Y + 2);
						}
						else
							continue;
						break;
					case 4:
						if (last.Position.X - 2 >= 0)
						{
							pos = new MPos(last.Position.X - 2, last.Position.Y);
						}
						else
							continue;
						break;
					case 8:
						if (last.Position.Y - 2 >= 0)
						{
							pos = new MPos(last.Position.X, last.Position.Y - 2);
						}
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
					fields[last.Position.X + (dest.Position.X - last.Position.X) / 2, last.Position.Y + (dest.Position.Y - last.Position.Y) / 2].IsWall = false;

					return dest;
				}
			}
			return last.Parent;
		}
	}
}
