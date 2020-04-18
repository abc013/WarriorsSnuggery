using WarriorsSnuggery.Objects.Bot;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Parts
{
	public class BotPart : ActorPart
	{
		readonly BotBehavior bot;

		public Target Target
		{
			get { return bot.Target; }
			set { bot.Target = value; }
		}

		public BotPart(Actor self, BotBehaviorType type) : base(self)
		{
			switch (type)
			{
				case BotBehaviorType.HIDE_AND_SEEK:
					bot = new HideAndSeekBotBehavior(self.World, self);
					break;
				case BotBehaviorType.MOTH:
					bot = new MothBotBehavior(self.World, self);
					break;
				case BotBehaviorType.PANIC:
					bot = new PanicBotBehavior(self.World, self);
					break;
				default:
					bot = new NormalBotBehavior(self.World, self);
					break;
			}
		}

		public override void Tick()
		{
			bot.Tick();
		}

		public override void OnDamage(Actor damager, int damage)
		{
			bot.OnDamage(damager, damage);
		}

		public override void OnKill(Actor killer)
		{
			bot.OnKill(killer);
		}
	}
}
