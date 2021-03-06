﻿using System;
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

		[Desc("Determines whether this actor needs to be killed in order to win a match.")]
		public readonly bool KillForVictory = false;

		[Desc("Height of the actor.")]
		public readonly int Height;

		[Desc("Hovering strength.", "This will create a hover-effect for flying actors.")]
		public readonly int Hover;
		[Desc("Hovering speed.", "If set lower, the hover frequency decreases.")]
		public readonly int HoverSpeed = 32;

		[Desc("Hides the actor when the cursor/player is behind it so the player can see more.")]
		public readonly bool Hideable;

		[Desc("Up-left-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner1 = new CPos(-256, 256, 0);
		[Desc("Down-right-corner of the selection box for possible targets.")]
		public readonly CPos TargetBoxCorner2 = new CPos(256, -256, 0);

		[Desc("Size of the visbility box.", "This is used to determine when to hide the actor after it is out of sight.")]
		public readonly MPos VisibilityBox = new MPos(512, 512);
		[Desc("offset of the visibility box.", "This is used to determine when to hide the actor after it is out of sight.")]
		public readonly CPos VisibilityBoxOffset = CPos.Zero;

		[Desc("Selects the bot behavior that will be used if the actor is controlled by a bot.")]
		public readonly BotBehaviorType BotBehavior = BotBehaviorType.TYPICAL;

		[Desc("Selects a sound that will always be played while the actor is alive.")]
		public readonly SoundType IdleSound;

		[Desc("Determines an actor that is used when switching to another actor.", "When left empty, the switch to the next actor will be instant.")]
		public readonly string PlayerSwitchActor = string.Empty;

		[Desc("Adds this actor the the selection list in the editor.")]
		public readonly bool ShowInEditor = true;

		public WorldPartInfo(PartInitSet set) : base(set) { }

		public override ActorPart Create(Actor self)
		{
			return new WorldPart(self, this);
		}
	}

	public class WorldPart : ActorPart, ITick, INoticeMove, INoticeDispose
	{
		readonly WorldPartInfo info;

		public MPos VisibilityBox => info.VisibilityBox;
		public CPos VisibilityBoxOffset => info.VisibilityBoxOffset;

		public bool ShowDamage => info.ShowDamage;

		public bool Targetable => info.Targetable;

		public bool CanTrigger => info.CanTrigger;
		public bool KillForVictory => info.KillForVictory;

		public int Height => info.Height;

		public int Hover => info.Hover;

		public bool Hideable => info.Hideable;
		public BotBehaviorType BotBehavior => info.BotBehavior;

		public string PlayerSwitchActor => info.PlayerSwitchActor;

		readonly Sound sound;

		int hoverTick;

		public WorldPart(Actor self, WorldPartInfo info) : base(self)
		{
			this.info = info;
			if (info.IdleSound != null)
			{
				sound = new Sound(info.IdleSound);
				sound.Play(self.Position, true);
			}
		}

		public bool InTargetBox(CPos pos)
		{
			var diff = pos - self.Position;
			return diff.X > info.TargetBoxCorner1.X && diff.X < info.TargetBoxCorner2.X && diff.Y > -info.TargetBoxCorner1.Y && diff.Y < -info.TargetBoxCorner2.Y;
		}

		public void Tick()
		{
			if (Hover > 0)
				self.Height += (int)(Math.Sin(hoverTick++ / (float)info.HoverSpeed) * Hover);

			if (self.Mobility != null && self.Mobility.CanFly)
			{
				if (Height > Height + Hover * 64)
					self.AccelerateHeight(false);
				else if (Height < Height - Hover * 64)
					self.AccelerateHeight(true);
			}
		}

		public void OnMove(CPos old, CPos speed)
		{
			sound?.SetPosition(self.Position);
		}

		public void OnDispose()
		{
			sound?.Stop();
		}
	}
}
