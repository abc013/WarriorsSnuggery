using System;
using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to an actor to activate mobility features.")]
	public class MobilityPartInfo : PartInfo
	{
		[Desc("Speed of the Actor.")]
		public readonly int Speed;
		[Desc("Acceleration of the Actor. If 0, Speed is used.")]
		public readonly int Acceleration;
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
		}

		public override ActorPart Create(Actor self)
		{
			return new MobilityPart(self, this);
		}
	}

	public class MobilityPart : ActorPart, ITick, INoticeDispose, ISaveLoadable
	{
		readonly MobilityPartInfo info;
		readonly Sound sound;

		public CPos Force;
		public CPos Velocity;
		bool wasMoving;

		public bool CanFly => info.CanFly;

		public MobilityPart(Actor self, MobilityPartInfo info) : base(self)
		{
			this.info = info;
			if (info.Sound != null)
				sound = new Sound(info.Sound);
		}

		public void OnLoad(List<TextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == nameof(MobilityPart) && n.Value == info.InternalName);
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == nameof(Force))
					Force = node.Convert<CPos>();
				else if (node.Key == nameof(Velocity))
					Velocity = node.Convert<CPos>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, info.InternalName);

			saver.Add(nameof(Force), Force, CPos.Zero);
			saver.Add(nameof(Velocity), Velocity, CPos.Zero);

			return saver;
		}

		public void Tick()
		{
			if (self.Height > 0 && !CanFly)
				Force += info.Gravity;

			wasMoving = Velocity != CPos.Zero;
			if (wasMoving && (self.Height == 0 || CanFly))
			{
				var decFactor = Velocity.FlatDist / info.Speed;
				var deceleration = (int)-Math.Ceiling(decFactor * info.Acceleration);

				var decX = deceleration * Math.Sign(Velocity.X);
				var decY = deceleration * Math.Sign(Velocity.Y);
				var decZ = 0;

				if (Math.Sign(Velocity.X + decX) != Math.Sign(Velocity.X))
					decX = -Velocity.X;
				if (Math.Sign(Velocity.Y + decY) != Math.Sign(Velocity.Y))
					decY = -Velocity.Y;

				if (CanFly)
				{
					var heightDecFactor = Math.Abs(Velocity.Z) / 10f;
					var heightDeceleration = (int)-Math.Ceiling(heightDecFactor * info.HeightAcceleration);
					decZ = heightDeceleration * Math.Sign(Velocity.Z);

					if (Math.Sign(Velocity.Z + decZ) != Math.Sign(Velocity.Z))
						decZ = -Velocity.Z;
				}

				Force += new CPos(decX, decY, decZ);
			}

			Velocity += Force;
			Force = CPos.Zero;

			if (Velocity != CPos.Zero)
			{
				moveTick();

				if (!wasMoving)
					sound?.Play(self.Position, true, false);
			}
			else if (wasMoving)
				sound?.Stop();
		}

		void moveTick()
		{
			var speedModifier = 1f;
			if (self.Height == 0 && self.World.TerrainAt(self.Position) != null)
				speedModifier = self.World.TerrainAt(self.Position).Type.Speed;

			foreach (var effect in self.Effects.Where(e => e.Active && e.Effect.Type == Spells.EffectType.SPEED))
				speedModifier *= effect.Effect.Value;

			var currentVelocity = new CPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier), (int)Math.Round(Velocity.Z * speedModifier));

			if (currentVelocity == CPos.Zero)
				return;

			var height = self.Height + currentVelocity.Z;

			// Move only in z direction
			if (currentVelocity.X == 0 && currentVelocity.Y == 0 && checkMove(self.Position, height, Velocity))
				return;

			// Move in both x and y direction
			if (currentVelocity.X != 0 && currentVelocity.Y != 0)
			{
				var pos = self.Position + new CPos(currentVelocity.X, currentVelocity.Y, 0);
				if (checkMove(pos, height, Velocity))
					return;
			}

			// Move only in x direction
			if (currentVelocity.X != 0)
			{
				var posX = self.Position + new CPos(currentVelocity.X, 0, 0);
				if (checkMove(posX, height, new CPos(Velocity.X, 0, Velocity.Z)))
					return;
			}

			// Move only in y direction
			if (currentVelocity.Y != 0)
			{
				var posY = self.Position + new CPos(0, currentVelocity.Y, 0);
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

			sound?.SetPosition(self.Position);

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

		public int AccelerateSelf(float angle)
		{
			if (!CanFly && self.Height != 0)
				return 0;

			return Accelerate(angle, info.Acceleration);
		}

		public int Accelerate(float angle, int acceleration)
		{
			var x = (int)Math.Round(MathF.Cos(angle) * acceleration);
			var y = (int)Math.Round(MathF.Sin(angle) * acceleration);

			Force += new CPos(x, y, 0);

			return acceleration;
		}

		public int AccelerateHeightSelf(bool up)
		{
			if (!CanFly)
				return 0;

			return AccelerateHeight(up ? info.HeightAcceleration : -info.HeightAcceleration);
		}

		public int AccelerateHeight(int acceleration)
		{
			Force += new CPos(0, 0, acceleration);

			return acceleration;
		}

		public void OnDispose()
		{
			sound?.Stop();
		}
	}
}
