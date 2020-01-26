using System;

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
				self.Accelerate((float)Math.PI * 1.5f);
			if (KeyInput.IsKeyDown(Settings.Key("MoveDown")))
				self.Accelerate((float)Math.PI * 0.5f);
			if (KeyInput.IsKeyDown(Settings.Key("MoveRight")))
				self.Accelerate(0);
			if (KeyInput.IsKeyDown(Settings.Key("MoveLeft")))
				self.Accelerate((float)Math.PI);

			if (KeyInput.IsKeyDown(OpenTK.Input.Key.AltLeft))
			{
				if (KeyInput.IsKeyDown(Settings.Key("MoveAbove")))
					self.AccelerateHeight(true);
				if (KeyInput.IsKeyDown(Settings.Key("MoveBelow")))
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
		}

		void attackTarget(CPos pos)
		{
			if (KeyInput.IsKeyDown(OpenTK.Input.Key.ShiftLeft, 0))
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
