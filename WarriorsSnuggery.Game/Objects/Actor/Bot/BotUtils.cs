using System;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Objects.Weapons.Projectiles;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	internal static class BotUtils
	{
		internal static bool IsGoodTarget(Target target)
        {
            if (target == null || target.Type != TargetType.ACTOR)
                return false;

            return target.Actor.IsAlive && !target.Actor.Disposed;
        }

		internal static Actor GetNeighborActor(Actor self, bool sameTeam = true, int range = 1536)
		{
			var sectors = self.World.ActorLayer.GetSectors(self.Position, range);
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor == self || (sameTeam && actor.Team != self.Team))
						continue;

					var dist = self.Position - actor.Position;
					if (dist.SquaredFlatDist < range * range)
						return actor;
				}
			}

			return null;
		}

		internal static void PredictiveAttack(Actor self, Target target)
		{
			if (!self.CanAttack)
				return;

			if (target.Actor == null || !target.Actor.CanMove || target.Actor.Mobile.Velocity == CPos.Zero)
			{
				self.PrepareAttack(target);
				return;
			}

			var projectileType = self.Weapon.Type.Projectile;
			if (projectileType is not BulletProjectile and not SplashProjectile)
			{
				self.PrepareAttack(target);
				return;
			}

			var delta = target.Position - self.Position;
			var deltaMagnitude = delta.FlatDist;
			var velTarget = target.Actor.Mobile.Velocity;
			var velBullet = projectileType is BulletProjectile projectile ? projectile.MaxSpeed : ((SplashProjectile)projectileType).DistancePerTick;

			// See http://danikgames.com/blog/how-to-intersect-a-moving-target-in-2d/ for more information
			// uj, ui: vectors for target velocity in projected space
			// vj, vi: vectors for bullet direction in projected space

			// Find the vector AB, normalize
			var ABx = delta.X / deltaMagnitude;
			var ABy = delta.Y / deltaMagnitude;

			// Project velTarget onto AB
			var uDotAB = ABx * velTarget.X + ABy * velTarget.Y;
			var ujx = uDotAB * ABx;
			var ujy = uDotAB * ABy;

			// Subtract uj from velTarget to get ui
			var uix = velTarget.X - ujx;
			var uiy = velTarget.Y - ujy;

			// Set vi to ui (for clarity)
			var vix = uix;
			var viy = uiy;

			if (velTarget.X < vix && velTarget.Y < viy)
				return;

			// Calculate the magnitude of vj
			var viMag = MathF.Sqrt(vix * vix + viy * viy);
			var vjMag = MathF.Sqrt(velBullet * velBullet - viMag * viMag);

			// Get vj by multiplying it's magnitude with the unit vector AB
			var vjx = ABx * vjMag;
			var vjy = ABy * vjMag;

			// Add vj and vi to get direction
			var direction = new CPos((int)(vjx + vix), (int)(vjy + viy), 0);

			var t = Math.Abs(delta.X / (float)(velTarget.X - direction.X));

			var newTarget = new Target(new CPos(self.Position.X + (int)(direction.X * t), self.Position.Y + (int)(direction.Y * t), target.Position.Z));

			self.PrepareAttack(newTarget);
		}
	}
}
