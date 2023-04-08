﻿using System;
using System.Collections.Generic;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Bot
{
	public abstract class BotBehaviorType
	{
		public readonly string InternalName;

		protected BotBehaviorType(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public abstract BotBehavior Create(Actor self);
	}

	public abstract class BotBehavior
	{
		protected const int SearchIntervall = 20;

		protected readonly World World;
		protected readonly Actor Self;

		public Target Target
		{
			get => target;
			set
			{
				target = value;
				if (IsLeader)
					Patrol?.SetNewTarget(value);

				if (target == null)
				{
					Waypoints.Clear();

					return;
				}

				if (CanMove)
					calculatePathToTarget();
			}
		}
		Target target;
		CPos pathfindingTarget;

		protected float DistToTarget => (Target.Position - Self.Position).FlatDist;
		protected float AngleToTarget => (Self.Position - Target.Position).FlatAngle;

		protected virtual bool HasGoodTarget => BotUtils.IsGoodTarget(Target);

		protected readonly Queue<CPos> Waypoints = new Queue<CPos>();

		public Patrol Patrol;
		protected bool IsLeader => Patrol == null || Patrol.Leader == Self;

		protected bool CanMove => Self.Mobile != null;
		protected bool CanAttack => Self.Weapon != null;

		protected BotBehavior(Actor self)
		{
			Self = self;
			World = self.World;
		}

		public abstract void Tick();

		public void DefaultTickBehavior()
		{
			SearchTarget();
			DefaultMoveBehavior();
		}

		public void DefaultAttackBehavior()
		{
			if (!Self.CanAttack || Target == null)
				return;

			Self.Weapon.Target = Target;
			int range = Self.Weapon.Type.MaxRange;
			if (DistToTarget < range * 1.1f)
			{
				BotUtils.PredictiveAttack(Self, Target);
				return;
			}

			if (!CanMove)
				Target = null; // Discard target if out of range
		}

		public void DefaultMoveBehavior(float rangeA = 0.5f, float rangeB = 0.9f)
		{
			if (!Self.CanMove || Target == null)
				return;

			var range = 5120;
			if (CanAttack)
				range = Math.Max(range, Self.Weapon.Type.MaxRange);
			else if (Self.RevealsShroud != null)
				range = Math.Max(range, Self.RevealsShroud.Range * 512);

			if (target.Type == TargetType.POSITION || DistToTarget > range)
			{
				var squaredDist = (target.Position - pathfindingTarget).SquaredFlatDist;
				if (target.Type == TargetType.ACTOR && squaredDist > range * range * 4) // Target has moved too far
				{
					Target = null;
					return;
				}

				if (target.Type == TargetType.ACTOR && squaredDist > range * range)
					calculatePathToTarget(); // Target has moved

				if (Waypoints.Count > 0)
				{
					var nearest = Waypoints.Peek();

					var diff = Self.Position - nearest;

					Self.AccelerateSelf(diff.FlatAngle);

					const int maxDistToTarget = 256;
					if (diff.SquaredFlatDist <= maxDistToTarget * maxDistToTarget)
						Waypoints.Dequeue();

					return;
				}
			}

			var actor = BotUtils.GetNeighborActor(Self);
			float angle = actor != null ? (Self.Position - actor.Position).FlatAngle : AngleToTarget;

			if (DistToTarget < range * rangeA)
				Self.AccelerateSelf(-angle);
			else if (DistToTarget > range * rangeB)
				Self.AccelerateSelf(angle);
		}

		void calculatePathToTarget()
		{
			pathfindingTarget = target.Position;

			Waypoints.Clear();

			var path = Self.World.PathfinderLayer.CalculatePath(Self.TerrainPosition, target.Position.ToMPos(), Self.Mobile.CanFly);

			foreach (var waypoint in path)
				Waypoints.Enqueue(waypoint.ToCPos());
		}

		public virtual void OnDamage(Actor damager, int damage)
		{
			if (damager != null)
				CheckTarget(new Target(damager.Position));
		}

		public virtual void OnKill(Actor killed)
		{
			if (Target.Type == TargetType.POSITION)
				return;

			if (killed == Target.Actor)
				SearchTarget();
		}

		protected virtual void SearchTarget()
		{
			if (World.Game.LocalTick % SearchIntervall != 0)
				return;

			var range = (Self.RevealsShroud == null ? 5 : Self.RevealsShroud.Range / 2) * 1024;

			// Find all possible targets in range, find the best target
			foreach (var sector in World.ActorLayer.GetSectors(Self.Position, range))
			{
				foreach (var actor in sector.Actors)
				{
					if (!World.IsVisibleTo(Self, actor))
						continue;

					if ((actor.Position - Self.Position).SquaredFlatDist > range * range)
						continue;

					if (CheckTarget(actor))
						return; // Found target, cancel and return
				}
			}
		}

		public virtual bool CheckTarget(Target target)
		{
			if (!IsLeader)
				return Patrol.LeaderCheckTarget(target);

			if (target.Actor != null)
				return CheckTarget(target.Actor);

			if (HasGoodTarget)
				return false;

			Target = target;
			return true;
		}

		protected virtual bool CheckTarget(Actor actor)
		{
			if (!IsLeader)
				return Patrol.LeaderCheckTarget(new Target(actor));

			if (actor.Team == Self.Team || actor.Team == Actor.NeutralTeam)
				return false;

			if (actor.Health == null || !actor.IsAlive)
				return false;

			if (!HasGoodTarget)
			{
				Target = new Target(actor);
				return true;
			}
			
			// Factor: Player. from 0 to 1
			// If current target is player, then keep attacking it
			if (Target.Actor.IsPlayer)
				return false;

			// Factor: Health. from 0 to 1
			// If current target has less health, then keep attacking it
			if (Target.Actor.Health.RelativeHP < actor.Health.RelativeHP)
				return false;

			Target = new Target(actor);
			return true;
		}
	}
}
