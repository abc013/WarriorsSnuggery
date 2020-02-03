using WarriorsSnuggery.Objects.Bot;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("Basic information about the object.")]
	public class WorldPartInfo : PartInfo
	{
		[Desc("When true, the actor will show a damage text.")]
		public readonly bool ShowDamage;

		[Desc("When true, the actor can be targeted by the automatic target system.")]
		public readonly bool Targetable;

		[Desc("When true, the actor will be able to trigger other objects.")]
		public readonly bool CanTrigger;

		[Desc("Height of the actor.")]
		public readonly int Height;

		[Desc("Hovering of the actor.", "This will create a hover-effect for flying actors.")]
		public readonly int Hover;

		[Desc("Hides the actor when the cursor/player is behind it so the player can see more.")]
		public readonly bool Hideable;

		[Desc("Up-left-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner1 = new CPos(-256, 256, 0);
		[Desc("Down-right-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner2 = new CPos(256, -256, 0);

		[Desc("Selects the bot behavior that will be used if the actor is controlled by a bot.", "Possible: TYPICAL, PANIC, MOTH, HIDE_AND_SEEK")]
		public readonly BotBehaviorType BotBehavior = BotBehaviorType.TYPICAL;

		public override ActorPart Create(Actor self)
		{
			return new WorldPart(self, this);
		}

		public WorldPartInfo(string internalName, MiniTextNode[] nodes) : base(internalName, nodes)
		{

		}
	}

	public class WorldPart : ActorPart
	{
		readonly WorldPartInfo info;

		public bool ShowDamage
		{
			get { return info.ShowDamage; }
			set { }
		}

		public bool Targetable
		{
			get { return info.Targetable; }
			set { }
		}

		public bool CanTrigger
		{
			get { return info.CanTrigger; }
			set { }
		}

		public int Height
		{
			get { return info.Height; }
			set { }
		}

		public int Hover
		{
			get { return info.Hover; }
			set { }
		}

		public bool Hideable
		{
			get { return info.Hideable; }
			set { }
		}
		public BotBehaviorType BotBehavior
		{
			get { return info.BotBehavior; }
			set { }
		}

		public WorldPart(Actor self, WorldPartInfo info) : base(self)
		{
			this.info = info;
		}

		public bool InTargetBox(CPos pos)
		{
			var diff = pos - self.Position;
			return diff.X > info.TargetBoxCorner1.X && diff.X < info.TargetBoxCorner2.X && diff.Y > -info.TargetBoxCorner1.Y  && diff.Y < -info.TargetBoxCorner2.Y;
		}
	}
}
