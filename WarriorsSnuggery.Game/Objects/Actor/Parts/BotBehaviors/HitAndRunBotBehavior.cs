﻿namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Bot-Behavior type that aims to reproduce a \"hit and run\" technique.")]
	public class HitAndRunBotBehaviorPartInfo : BotBehaviorPartInfo
	{
		public HitAndRunBotBehaviorPartInfo(PartInitSet set) : base(set) { }
	}

	public class HitAndRunBotBehaviorPart : BotBehaviorPart
	{
		[Save("Hide"), DefaultValue(0)]
		bool hide;
		[Save("HideDuration"), DefaultValue(0)]
		int hideDuration;

		public HitAndRunBotBehaviorPart(Actor self, HitAndRunBotBehaviorPartInfo info) : base(self, info) { }

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

			if (hide && hideDuration-- <= 0 && (Self.Weapon == null || Self.Weapon.Ready))
				hide = false;
			else if (CanAttack)
				DefaultAttackBehavior();

			if (CanMove)
			{
				if (hide)
					DefaultMoveBehavior(8f, 9f);
				else
					DefaultMoveBehavior();
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			base.OnDamage(damager, damage);

			hideDuration += damage;
			hide = true;
		}

		public override void OnKill(Actor killer)
		{
			hideDuration -= 10;
		}
	}
}

