/*
 * User: Andreas
 * Date: 01.10.2017
 * 
 */
using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public sealed class World : ITick, IDisposable
	{
		public readonly TerrainLayer TerrainLayer;
		public readonly WallLayer WallLayer;
		public readonly PhysicsLayer PhysicsLayer;

		public readonly List<Actor> Actors = new List<Actor>();
		public readonly List<PhysicsObject> Objects = new List<PhysicsObject>();
		public List<PhysicsObject> ToRender { get; private set; }

		readonly List<PhysicsObject> objectsToAdd = new List<PhysicsObject>();
		readonly List<Actor> actorsToAdd = new List<Actor>();

		public readonly Map Map;
		public readonly Game Game;

		public Actor LocalPlayer;
		public bool PlayerAlive = true;

		public Actor Selected;

		public World(Game game, int seed, int level, int difficulty)
		{
			Game = game;

			TerrainLayer = new TerrainLayer();
			WallLayer = new WallLayer();
			PhysicsLayer = new PhysicsLayer();

			Map = new Map(this, game.MapType, seed, level, difficulty);
		}

		public void Load()
		{
			Map.Load();

			if (Game.Type != GameType.EDITOR)
			{
				if (!Map.Type.FromSave)
				{
					var start = Map.PlayerSpawn != new CPos(-1024, -1024, 0) ? Map.PlayerSpawn : new MPos(Map.Size.X / 2, Map.Size.Y / 2).ToCPos();

					LocalPlayer = ActorCreator.Create(this, Game.Statistics.Actor, start, Actor.PlayerTeam, isPlayer: true);
					Add(LocalPlayer);
				}

				Camera.Position(LocalPlayer.Position, true);

				if (Game.Type == GameType.NORMAL)
					Add(new ActionText(LocalPlayer.Position + new CPos(0,0,1024), IFont.Papyrus24, new CPos(0, -15, 30), 300, @"Level" + Game.Statistics.Level));
			}
			else
			{
				PlayerAlive = false;
				Camera.Position(new MPos(Map.Size.X / 2, Map.Size.Y / 2).ToCPos(), true);
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
		}

		public void Tick()
		{
			if (LocalPlayer != null)
			{
				if (Camera.LockedToPlayer)
					Camera.Position(LocalPlayer.Position);

				foreach(var effect in LocalPlayer.Effects.Where(e => e.Active && e.Effect.Type == EffectType.MANA))
				{
					Game.Statistics.Mana += (int) effect.Effect.Value;
				}
			}

			internalTick();

			AddObjects();
		}

		public void AddObjects()
		{
			int removed = 0;
			removed += Actors.RemoveAll(a => a.Disposed);
			removed += Objects.RemoveAll(o => o.Disposed);

			if (actorsToAdd.Any())
			{
				Actors.AddRange(actorsToAdd);
				actorsToAdd.Clear();
			}

			if (objectsToAdd.Any())
			{
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

		int healthdisplaycooldown;
		void internalTick()
		{
			Actors.ForEach(a => a.Tick());
			Objects.ForEach(o => o.Tick());

			healthdisplaycooldown--;
			if (MouseInput.isMiddleDown && healthdisplaycooldown <= 0)
			{
				Add(ParticleCreator.Create("glitter", MouseInput.GamePosition));

				var selected = Actors.Find(a => a.Position.DistToXY(MouseInput.GamePosition) < 512);
				if (selected != null)
				{
					Selected = selected;
					Add(new ActionText(selected.Position, IFont.Pixel16, new CPos(0, -15, 30), 100, Color.Cyan + "" +selected.Health + " HP"));
					healthdisplaycooldown = 30;
				}
			}
		}

		public void PlayerKilled(Actor killer)
		{
			if (PlayerAlive)
			{
				PlayerAlive = false;
				Game.Statistics.Deaths++;
				Game.Statistics.Money = 0;

				Game.Pause();
				Game.ChangeScreen(UI.ScreenType.FAILURE);
			}
		}

		public bool CheckCollision(PhysicsObject obj, bool ignoreHeight, Type[] ignoreTypes = null, PhysicsObject[] ignoreObjects = null)
		{
			if (obj.Physics == null || obj.Physics.RadiusX == 0 || obj.Physics.Shape == Shape.NONE)
				return false;

			foreach(var p in obj.PhysicsSectors)
				if (p.Check(obj, ignoreHeight, ignoreTypes, ignoreObjects))
					return true;

			if (ignoreTypes == null || !ignoreTypes.Contains(typeof(Wall)))
			{
				foreach(var wall in WallLayer.Walls)
				{
					if (wall == null/* || obj.Physics == wall.Physics*/) continue;

					if(ignoreObjects != null && ignoreObjects.Contains(wall))
						continue;

					if (obj.Physics.Intersects(wall.Physics, ignoreHeight))
						return true;
				}
			}

			return false;
		}

		public void Add(PhysicsObject @object)
		{
			if (@object == null)
				return;

			if(@object as Actor != null)
				actorsToAdd.Add(@object as Actor);
			else
				objectsToAdd.Add(@object);

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
			if (position.X < 0 || position.Y < 0 || position.X > Map.Size.X || position.Y > Map.Size.Y)
				return null;

			return TerrainLayer.Terrain[position.X, position.Y];
		}

		public bool IsInWorld(CPos pos)
		{
			var size = Map.Size.ToCPos();

			return pos.X >= -512 && pos.X < size.X - 512 && pos.Y >= -512 && pos.Y < size.Y - 512;
		}

		public void Dispose()
		{
			foreach(Actor a in Actors)
				a.Dispose();
			Actors.Clear();

			foreach(PhysicsObject o in Objects)
				o.Dispose();
			Objects.Clear();

			TerrainLayer.Dispose();
			WallLayer.Dispose();
		}
	}
}
