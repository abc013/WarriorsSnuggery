using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Objects.Weather;
using WarriorsSnuggery.Physics;
using WarriorsSnuggery.Trophies;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Maps;
using WarriorsSnuggery.Maps.Layers;

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

		public readonly PathfinderLayer PathfinderLayer;

		public readonly WeatherManager WeatherManager;

		public readonly List<PositionableObject> Objects = new List<PositionableObject>();
		readonly List<PositionableObject> objectsToAdd = new List<PositionableObject>();

		public Actor LocalPlayer;
		public bool PlayerAlive => LocalPlayer != null && LocalPlayer.IsAlive;

		public World(Game game, int seed, GameSave save)
		{
			Game = game;

			Map = new Map(this, game.MapType, seed, save.Level, save.Difficulty);

			var bounds = Map.Bounds;
			TerrainLayer = new TerrainLayer(bounds);
			ShroudLayer = new ShroudLayer(bounds);

			PathfinderLayer = new PathfinderLayer(bounds);

			WallLayer = new WallLayer(bounds, this);
			PhysicsLayer = new PhysicsLayer(bounds);
			SmudgeLayer = new SmudgeLayer();

			ActorLayer = new ActorLayer(bounds);
			WeaponLayer = new WeaponLayer();
			ParticleLayer = new ParticleLayer(bounds);

			WeatherManager = new WeatherManager(this, game.MapType);
		}

		public void Load()
		{
			Map.Load();
			PathfinderLayer.Update(WallLayer, TerrainLayer);

			if (Game.InteractionMode != InteractionMode.EDITOR)
			{
				if (!Map.Type.IsSave)
				{
					LocalPlayer = ActorCache.Create(this, Game.Save.Actor, Map.PlayerSpawn, Actor.PlayerTeam, isPlayer: true);
					Add(LocalPlayer);
				}
				else
				{
					foreach (var team in Game.Save.Shroud.Keys)
						ShroudLayer.RevealShroudList(team, Game.Save.Shroud[team]);

					foreach (var condition in Game.Save.CustomConditions)
						Game.ConditionManager.SetCondition(condition.Key, condition.Value);

					LocalPlayer = ActorLayer.ToAdd().First(a => a.IsPlayer);
				}

				if (LocalPlayer.Health != null && Game.Save.Health > 0)
					LocalPlayer.Health.RelativeHP = Game.Save.Health;

				if (Game.IsCampaign && !Game.IsMenu)
					AddText(LocalPlayer.Position, 300, ActionText.ActionTextType.TRANSFORM, $"Level {Game.Save.Level}");

				ShroudLayer.RevealAll = Program.DisableShroud || Game.MapType.RevealMap;
			}
			else
			{
				ShroudLayer.RevealAll = true;

				Camera.Position(Map.Center.ToCPos(), true);
			}

			// First tick, does only add objects, TODO replace with direct call to Tick()
			ActorLayer.Tick();
			WeaponLayer.Tick();
			ParticleLayer.Tick();
			addObjects();

			WorldRenderer.CheckVisibilityAll();
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
			ShroudLayer.Tick();

			WeatherManager.Tick();

			addObjects();
		}

		void addObjects()
		{
			Objects.RemoveAll(o => o.Disposed);

			if (objectsToAdd.Count != 0)
			{
				foreach (var @object in objectsToAdd)
				{
					@object.CheckVisibility();
					Objects.Add(@object);
				}
				objectsToAdd.Clear();
			}
		}

		public void TrophyCollected(string collected)
		{
			if (Game.Stats.TrophyUnlocked(collected))
				return;

			if (!TrophyCache.Trophies.ContainsKey(collected))
				throw new NullReferenceException($"Unable to get Trophy with internal name '{collected}'.");

			Game.AddInfoMessage(250, "Trophy collected!");
			Game.Stats.AddTrophy(collected);

			var trophy = TrophyCache.Trophies[collected];
			Game.Stats.MaxMana += trophy.MaxManaIncrease;
			Game.Stats.MaxLifes += trophy.MaxLifesIncrease;
		}

		public void FinishPlayerSwitch(Actor @new)
		{
			LocalPlayer.FollowupActor = @new;
			LocalPlayer = @new;
			Add(@new);

			CameraVisibility.ShroudUpdated();
		}

		public void BeginPlayerSwitch(ActorType to)
		{
			Game.SpellManager.CancelActive();

			var health = LocalPlayer.Health != null ? LocalPlayer.Health.RelativeHP : 1;
			var playablePart = LocalPlayer.GetPartOrDefault<PlayablePart>();

			if (playablePart.PlayerSwitchActor == null)
			{
				FinishPlayerSwitch(ActorCache.Create(this, to, LocalPlayer.Position, LocalPlayer.Team, isPlayer: true, health: health));
				LocalPlayer.Dispose();
				return;
			}

			var switchActor = ActorCache.Create(this, playablePart.PlayerSwitchActor, LocalPlayer.Position, LocalPlayer.Team, isPlayer: true);
			var switchPart = switchActor.GetPart<PlayerSwitchPart>();
			switchPart.RelativeHP = health;
			switchPart.ActorType = to;
			LocalPlayer.Dispose();

			LocalPlayer.FollowupActor = switchActor;
			LocalPlayer = switchActor;
			Add(switchActor);
		}

		public void PlayerKilled()
		{
			Game.Stats.Deaths++;
			Game.Save.IncreaseDeathCount();

			if (Game.Stats.Lifes == 0)
			{
				Game.DefeatConditionsMet();
				return;
			}

			Game.Stats.Lifes--;
			LocalPlayer = ActorCache.Create(this, Game.Save.Actor, Map.PlayerSpawn, Actor.PlayerTeam, isPlayer: true);
			Add(LocalPlayer);
		}

		public bool CheckCollision(SimplePhysics physics)
		{
			if (physics.IsEmpty)
				return false;

			var top = PhysicsLayer.Bounds.Y;
			var left = PhysicsLayer.Bounds.X;
			var bot = 0;
			var right = 0;
			foreach (var p in physics.Sectors)
			{
				if (p.CheckIntersection(physics))
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
				if (Collision.CheckCollision(physics, wall.Physics))
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
			PhysicsLayer.UpdateSectors(actor.Physics, true);
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

		public void AddText(CPos position, int duration, ActionText.ActionTextType type, params string[] text)
		{
			var @object = new ActionText(position, new CPos(0, -15, 30), duration, type, text);
			@object.ZOffset += 1024;

			Add(@object);
		}

		public Terrain TerrainAt(CPos pos)
		{
			return !IsInWorld(pos) ? null : TerrainAt(pos.ToMPos());
		}

		public Terrain TerrainAt(MPos pos)
		{
			if (pos.X < 0 || pos.Y < 0 || pos.X >= Map.Bounds.X || pos.Y >= Map.Bounds.Y)
				return null;

			return TerrainLayer.Terrain[pos.X, pos.Y];
		}

		public bool IsInWorld(CPos pos)
		{
			var bounds = Map.Bounds.ToCPos();

			return pos.X >= -512 && pos.X < bounds.X - 512 && pos.Y >= -512 && pos.Y < bounds.Y - 512;
		}

		public bool IsInWorld(Actor actor)
		{
			var pos = actor.Position;

			if (actor.Physics.IsEmpty)
				return IsInWorld(pos);

			var bounds = Map.Bounds.ToCPos();
			var X = actor.Physics.Boundaries.X;
			var Y = actor.Physics.Boundaries.Y;

			return pos.X >= -512 + X && pos.X < bounds.X - 512 - X && pos.Y >= -512 + Y && pos.Y < bounds.Y - 512 - Y;
		}

		public bool IsVisibleTo(Actor eye, Actor target)
		{
			if (eye.Team == target.Team)
				return true;

			return ShroudLayer.ShroudRevealed(eye.Team, (int)(target.Position.X / 512f), (int)(target.Position.Y / 512f));
		}

		public void Dispose()
		{
			foreach (var o in Objects)
				o.Dispose();
			Objects.Clear();

			ActorLayer.Dispose();
			ParticleLayer.Dispose();
			WeaponLayer.Dispose();

			WallLayer.Dispose();
			SmudgeLayer.Dispose();
		}
	}
}
