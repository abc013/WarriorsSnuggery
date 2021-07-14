using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	class PlayerPart : ActorPart, ITick, INoticeDamage, INoticeKilled, INoticeKill, INoticeMove, ISaveLoadable
	{
		bool firstTick = true;

		public PlayerPart(Actor self) : base(self) { }

		public void OnLoad(PartLoader loader) { }

		public PartSaver OnSave()
		{
			return new PartSaver(this, string.Empty);
		}

		public void Tick()
		{
			var screenControl = self.World.Game.ScreenControl;
			if (screenControl.ChatOpen || screenControl.CursorOnUI())
				return;

			if (firstTick && Camera.LockedToPlayer)
			{
				firstTick = false;
				positionCamera(false);
			}

			var vertical = 0;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveUp")))
				vertical += 1;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveDown")))
				vertical -= 1;

			var horizontal = 0;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveRight")))
				horizontal -= 1;
			if (KeyInput.IsKeyDown(Settings.GetKey("MoveLeft")))
				horizontal += 1;

			if (vertical != 0 && horizontal != 0)
			{
				var verticalAngle = (2 + vertical) * 0.5f * MathF.PI;
				var horizontalAngle = (1 + horizontal) * 0.5f * MathF.PI;
				self.AccelerateSelf(Angle.Cast(horizontalAngle + Angle.Diff(verticalAngle, horizontalAngle) / 2));
			}
			else if (vertical != 0)
				self.AccelerateSelf((2 + vertical) * 0.5f * MathF.PI);
			else if (horizontal != 0)
				self.AccelerateSelf((1 + horizontal) * 0.5f * MathF.PI);

			if (Settings.DeveloperMode)
			{
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveAbove")))
					self.AccelerateHeightSelf(true);
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveBelow")))
					self.AccelerateHeightSelf(false);
			}

			if (self.Weapon != null)
			{
				var actor = FindValidTarget(MouseInput.GamePosition);

				if (actor == null)
				{
					self.Weapon.Target = MouseInput.GamePosition;
					self.Weapon.TargetHeight = 0;
				}
				else
				{
					self.Weapon.Target = actor.Position;
					self.Weapon.TargetHeight = actor.Height;
				}
			}

			if (MouseInput.IsLeftDown && !self.World.Game.ScreenControl.CursorOnUI())
				attackTarget(MouseInput.GamePosition);

			foreach (var effect in self.GetEffects(EffectType.MANA))
				self.World.Game.Stats.Mana += (int)effect.Effect.Value;

			self.World.PlayerDamagedTick++;
		}

		public Actor FindValidTarget(CPos pos, int team = Actor.PlayerTeam)
		{
			const int range = 1024;

			if (KeyInput.IsKeyDown(Keys.LeftShift))
				return null;

			// Look for actors in range.
			var sectors = self.World.ActorLayer.GetSectors(pos, range);
			var currentRange = long.MaxValue;
			Actor validTarget = null;
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor.Team == team || actor.WorldPart == null || !CameraVisibility.IsVisible(actor.Position))
						continue;

					var targetPart = actor.GetPartOrDefault<TargetablePart>();
					if (targetPart == null || !targetPart.InTargetBox(pos))
						continue;

					var dist = (actor.Position - pos).SquaredFlatDist;
					if (dist < currentRange)
					{
						currentRange = dist;
						validTarget = actor;
					}
				}
			}

			return validTarget;
		}

		void attackTarget(CPos pos)
		{
			if (KeyInput.IsKeyDown(Keys.LeftShift))
				self.PrepareAttack(pos, 0);
			else
			{
				var actor = FindValidTarget(pos);

				if (actor == null)
					self.PrepareAttack(pos, 0);
				else
					self.PrepareAttack(actor);
			}
		}

		void positionCamera(bool tinyMove)
		{
			Camera.Position(self.Position + new CPos(0, 512, 0), tinyMove: tinyMove);
		}

		public void OnDamage(Actor damager, int damage)
		{
			self.World.PlayerDamagedTick = 0;
			MusicController.FadeIntenseIn(Settings.UpdatesPerSecond * 20);
		}

		public void OnKilled(Actor killer)
		{
			self.World.PlayerKilled();
		}

		public void OnKill(Actor killed)
		{
			self.World.Game.Stats.Kills++;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (Camera.LockedToPlayer)
				positionCamera(true);
		}
	}
}
