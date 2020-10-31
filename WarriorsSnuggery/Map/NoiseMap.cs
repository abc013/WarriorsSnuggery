﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Maps
{
	[Desc("Trait designed to give information about noise that will be used by various generators.")]
	public sealed class NoiseMapInfo
	{
		[Desc("Unique ID that belongs to this noise.")]
		public readonly int ID;

		[Desc("Type of noise to use.", "Available are: CLOUDS, MAZE, NOISE, NONE")]
		public readonly NoiseType NoiseType = NoiseType.NONE;
		[Desc("Strength of the noise [CLOUDS].", "count of additional pathways [MAZE].")]
		public readonly int Strength = 4;
		[Desc("Scale of the noise [NOISE, CLOUDS].")]
		public readonly float Scale = 1f;

		[Desc("Intensity parameter.")]
		public readonly float Intensity = 0f;
		[Desc("Contrast parameter.")]
		public readonly float Contrast = 1f;

		public NoiseMapInfo(int id, List<MiniTextNode> nodes)
		{
			ID = id;
			Loader.PartLoader.SetValues(this, nodes);
		}
	}

	public sealed class NoiseMap
	{
		public readonly int ID;
		public readonly float[] Values;
		public readonly MPos Bounds;

		public NoiseMap(MPos bounds, int seed, NoiseMapInfo info)
		{
			Bounds = bounds;
			ID = info.ID;
			var random = new Random(seed + info.ID);

			switch (info.NoiseType)
			{
				case NoiseType.CLOUDS:
					Values = Noise.GenerateClouds(bounds, random, info.Strength, info.Scale);
					break;
				case NoiseType.NOISE:
					Values = Noise.GenerateNoise(bounds, random, info.Scale);
					break;
				case NoiseType.MAZE:
					Values = Maze.GenerateMaze(bounds * new MPos(2, 2) + new MPos(1, 1), random, new MPos(1, 1), info.Strength);
					break;
				case NoiseType.NONE:
					Values = new float[bounds.X * bounds.Y];

					if (info.Strength > 0)
						Array.Fill(Values, 1);

					break;
			}

			for (int i = 0; i < Values.Length; i++)
			{
				// Intensity and contrast
				var value = Values[i] + info.Intensity;
				value = (value - 0.5f) * info.Contrast + 0.5f;

				// Fit to area of 0 to 1.
				Values[i] = Math.Clamp(value, 0f, 1f);
			}
		}

		public void Render()
		{
			var bounds = VisibilitySolver.GetBounds(out var position);

			position = new MPos(Math.Clamp(position.X, 0, Bounds.X), Math.Clamp(position.Y, 0, Bounds.Y));
			bounds = new MPos(Math.Clamp(position.X + bounds.X, 0, Bounds.X), Math.Clamp(position.Y + bounds.Y, 0, Bounds.Y));

			for (var x = position.X; x < bounds.X; x++)
			{
				for (var y = position.Y; y < bounds.Y; y++)
				{
					var value = Values[y * Bounds.X + x];
					ColorManager.DrawQuad(new CPos(x * 1024, y * 1024, 0), 256, new Color(value, value, value, 0.8f));
				}
			}
		}
	}
}
