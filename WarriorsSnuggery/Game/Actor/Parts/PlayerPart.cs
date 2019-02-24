using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarriorsSnuggery.Objects.Parts
{
	/// <summary>
	/// Controls player input.
	/// </summary>
	class PlayerPart : ActorPart
	{
		public PlayerPart(Actor self) : base(self)
		{

		}

		public override void Tick()
		{
			if (KeyInput.IsKeyDown(Settings.Key("MoveUp")))
			{
				self.Accelerate(270);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveDown")))
			{
				self.Accelerate(90);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveRight")))
			{
				self.Accelerate(0);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveLeft")))
			{
				self.Accelerate(180);
			}

			//TODO: you know SUPERPOWERS? then do them properly.
			if (self.Type.ManaWeapon != null && self.World.Game.Statistics.Mana >= self.Type.ManaCost && KeyInput.IsKeyDown("E", 5))
			{
				self.World.Game.Statistics.Mana -= self.Type.ManaCost;
				var weapon = WeaponCreator.Create(self.World, self.Type.ManaWeapon, self, MouseInput.GamePosition);
				weapon.Position = MouseInput.GamePosition;
				self.World.Add(weapon);
				weapon.Detonate();
			}

			if (MouseInput.isLeftDown)
				self.Attack(MouseInput.GamePosition);
		}

		public override void OnKilled(Actor killer)
		{
			self.World.PlayerKilled(killer);
		}

		public override void OnKill(Actor killed)
		{
			self.World.Game.Statistics.Kills++;
		}
	}
}
