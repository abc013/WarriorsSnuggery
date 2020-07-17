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
		public readonly List<Actor> NonNeutralActors = new List<Actor>();
		public readonly List<Actor> VisibleActors = new List<Actor>();

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

			if (actor.CheckVisibility() && !VisibleActors.Contains(actor))
				VisibleActors.Add(actor);
			
			actor.Sector = newSector;
		}

		ActorSector getSector(Actor actor)
		{
			var position = actor.Position - Map.Offset;
			var x = (int)Math.Floor(position.X / 4096f);
			var y = (int)Math.Floor(position.Y / 4096f);
			x = Math.Clamp(x, 0, bounds.X - 1);
			y = Math.Clamp(y, 0, bounds.Y - 1);

			return sectors[x, y];
		}

		public ActorSector[] GetSectors(CPos position, int radius)
		{
			var topleft = position - new CPos(radius, radius, 0) - Map.Offset;
			var botright = position + new CPos(radius, radius, 0) - Map.Offset;

			return getSectors(topleft, botright);
		}

		ActorSector[] getSectors(CPos topleft, CPos botright)
		{
			var pos1 = new MPos((int)Math.Clamp(Math.Floor(topleft.X / 4096f), 0, bounds.X - 1), (int)Math.Clamp(Math.Floor(topleft.Y / 4096f), 0, bounds.Y - 1));
			var pos2 = new MPos((int)Math.Clamp(Math.Ceiling(botright.X / 4096f), 0, bounds.X - 1), (int)Math.Clamp(Math.Ceiling(botright.Y / 4096f), 0, bounds.Y - 1));

			var sectors = new ActorSector[(pos2.X - pos1.X + 1) * (pos2.Y - pos1.Y + 1)];
			var i = 0;
			for (var x = pos1.X; x < pos2.X + 1; x++)
				for (var y = pos1.Y; y < pos2.Y + 1; y++)
					sectors[i++] = this.sectors[x, y];

			return sectors;
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
					if (actor.Team != Actor.NeutralTeam)
						NonNeutralActors.Add(actor);
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
					if (actor.Team != Actor.NeutralTeam)
						NonNeutralActors.Remove(actor);
					if (actor.CheckVisibility())
						VisibleActors.Remove(actor);
					actor.Sector.Leave(actor);
				}
				actorsToRemove.Clear();
			}
		}

		public void CheckVisibility()
		{
			foreach (var a in Actors)
				a.CheckVisibility();
			VisibleActors.Clear();
			VisibleActors.AddRange(Actors);
		}

		public void CheckVisibility(CPos topleft, CPos bottomright)
		{
			VisibleActors.Clear();
			var sectors = getSectors(topleft, bottomright);

			foreach (var sector in sectors)
			{
				foreach (var a in sector.Actors)
				{
					if (a.CheckVisibility())
						VisibleActors.Add(a);
				}
			}
		}

		public void Clear()
		{
			foreach (var actor in Actors)
				actor.Dispose();
			Actors.Clear();
			NonNeutralActors.Clear();
		}
	}

	public class ActorSector
	{
		public readonly MPos Position;

		public readonly List<Actor> Actors = new List<Actor>();

		public ActorSector(MPos position)
		{
			Position = position;
		}

		public void Enter(Actor actor)
		{
			Actors.Add(actor);
		}

		public void Leave(Actor actor)
		{
			Actors.Remove(actor);
		}
	}
}
