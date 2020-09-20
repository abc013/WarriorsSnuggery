using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.Maps
{
	public sealed class MapLoader
	{
		readonly World world;
		readonly Map map;

		public MPos Bounds => map.Bounds;
		public MPos Center => map.Center;
		public CPos TopLeftCorner => map.TopLeftCorner;
		public CPos TopRightCorner => map.TopRightCorner;
		public CPos BottomLeftCorner => map.BottomLeftCorner;
		public CPos BottomRightCorner => map.BottomRightCorner;
		public CPos PlayerSpawn { get => map.PlayerSpawn; set => map.PlayerSpawn = value; }
		public CPos Exit { get => map.Exit; set => map.Exit = value; }

		public List<MapGeneratorInfo> Infos => map.Type.GeneratorInfos;
		public bool FromSave => map.Type.FromSave;

		public GameMode GameMode => world.Game.Mode;
		public GameStatistics Statistics => world.Game.Statistics;

		public readonly Random Random;
		public readonly Dictionary<int, NoiseMap> NoiseMaps = new Dictionary<int, NoiseMap>();

		readonly int[,] generatorReservations;

		readonly ushort[,] terrainIDs;

		public MapLoader(World world, Map map)
		{
			this.world = world;
			this.map = map;

			Random = new Random(map.Seed);
			// NoiseMaps
			foreach (var info in map.Type.NoiseMapInfos)
			{
				var noiseMap = new NoiseMap(Bounds, map.Seed, info);
				NoiseMaps.Add(info.ID, noiseMap);
				MapPrinter.PrintNoiseMap(noiseMap);
			}

			generatorReservations = new int[Bounds.X, Bounds.Y];
			terrainIDs = new ushort[Bounds.X, Bounds.Y];
		}

		public void Generate()
		{
			// Check whether from save first. Saves must have a OverridePiece: the saved map.
			if (map.Type.FromSave)
			{
				var path = FileExplorer.Saves;
				var file = map.Type.OverridePiece + ".yaml";

				var input = new Piece(map.Type.OverridePiece, path + file, RuleReader.Read(path, file));
				GenerateCrucialPiece(input, MPos.Zero);

				// Local player should be somewhere within the piece
				world.LocalPlayer = world.ActorLayer.ToAdd().FirstOrDefault(a => a.IsPlayer);

				return;
			}

			if (!string.IsNullOrEmpty(map.Type.OverridePiece))
				GeneratePiece(PieceManager.GetPiece(map.Type.OverridePiece), MPos.Zero, 100, true);
			else
				map.Type.TerrainGenerationBase.GetGenerator(Random, this).Generate();

			// Generators
			foreach (var info in map.Type.GeneratorInfos)
				info.GetGenerator(Random, this)?.Generate();
		}

		public void Apply()
		{
			for (int x = 0; x < Bounds.X; x++)
				for (int y = 0; y < Bounds.Y; y++)
					world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(x, y), terrainIDs[x, y]));
		}

		public bool AcquireCell(MPos pos, int id)
		{
			if (generatorReservations[pos.X, pos.Y] > id)
				return false;

			generatorReservations[pos.X, pos.Y] = id;
			return true;
		}

		public bool CanAcquireCell(MPos pos, int id)
		{
			if (generatorReservations[pos.X, pos.Y] > id)
				return false;

			return true;
		}

		public void SetTerrain(int x, int y, ushort id)
		{
			terrainIDs[x, y] = id;
		}

		public Actor AddActor(CPos pos, ActorProbabilityInfo info)
		{
			var actor = ActorCreator.Create(world, info.Type, pos, info.Team, info.IsBot, health: info.Health);
			world.Add(actor);

			return actor;
		}

		public Actor AddActor(CPos position, string name, byte team = 0, bool isBot = false, float health = 1f)
		{
			var actor = ActorCreator.Create(world, name, position, team, isBot, false, health);
			world.Add(actor);

			return actor;
		}
		
		public Actor AddActor(ActorInit init, bool overrideID, CPos offset)
		{
			var actor = ActorCreator.Create(world, init, overrideID, offset);
			world.Add(actor);

			return actor;
		}

		public bool GenerateCrucialPiece(Piece piece, MPos position, int ID = int.MaxValue)
		{
			return GeneratePiece(piece, position, ID, true);
		}

		public bool GeneratePiece(Piece piece, MPos position, int ID, bool important = false, bool cancelIfAcquiredBySameID = false)
		{
			if (!piece.IsInMap(position, Bounds))
			{
				Log.WriteDebug(string.Format("Piece '{0}' at Position '{1}' could not be created because it overlaps to the world's edge.", piece.Name, position));
				return false;
			}

			if (!important)
			{
				for (int x = position.X; x < (piece.Size.X + position.X); x++)
				{
					for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					{
						if (!CanAcquireCell(new MPos(x, y), ID) || (cancelIfAcquiredBySameID && generatorReservations[x, y] == ID))
							return false;
					}
				}
			}
			for (int x = position.X; x < (piece.Size.X + position.X); x++)
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					AcquireCell(new MPos(x, y), ID);

			piece.PlacePiece(position, this, world);

			return true;
		}
	}
}
