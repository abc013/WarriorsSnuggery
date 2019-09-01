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
			{
				self.Accelerate((float)Math.PI * 1.5f);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveDown")))
			{
				self.Accelerate((float)Math.PI * 0.5f);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveRight")))
			{
				self.Accelerate(0);
			}

			if (KeyInput.IsKeyDown(Settings.Key("MoveLeft")))
			{
				self.Accelerate((float)Math.PI);
			}

			if (MouseInput.IsLeftDown && !self.World.Game.ScreenControl.CursorOnUI())
			{
				self.Attack(MouseInput.GamePosition);
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
