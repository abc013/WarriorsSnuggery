using System;

namespace WarriorsSnuggery
{
	public sealed class ShroudLayer : IDisposable
	{
		bool[,,] shroudRevealed; // First: Team Second: X Third: Y
		public bool AllRevealed;
		public MPos Size;

		public ShroudLayer()
		{
			shroudRevealed = new bool[0, 0, 0];
			Size = MPos.Zero;
		}

		public void SetMapDimensions(MPos size, int teams, bool allShroudRevealed)
		{
			Dispose();
			Size = size * new MPos(2, 2);
			shroudRevealed = new bool[teams, size.X * 2, size.Y * 2];
			AllRevealed = allShroudRevealed;
		}

		public bool ShroudRevealed(int team, int x, int y)
		{
			return AllRevealed || shroudRevealed[team, x, y];
		}

		public bool ShroudRevealed(int team, MPos position)
		{
			return ShroudRevealed(team, position.X, position.Y);
		}

		public void RevealShroudList(int team, bool[] values)
		{
			if (values == null)
				return;

			for (int i = 0; i < values.Length; i++)
			{
				var x = (int)Math.Floor(i / (float)Size.X);
				var y = i % Size.X;

				shroudRevealed[team, x, y] = values[i];
			}
		}

		public void RevealShroudRectangular(int team, MPos position, int radius)
		{
			if (AllRevealed)
				return;

			for (int x = position.X - radius; x < position.X + radius; x++)
			{
				if (x >= 0 && x < Size.X)
				{
					for (int y = position.Y - radius; y < position.Y + radius; y++)
					{
						if (y >= 0 && y < Size.Y)
						{
							shroudRevealed[team, x, y] = true;
						}
					}
				}
			}

			VisibilitySolver.ShroudUpdated();
			// Camera automatically updates shroud, so we don't want to do that if we move anyways TODO how about other actors?
			if (!Camera.LockedToPlayer)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		public void RevealShroudCircular(int team, MPos position, int radius)
		{
			if (AllRevealed)
				return;

			for (int x = position.X - radius; x < position.X + radius; x++)
			{
				if (x >= 0 && x < Size.X)
				{
					for (int y = position.Y - radius; y < position.Y + radius; y++)
					{
						if (y >= 0 && y < Size.Y)
						{
							shroudRevealed[team, x, y] = shroudRevealed[team, x, y] || Math.Pow(x - position.X, 2) + Math.Pow(y - position.Y, 2) <= radius * radius;
						}
					}
				}
			}

			VisibilitySolver.ShroudUpdated();
			// Camera automatically updates shroud, so we don't want to do that if we move anyways 
			if (!Camera.LockedToPlayer)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		public void Dispose()
		{
			shroudRevealed = new bool[0, 0, 0];
			Size = MPos.Zero;
		}
	}
}
