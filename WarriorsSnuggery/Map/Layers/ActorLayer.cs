using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery
{
	public sealed class ActorLayer
	{
		public const int SectorSize = 4;
		public readonly List<Actor> Actors = new List<Actor>();

		readonly List<Actor> actorsToRemove = new List<Actor>();
		readonly List<Actor> actorsToAdd = new List<Actor>();

		readonly ActorSector[,] sectors;
		readonly MPos bounds;

		bool firstTick = true;

		public ActorLayer(MPos bounds)
		{
			this.bounds = new MPos((int)Math.Ceiling(bounds.X / (float)SectorSize), (int)Math.Ceiling(bounds.Y / (float)SectorSize));
			
			sectors = new ActorSector[this.bounds.X, this.bounds.Y];
			for (var x = 0; x < this.bounds.X; x++)
				for (var y = 0; y < this.bounds.Y; y++)
					sectors[x, y] = new ActorSector(new MPos(x, y));
		}

		public void Add(Actor actor)
		{
			actor.CheckVisibility();
			actorsToAdd.Add(actor);
		}

		public void Update(Actor actor, bool first = false)
		{
			var oldSector = actor.Sector;
			var newSector = getSector(actor);

			if (oldSector != newSector)
			{
				if (!first)
					oldSector.Leave(actor);
				newSector.Enter(actor);
			}

			actor.Sector = newSector;
		}

		ActorSector getSector(Actor actor)
		{
			var position = actor.Position - Map.Offset;
			var x = (int)Math.Floor(position.X / 4096f);
			var y = (int)Math.Floor(position.Y / 4096f);

			return sectors[x, y];
		}

		public void Remove(Actor actor)
		{
			actorsToRemove.Add(actor);
		}

		public void Tick()
		{
			if (actorsToAdd.Any())
			{
				foreach (var actor in actorsToAdd)
				{
					Actors.Add(actor);
					Update(actor, true);
				}
				actorsToAdd.Clear();
			}

			if (!firstTick)
			{
				foreach (var actor in Actors)
					actor.Tick();
			}
			firstTick = false;

			if (actorsToRemove.Any())
			{
				foreach (var actor in actorsToRemove)
				{
					Actors.Remove(actor);
					actor.Sector.Leave(actor);
				}
				actorsToRemove.Clear();
			}
		}

		public void Clear()
		{
			foreach (var actor in Actors)
				actor.Dispose();
			Actors.Clear();
		}
	}

	public class ActorSector
	{
		public readonly MPos Position;

		readonly List<Actor> actors = new List<Actor>();

		public ActorSector(MPos position)
		{
			Position = position;
		}

		public void Enter(Actor actor)
		{
			actors.Add(actor);
		}

		public void Leave(Actor actor)
		{
			actors.Remove(actor);
		}
	}
}
