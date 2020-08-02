using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Trophies;

namespace WarriorsSnuggery
{
	public sealed class World : ITick, IDisposable
	{
		public readonly Map Map;
		public readonly Game Game;

		public readonly TerrainLayer TerrainLayer;
		public readonly WallLayer WallLayer;
		public readonly PhysicsLayer PhysicsLayer;
		public readonly ShroudLayer ShroudLayer;
		public readonly SmudgeLayer SmudgeLayer;

		public readonly ActorLayer ActorLayer;
		public readonly WeaponLayer WeaponLayer;
		public readonly ParticleLayer ParticleLayer;

		public readonly List<PositionableObject> Objects = new List<PositionableObject>();
		readonly List<PositionableObject> objectsToAdd = new List<PositionableObject>();

		public List<PositionableObject> ToRender { get; private set; }

		public Actor LocalPlayer;

		public bool PlayerSwitching { get { return LocalPlayer.IsPlayerSwitch; } }
		public bool PlayerAlive = true;

		public int PlayerDamagedTick = 0;
		public bool KeyFound;

		public World(Game game, int seed, GameStatistics stats)
		{
			Game = game;

			Map = new Map(this, game.MapType, seed, stats.Level, stats.Difficulty);

			var bounds = Map.Bounds;
			TerrainLayer = new TerrainLayer(bounds);
			WallLayer = new WallLayer(bounds);
			PhysicsLayer = new PhysicsLayer(bounds);
			ShroudLayer = new ShroudLayer(bounds);
			SmudgeLayer = new SmudgeLayer();

			ActorLayer = new ActorLayer(bounds);
			WeaponLayer = new WeaponLayer();
			ParticleLayer = new ParticleLayer(bounds);
		}

		public void Load()
		{
			Map.Load();

			if (Game.Type != GameType.EDITOR)
			{
				if (!Map.Type.FromSave)
				{
					var start = Map.PlayerSpawn != new CPos(-1024, -1024, 0) ? Map.PlayerSpawn : new MPos(Map.Bounds.X / 2, Map.Bounds.Y / 2).ToCPos();

					LocalPlayer = ActorCreator.Create(this, Game.Statistics.Actor, start, Actor.PlayerTeam, isPlayer: true);
					Add(LocalPlayer);
				}
				else
				{
					for(int i = 0; i < Game.Statistics.Shroud.Count; i++)
						ShroudLayer.RevealShroudList(i, Game.Statistics.Shroud[i]);
				}

				if (Game.Type == GameType.NORMAL)
					Add(new ActionText(LocalPlayer.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 300, ActionText.ActionTextType.TRANSFORM, @"Level" + Game.Statistics.Level));
			}
			else
			{
				PlayerAlive = false;
				Camera.Position(new MPos(Map.Bounds.X / 2, Map.Bounds.Y / 2).ToCPos(), true);
			}

			ShroudLayer.RevealAll = Game.Type != GameType.NORMAL && Game.Type != GameType.TEST;

			// First tick, does only add objects
			ActorLayer.Tick();
			WeaponLayer.Tick();
			ParticleLayer.Tick();
			addObjects();
		}

		public void Tick()
		{
			foreach (var @object in Objects)
				@object.Tick();

			ActorLayer.Tick();
			ParticleLayer.Tick();
			WeaponLayer.Tick();

			TerrainLayer.Tick();
			SmudgeLayer.Tick();

			addObjects();
			prepareRenderList();
		}

		void addObjects()
		{
			Objects.RemoveAll(o => o.Disposed);

			if (objectsToAdd.Any())
			{
				foreach (var @object in objectsToAdd)
				{
					@object.CheckVisibility();

					Objects.Add(@object);
				}
				objectsToAdd.Clear();
			}
		}

		void prepareRenderList()
		{
			ToRender = Objects.ToList(); // Copy array
			ToRender.AddRange(ActorLayer.VisibleActors);
			ToRender.AddRange(WeaponLayer.VisibleWeapons);
			ToRender.AddRange(ParticleLayer.VisibleParticles);
			ToRender.AddRange(WallLayer.VisibleWalls);

			ToRender = ToRender.OrderBy(e => e.GraphicPosition.Z + (e.Position.Y - 512) * 2).ToList();
		}

		public void TrophyCollected(string collected)
		{
			if (Game.Statistics.UnlockedTrophies.Contains(collected))
				return;

			if (!TrophyManager.Trophies.ContainsKey(collected))
				throw new YamlInvalidNodeException("Unable to get Trophy with internal name " + collected);

			Game.AddInfoMessage(250, "Trophy collected!");
			Game.Statistics.UnlockedTrophies.Add(collected);
			Game.Statistics.MaxMana += TrophyManager.Trophies[collected].MaxManaIncrease;
		}

		public void FinishPlayerSwitch(Actor @new, ActorType type)
		{
			LocalPlayer = @new;
			Add(@new);

			Game.Statistics.Actor = ActorCreator.GetName(type);

			VisibilitySolver.ShroudUpdated();
		}

		public void BeginPlayerSwitch(ActorType to)
		{
			var health = LocalPlayer.Health != null ? LocalPlayer.Health.RelativeHP : 1;
			if (LocalPlayer.WorldPart == null || string.IsNullOrWhiteSpace(LocalPlayer.WorldPart.PlayerSwitchActor))
			{
				FinishPlayerSwitch(ActorCreator.Create(this, to, LocalPlayer.Position, LocalPlayer.Team, isPlayer: true, health: health), to);
				LocalPlayer.Dispose();
				return;
			}

			var switchActor = ActorCreator.Create(this, LocalPlayer.WorldPart.PlayerSwitchActor, LocalPlayer.Position, LocalPlayer.Team, isPlayer: true);
			var switchPart = (PlayerSwitchPart)switchActor.Parts.Find(p => p is PlayerSwitchPart);
			switchPart.RelativeHP = health;
			switchPart.ActorType = to;
			LocalPlayer.Dispose();
			LocalPlayer = switchActor;
			Add(switchActor);
		}

		public void PlayerKilled()
		{
			if (PlayerAlive)
			{
				PlayerAlive = false;
				Game.OldStatistics.Deaths++;

				Game.DefeatConditionsMet();
			}
		}

		public bool CheckCollision(PhysicsObject obj, Actor toIgnore = null)
		{
			if (obj.Physics.Shape == Physics.Shape.NONE)
				return false;

			var top = PhysicsLayer.Bounds.Y;
			var left = PhysicsLayer.Bounds.X;
			var bot = 0;
			var right = 0;
			foreach (var p in obj.PhysicsSectors)
			{
				if (p.Check(obj, toIgnore))
					return true;

				if (p.Position.X < left)
					left = p.Position.X;
				if (p.Position.Y < top)
					top = p.Position.Y;

				if (p.Position.X > right)
					right = p.Position.X;
				if (p.Position.Y > bot)
					bot = p.Position.Y;
			}

			var walls = WallLayer.WallList.Where(
				w => (w.TerrainPosition.X >= left * PhysicsLayer.SectorSize - 2)
			 && (w.TerrainPosition.X < (right + 1) * PhysicsLayer.SectorSize + 2)
			 && (w.TerrainPosition.Y >= top * PhysicsLayer.SectorSize - 1)
			 && (w.TerrainPosition.Y < (bot + 1) * PhysicsLayer.SectorSize + 1));

			foreach (var wall in walls)
			{
				if (obj.Physics.Intersects(wall.Physics))
					return true;
			}

			return false;
		}

		public void Add(Weapon weapon)
		{
			WeaponLayer.Add(weapon);
		}

		public void Add(Actor actor)
		{
			ActorLayer.Add(actor);
			PhysicsLayer.UpdateSectors(actor, true);
		}

		public void Add(Particle particle)
		{
			ParticleLayer.Add(particle);
		}

		public void Add(Particle[] particles)
		{
			ParticleLayer.Add(particles);
		}

		public void Add(PositionableObject @object)
		{
			objectsToAdd.Add(@object);
		}

		public Terrain TerrainAt(CPos pos)
		{
			if (pos.X < 0 && pos.X >= -512)
				pos = new CPos(0, pos.Y, pos.Z);
			if (pos.Y < 0 && pos.Y >= -512)
				pos = new CPos(pos.X, 0, pos.Z);

			return !IsInWorld(pos) ? null : TerrainAt(pos.ToWPos());
		}

		public Terrain TerrainAt(WPos position)
		{
			if (position.X < 0 || position.Y < 0 || position.X >= Map.Bounds.X || position.Y >= Map.Bounds.Y)
				return null;

			return TerrainLayer.Terrain[position.X, position.Y];
		}

		public bool ActorInWorld(CPos pos, Actor actor)
		{
			var size = Map.Bounds.ToCPos();

			return pos.X >= -512 + actor.Physics.RadiusX && pos.X < size.X - 512 - actor.Physics.RadiusX && pos.Y >= -512 + actor.Physics.RadiusY && pos.Y < size.Y - 512 - actor.Physics.RadiusY;
		}

		public bool IsInWorld(CPos pos)
		{
			var size = Map.Bounds.ToCPos();

			return pos.X >= -512 && pos.X < size.X - 512 && pos.Y >= -512 && pos.Y < size.Y - 512;
		}

		public void Save(string directory, string name, bool isSavegame)
		{
			var saver = new MapSaver(this, isSavegame);
			saver.Save(directory, name);
		}

		public void Dispose()
		{
			foreach (var o in Objects)
				o.Dispose();
			Objects.Clear();

			ActorLayer.Clear();
			ParticleLayer.Clear();
			WeaponLayer.Clear();

			TerrainLayer.Clear();
			WallLayer.Clear();
			ShroudLayer.Clear();
			SmudgeLayer.Clear();
		}
	}
}
