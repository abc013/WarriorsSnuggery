using System;
using System.Linq;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Attach this to an actor to activate mobility features.")]
	public class MobilityPartInfo : PartInfo
	{
		[Desc("Speed of the Actor.")]
		public readonly int Speed;
		[Desc("Acceleration of the Actor.")]
		public readonly int Acceleration;
		[Desc("Deceleration of the Actor.")]
		public readonly int Deceleration;
		[Desc("Gravity. Unused.")]
		public readonly int Gravity;

		public override ActorPart Create(Actor self)
		{
			return new MobilityPart(self, this);
		}

		public MobilityPartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Speed":
						Speed = node.Convert<int>();
						break;
					case "Acceleration":
						Acceleration = node.Convert<int>();
						break;
					case "Deceleration":
						Deceleration = node.Convert<int>();
						break;
					case "Gravity":
						Gravity = node.Convert<int>();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "MobilityPart");
				}
			}
		}
	}
	
	public class MobilityPart : ActorPart
	{
		readonly MobilityPartInfo info;

		public CPos Velocity;

		public MobilityPart(Actor self, MobilityPartInfo info) : base(self)
		{
			this.info = info;
		}

		public new void Tick()
		{
			if (Velocity != CPos.Zero)
			{
				if (self.World.Game.Type == GameType.EDITOR)
					return;

				var signX = Math.Sign(Velocity.X);
				var signY = Math.Sign(Velocity.Y);
				Velocity -= new CPos(info.Deceleration * signX, info.Deceleration * signY, 0);

				if (Math.Sign(Velocity.X) != signX)
					Velocity = new CPos(0, Velocity.Y, 0);
				if (Math.Sign(Velocity.Y) != signY)
					Velocity = new CPos(Velocity.X, 0, 0);
			}
		}

		public new int OnAccelerate(float angle, int customAcceleration)
		{
			var speedFactor = 1f;
			foreach(var effect in self.Effects.Where(e => e.Active && e.Effect.Type == EffectType.SPEED))
			{
				speedFactor *= effect.Effect.Value;
			}

			var acceleration = customAcceleration == 0 ? info.Acceleration * 2 : customAcceleration;
			var x = (int)Math.Round(Math.Cos(angle) * acceleration);
			var y = (int)Math.Round(Math.Sin(angle) * acceleration);

			Velocity += new CPos(x, y, 0);
			if (Math.Abs(Velocity.X) >= info.Speed * speedFactor)
				Velocity = new CPos((int) (Math.Sign(Velocity.X) * info.Speed * speedFactor), Velocity.Y, 0);
			if (Math.Abs(Velocity.Y) >= info.Speed)
				Velocity = new CPos(Velocity.X, (int) (Math.Sign(Velocity.Y) * info.Speed * speedFactor), 0);

			return acceleration;
		}

		CPos randomPosition()
		{
			var size = self.Physics != null ? self.Physics.RadiusX : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0);
		}
	}
}
