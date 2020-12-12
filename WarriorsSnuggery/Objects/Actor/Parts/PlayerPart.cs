using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;
using WarriorsSnuggery.UI.Screens;

namespace WarriorsSnuggery.Objects.Parts
{
	class PlayerPart : ActorPart, ITick, INoticeDamage, INoticeKilled, INoticeKill, INoticeMove
	{
		bool firstTick = true;

		public PlayerPart(Actor self) : base(self) { }

		public override PartSaver OnSave()
		{
			return new PartSaver(this, string.Empty, true);
		}

		public void Tick()
		{
			if (self.World.Game.Editor)
				return;

			if (self.World.Game.ScreenControl.ChatOpen && self.World.Game.ScreenControl.CursorOnUI())
				return;

			if (firstTick && Camera.LockedToPlayer)
			{
				firstTick = false;
				positionCamera();
			}

			if (KeyInput.IsKeyDown(Settings.GetKey("CameraLock"), 5))
			{
				Camera.LockedToPlayer = !Camera.LockedToPlayer;
				positionCamera();
			}

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

			if (KeyInput.IsKeyDown(Keys.LeftAlt))
			{
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveAbove")))
					self.AccelerateHeight(true);
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveBelow")))
					self.AccelerateHeight(false);
			}

			if (self.ActiveWeapon != null)
			{
				var actor = self.World.FindValidTarget(MouseInput.GamePosition, self.Team);

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

			foreach (var effect in self.Effects.Where(e => e.Active && e.Spell.Type == Spells.EffectType.MANA))
				self.World.Game.Statistics.Mana += (int)effect.Spell.Value;

			self.World.PlayerDamagedTick++;
		}

		void attackTarget(CPos pos)
		{
			if (KeyInput.IsKeyDown(Keys.LeftShift))
				self.PrepareAttack(pos, 0);
			else
			{
				var actor = self.World.FindValidTarget(pos, self.Team);

				if (actor == null)
					self.PrepareAttack(pos, 0);
				else
					self.PrepareAttack(actor);
			}
		}

		void positionCamera()
		{
			Camera.Position(self.Position + (self.World.Game.ScreenControl.Focused is DefaultScreen ? Camera.CamPlayerOffset : CPos.Zero));
		}

		public void OnDamage(Actor damager, int damage)
		{
			self.World.PlayerDamagedTick = 0;
		}

		public void OnKilled(Actor killer)
		{
			self.World.PlayerKilled();
		}

		public void OnKill(Actor killed)
		{
			self.World.Game.Statistics.Kills++;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (Camera.LockedToPlayer)
				positionCamera();
		}
	}
}
