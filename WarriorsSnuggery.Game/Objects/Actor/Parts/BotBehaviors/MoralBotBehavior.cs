using System;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Bot-Behavior type uses a moral system to determine its strategy.")]
	public class MoralBotBehaviorPartInfo : BotBehaviorPartInfo
	{
		public MoralBotBehaviorPartInfo(PartInitSet set) : base(set) { }
	}

	public class MoralBotBehaviorPart : BotBehaviorPart
	{
		int moral
		{
			get => moralVal;
			set => moralVal = Math.Clamp(value, -50, 50);
		}
		[Save("MoralValue"), DefaultValue(50)]
		int moralVal;

		public MoralBotBehaviorPart(Actor self, MoralBotBehaviorPartInfo info) : base(self, info) { }
		
		public override void OnLoad(PartLoader loader)
		{
			base.OnLoad(loader);
			loader.SetSaveFields(this, false);
		}

		public override PartSaver OnSave()
		{
			var saver = base.OnSave();
			saver.AddSaveFields(this, false);

			return saver;
		}

		public override void Tick()
		{
			if (!CanMove && !CanAttack)
				return;

			if (!HasGoodTarget)
			{
				DefaultTickBehavior();
				return;
			}

			DefaultAttackBehavior();
			DefaultMoveBehavior(moral < 0 ? 0.8f : 0.3f, moral < 0 ? 1.0f : 0.8f);

			moral++;
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			moral -= 5;
		}

		public override void OnKill(Actor killed)
		{
			base.OnKill(killed);
			moral += 50;
		}
	}
}
