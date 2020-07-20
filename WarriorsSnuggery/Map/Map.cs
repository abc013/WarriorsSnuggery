using System;
using System.IO;
using System.Text;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Parts;

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

		// Map Information
		public MPos Bounds { get; private set; }
		public MPos Center { get { return Bounds / new MPos(2, 2); } }
		public MPos DefaultEdgeDistance { get { return Bounds / new MPos(8, 8); } }

		public CPos TopLeftCorner { get { return new CPos(-512, -512, 0); } }
		public CPos TopRightCorner { get { return new CPos(-512 + Bounds.X * 1024, -512, 0); } }
		public CPos BottomLeftCorner { get { return new CPos(-512, -512 + Bounds.Y * 1024, 0); } }
		public CPos BottomRightCorner { get { return new CPos(-512 + Bounds.X * 1024, -512 + Bounds.Y * 1024, 0); } }

		public CPos PlayerSpawn;
		public MPos Exit;

		int[,] tilesWithAssignedGenerator;

		public Map(World world, MapInfo type, int seed, int level, int difficulty)
		{
			this.world = world;

			PlayerSpawn = type.SpawnPoint.ToCPos();
			Type = type;
			Seed = seed;
			random = new Random(seed);

			Bounds = type.CustomSize != MPos.Zero ? type.CustomSize : MapUtils.RandomMapBounds(random, difficulty, level, MapUtils.MinimumMapBounds, MapUtils.MaximumMapBounds);
		}

		public void Load()
		{
			Camera.SetBounds(Bounds);
			VisibilitySolver.SetBounds(Bounds, world.ShroudLayer);

			tilesWithAssignedGenerator = new int[Bounds.X, Bounds.Y];

			// Important Parts
			if (!string.IsNullOrEmpty(Type.OverridePiece))
			{
				if (Type.FromSave)
				{
					var input = Piece.Load(Type.OverridePiece, string.Empty, RuleReader.Read(FileExplorer.Saves, Type.OverridePiece + ".yaml").ToArray());
					GeneratePiece(input, MPos.Zero, 100, true);
				}
				else
				{
					GeneratePiece(PieceManager.GetPiece(Type.OverridePiece), MPos.Zero, 100, true);
				}
			}
			else // If there is nothing important to generate, then generate base terrain
				Type.TerrainGenerationBase.GetGenerator(random, this, world).Generate();


			// Generators
			if (!Type.FromSave)
			{
				foreach (var info in Type.GeneratorInfos)
				{
					var generator = info.GetGenerator(random, this, world);
					if (generator != null)
						generator.Generate();
				}
			}

			//MapPrinter.PrintMapGeneration("debug", TerrainGenerationArray, TilesWithAssignedGenerator, Type.GeneratorInfos.Length);
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
