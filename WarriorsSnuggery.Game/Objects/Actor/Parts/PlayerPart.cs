using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using WarriorsSnuggery.Audio.Music;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Weapons;
using WarriorsSnuggery.Spells;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Only used for internal purposes. Do not use.")]
	public class PlayerPartInfo : PartInfo
	{
		public PlayerPartInfo(PartInitSet set) : base(set) { }
	}

	class PlayerPart : ActorPart, ITick, INoticeDamage, INoticeKilled, INoticeKill, INoticeMove, ISaveLoadable
	{
		public PlayerPart(Actor self, PlayerPartInfo info) : base(self, info) { }

		public void OnLoad(PartLoader loader)
		{
			if (Settings.LockCameraToPlayer)
				positionCamera(false);
		}

		public PartSaver OnSave()
		{
			return new PartSaver(this);
		}

		public void Tick()
		{
			var screenControl = Self.World.Game.ScreenControl;
			if (screenControl.ChatOpen)
				return;

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
				Self.AccelerateSelf(Angle.Cast(horizontalAngle + Angle.Diff(verticalAngle, horizontalAngle) / 2));
			}
			else if (vertical != 0)
				Self.AccelerateSelf((2 + vertical) * 0.5f * MathF.PI);
			else if (horizontal != 0)
				Self.AccelerateSelf((1 + horizontal) * 0.5f * MathF.PI);

			if (Settings.DeveloperMode)
			{
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveAbove")))
					Self.AccelerateHeightSelf(true);
				if (KeyInput.IsKeyDown(Settings.GetKey("MoveBelow")))
					Self.AccelerateHeightSelf(false);
			}

			if (Self.Weapon != null)
			{
				var actor = FindValidTarget(MouseInput.GamePosition);
				Self.Weapon.Target = actor == null ? new Target(MouseInput.GamePosition) : new Target(actor);
			}

			if (MouseInput.IsLeftDown && !Self.World.Game.ScreenControl.CursorOnUI())
				attackTarget(MouseInput.GamePosition);

			foreach (var effect in Self.GetActiveEffects(EffectType.MANA))
				Self.World.Game.Player.Mana += (int)effect.Effect.Value;
		}

		public Actor FindValidTarget(CPos pos, int team = Actor.PlayerTeam)
		{
			const int range = 1024;

			if (KeyInput.IsKeyDown(Keys.LeftShift))
				return null;

			// Look for actors in range.
			var sectors = Self.World.ActorLayer.GetSectors(pos, range);
			var currentRange = long.MaxValue;
			Actor validTarget = null;
			foreach (var sector in sectors)
			{
				foreach (var actor in sector.Actors)
				{
					if (actor.Team == team || actor.WorldPart == null || !Self.World.IsVisibleTo(Self, actor))
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
				Self.PrepareAttack(pos);
			else
			{
				var actor = FindValidTarget(pos);

				if (actor == null)
					Self.PrepareAttack(pos);
				else
					Self.PrepareAttack(actor);
			}
		}

		void positionCamera(bool tinyMove)
		{
			Camera.Position(Self.Position, tinyMove: tinyMove);
		}

		public void OnDamage(Actor damager, int damage)
		{
			Self.World.Game.ScreenControl.HideArrow();
			MusicController.FadeIntenseIn(Settings.UpdatesPerSecond * 20);
		}

		public void OnKilled(Actor killer)
		{
			Self.World.PlayerKilled();
		}

		public void OnKill(Actor killed)
		{
			Self.World.Game.Player.Kills++;
		}

		public void OnMove(CPos old, CPos speed)
		{
			if (Settings.LockCameraToPlayer)
				positionCamera(true);
		}
	}
}
