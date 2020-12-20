using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public sealed class ShroudLayer
	{
		public bool RevealAll;
		public MPos Bounds { get; private set; }

		readonly bool[,,] shroudRevealed; // First: Team Second: X Third: Y
		readonly byte[,] shroudAlpha;

		public ShroudLayer(MPos bounds)
		{
			Bounds = bounds * new MPos(2, 2);

			shroudRevealed = new bool[Settings.MaxTeams, bounds.X * 2, bounds.Y * 2];
			shroudAlpha = new byte[bounds.X * 2, bounds.Y * 2];
		}

		public bool ShroudRevealed(int team, int x, int y)
		{
			return RevealAll || shroudRevealed[team, x, y];
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
				var x = (int)Math.Floor(i / (float)Bounds.X);
				var y = i % Bounds.X;

				shroudRevealed[team, x, y] = values[i];
			}
		}

		public void RevealShroudRectangular(int team, MPos position, int radius, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			for (int x = position.X - radius; x < position.X + radius; x++)
			{
				if (x >= 0 && x < Bounds.X)
				{
					for (int y = position.Y - radius; y < position.Y + radius; y++)
					{
						if (y >= 0 && y < Bounds.Y)
							shroudRevealed[team, x, y] = true;
					}
				}
			}

			if (team == Objects.Actor.PlayerTeam)
				VisibilitySolver.ShroudUpdated();
			// Camera automatically updates shroud, so we don't want to do that if we move anyways TODO how about other actors?
			if (!Camera.LockedToPlayer || ignoreLock)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		public void RevealShroudCircular(int team, MPos position, int radius, bool ignoreLock = false)
		{
			if (RevealAll)
				return;

			var radiusSquared = radius * radius;

			for (int x = position.X - radius; x < position.X + radius; x++)
			{
				if (x >= 0 && x < Bounds.X)
				{
					var dx = x - position.X;
					dx *= dx;

					for (int y = position.Y - radius; y < position.Y + radius; y++)
					{
						if (y >= 0 && y < Bounds.Y)
						{
							var dy = y - position.Y;
							dy *= dy;

							shroudRevealed[team, x, y] = shroudRevealed[team, x, y] || dx + dy <= radiusSquared;
						}
					}
				}
			}

			if (team == Objects.Actor.PlayerTeam)
				VisibilitySolver.ShroudUpdated();
			// Camera automatically updates shroud, so we don't want to do that if we move anyways 
			if (!Camera.LockedToPlayer || ignoreLock)
				WorldRenderer.CheckVisibility(Camera.LookAt, Camera.DefaultZoom);
		}

		public float ShroudAlpha(MPos position, int team)
		{
			var alpha = shroudAlpha[position.X, position.Y];
			if (alpha < 4 && shroudRevealed[team, position.X, position.Y])
				shroudAlpha[position.X, position.Y]++;

			return 1 - alpha / 4f;
		}

		public string ToString(int team)
		{
			var shroud = team + "=";

			for (int x = 0; x < Bounds.X; x++)
				for (int y = 0; y < Bounds.Y; y++)
					shroud += ShroudRevealed(team, x, y).GetHashCode() + ",";

			shroud = shroud.TrimEnd(',');

			return shroud;
		}

		public void Clear()
		{
			Bounds = MPos.Zero;
		}
	}
}
