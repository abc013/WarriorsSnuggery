using OpenToolkit.Windowing.Common.Input;
using System;

namespace WarriorsSnuggery.Objects.Parts
{
	/// <summary>
	/// Controls player input.
	/// </summary>
	class PlayerPart : ActorPart
	{
		public PlayerPart(Actor self) : base(self) { }

		public override void Tick()
		{
			var vertical = 0;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveUp")))
				vertical += 1;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveDown")))
				vertical -= 1;

			var horizontal = 0;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveRight")))
				horizontal += 1;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveLeft")))
				horizontal -= 1;

			if (vertical != 0)
				self.Accelerate((2 + vertical) * 0.5f * (float)Math.PI);
			if (horizontal != 0)
				self.Accelerate((3 + horizontal) * 0.5f * (float)Math.PI);

			if (KeyInput.IsKeyDown(Key.AltLeft))
			{
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveAbove")))
					self.AccelerateHeight(true);
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveBelow")))
					self.AccelerateHeight(false);
			}

			if (self.ActiveWeapon != null)
			{
				var actor = self.World.Game.FindValidTarget(MouseInput.GamePosition, self.Team);

				if (actor == null)
				{
					self.ActiveWeapon.Target = MouseInput.GamePosition;
					self.ActiveWeapon.TargetHeight = 0;
				}
				else
				{
					self.ActiveWeapon.Target = actor.Position;
					self.ActiveWeapon.TargetHeight = actor.Height;
				}
			}

			if (MouseInput.IsLeftDown && !self.World.Game.ScreenControl.CursorOnUI())
				attackTarget(MouseInput.GamePosition);

			self.World.PlayerDamagedTick++;
		}

		void attackTarget(CPos pos)
		{
			if (KeyInput.IsKeyDown(Key.ShiftLeft, 0))
			{
				self.Attack(pos, 0);
			}
			else
			{
				var actor = self.World.Game.FindValidTarget(pos, self.Team);

				if (actor == null)
					self.Attack(pos, 0);
				else
					self.Attack(actor);
			}
		}

		public override void OnDamage(Actor damager, int damage)
		{
			self.World.PlayerDamagedTick = 0;
		}

		public override void OnKilled(Actor killer)
		{
			self.World.PlayerKilled();
		}

		public override void OnKill(Actor killed)
		{
			self.World.Game.Statistics.Kills++;
		}
	}
}
