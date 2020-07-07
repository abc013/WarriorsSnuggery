﻿namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Determines whether this is an actor used for switching to another actor.", "Please note that to change actor, the transition actor has to be dead or the switchDuration is completed.")]
	public class PlayerSwitchPartInfo : PartInfo
	{
		[Desc("Time it takes to switch actor in ticks.")]
		public readonly int SwitchDuration = 120;

		public PlayerSwitchPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes) { }

		public override ActorPart Create(Actor self)
		{
			return new PlayerSwitchPart(self, this);
		}
	}

	public class PlayerSwitchPart : ActorPart
	{
		public float RelativeHP;
		public ActorType ActorType;
		public int CurrentTick;

		public PlayerSwitchPart(Actor self, PlayerSwitchPartInfo info) : base(self)
		{
			CurrentTick = info.SwitchDuration;
		}

		public override void Tick()
		{
			if (CurrentTick-- == 0 && !self.Disposed)
				switchPlayer();
		}

		public override void OnKilled(Actor killer)
		{
			switchPlayer();
		}

		void switchPlayer()
		{
			var actor = ActorCreator.Create(self.World, ActorType, self.Position, self.Team, isPlayer: true, health: RelativeHP);
			self.World.FinishPlayerSwitch(actor, ActorType);
			self.Dispose();
		}
	}
}