using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Objects.Parts
{
	class EffectPart : ActorPart
	{
		readonly Effect effect;
		bool firstTick = true;
		bool isReady;
		int Cooldown;
		//bool isActive;
		int Duration;

		public EffectPart(Actor self, Effect effect) : base(self)
		{
			this.effect = effect;
		}

		public override void Tick()
		{
			if (!isReady)
			{
				if (Cooldown-- <= 0)
					isReady = true;
			}

			if (Duration > 0)
			{
				Duration--;
			}
			else
			{
				//isActive = false;
			}

			if (effect.Occurence == EffectOccurTypes.ALWAYS)
				TakeEffect();

			if (firstTick && effect.Occurence == EffectOccurTypes.FIRST)
				TakeEffect();

			firstTick = false;
		}

		public override void OnAttack(CPos target, Weapon weapon)
		{
			if (effect.Occurence == EffectOccurTypes.ON_ATTACK)
				TakeEffect();

			//if (isActive && effect.Type == EffectType.INACCURACY)
			//{
			//	var dist = target - weapon.Target;
			//	var x = (int)(dist.X * effect.Value);
			//	var y = (int)(dist.Y * effect.Value);
			//	var z = (int)(dist.Z * effect.Value);
			//	weapon.Target = target + new CPos(x, y, z);
			//}
		}

		public override void OnMove(CPos old, CPos speed)
		{
			if (effect.Occurence == EffectOccurTypes.ON_MOVE)
				TakeEffect();

			//if (isActive && effect.Type == EffectType.SPEED)
			//{
			//	var x = (int)(self.Mobility.Velocity.X * effect.Value);
			//	var y = (int)(self.Mobility.Velocity.Y * effect.Value);
			//	var z = (int)(self.Mobility.Velocity.Z * effect.Value);
			//	self.Mobility.Velocity = new CPos(x, y, z);
			//}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			if (effect.Occurence == EffectOccurTypes.ON_DAMAGE)
				TakeEffect();

			//if (isActive && effect.Type == EffectType.SHIELD)
			//{
			//	if (effect.Value - damage > 0)
			//		self.Health.HP += damage;
			//}
		}

		public override void OnKilled(Actor killer)
		{
			if (effect.Occurence == EffectOccurTypes.ON_KILLED)
				TakeEffect();
		}

		public void TakeEffect()
		{
			if (!isReady)
				return;

			isReady = false;
			Cooldown = effect.Cooldown;
			//isActive = true;
			Duration = effect.Duration;

			//Console.WriteLine(effect.Type + "-Effect has happened! Duration: " + Duration / 30f);
		}
	}
}
