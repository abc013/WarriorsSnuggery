using System;
using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.UI.Objects
{
	class EnemyPointer : UIPositionable, ITick, IRenderable
	{
		readonly Game game;
		readonly UIPos clampingRadius;
		readonly BatchObject pointer;
		Actor targetedEnemy;
		bool enabled;

		// 30 seconds
		const int enableTick = 30 * 30;
		int showTick;

		public EnemyPointer(Game game, UIPos clampingRadius)
		{
			this.game = game;
			this.clampingRadius = clampingRadius;
			pointer = new BatchObject(UISpriteManager.Get("UI_enemy_arrow")[0]);

			ShowArrow();
		}

		public void Tick()
		{
			if (enabled)
			{
				if (targetedEnemy != null && targetedEnemy.IsAlive)
					reaimPointer();
				else
					newTarget();
			}
			else if (game.MissionType.IsCampaign() && !game.MissionType.IsMenu() && showTick++ >= enableTick)
				ShowArrow();
		}

		public void ShowArrow()
		{
			enabled = true;
		}

		public void HideArrow()
		{
			enabled = false;
			targetedEnemy = null;
		}
		
		void newTarget()
		{
			// Find target closest to origin Position
			var originPosition = game.World.LocalPlayer.Position;
			var currentDistance = float.PositiveInfinity;
			Actor currentEnemy = null;

			foreach (var enemy in game.World.ActorLayer.NonNeutralActors.Where(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory))
			{
				var dist = (originPosition - enemy.Position).FlatDist;
				if (dist >= currentDistance)
					continue;

				currentDistance = dist;
				currentEnemy = enemy;
			}

			targetedEnemy = currentEnemy;
		}

		void reaimPointer()
		{
			var pos = game.World.LocalPlayer.GraphicPosition - targetedEnemy.GraphicPosition;

			pointer.Visible = pos.SquaredFlatDist > 7168 * 7168;
			if (!pointer.Visible)
				return;

			var angle = pos.FlatAngle;
			pointer.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));

			var position = CPos.FromFlatAngle(angle, 8120 * 2);
			var clampedX = Math.Clamp(position.X, Position.X - clampingRadius.X, Position.Y + clampingRadius.X);
			var clampedY = Math.Clamp(position.Y, Position.Y - clampingRadius.Y, Position.Y + clampingRadius.Y);
			position = new UIPos(clampedX, clampedY);

			pointer.SetPosition(position);
		}

		public void Render()
		{
			if (targetedEnemy != null)
				pointer.Render();
		}
	}
}
