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

			if (MouseInput.isLeftDown && !self.World.Game.ScreenControl.CursorOnUI())
			{
				self.Attack(MouseInput.GamePosition);
			}
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
