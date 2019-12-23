using System;
using System.Linq;

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

			if (self.ActiveWeapon != null)
			{
				if (KeyInput.IsKeyDown(OpenTK.Input.Key.ShiftLeft, 0))
				{
					self.ActiveWeapon.Target = MouseInput.GamePosition;
					self.ActiveWeapon.TargetHeight = 0;
				}
				else
				{
					// Look for actors in range.
					var valid = self.World.Actors.Where(a => a.IsAlive && a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.Targetable && (MouseInput.GamePosition - a.Position).Dist < 256).ToArray();

					// If any, pick one and fire the weapon on it.
					if (valid.Any())
					{
						self.ActiveWeapon.Target = valid.First().Position;
						self.ActiveWeapon.TargetHeight = valid.First().Height;
					}
					else
					{
						self.ActiveWeapon.Target = MouseInput.GamePosition;
						self.ActiveWeapon.TargetHeight = 0;
					}
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
				// Look for actors in range.
				var valid = self.World.Actors.Where(a => a.IsAlive && a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.Targetable && (pos - a.Position).Dist < 256).ToArray();

				// If any, pick one and fire the weapon on it.
				if (valid.Any())
					self.Attack(valid.First());
				else
					self.Attack(pos, 0);
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
