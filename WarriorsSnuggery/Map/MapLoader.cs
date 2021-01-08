using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Maps.Generators;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

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

		public MapGeneratorInfo[] Infos => map.Type.Generators;
		public bool FromSave => map.Type.IsSave;

		public ObjectiveType ObjectiveType => world.Game.ObjectiveType;
		public GameStatistics Statistics => world.Game.Statistics;

		public readonly Random Random;
		public readonly NoiseMap EmptyNoiseMap;
		public readonly Dictionary<int, NoiseMap> NoiseMaps = new Dictionary<int, NoiseMap>();

		readonly int[,] generatorReservations;

		readonly ushort[,] terrainInformation;
		readonly (short id, short health)[,] wallInformation;
		readonly List<(ActorInit init, CPos offset)> actorInformation = new List<(ActorInit init, CPos offset)>();
		readonly List<WeaponInit> weaponInformation = new List<WeaponInit>();
		readonly List<ParticleInit> particleInformation = new List<ParticleInit>();

		public MapLoader(World world, Map map)
		{
			this.world = world;
			this.map = map;

			Random = new Random(map.Seed);
			// NoiseMaps
			foreach (var info in map.Type.NoiseMaps)
			{
				var noiseMap = new NoiseMap(Bounds, map.Seed, info);

				NoiseMaps.Add(info.ID, noiseMap);
				MapPrinter.PrintNoiseMap(Bounds, noiseMap);
			}

			// Empty NoiseMap
			EmptyNoiseMap = new NoiseMap(Bounds, 0, new NoiseMapInfo(0, new List<MiniTextNode>()));

			generatorReservations = new int[Bounds.X, Bounds.Y];
			terrainInformation = new ushort[Bounds.X, Bounds.Y];
			wallInformation = new (short, short)[world.WallLayer.Bounds.X, world.WallLayer.Bounds.Y];
		}

		public void Generate()
		{
			// Check whether from save first. Saves must have a OverridePiece: the saved map.
			if (map.Type.IsSave)
			{
				var path = FileExplorer.Saves;
				var file = map.Type.OverridePiece + ".yaml";

				var input = new Piece(map.Type.OverridePiece, path + file, RuleReader.FromFile(path, file));
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
			foreach (var info in map.Type.Generators)
				info.GetGenerator(Random, this)?.Generate();
		}

		public void Apply()
		{
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
				{
					world.TerrainLayer.Set(TerrainCreator.Create(world, new MPos(x, y), terrainInformation[x, y]));

					for (int i = 0; i < 2; i++)
						applyWall(new MPos(x * 2 + i, y));
				}
			}

			// Generate walls that are at the edge of the map
			for (int y = 0; y < Bounds.Y; y++)
			{
				for (int i = 0; i < 2; i++)
					applyWall(new MPos(Bounds.X * 2 + i, y));
			}

			for (int x = 0; x < Bounds.X + 1; x++)
			{
				for (int i = 0; i < 2; i++)
					applyWall(new MPos(x * 2 + i, Bounds.Y));
			}

			var actors = new List<Actor>();

			foreach (var (init, offset) in actorInformation)
			{
				var actor = ActorCreator.Create(world, init, !FromSave, offset);
				actors.Add(actor);
				world.Add(actor);
			}

			foreach (var actor in actors)
				actor.OnLoad();

			foreach (var init in weaponInformation)
				world.Add(WeaponCreator.Create(world, init));

			foreach (var init in particleInformation)
				world.Add(ParticleCreator.Create(world, init));
		}

		void applyWall(MPos pos)
		{
			var (id, health) = wallInformation[pos.X, pos.Y];

			if (id == 0 && health == 0)
				return;

			var wall = WallCreator.Create(pos, world.WallLayer, id);
			wall.Health = health;

			world.WallLayer.Set(wall);
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
			terrainInformation[x, y] = id;
		}

		public void SetWall(int x, int y, short id, short health)
		{
			wallInformation[x, y] = (id, health);
		}

		public void AddActor(CPos pos, Generators.ActorProbabilityInfo info)
		{
			var init = ActorCreator.CreateInit(world, info.Type, pos, info.Team, info.IsBot, health: info.Health);

			actorInformation.Add((init, CPos.Zero));
		}

		public void AddActor(CPos position, string name, byte team = 0, bool isBot = false, float health = 1f)
		{
			var init = ActorCreator.CreateInit(world, name, position, team, isBot, false, health);

			actorInformation.Add((init, CPos.Zero));
		}
		
		public void AddActor(ActorInit init, CPos offset)
		{
			actorInformation.Add((init, offset));
		}

		public void AddWeapon(WeaponInit init)
		{
			weaponInformation.Add(init);
		}

		public void AddParticle(ParticleInit init)
		{
			particleInformation.Add(init);
		}

		public bool WallExists(int x, int y)
		{
			var (id, health) = wallInformation[x, y];
			if (id == 0 && health == 0)
				return false;

			return true;
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
			else if (FromSave)
			{
				world.Game.CurrentActorID = piece.MaxActorID + 1;
				world.Game.CurrentWeaponID = piece.MaxWeaponID + 1;
			}

			for (int x = position.X; x < (piece.Size.X + position.X); x++)
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					AcquireCell(new MPos(x, y), ID);

			piece.PlacePiece(position, this);

			return true;
		}
	}
}
