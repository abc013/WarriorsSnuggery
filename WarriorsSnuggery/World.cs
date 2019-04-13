using System;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public class World : ITick, IDisposable
	{
		public readonly TerrainLayer TerrainLayer;
		public readonly WallLayer WallLayer;
		public readonly PhysicsLayer PhysicsLayer;

		public readonly List<Actor> Actors = new List<Actor>();
		public readonly List<GameObject> Objects = new List<GameObject>();
		public List<GameObject> ToRender;

		readonly List<GameObject> objectsToAdd = new List<GameObject>();
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
				var start = Map.PlayerStart != new CPos(-1024,-1024,0) ? Map.PlayerStart : new MPos(Map.Size.X / 2, Map.Size.Y / 2).ToCPos();

				LocalPlayer = ActorCreator.Create(this, Game.Statistics.Actor, start, 2, isPlayer: true);
				Add(LocalPlayer);

				Camera.Position(LocalPlayer.Position, true);
				if (Game.Type == GameType.NORMAL)
					Add(new ActionText(LocalPlayer.Position + new CPos(0,0,1024), @"Level" + Game.Statistics.Level, Color.White, IFont.Papyrus24, new CPos(0, -15, 30), 300));
			}
			else
			{
				PlayerAlive = false;
				Camera.Position(new MPos(Map.Size.X / 2, Map.Size.Y / 2).ToCPos(), true);
			}

			Add(new GameObject(new CPos(0, 0, 1024), new ImageRenderable(TextureManager.NoiseTexture(
				new MPos(128, 128),
				6,
				intensity: -0.1f,
				contrast: 5f
				), 1f)));
			Add(new GameObject(new CPos(0, 6000, 1024), new ImageRenderable(TextureManager.NoiseTexture(
				new MPos(128, 128),
				2,
				1,
				intensity: -0.3f,
				contrast: 1.5f
				), 1f)));
			// WallLayer.Set(WallCreator.Create(new WPos(4,6,0), 0));
			// Add(new Trigger(new CPos(1024, 1024, 100), this, 768, (Actor a) => { Game.End = true; }, true, true, 60, renderable: new ColoredCircleRenderable(Color.Black, 1f, 45, WarriorsSnuggery.Graphics.DrawMethod.LINELOOP)));
		}

		public void Tick()
		{

			if (Camera.LockedToPlayer && LocalPlayer != null)
				Camera.Position(LocalPlayer.Position);

			internalTick();

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

			var toRender= Objects.ToList(); // Copy array
			toRender.AddRange(Actors); // Add actors
			foreach(var wall in WallLayer.Walls)
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

				var selected = Actors.Find(a => a.Position.GetDistToXY(MouseInput.GamePosition) < 512);
				if (selected != null)
				{
					Selected = selected;
					Add(new ActionText(selected.Position, selected.Health + " HP", Color.Cyan, IFont.Pixel16, new CPos(0, -15, 30)));
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

		public bool CheckCollision(GameObject obj, bool ignoreHeight, Type[] ignoreTypes = null, GameObject[] ignoreObjects = null)
		{
			if (obj.Physics == null || obj.Physics.Radius == 0 || obj.Physics.Shape == Shape.NONE)
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

		public void Add(GameObject @object)
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

			foreach(GameObject o in Objects)
				o.Dispose();
			Objects.Clear();

			TerrainLayer.Dispose();
			WallLayer.Dispose();
		}
	}
}
