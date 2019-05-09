using System;

namespace WarriorsSnuggery.Objects.Parts
{
	public class MobilityPartInfo : PartInfo
	{
		public readonly int Speed;
		public readonly int Acceleration;
		public readonly int Deceleration;
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
						Speed = node.ToInt();
						break;
					case "Acceleration":
						Acceleration = node.ToInt();
						break;
					case "Deceleration":
						Deceleration = node.ToInt();
						break;
					case "Gravity":
						Gravity = node.ToInt();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "MobilityPart");
				}
			}
		}
	}

	/// <summary>
	/// Activates mobility features.
	/// </summary>
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
			var acceleration = customAcceleration == 0 ? info.Acceleration * 2 : customAcceleration;
			var x = (int)Math.Round(Math.Cos((angle * Math.PI) / 180) * acceleration);
			var y = (int)Math.Round(Math.Sin((angle * Math.PI) / 180) * acceleration);

			Velocity += new CPos(x, y, 0);
			if (Math.Abs(Velocity.X) >= info.Speed)
				Velocity = new CPos(Math.Sign(Velocity.X) * info.Speed, Velocity.Y, 0);
			if (Math.Abs(Velocity.Y) >= info.Speed)
				Velocity = new CPos(Velocity.X, Math.Sign(Velocity.Y) * info.Speed, 0);

			return acceleration;
		}

		CPos randomPosition()
		{
			var size = self.Physics != null ? self.Physics.Radius : 40;
			var x = Program.SharedRandom.Next(size) - size / 2;
			var y = Program.SharedRandom.Next(size) - size / 2;
			return self.Position + new CPos(x, y, 0);
		}
	}
}
