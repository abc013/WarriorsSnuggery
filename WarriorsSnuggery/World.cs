using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Weapons;

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

		public readonly List<Actor> Actors = new List<Actor>();
		public readonly List<PhysicsObject> Objects = new List<PhysicsObject>();
		public List<PhysicsObject> ToRender { get; private set; }

		readonly List<PhysicsObject> objectsToAdd = new List<PhysicsObject>();
		readonly List<Actor> actorsToAdd = new List<Actor>();

		public Actor LocalPlayer;
		public bool PlayerAlive = true;

		public World(Game game, int seed, GameStatistics stats)
		{
			Game = game;

			TerrainLayer = new TerrainLayer();
			WallLayer = new WallLayer();
			PhysicsLayer = new PhysicsLayer();
			ShroudLayer = new ShroudLayer();

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
					ShroudLayer.RevealShroudList(Actor.PlayerTeam, Game.Statistics.Shroud);
				}

				Camera.Position(LocalPlayer.Position, true);

				if (Game.Type == GameType.NORMAL)
					Add(new ActionText(LocalPlayer.Position + new CPos(0, 0, 1024), new CPos(0, -15, 30), 300, ActionText.ActionTextType.TRANSFORM, @"Level" + Game.Statistics.Level));
			}
			else
			{
				PlayerAlive = false;
				Camera.Position(new MPos(Map.Bounds.X / 2, Map.Bounds.Y / 2).ToCPos(), true);
			}
			//Add(new PhysicsObject(new CPos(0, 0, 1024), new ImageRenderable(TextureManager.NoiseTexture(
			//	new MPos(128, 128),
			//	6,
			//	scale: 1f,
			//	intensity: -0.1f,
			//	contrast: 5f
			//	), 1f)));
			//Add(new PhysicsObject(new CPos(0, 6000, 1024), new ImageRenderable(TextureManager.NoiseTexture(
			//	new MPos(128, 128),
			//	2,
			//	8f,
			//	1,
			//	intensity: -0.3f,
			//	contrast: 1.5f
			//	), 1f)));
			//Add(ActorCreator.Create(this, "heal", CPos.Zero, 1, true));
			if (actorsToAdd.Any())
				Game.Teams = actorsToAdd.Max(a => a.Team);
		}

		public void Tick()
		{
			if (LocalPlayer != null)
			{
				if (Camera.LockedToPlayer)
					Camera.Position(LocalPlayer.Position);

				foreach (var effect in LocalPlayer.Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.MANA))
				{
					Game.Statistics.Mana += (int)effect.Spell.Value;
				}
			}

			Actors.ForEach(a => a.Tick());
			Objects.ForEach(o => o.Tick());

			AddObjects();
		}

		public void AddObjects()
		{
			int removed = 0;
			removed += Actors.RemoveAll(a => a.Disposed);
			removed += Objects.RemoveAll(o => o.Disposed);

			if (actorsToAdd.Any())
			{
				foreach (var actor in actorsToAdd)
				{
					actor.CheckVisibility();
				}
				Actors.AddRange(actorsToAdd);
				actorsToAdd.Clear();
			}

			if (objectsToAdd.Any())
			{
				foreach (var @object in objectsToAdd)
				{
					@object.CheckVisibility();
				}
				Objects.AddRange(objectsToAdd);
				objectsToAdd.Clear();
			}

			var toRender = Objects.ToList(); // Copy array
			toRender.AddRange(Actors); // Add actors
			foreach (var wall in WallLayer.Walls)
				if (wall != null)
					toRender.Add(wall);
			toRender = toRender.OrderBy(e => (e.GraphicPosition.Z + (e.Position.Y - 512) * 2)).ToList();
			ToRender = toRender;
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

		public bool CheckCollision(PhysicsObject obj, bool ignoreHeight, PhysicsObject[] ignoreObjects = null)
		{
			if (obj.Physics == null || obj.Physics.RadiusX == 0 || obj.Physics.Shape == Physics.Shape.NONE)
				return false;

			foreach (var p in obj.PhysicsSectors)
				if (p.Check(obj, ignoreHeight, ignoreObjects))
					return true;

			foreach (var wall in WallLayer.Walls)
			{
				if (wall == null/* || obj.Physics == wall.Physics*/) continue;
					if (ignoreObjects != null && ignoreObjects.Contains(wall))
						continue;

				if (obj.Physics.Intersects(wall.Physics, ignoreHeight))
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
			if (position.X < 0 || position.Y < 0 || position.X > Map.Bounds.X || position.Y > Map.Bounds.Y)
				return null;

			return TerrainLayer.Terrain[position.X, position.Y];
		}

		public bool IsInWorld(CPos pos)
		{
			var size = Map.Bounds.ToCPos();

			return pos.X >= -512 && pos.X < size.X - 512 && pos.Y >= -512 && pos.Y < size.Y - 512;
		}

		public void Dispose()
		{
			foreach (Actor a in Actors)
				a.Dispose();
			Actors.Clear();

			foreach (PhysicsObject o in Objects)
				o.Dispose();
			Objects.Clear();

			TerrainLayer.Dispose();
			WallLayer.Dispose();
			ShroudLayer.Dispose();
		}
	}
}
