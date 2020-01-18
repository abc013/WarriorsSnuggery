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
			set { bot.GiveTarget(value); }
		}

		public BotPart(Actor self) : base(self)
		{
			bot = new NormalBotBehavior(self.World, self);
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
