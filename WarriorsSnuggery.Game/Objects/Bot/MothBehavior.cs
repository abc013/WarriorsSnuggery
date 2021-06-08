using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Bot
{
	public class MothBotBehavior : BotBehavior
	{
		int tick;

		public MothBotBehavior(World world, Actor self) : base(world, self) { }

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!HasGoodTarget)
			{
				if (tick++ % 20 == 0)
					Target = new Target(randomPosition(), 0);

				DefaultTickBehavior();
				return;
			}

			if (CanAttack)
				DefaultAttackBehavior();

			if (CanMove)
				DefaultMoveBehavior();
		}

		CPos randomPosition()
		{
			var x = Program.SharedRandom.Next(2048) - 1024;
			var y = Program.SharedRandom.Next(2048) - 1024;

			return Self.Position + new CPos(x, y, 0);
		}

		public override void OnKill(Actor killer) { }
	}
}
