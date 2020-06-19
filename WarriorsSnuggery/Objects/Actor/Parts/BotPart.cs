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
			bot = type switch
			{
				BotBehaviorType.HIDE_AND_SEEK => new HideAndSeekBotBehavior(self.World, self),
				BotBehaviorType.MOTH => new MothBotBehavior(self.World, self),
				BotBehaviorType.PANIC => new PanicBotBehavior(self.World, self),
				_ => new NormalBotBehavior(self.World, self),
			};
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
