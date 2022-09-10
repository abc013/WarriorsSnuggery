using System;
using System.Collections.Generic;
using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Maps
{
	public static class ActorDistribution
	{
		public static List<Actor> DistributeAround(World world, CPos position, List<ActorType> types, int margin = 128, byte team = 0, bool isBot = false)
		{
			var area = 0;
			foreach (var type in types)
			{
				var physics = type.Physics;
				if (physics == null)
					continue;

				var radius = Math.Max(physics.Type.Boundaries.X, physics.Type.Boundaries.Y) + margin;
				area += radius * radius;
			}

			var totalRadius = (int)MathF.Sqrt(area);

			return DistributeAround(world, position, totalRadius, types, team, isBot);
		}

		public static List<Actor> DistributeAround(World world, CPos position, int radius, List<ActorType> types, byte team = 0, bool isBot = false)
		{
			var actors = new List<Actor>();

			var sectors = world.ActorLayer.GetSectors(position, radius);

			CPos randomPosition()
			{
				var x = world.Game.SharedRandom.Next(2 * radius) - radius;
				var y = world.Game.SharedRandom.Next(2 * radius) - radius;

				return new CPos(x, y, 0) + position;
			}

			// bruteforce our way, it works remarkably well
			const int maxAttempts = 20;

			foreach (var type in types)
			{
				for (int attempt = 0; attempt < maxAttempts; attempt++)
				{
					var localPosition = randomPosition();

					if (!world.IsInPlayableWorld(localPosition))
						continue;

					if (type.Physics == null || attemptPlacement(world, type, localPosition, actors, sectors))
					{
						var actor = ActorCache.Create(world, type, localPosition, team, isBot);
						actors.Add(actor);
						world.Add(actor);
						break;
					}
				}
			}

			return actors;
		}

		static bool attemptPlacement(World world, ActorType type, CPos position, List<Actor> toAdd, Layers.ActorSector[] sectors)
		{
			var physics = new SimplePhysics(new PositionableObject() { Position = position }, type.Physics.Type);
			world.PhysicsLayer.SetSectors(physics);

			if (world.CheckCollision(physics, out _))
				return false;

			if (world.TerrainAt(position).Type.Speed == 0)
				return false;

			foreach (var actor in toAdd)
			{
				if (Collision.CheckCollision(physics, actor.Physics, out _))
					return false;
			}

			return true;
		}
	}
}
