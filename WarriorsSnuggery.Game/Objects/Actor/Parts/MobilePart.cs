using System;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to an actor to activate mobility features.")]
	public class MobilePartInfo : PartInfo
	{
		[Desc("Friction when being on ground.")]
		public readonly float Friction = 0.5f;
		[Desc("Friction when being in air.")]
		public readonly float AirFriction = 0.05f;
		[Desc("Actor can stay in air.", "When setting this to true, Gravity will not be used.")]
		public readonly bool CanFly;
		[Desc("Gravity to apply while in air.", "Gravity will not be applied when CanFly is the to true.")]
		public readonly CPos Gravity = new CPos(0, 0, -9);
		[Desc("Sound to be played while moving.")]
		public readonly SoundType Sound;

		public MobilePartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new MobilePart(self, this);
		}
	}

	public class MobilePart : ActorPart, ITick, INoticeDispose, ISaveLoadable
	{
		readonly MobilePartInfo info;
		readonly Sound sound;

		public CPos Force { get; private set; }
		public CPos Velocity { get; private set; }
		bool wasMoving;

		public bool CanFly => info.CanFly;

		public MobilePart(Actor self, MobilePartInfo info) : base(self)
		{
			this.info = info;
			if (info.Sound != null)
				sound = new Sound(info.Sound);
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(MobilePart), info.InternalName))
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

			if (wasMoving)
			{
				var friction = self.Height == 0 ? info.Friction : info.AirFriction;
				var x = Velocity.X > 0 ? (int)Math.Ceiling(Velocity.X * friction) : (int)Math.Floor(Velocity.X * friction);
				var y = Velocity.Y > 0 ? (int)Math.Ceiling(Velocity.Y * friction) : (int)Math.Floor(Velocity.Y * friction);
				var z = Velocity.Z > 0 ? (int)Math.Ceiling(Velocity.Z * friction) : (int)Math.Floor(Velocity.Z * friction);
				Force -= new CPos(x, y, z);
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
			{
				sound?.Stop();

				self.StopMove();
			}
		}

		void moveTick()
		{
			var speedModifier = 1f;
			if (self.Height == 0 && self.World.TerrainAt(self.Position) != null)
				speedModifier = self.World.TerrainAt(self.Position).Type.Speed;

			foreach (var effect in self.GetEffects(EffectType.SPEED))
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
			self.CurrentTerrain = terrain;

			self.Angle = (old - position).FlatAngle;
			self.World.PhysicsLayer.UpdateSectors(self.Physics);
			self.World.ActorLayer.Update(self);

			self.Move(old);

			sound?.SetPosition(self.Position);

			// Sustain movement for at least one tick
			self.AddAction(ActionType.MOVE, 1);
		}

		void denyMove()
		{
			Velocity = CPos.Zero;

			self.StopMove();
		}

		public int Accelerate(float angle, int acceleration)
		{
			var x = (int)Math.Round(MathF.Cos(angle) * acceleration);
			var y = (int)Math.Round(MathF.Sin(angle) * acceleration);

			Force += new CPos(x, y, 0);

			return acceleration;
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
