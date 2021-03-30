using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor to activate mobility features.")]
	public class MobilityPartInfo : PartInfo
	{
		[Desc("Speed of the Actor.")]
		public readonly int Speed;
		[Desc("Acceleration of the Actor. If 0, Speed is used.")]
		public readonly int Acceleration;
		[Desc("Deceleration of the Actor. If 0, Speed is used.")]
		public readonly int Deceleration;
		[Desc("Acceleration to use for the vertical axis.")]
		public readonly int HeightAcceleration;
		[Desc("Actor may also control velocity while in air.")]
		public readonly bool CanFly;
		[Desc("Gravity to apply while flying.", "Gravity will not be applied when the actor can fly.")]
		public readonly CPos Gravity = new CPos(0, 0, -9);
		[Desc("Sound to be played while moving.")]
		public readonly SoundType Sound;

		public MobilityPartInfo(PartInitSet set) : base(set)
		{
			if (Acceleration == 0)
				Acceleration = Speed;
			if (Deceleration == 0)
				Deceleration = Speed;
		}

		public override ActorPart Create(Actor self)
		{
			return new MobilityPart(self, this);
		}
	}

	public class MobilityPart : ActorPart, ITick
	{
		readonly MobilityPartInfo info;

		public CPos Force;
		public CPos Velocity;
		public CPos oldVelocity;
		public Sound sound;

		public bool CanFly => info.CanFly;

		public MobilityPart(Actor self, MobilityPartInfo info) : base(self)
		{
			this.info = info;
			if (info.Sound != null)
				sound = new Sound(info.Sound);
		}

		public override void OnLoad(List<TextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "MobilityPart" && n.Value == info.InternalName);
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "Force")
					Force = node.Convert<CPos>();
				if (node.Key == "Velocity")
					Velocity = node.Convert<CPos>();
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add("Force", Force, CPos.Zero);
			saver.Add("Velocity", Velocity, CPos.Zero);

			return saver;
		}

		public void Tick()
		{
			if (Velocity != CPos.Zero)
			{
				moveTick();

				if (self.Height == 0 || CanFly)
				{
					var signX = Math.Sign(Velocity.X);
					var signY = Math.Sign(Velocity.Y);
					var signZ = Math.Sign(Velocity.Z);
					Velocity -= new CPos(info.Deceleration * signX, info.Deceleration * signY, info.Deceleration * signZ);

					if (Math.Sign(Velocity.X) != signX)
						Velocity = new CPos(0, Velocity.Y, Velocity.Z);
					if (Math.Sign(Velocity.Y) != signY)
						Velocity = new CPos(Velocity.X, 0, Velocity.Z);
					if (Math.Sign(Velocity.Z) != signZ)
						Velocity = new CPos(Velocity.X, Velocity.Y, 0);
				}

				if (oldVelocity == CPos.Zero)
					sound?.Play(self.Position, true, false);
			}
			else if (oldVelocity == CPos.Zero)
				sound?.Stop();
			oldVelocity = Velocity;

			if (self.Height > 0 && !CanFly)
				Force += info.Gravity;

			Velocity += Force;
			Force = CPos.Zero;

			var speedFactor = 1f;
			foreach (var effect in self.Effects.Where(e => e.Active && e.Effect.Type == Spells.EffectType.SPEED))
				speedFactor *= effect.Effect.Value;

			var maxSpeed = speedFactor * info.Speed;
			if (Math.Abs(Velocity.X) > maxSpeed)
				Velocity = new CPos((int)maxSpeed * Math.Sign(Velocity.X), Velocity.Y, Velocity.Z);
			if (Math.Abs(Velocity.Y) > maxSpeed)
				Velocity = new CPos(Velocity.X, (int)maxSpeed * Math.Sign(Velocity.Y), Velocity.Z);
			if (Math.Abs(Velocity.Z) > maxSpeed)
				Velocity = new CPos(Velocity.X, Velocity.Y, (int)maxSpeed * Math.Sign(Velocity.Z));

			sound?.SetPosition(self.Position);
		}

		void moveTick()
		{
			var speedModifier = 1f;
			if (self.Height == 0 && self.World.TerrainAt(self.Position) != null)
				speedModifier = self.World.TerrainAt(self.Position).Type.Speed;

			if (speedModifier <= 0.01f)
				return;

			var movement = new CPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier), (int)Math.Round(Velocity.Z * speedModifier));
			if (movement == CPos.Zero)
				return;

			var height = self.Height + movement.Z;

			// Move only in z direction
			if (movement.X == 0 && movement.Y == 0 && checkMove(self.Position, height, Velocity))
				return;

			// Move in both x and y direction
			if (movement.X != 0 && movement.Y != 0)
			{
				var pos = self.Position + new CPos(movement.X, movement.Y, 0);
				if (checkMove(pos, height, Velocity))
					return;
			}

			// Move only in x direction
			if (movement.X != 0)
			{
				var posX = self.Position + new CPos(movement.X, 0, 0);
				if (checkMove(posX, height, new CPos(Velocity.X, 0, Velocity.Z)))
					return;
			}

			// Move only in y direction
			if (movement.Y != 0)
			{
				var posY = self.Position + new CPos(0, movement.Y, 0);
				if (checkMove(posY, height, new CPos(0, Velocity.Y, Velocity.Z)))
					return;
			}

			denyMove();
		}

		bool checkMove(CPos pos, int height, CPos velocity)
		{
			if (!self.World.IsInWorld(pos))
				return false;

			var oldPos = self.Position;
			var oldHeight = self.Height;

			self.Height = height;
			self.Position = pos;

			var intersects = self.World.CheckCollision(self.Physics);

			self.Position = oldPos;
			self.Height = oldHeight;

			if (intersects)
				return false;

			var terrain = self.World.TerrainAt(pos);
			if (terrain != null && height == 0 && terrain.Type.Speed == 0)
				return false;

			acceptMove(pos, height, terrain);
			Velocity = velocity;

			return true;
		}

		void acceptMove(CPos position, int height, Terrain terrain)
		{
			var old = self.Position;
			self.Height = height;
			self.Position = position;
			self.TerrainPosition = self.Position.ToMPos();
			self.CurrentTerrain = terrain;

			self.Angle = (old - position).FlatAngle;
			self.World.PhysicsLayer.UpdateSectors(self.Physics);
			self.World.ActorLayer.Update(self);

			self.Move(old);

			if (self.CurrentAction.Type == ActionType.MOVE)
				self.CurrentAction.ExtendAction(1);
			else
				self.SetAction(new ActorAction(ActionType.MOVE, true, 1));
		}

		void denyMove()
		{
			Velocity = CPos.Zero;

			self.CurrentAction = ActorAction.Default;

			self.StopMove();
		}

		public int OnAccelerate(float angle, int customAcceleration)
		{
			var acceleration = customAcceleration == 0 ? info.Acceleration : customAcceleration;
			var x = (int)Math.Round(MathF.Cos(angle) * acceleration);
			var y = (int)Math.Round(MathF.Sin(angle) * acceleration);

			Force += new CPos(x * 2, y * 2, 0);

			return acceleration;
		}

		public int OnAccelerateHeight(bool up, int customAcceleration)
		{
			var acceleration = customAcceleration == 0 ? info.Acceleration : customAcceleration;

			if (!up)
				acceleration *= -1;

			Force += new CPos(0, 0, acceleration * 2);

			return acceleration;
		}
	}
}
