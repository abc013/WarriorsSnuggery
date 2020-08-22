using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;

namespace WarriorsSnuggery
{
	public enum NoiseType
	{
		NONE,
		NOISE,
		CLOUDS,
		MAZE
	}

	public class Map
	{
		readonly World world;
		readonly Random random;

		public readonly MapInfo Type;
		public readonly int Seed;

		public static readonly CPos Offset = new CPos(-512, -512, 0);

		public CPos PlayerSpawn { get; private set; }
		public MPos Exit;

		public MPos Bounds { get; private set; }
		public MPos Center => Bounds / new MPos(2, 2);
		public MPos DefaultEdgeDistance => Bounds / new MPos(8, 8);

		public CPos TopLeftCorner => Offset;
		public CPos TopRightCorner => new CPos(Offset.X + (Bounds.X * 1024), Offset.Y, 0);
		public CPos BottomLeftCorner => new CPos(Offset.X, Offset.Y + (Bounds.Y * 1024), 0);
		public CPos BottomRightCorner => new CPos(Offset.X + (Bounds.X * 1024), Offset.Y + (Bounds.Y * 1024), 0);

		public readonly Dictionary<int, NoiseMap> Noises = new Dictionary<int, NoiseMap>();

		int[,] tilesWithAssignedGenerator;

		public Map(World world, MapInfo type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerSpawn = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;
			random = new Random(seed);

			Bounds = determineBounds(difficulty, level);
		}

		MPos determineBounds(int difficulty, int level)
		{
			if (Type.CustomSize != MPos.Zero)
				return Type.CustomSize;

			return MapUtils.RandomMapBounds(random, difficulty, level);
		}

		public void Load()
		{
			Camera.SetBounds(Bounds);
			VisibilitySolver.SetBounds(Bounds, world.ShroudLayer);

			tilesWithAssignedGenerator = new int[Bounds.X, Bounds.Y];

			// Check whether from save first. Saves must have a OverridePiece, the saved map.
			if (Type.FromSave)
			{
				var path = FileExplorer.Saves;
				var file = Type.OverridePiece + ".yaml";

				var input = new Piece(Type.OverridePiece, path + file, RuleReader.Read(path, file));
				GeneratePiece(input, MPos.Zero, 100, true);
				return;
			}

			// NoiseMaps
			foreach (var info in Type.NoiseMapInfos)
			{
				var noiseMap = new NoiseMap(Bounds, Seed, info);
				Noises.Add(info.ID, noiseMap);
				MapPrinter.PrintNoiseMap(noiseMap);
			}

			if (!string.IsNullOrEmpty(Type.OverridePiece))
				GeneratePiece(PieceManager.GetPiece(Type.OverridePiece), MPos.Zero, 100, true);
			else
				Type.TerrainGenerationBase.GetGenerator(random, this, world).Generate();

			// Generators
			foreach (var info in Type.GeneratorInfos)
				info.GetGenerator(random, this, world)?.Generate();
			// empty data because it is not needed anymore
			tilesWithAssignedGenerator = null;
		}

		public bool AcquireCell(MPos pos, int id)
		{
			if (tilesWithAssignedGenerator[pos.X, pos.Y] > id)
				return false;

			tilesWithAssignedGenerator[pos.X, pos.Y] = id;
			return true;
		}

		public bool CanAcquireCell(MPos pos, int id)
		{
			if (tilesWithAssignedGenerator[pos.X, pos.Y] > id)
				return false;

			return true;
		}

		public bool GeneratePiece(Piece piece, MPos position, int ID, bool important = false, bool playerSpawn = false, bool cancelIfAcquiredBySameID = false)
		{
			if (!piece.IsInMap(position, Bounds))
			{
				Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}' could not be created because it overlaps to the world's edge.", piece.Name, position));
				return false;
			}

			for (int x = position.X; x < (piece.Size.X + position.X); x++)
			{
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
				{
					if (important)
						AcquireCell(new MPos(x, y), ID);
					else if (!CanAcquireCell(new MPos(x, y), ID) || (cancelIfAcquiredBySameID && tilesWithAssignedGenerator[x, y] == ID))
						return false;
				}
			}
			if (!important)
			{
				for (int x = position.X; x < (piece.Size.X + position.X); x++)
				{
					for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					{
						AcquireCell(new MPos(x, y), ID);
					}
				}
			}

			piece.PlacePiece(position, world);

			if (playerSpawn)
				PlayerSpawn = new CPos(position.X * 1024 + piece.Size.X * 512, position.Y * 1024 + piece.Size.Y * 512, 0);

			return true;
		}
	}
}
