using System;
using System.IO;
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
			world.TerrainLayer.SetMapDimensions(Bounds);
			world.WallLayer.SetMapSize(Bounds);
			world.PhysicsLayer.SetMapDimensions(Bounds);
			world.ShroudLayer.SetMapDimensions(Bounds, Settings.MaxTeams, Type.DefaultType == GameType.MAINMENU || Type.DefaultType == GameType.MENU || Type.DefaultType == GameType.EDITOR || Type.DefaultType == GameType.TUTORIAL);

			VisibilitySolver.SetMapDimensions(Bounds, world.ShroudLayer);

			tilesWithAssignedGenerator = new int[Bounds.X, Bounds.Y];

			// Basic terrain
			Type.TerrainGenerationBase.GetGenerator(random, this, world).Generate();

			// Important Parts
			if (!string.IsNullOrEmpty(Type.OverridePiece))
			{
				var input = RuleReader.Read(!Type.FromSave ? FileExplorer.FindPath(FileExplorer.Maps, Type.OverridePiece, ".yaml") : FileExplorer.Saves, Type.OverridePiece + ".yaml");

				GeneratePiece(input.ToArray(), MPos.Zero, 100, true);
			}

			// Generators
			foreach (var info in Type.GeneratorInfos)
			{
				var generator = info.GetGenerator(random, this, world);
				generator.Generate();
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

		public bool GeneratePiece(MiniTextNode[] nodes, MPos position, int ID, bool important = false, bool playerSpawn = false, bool cancelIfAcquiredBySameID = false)
		{
			return GeneratePiece(Piece.LoadPiece("generated", nodes), position, ID, important, playerSpawn, cancelIfAcquiredBySameID);
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

		public void Save(string name)
		{
			Save(FileExplorer.Maps, name);
		}

		public void Save(string directory, string name)
		{
			SaveFile(directory + @"maps/" + name + ".yaml", name);
		}

		public void SaveFile(string file, string name)
		{
			using (var writer = new StreamWriter(file, false))
			{
				writer.WriteLine("Name=" + name);
				writer.WriteLine("Size=" + Bounds.X + "," + Bounds.Y);

				var terrain = "Terrain=";
				for (int y = 0; y < Bounds.Y; y++)
				{
					for (int x = 0; x < Bounds.X; x++)
					{
						terrain += world.TerrainLayer.Terrain[x, y].Type.ID + ",";
					}
				}

				terrain = terrain.Substring(0, terrain.Length - 1);
				writer.WriteLine(terrain);

				var walls = "Walls=";
				for (int y = 0; y < world.WallLayer.Size.Y - 1; y++)
				{
					for (int x = 0; x < world.WallLayer.Size.X - 1; x++)
					{
						walls += (world.WallLayer.Walls[x, y] == null ? -1 : world.WallLayer.Walls[x, y].Type.ID) + ",";
					}
				}

				walls = walls.Substring(0, walls.Length - 1);
				writer.WriteLine(walls);

				writer.WriteLine("Actors=");
				for (int i = 0; i < world.Actors.Count; i++)
				{
					var a = world.Actors[i];
					writer.WriteLine("\t" + i + "=" + a.Position.X + "," + a.Position.Y + "," + a.Position.Z);
					writer.WriteLine("\t\t" + "Type=" + ActorCreator.GetName(a.Type));
					if (a.Team != Objects.Actor.NeutralTeam)
						writer.WriteLine("\t\t" + "Team=" + a.Team);
					if (a.Health != null && a.Health.HP != a.Health.MaxHP)
						writer.WriteLine("\t\t" + "Health=" + a.Health.HPRelativeToMax);
					if (a.IsBot)
						writer.WriteLine("\t\t" + "IsBot=" + a.IsBot);
					if (a.IsPlayer)
						writer.WriteLine("\t\t" + "IsPlayer=" + a.IsPlayer);
				}

				writer.Flush();
				writer.Close();
			}
		}
	}
}
