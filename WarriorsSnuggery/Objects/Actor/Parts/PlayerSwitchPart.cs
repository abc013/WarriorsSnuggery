using System.Collections.Generic;
using System.Linq;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Objects.Parts
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
		public float RelativeHP = 1f;
		public ActorType ActorType;
		public int CurrentTick = 0;

		public PlayerSwitchPart(Actor self, PlayerSwitchPartInfo info) : base(self)
		{
			CurrentTick = info.SwitchDuration;
		}

		public override void OnLoad(List<MiniTextNode> nodes)
		{
			var parent = nodes.FirstOrDefault(n => n.Key == "PlayerSwitchPart");
			if (parent == null)
				return;

			foreach (var node in parent.Children)
			{
				if (node.Key == "RelativeHP")
					RelativeHP = node.Convert<float>();
				if (node.Key == "ActorType")
					ActorType = node.Convert<ActorType>();
				if (node.Key == "CurrentTick")
					CurrentTick = node.Convert<int>();
			}
		}

		public override PartSaver OnSave()
		{
			var saver = new PartSaver(this);

			saver.Add("RelativeHP", RelativeHP, 1f);
			saver.Add("ActorType", ActorCreator.GetName(ActorType), null);
			saver.Add("CurrentTick", CurrentTick, 0);

			return saver;
		}

		public override void Tick()
		{
			if (self.World.Game.Editor)
				return;

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
