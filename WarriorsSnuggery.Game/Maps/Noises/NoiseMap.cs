using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Maps.Noises
{
	[Desc("This object is designed to give information about noise that will be used by various generators.")]
	public sealed class NoiseMapInfo
	{
		[Desc("Unique ID that belongs to this noise.")]
		public readonly int ID;

		[Desc("Type of noise to use.", "Available are: CLOUDS, MAZE, NOISE, NONE")]
		public readonly NoiseType NoiseType = NoiseType.NONE;
		[Desc("Strength of the noise [CLOUDS].", "Percentage of generating additional pathways from 0 to 100 [MAZE].")]
		public readonly int Strength = 4;
		[Desc("Scale of the noise [NOISE, CLOUDS].", "Scale of the maze pathways [MAZE].")]
		public readonly float Scale = 1f;

		[Desc("Intensity parameter.")]
		public readonly float Intensity = 0f;
		[Desc("Contrast parameter.")]
		public readonly float Contrast = 1f;

		public NoiseMapInfo(int id, List<TextNode> nodes)
		{
			ID = id;
			TypeLoader.SetValues(this, nodes);
		}
	}

	public sealed class NoiseMap
	{
		public static NoiseMap Empty = new NoiseMap();

		public readonly int ID;
		readonly float[] values;
		readonly MPos bounds;
		bool empty;

		NoiseMap()
		{
			empty = true;
		}

		public NoiseMap(MPos bounds, int seed, NoiseMapInfo info)
		{
			this.bounds = bounds;
			ID = info.ID;

			var random = new Random(seed + info.ID);

			switch (info.NoiseType)
			{
				case NoiseType.CLOUDS:
					values = Noise.GenerateClouds(bounds, random, info.Strength, info.Scale);
					break;
				case NoiseType.NOISE:
					values = Noise.GenerateNoise(bounds, random, info.Scale);
					break;
				case NoiseType.MAZE:
					// For walls, multiply with the following value
					/* * new MPos(2, 2) + new MPos(1, 1) */
					var scale = 1 / info.Scale;

					if (scale == 1f)
					{
						values = MazeNoise.GenerateMaze(bounds, random, info.Strength);
						break;
					}

					var scaledBounds = new MPos((int)Math.Ceiling(bounds.X * scale), (int)Math.Ceiling(bounds.Y * scale));

					var rawValues = MazeNoise.GenerateMaze(scaledBounds, random, info.Strength);

					values = new float[bounds.X * bounds.Y];
					for (var x = 0; x < bounds.X; x++)
					{
						for (var y = 0; y < bounds.Y; y++)
						{
							var scaledX = (int)((x / (float)bounds.X) * scaledBounds.X);
							var scaledY = (int)((y / (float)bounds.Y) * scaledBounds.Y);

							values[x * bounds.Y + y] = rawValues[scaledX * scaledBounds.Y + scaledY];
						}
					}

					break;
				case NoiseType.NONE:
					values = new float[bounds.X * bounds.Y];

					if (info.Strength > 0)
						Array.Fill(values, 1);

					break;
			}

			for (int i = 0; i < values.Length; i++)
			{
				// Intensity and contrast
				var value = values[i] + info.Intensity;
				value = (value - 0.5f) * info.Contrast + 0.5f;

				// Fit to area of 0 to 1.
				values[i] = Math.Clamp(value, 0f, 1f);
			}
		}

		public float this[int x, int y] => empty ? 0f : values[y * bounds.X + x];

		public void Render()
		{
			if (empty)
				return;

			var bounds = VisibilitySolver.GetBounds(out var position);

			position = new MPos(Math.Clamp(position.X, 0, this.bounds.X), Math.Clamp(position.Y, 0, this.bounds.Y));
			bounds = new MPos(Math.Clamp(position.X + bounds.X, 0, this.bounds.X), Math.Clamp(position.Y + bounds.Y, 0, this.bounds.Y));

			for (var x = position.X; x < bounds.X; x++)
			{
				for (var y = position.Y; y < bounds.Y; y++)
				{
					var value = values[y * this.bounds.X + x];
					ColorManager.DrawQuad(new CPos(x * 1024, y * 1024, 0), 256, new Color(value, value, value, 0.8f));
				}
			}
		}
	}
}
