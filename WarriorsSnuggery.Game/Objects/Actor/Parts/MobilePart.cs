using System;
using WarriorsSnuggery.Audio.Sound;
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
	}

	public class MobilePart : ActorPart, ITick, INoticeDispose, ISaveLoadable
	{
		readonly MobilePartInfo info;
		readonly Sound sound;

		public CPos Force { get; private set; }
		public CPos Velocity { get; private set; }
		bool wasMoving;

		public bool CanFly => info.CanFly;

		public MobilePart(Actor self, MobilePartInfo info) : base(self, info)
		{
			this.info = info;
			if (info.Sound != null)
				sound = new Sound(info.Sound);
		}

		public void OnLoad(PartLoader loader)
		{
			foreach (var node in loader.GetNodes(typeof(MobilePart), Specification))
			{
				if (node.Key == nameof(Force))
					Force = node.Convert<CPos>();
				else if (node.Key == nameof(Velocity))
					Velocity = node.Convert<CPos>();
			}
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, Specification);

			saver.Add(nameof(Force), Force, CPos.Zero);
			saver.Add(nameof(Velocity), Velocity, CPos.Zero);

			return saver;
		}

		public void Tick()
		{
			if (!Self.OnGround && !CanFly)
				Force += info.Gravity;

			wasMoving = Velocity != CPos.Zero;

			if (wasMoving)
			{
				var friction = Self.OnGround ? info.Friction : info.AirFriction;
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
					sound?.Play(Self.Position, true, false);
			}
			else if (wasMoving)
			{
				sound?.Stop();

				Self.StopMove();
			}
		}

		void moveTick()
		{
			if (Self.Disposed) // Prevent readding physics sectors if already disposed
				return;

			var speedModifier = 1f;
			if (Self.OnGround && Self.CurrentTerrain != null)
				speedModifier = Self.CurrentTerrain.Type.Speed;

			foreach (var effect in Self.GetActiveEffects(EffectType.SPEED))
				speedModifier *= effect.Effect.Value;

			var currentVelocity = new CPos((int)Math.Round(Velocity.X * speedModifier), (int)Math.Round(Velocity.Y * speedModifier), (int)Math.Round(Velocity.Z * speedModifier));

			if (currentVelocity == CPos.Zero)
				return;

			// Move only in z direction
			if (currentVelocity.X == 0 && currentVelocity.Y == 0 && checkMove(Self.Position + new CPos(0, 0, currentVelocity.Z), Velocity))
				return;

			// Move in all directions
			if (currentVelocity.X != 0 && currentVelocity.Y != 0)
			{
				var pos = Self.Position + currentVelocity;
				if (checkMove(pos, Velocity))
					return;
			}

			// Move only in x,z direction
			if (currentVelocity.X != 0)
			{
				var posX = Self.Position + new CPos(currentVelocity.X, 0, currentVelocity.Z);
				if (checkMove(posX, new CPos(Velocity.X, 0, Velocity.Z)))
					return;
			}

			// Move only in y,z direction
			if (currentVelocity.Y != 0)
			{
				var posY = Self.Position + new CPos(0, currentVelocity.Y, currentVelocity.Z);
				if (checkMove(posY, new CPos(0, Velocity.Y, Velocity.Z)))
					return;
			}

			denyMove();
		}

		bool checkMove(CPos pos, CPos velocity, bool evading = false)
		{
			if (!Self.World.IsInPlayableWorld(pos))
				return false;

			var oldPos = Self.Position;

			Self.Position = pos;

			var intersects = Self.World.CheckCollision(Self.Physics, out var collision);

			Self.Position = oldPos;

			if (intersects)
			{
				if (collision == null || evading)
					return false;

				var newVelocity = CPos.FromFlatAngle(collision.Angle, velocity.FlatDist);
				if (newVelocity == CPos.Zero)
					return false;

				return checkMove(Self.Position + newVelocity, newVelocity, true);
			}

			var terrain = Self.World.TerrainAt(pos);
			if (terrain != null && pos.Z == 0 && terrain.Type.Speed == 0)
				return false;

			acceptMove(pos, evading);
			Velocity = velocity;

			return true;
		}

		void acceptMove(CPos position, bool evading = false)
		{
			var old = Self.Position;
			Self.Position = position;
			Self.World.EnsureInBounds(Self);

			if (!evading)
				Self.Angle = (old - position).FlatAngle;
			Self.World.PhysicsLayer.UpdateSectors(Self.Physics);
			Self.World.ActorLayer.Update(Self);

			Self.Move(old);

			sound?.SetPosition(Self.Position);

			// Sustain movement for at least one tick
			Self.AddAction(ActionType.MOVE, 1);
		}

		void denyMove()
		{
			Velocity = CPos.Zero;

			Self.StopMove();
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
