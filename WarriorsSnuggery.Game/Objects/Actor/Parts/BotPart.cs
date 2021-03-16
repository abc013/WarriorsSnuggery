using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.Objects.Bot;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Parts
{
	public class BotPart : ActorPart, ITick, INoticeDamage, INoticeKill
	{
		readonly BotBehavior bot;

		public Target Target
		{
			get => bot.Target;
			set => bot.Target = value;
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

		public override void OnLoad(List<MiniTextNode> nodes)
		{
			var position = self.Position;
			var height = self.Height;
			var targetID = uint.MaxValue;

			var parent = nodes.FirstOrDefault(n => n.Key == "BotPart");
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "TargetPosition")
					position = node.Convert<CPos>();
				if (node.Key == "TargetHeight")
					height = node.Convert<int>();
				if (node.Key == "TargetActor")
					targetID = node.Convert<uint>();
			}

			if (targetID == uint.MaxValue)
				Target = new Target(position, height);
			else
				Target = new Target(self.World.ActorLayer.ToAdd().Find(a => a.ID == targetID));
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this, string.Empty, true);

			if (Target == null)
				return saver;

			saver.Add("TargetPosition", Target.Position, self.Position);
			saver.Add("TargetHeight", Target.Height, self.Height);
			if (Target.Actor != null)
				saver.Add("TargetActor", Target.Actor.ID, uint.MaxValue);

			return saver;
		}

		public void Tick()
		{
			bot.Tick();
		}

		public void OnDamage(Actor damager, int damage)
		{
			bot.OnDamage(damager, damage);
		}

		public void OnKill(Actor killer)
		{
			bot.OnKill(killer);
		}
	}
}
