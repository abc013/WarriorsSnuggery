using WarriorsSnuggery.Objects.Actors.Bot;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to the actor type to activate bot functions.")]
	public class BotPartInfo : PartInfo
	{
		[Require, Desc("Behavior of the bot.")]
		public readonly BotBehaviorType Behavior;

		public BotPartInfo(PartInitSet set) : base(set) { }
	}

	public class BotPart : ActorPart, ITick, INoticeDamage, INoticeKill, INoticeKilled, ISaveLoadable
	{
		readonly BotBehavior bot;

		// Saved separately
		public Target Target
		{
			get => bot.Target;
			set => bot.Target = value;
		}

		// TODO: save
		public Patrol Patrol
		{
			get => bot.Patrol;
			set => bot.Patrol = value;
		}

		public BotPart(Actor self, BotPartInfo info) : base(self, info)
		{
			bot = info.Behavior.Create(self);
		}

		public void OnLoad(PartLoader loader)
		{
			if (loader.ContainsRule(nameof(Target)))
				Target = new Target(loader.MakeInitializerWith(nameof(Target)), Self.World);
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this);
			if (Target != null)
				saver.AddChildren(nameof(Target), Target.Save());

			return saver;
		}

		public void Tick()
		{
			bot.Tick();
		}

		public bool CheckTarget(Target target)
		{
			return bot.CheckTarget(target);
		}

		public void OnDamage(Actor damager, int damage)
		{
			bot.OnDamage(damager, damage);
		}

		public void OnKill(Actor killed)
		{
			bot.OnKill(killed);
		}

		public void OnKilled(Actor killer)
		{
			Patrol?.ActorDied(Self);
		}
	}
}
