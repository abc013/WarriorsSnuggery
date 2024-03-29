﻿using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Maps.Generators;
using WarriorsSnuggery.Maps.Noises;
using WarriorsSnuggery.Maps.Pieces;
using WarriorsSnuggery.Objectives;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Maps
{
	public sealed class MapLoader
	{
		readonly World world;
		readonly Map map;

		public MPos PlayableOffset => map.PlayableOffset;
		public MPos PlayableBounds => map.PlayableBounds;
		public MPos Bounds => map.Bounds;
		public MPos Center => map.Center;

		public CPos PlayerSpawn { get => map.PlayerSpawn; set => map.PlayerSpawn = value; }
		public CPos Exit;

		public MapType Type => map.Type;
		public ObjectiveType ObjectiveType => world.Game.ObjectiveType;

		public readonly Random Random;

		public readonly Dictionary<int, NoiseMap> NoiseMaps = new Dictionary<int, NoiseMap>();
		public readonly List<Waypoint> Waypoints = new List<Waypoint>();
		public readonly List<MPos> PatrolLocations = new List<MPos>();

		readonly int[,] generatorReservations;
		readonly bool[,] invalidForPatrols;

		readonly ushort[,] terrainInformation;
		readonly (short id, short health)[,] wallInformation;
		readonly Dictionary<MPos, List<(ActorInit init, CPos offset)>> actorInformation = new Dictionary<MPos, List<(ActorInit init, CPos offset)>>();
		readonly List<WeaponInit> weaponInformation = new List<WeaponInit>();
		readonly List<ParticleInit> particleInformation = new List<ParticleInit>();
		readonly Dictionary<byte, bool[]> shroudInformation = new Dictionary<byte, bool[]>();

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

			generatorReservations = new int[Bounds.X, Bounds.Y];
			invalidForPatrols = new bool[Bounds.X, Bounds.Y];
			terrainInformation = new ushort[Bounds.X, Bounds.Y];
			wallInformation = new (short, short)[world.WallLayer.Bounds.X, world.WallLayer.Bounds.Y];
		}

		public void Generate()
		{
			// Check whether from save first. Saves must have a OverridePiece: the saved map.
			if (map.Type.IsSave)
			{
				var filepath = FileExplorer.Saves + map.Type.OverridePiece.ToString() + ".yaml";
				var input = new Piece(map.Type.OverridePiece, filepath);
				GenerateCrucialPiece(input, MPos.Zero);

				// Local player should be somewhere within the piece
				world.LocalPlayer = world.ActorLayer.ToAdd().FirstOrDefault(a => a.IsPlayer);

				return;
			}

			if (map.Type.OverridePiece != null)
				GenerateCrucialPiece(PieceManager.GetPiece(map.Type.OverridePiece), MPos.Zero);

			// Generators
			foreach (var info in map.Type.Generators)
				info.GetGenerator(Random, this)?.Generate();
			
			map.DebugInformation = new MapDebugInformation(NoiseMaps, Waypoints, new List<MPos>(PatrolLocations));
			map.PossiblePatrolLocations = new List<MPos>(PatrolLocations);

			// Applying the changes
			for (int x = 0; x < Bounds.X; x++)
			{
				for (int y = 0; y < Bounds.Y; y++)
					world.TerrainLayer.Set(TerrainCache.Create(world, new MPos(x, y), terrainInformation[x, y]));
			}

			for (int x = 0; x <= Bounds.X; x++)
			{
				for (int y = 0; y <= Bounds.Y; y++)
				{
					applyWall(new WPos(x, y, false));
					applyWall(new WPos(x, y, true));
				}
			}

			var actors = new List<Actor>();

			foreach (var list in actorInformation.Values)
			{
				foreach (var (init, offset) in list)
				{
					var actor = ActorCache.Create(world, init, !Type.IsSave);
					actor.Position += offset;

					actors.Add(actor);
					world.Add(actor);
				}
			}

			foreach (var actor in actors)
				actor.OnLoad();

			foreach (var init in weaponInformation)
				world.Add(WeaponCache.Create(world, init));

			foreach (var init in particleInformation)
				world.Add(ParticleCache.Create(world, init));

			if (world.Game.ObjectiveType != ObjectiveType.SURVIVE_WAVES)
			{
				foreach (var info in map.Type.PatrolPlacers)
				{
					if (info.UseForWaves)
						continue;

					var placer = new PatrolPlacer(Random, world, info, invalidForPatrols);
					placer.PlacePatrols();
				}
			}

			foreach (var (team, values) in shroudInformation)
				world.ShroudLayer.RevealShroudList(team, values);

			world.PathfinderLayer.Update(world);
		}

		void applyWall(WPos pos)
		{
			var (id, health) = wallInformation[pos.X, pos.Y];

			if (id == 0 && health == 0)
				return;

			var wall = WallCache.Create(pos, world, id);
			wall.Health = health;

			world.WallLayer.Set(wall);
		}

		public bool AcquireCell(MPos pos, int id, bool check = true, bool removeActors = true, bool denyPatrols = false)
		{
			if (check && !CanAcquireCell(pos, id))
				return false;

			if (removeActors)
				RemoveActors(pos);

			generatorReservations[pos.X, pos.Y] = id;
			invalidForPatrols[pos.X, pos.Y] = denyPatrols;
			return true;
		}

		public bool CanAcquireCell(MPos pos, int id)
		{
			return canAcquireCell(pos.X, pos.Y, id);
		}

		public bool CanAcquireArea(MPos pos, MPos bounds, int id, bool idInclusive = false)
		{
			for (int x = pos.X; x < pos.X + bounds.X; x++)
			{
				for (int y = pos.Y; y < pos.Y + bounds.Y; y++)
				{
					if (!canAcquireCell(x, y, id) && (!idInclusive || generatorReservations[x, y] != id))
						return false;
				}
			}

			return true;
		}

		bool canAcquireCell(int x, int y, int id)
		{
			return generatorReservations[x, y] < id;
		}

		public void SetTerrain(int x, int y, ushort id)
		{
			terrainInformation[x, y] = id;
		}

		public void SetWall(int x, int y, short id, short health)
		{
			wallInformation[x, y] = (id, health);
		}

		public void AddActor(CPos pos, ActorProbabilityInfo info)
		{
			var init = ActorCache.CreateInit(world, info.Type, pos, info.Team, info.IsBot);

			AddActor(init, CPos.Zero);
		}
		
		public void AddActor(ActorInit init, CPos offset)
		{
			var mpos = (init.Position + offset).ToMPos();
			if (actorInformation.ContainsKey(mpos))
			{
				actorInformation[mpos].Add((init, offset));
				return;
			}

			var list = new List<(ActorInit init, CPos offset)>
			{
				(init, offset)
			};

			actorInformation.Add(mpos, list);
		}

		public void RemoveActors(MPos pos)
		{
			actorInformation.Remove(pos);
		}

		public void AddWeapon(WeaponInit init)
		{
			weaponInformation.Add(init);
		}

		public void AddParticle(ParticleInit init)
		{
			particleInformation.Add(init);
		}

		public void RevealShroud(byte team, MPos offset, MPos bounds, bool[] values)
		{
			if (!shroudInformation.ContainsKey(team))
				shroudInformation.Add(team, new bool[Bounds.X * 2 * Bounds.Y * 2]);

			for (int y = offset.Y; y < offset.Y + bounds.Y; y++)
			{
				var xOffset = y * (Bounds.X * 2) + offset.X;
				Array.Copy(values, y * bounds.X, shroudInformation[team], xOffset, bounds.X);
			}
		}

		public bool WallExists(int x, int y)
		{
			var (id, health) = wallInformation[x, y];
			if (id == 0 && health == 0)
				return false;

			return true;
		}

		public bool GenerateCrucialPiece(Piece piece, MPos position, int ID = int.MaxValue, bool denyPatrols = false)
		{
			return GeneratePiece(piece, position, ID, true, denyPatrols: denyPatrols);
		}

		public bool GeneratePiece(Piece piece, MPos position, int ID, bool important = false, bool idInclusive = false, bool denyPatrols = false)
		{
			if (!piece.IsInMap(position, Bounds))
			{
				Log.Warning($"Piece '{piece.Name}' at Position '{position}' could not be created because it overlaps to the world's edge.");
				return false;
			}

			if (!important)
			{
				if (!CanAcquireArea(position, piece.Size, ID, idInclusive))
					return false;
			}
			else if (Type.IsSave)
			{
				world.Game.CurrentActorID = piece.MaxActorID + 1;
				world.Game.CurrentWeaponID = piece.MaxWeaponID + 1;
			}

			for (int x = position.X; x < (piece.Size.X + position.X); x++)
				for (int y = position.Y; y < (piece.Size.Y + position.Y); y++)
					AcquireCell(new MPos(x, y), ID, denyPatrols);

			piece.PlacePiece(position, this);

			return true;
		}

		public NoiseMap GetNoise(int id)
		{
			if (id < 0)
				return NoiseMap.Empty;

			if (!NoiseMaps.ContainsKey(id))
				throw new Loader.InvalidNodeException($"Map type {Type.Name} is missing a NoiseMap with ID {id}.");

			return NoiseMaps[id];
		}
	}
}
