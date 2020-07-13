﻿using System;
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

		public readonly List<Actor> Actors = new List<Actor>();
		public readonly List<Weapon> Weapons = new List<Weapon>();
		public readonly List<Particle> Particles = new List<Particle>();
		public readonly List<PhysicsObject> Objects = new List<PhysicsObject>();
		public List<PhysicsObject> ToRender { get; private set; }

		readonly List<PhysicsObject> objectsToAdd = new List<PhysicsObject>();
		readonly List<Actor> actorsToAdd = new List<Actor>();

		public Actor LocalPlayer;

		public bool PlayerSwitching { get { return LocalPlayer.IsPlayerSwitch; } }
		public bool PlayerAlive = true;

		public int PlayerDamagedTick = 0;
		public bool KeyFound;

		public World(Game game, int seed, GameStatistics stats)
		{
			Game = game;

			TerrainLayer = new TerrainLayer();
			WallLayer = new WallLayer();
			PhysicsLayer = new PhysicsLayer();
			ShroudLayer = new ShroudLayer();
			SmudgeLayer = new SmudgeLayer();

			Map = new Map(this, game.MapType, seed, stats.Level, stats.Difficulty);
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

				Camera.Position(LocalPlayer.GraphicPosition + (Game.ScreenControl.Focused is UI.DefaultScreen ? Camera.CamPlayerOffset : CPos.Zero), true);

				if (Game.Type == GameType.NORMAL)
					Add(new ActionText(LocalPlayer.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 300, ActionText.ActionTextType.TRANSFORM, @"Level" + Game.Statistics.Level));
			}
			else
			{
				PlayerAlive = false;
				Camera.Position(new MPos(Map.Bounds.X / 2, Map.Bounds.Y / 2).ToCPos(), true);
			}

			if (actorsToAdd.Any())
				Game.Teams = actorsToAdd.Max(a => a.Team);

			addObjects();
		}

		public void SetBounds(MPos bounds)
		{
			TerrainLayer.SetBounds(bounds);
			WallLayer.SetBounds(bounds);
			PhysicsLayer.SetBounds(bounds);
			ShroudLayer.SetBounds(bounds);
			ShroudLayer.RevealAll = Game.Type != GameType.NORMAL && Game.Type != GameType.TEST;

			VisibilitySolver.SetBounds(bounds, ShroudLayer);
		}

		public void Tick()
		{
			if (LocalPlayer != null && !Game.Editor && Camera.LockedToPlayer)
				Camera.Position(LocalPlayer.GraphicPosition + (Game.ScreenControl.Focused is UI.DefaultScreen ? Camera.CamPlayerOffset : CPos.Zero));

			foreach (var actor in Actors)
				actor.Tick();
			foreach (var @object in Objects)
				@object.Tick();
			foreach (var particle in Particles)
				particle.Tick();
			foreach (var weapon in Weapons)
				weapon.Tick();

			TerrainLayer.Tick();
			SmudgeLayer.Tick();

			addObjects();
		}

		void addObjects()
		{
			int removed = 0;
			removed += Actors.RemoveAll(a => a.Disposed);
			removed += Weapons.RemoveAll(o => o.Disposed);
			removed += Particles.RemoveAll(o => o.Disposed);
			removed += Objects.RemoveAll(o => o.Disposed);

			if (actorsToAdd.Any())
			{
				foreach (var actor in actorsToAdd)
				{
					actor.CheckVisibility();
					Actors.Add(actor);
				}
				actorsToAdd.Clear();
			}

			if (objectsToAdd.Any())
			{
				foreach (var @object in objectsToAdd)
				{
					@object.CheckVisibility();

					if (@object is Particle)
						Particles.Add((Particle)@object);
					else if (@object is Weapon)
						Weapons.Add((Weapon)@object);
					else
						Objects.Add(@object);
				}
				objectsToAdd.Clear();
			}

			ToRender = Objects.ToList(); // Copy array
			ToRender.AddRange(Actors);
			ToRender.AddRange(Weapons);
			ToRender.AddRange(Particles);
			ToRender.AddRange(WallLayer.WallList);

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

		public bool CheckCollision(PhysicsObject obj, Actor[] toIgnore = null)
		{
			if (obj.Physics == null || obj.Physics.RadiusX == 0 || obj.Physics.Shape == Physics.Shape.NONE)
				return false;

			foreach (var p in obj.PhysicsSectors)
			{
				if (p.Check(obj, toIgnore))
					return true;
			}
			
			foreach (var wall in WallLayer.WallList)
			{
				if (obj.Physics.Intersects(wall.Physics))
					return true;
			}

			return false;
		}

		public void Add(PhysicsObject @object)
		{
			if (@object == null)
				return;

			if (@object is Actor)
				actorsToAdd.Add(@object as Actor);
			else
				objectsToAdd.Add(@object);

			// Make sure that no weapons are added to the sectors, as they will not influence any movement
			if (!(@object is Weapon))
				PhysicsLayer.UpdateSectors(@object, true);
		}

		public void Add(PhysicsObject[] objects)
		{
			foreach (var obj in objects)
				Add(obj);
		}

		public List<Actor> GetActorsToAdd()
		{
			return actorsToAdd;
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

		public void Dispose()
		{
			foreach (var a in Actors)
				a.Dispose();
			Actors.Clear();

			foreach (var o in Objects)
				o.Dispose();
			Objects.Clear();

			foreach (var p in Particles)
				p.Dispose();
			Particles.Clear();

			foreach (var w in Weapons)
				w.Dispose();
			Weapons.Clear();

			TerrainLayer.Dispose();
			WallLayer.Dispose();
			ShroudLayer.Dispose();
			SmudgeLayer.Dispose();
		}
	}
}
