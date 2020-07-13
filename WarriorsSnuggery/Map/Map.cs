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
			world.SetBounds(Bounds);

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

		public void Save(string directory, string name)
		{
			SaveFile(directory + name + ".yaml", name);
		}

		public void SaveFile(string file, string name)
		{
			using var writer = new StreamWriter(file, false);

			writer.WriteLine("Name=" + name);
			writer.WriteLine("Size=" + Bounds.X + "," + Bounds.Y);

			var builder = new StringBuilder(8 + Bounds.X * Bounds.Y * 3, 8 + Bounds.X * Bounds.Y * 4);
			builder.Append("Terrain=");
			for (int y = 0; y < Bounds.Y; y++)
			{
				for (int x = 0; x < Bounds.X; x++)
				{
					builder.Append(world.TerrainLayer.Terrain[x, y].Type.ID);
					if (x == Bounds.X - 1 && y == Bounds.Y - 1)
						break;
					builder.Append(",");
				}
			}
			writer.WriteLine(builder);
			builder.Clear();

			var wallSize = world.WallLayer.Bounds;
			var builder2 = new StringBuilder(6 + wallSize.X * wallSize.Y * 6, 6 + wallSize.X * wallSize.Y * 12);
			builder2.Append("Walls=");
			for (int y = 0; y < wallSize.Y - 1; y++)
			{
				for (int x = 0; x < wallSize.X - 1; x++)
				{
					var wall = world.WallLayer.Walls[x, y];
					if (wall == null)
						builder2.Append("-1,0,");
					else
						builder2.Append(wall.Type.ID + "," + wall.Health + ",");
				}
			}
			builder2.Remove(builder2.Length - 1, 1);
			writer.WriteLine(builder2);
			builder2.Clear();

			writer.WriteLine("Actors=");
			for (int i = 0; i < world.Actors.Count; i++)
			{
				var a = world.Actors[i];
				writer.WriteLine("\t" + i + "=" + a.Position.X + "," + a.Position.Y + "," + a.Position.Z);
				writer.WriteLine("\t\t" + "Type=" + ActorCreator.GetName(a.Type));
				if (a.Team != Objects.Actor.NeutralTeam)
					writer.WriteLine("\t\t" + "Team=" + a.Team);
				if (a.Health != null && a.Health.HP != a.Health.MaxHP)
					writer.WriteLine("\t\t" + "Health=" + a.Health.RelativeHP.ToString(Settings.FloatFormat));
				if (a.IsBot)
				{
					writer.WriteLine("\t\t" + "IsBot=" + a.IsBot);

					if (a.BotPart.Target != null)
						writer.WriteLine("\t\t" + "BotTarget=" + a.BotPart.Target.Position);
				}
				if (a.IsPlayer)
					writer.WriteLine("\t\t" + "IsPlayer=true");
				if (a.IsPlayerSwitch)
				{
					writer.WriteLine("\t\t" + "IsPlayerSwitch=true");
					var part = (PlayerSwitchPart)a.Parts.Find(p => p is PlayerSwitchPart);

					writer.WriteLine("\t\t" + "Duration=" + part.CurrentTick);
					writer.WriteLine("\t\t" + "ToActor=" + ActorCreator.GetName(part.ActorType));
					writer.WriteLine("\t\t" + "RelativeHP=" + part.RelativeHP);
				}
			}

			writer.Flush();
			writer.Close();
		}
	}
}
