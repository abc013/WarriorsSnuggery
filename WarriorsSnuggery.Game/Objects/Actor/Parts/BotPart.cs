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

		public Target Target
		{
			get => bot.Target;
			set => bot.Target = value;
		}

		public Patrol Patrol
		{
			get => bot.Patrol;
			set => bot.Patrol = value;
		}

		public BotPart(Actor self, BotPartInfo info) : base(self)
		{
			bot = info.Behavior.Create(self);
		}

		public void OnLoad(PartLoader loader)
		{
			var position = self.Position;
			var targetID = uint.MaxValue;

			foreach (var node in loader.GetNodes(typeof(BotPart)))
			{
				if (node.Key == "TargetPosition")
					position = node.Convert<CPos>();
				if (node.Key == "TargetActor")
					targetID = node.Convert<uint>();
			}

			if (targetID != uint.MaxValue)
				Target = new Target(self.World.ActorLayer.ToAdd().Find(a => a.ID == targetID));
		}

		public PartSaver OnSave()
		{
			var saver = new PartSaver(this, string.Empty);

			if (Target == null)
				return saver;

			saver.Add("TargetPosition", Target.Position, self.Position);
			if (Target.Actor != null)
				saver.Add("TargetActor", Target.Actor.ID, uint.MaxValue);

			return saver;
		}

		public void Tick()
		{
			bot.Tick();
		}

		public void CheckTarget(Target target)
		{
			bot.CheckTarget(target);
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
			Patrol?.ActorDied(self);
		}
	}
}
