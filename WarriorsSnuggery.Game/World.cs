using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
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
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Maps.Pieces;

namespace WarriorsSnuggery
{
	public sealed class World : ITick
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
		public readonly EffectLayer EffectLayer;

		public readonly WeatherManager WeatherManager;

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

			EffectLayer = new EffectLayer();

			WeatherManager = new WeatherManager(this, game.MapType);
		}

		public void Load()
		{
			Map.Load();

			if (Game.InteractionMode != InteractionMode.EDITOR)
			{
				if (!Map.Type.IsSave)
				{
					LocalPlayer = ActorCache.Create(this, Game.Save.Actor, Map.PlayerSpawn, Actor.PlayerTeam, isPlayer: true);
					if (LocalPlayer.Health != null && Game.Save.Health > 0)
						LocalPlayer.Health.RelativeHP = Game.Save.Health;

					LocalPlayer.OnLoad();
					Add(LocalPlayer);
				}
				else
				{
					LocalPlayer = ActorLayer.ToAdd().First(a => a.IsPlayer);
				}

				if (Game.MissionType.IsCampaign() && !Game.MissionType.IsMenu())
					AddText(LocalPlayer.Position, 300, ActionText.ActionTextType.TRANSFORM, $"Level {Game.Save.Level}");

				ShroudLayer.RevealAll = Program.DisableShroud || Game.MapType.RevealMap;

				Camera.Position(LocalPlayer.Position, true);
			}
			else
			{
				ShroudLayer.RevealAll = true;

				Camera.Position(Map.Center.ToCPos(), true);
			}

			TerrainLayer.CheckBorders();
		}

		public void Tick()
		{
			ActorLayer.Tick();
			ParticleLayer.Tick();
			WeaponLayer.Tick();

			TerrainLayer.Tick();
			SmudgeLayer.Tick();
			ShroudLayer.Tick();

			EffectLayer.Tick();

			WeatherManager.Tick();
		}

		public void TrophyCollected(string collected)
		{
			if (Game.Player.HasTrophyUnlocked(collected))
				return;

			if (!TrophyCache.Trophies.ContainsKey(collected))
				throw new NullReferenceException($"Unable to get Trophy with internal name '{collected}'.");

			Game.AddInfoMessage(250, "Trophy collected!");
			Game.Player.UnlockTrophy(collected);

			var trophy = TrophyCache.Trophies[collected];
			Game.Player.MaxMana += trophy.MaxManaIncrease;
			Game.Player.MaxLifes += trophy.MaxLifesIncrease;
		}

		public void FinishPlayerSwitch(Actor @new)
		{
			LocalPlayer.FollowupActor = @new;
			LocalPlayer = @new;
			Add(@new);
		}

		public void BeginPlayerSwitch(ActorType to)
		{
			Game.SpellManager.CancelActive();

			var health = LocalPlayer.Health != null ? LocalPlayer.Health.RelativeHP : 1;
			var playablePart = LocalPlayer.GetPartOrDefault<PlayablePart>();

			if (playablePart.PlayerSwitchActor == null)
			{
				var player = ActorCache.Create(this, to, LocalPlayer.Position, LocalPlayer.Team, isPlayer: true);
				if (player.Health != null)
					player.Health.RelativeHP = health;
				FinishPlayerSwitch(player);
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
			if (Game.Player.Lifes == 0)
			{
				Game.Player.Deaths++;
				Game.DefeatConditionsMet();
				return;
			}

			Game.Save.Player.IncreaseDeathCount();
			Game.Player.IncreaseDeathCount();

			LocalPlayer = ActorCache.Create(this, Game.Save.Actor, Map.PlayerSpawn, Actor.PlayerTeam, isPlayer: true);
			Add(LocalPlayer);
		}

		public bool CheckCollision(SimplePhysics physics, out Collision collision)
		{
			collision = null;
			if (physics.IsEmpty)
				return false;

			var top = PhysicsLayer.Bounds.Y;
			var left = PhysicsLayer.Bounds.X;
			var bot = 0;
			var right = 0;
			foreach (var p in physics.Sectors)
			{
				if (p.CheckIntersection(physics, out collision))
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
				if (Collision.CheckCollision(physics, wall.Physics, out collision))
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
			EnsureInBounds(actor);
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
			EffectLayer.Add(@object);
		}

		public void AddText(CPos position, int duration, ActionText.ActionTextType type, params string[] text)
		{
			var @object = new ActionText(new CPos(0, -8, 16), duration, type, text)
			{
				Position = position,
				ZOffset = 1024
			};

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

		public bool IsInPlayableWorld(CPos pos)
		{
			var offset = Map.PlayableOffset.ToCPos() + Map.Offset;
			var bounds = Map.PlayableBounds.ToCPos() + offset;

			return pos.X >= offset.X && pos.X < bounds.X && pos.Y >= offset.Y && pos.Y < bounds.Y;
		}

		public bool IsInWorld(CPos pos)
		{
			return pos.X >= Map.TopLeftCorner.X && pos.X < Map.BottomRightCorner.X && pos.Y >= Map.TopLeftCorner.Y && pos.Y < Map.BottomRightCorner.Y;
		}

		public bool IsInWorld(MPos pos)
		{
			return pos.X >= 0 && pos.X < Map.Bounds.X && pos.Y >= 0 && pos.Y < Map.Bounds.Y;
		}

		public bool IsInWorld(WPos pos)
		{
			return pos.X >= 0 && pos.X < WallLayer.Bounds.X && pos.Y >= 0 && pos.Y < WallLayer.Bounds.Y;
		}

		public bool IsInPlayableWorld(Actor actor)
		{
			var pos = actor.Position;

			if (actor.Physics.IsEmpty)
				return IsInPlayableWorld(pos);

			var offset = Map.PlayableOffset.ToCPos() + Map.Offset;
			var bounds = Map.PlayableBounds.ToCPos() + offset;
			var X = actor.Physics.Boundaries.X;
			var Y = actor.Physics.Boundaries.Y;

			return pos.X >= offset.X + X && pos.X < bounds.X - X && pos.Y >= offset.Y + Y && pos.Y < bounds.Y - Y;
		}

		public void EnsureInBounds(Actor actor)
		{
			var pos = actor.Position;
			var empty = actor.Physics.IsEmpty;

			var bounds = Map.Bounds.ToCPos();
			var offset = Map.Offset;
			var X = empty ? 0 : actor.Physics.Boundaries.X;
			var Y = empty ? 0 : actor.Physics.Boundaries.Y;

			var x = Math.Clamp(pos.X, offset.X + X, bounds.X + offset.X - X);
			var y = Math.Clamp(pos.Y, offset.Y + Y, bounds.Y + offset.Y - Y);
			actor.Position = new CPos(x, y, pos.Z);
		}

		public bool IsVisibleTo(Actor eye, Actor target)
		{
			if (eye.Team == target.Team)
				return true;

			return ShroudLayer.ShroudRevealed(eye.Team, (int)(target.Position.X / 512f), (int)(target.Position.Y / 512f));
		}

		public TextNodeSaver Save() => Save(PieceSaverType.EDITOR);
		public TextNodeSaver Save(PieceSaverType saverType)
		{
			var saver = new TextNodeSaver();
			if (saverType != PieceSaverType.DIFF)
				saver.Append(TerrainLayer.Save());
			if (saverType != PieceSaverType.DIFF)
				saver.AddChildren("Walls", WallLayer.Save(), true); // TODO: we need to check whether walls have been destroyed
			saver.AddChildren("Actors", ActorLayer.Save(saverType), true);

			if (saverType != PieceSaverType.EDITOR)
				saver.AddChildren("Weapons", WeaponLayer.Save(), true);
			if (saverType == PieceSaverType.SAVE)
				saver.AddChildren("Particles", ParticleLayer.Save(), true);
			if (saverType != PieceSaverType.SAVE)
				saver.AddChildren("Shroud", ShroudLayer.Save(), true);

			return saver;
		}

		public void ApplyDiff(GameDiffData data)
		{
			var piece = new GameDiffPiece(data.MapNodes);

			foreach (var actor in ActorLayer.Actors)
			{
				var init = piece.actorInits.FirstOrDefault(a => a.ID == actor.ID);
				if (init != null)
					actor.Position = init.Position;
			}

			foreach (var wall in WallLayer.WallList)
			{
				var init = piece.wallInits.FirstOrDefault(w => w.Position == wall.LayerPosition);
				if (init != null)
				{
					if (wall.Type.ID != init.TypeID)
						Console.Write("IDs differing. ");
					wall.Health = init.Health;
					Console.WriteLine($"{wall.LayerPosition}->{wall.Health}");
				}
			}
		}
	}
}
