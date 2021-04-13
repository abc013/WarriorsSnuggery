﻿using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	class EnemyPointer : UIObject
	{
		readonly Game game;
		readonly BatchObject pointer;
		Actor targetedEnemy;

		public EnemyPointer(Game game)
		{
			this.game = game;
			pointer = new BatchObject(UITextureManager.Get("UI_enemy_arrow")[0], Color.White);
		}

		public override void Tick()
		{
			base.Tick();

			if (game.IsCampaign && !game.IsMenu)
			{
				if (game.World.PlayerDamagedTick < Settings.UpdatesPerSecond * 60)
				{
					targetedEnemy = null;
					return;
				}
				
				if (targetedEnemy != null && targetedEnemy.IsAlive)
					reaimPointer();
				else
					newTarget();
			}
		}

		void newTarget()
		{
			targetedEnemy = game.World.ActorLayer.NonNeutralActors.Find(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory);
		}

		void reaimPointer()
		{
			var pos = Camera.LookAt + new CPos(0, -2048, 0) - targetedEnemy.GraphicPosition;

			pointer.Visible = pos.SquaredFlatDist > 8192 * 8192;
			if (!pointer.Visible)
				return;

			var angle = pos.FlatAngle;
			pointer.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
			pointer.SetPosition(CPos.FromFlatAngle(angle, 2048) - new CPos(0, 2048, 0));
		}

		public override void Render()
		{
			base.Render();

			if (targetedEnemy != null)
				pointer.PushToBatchRenderer();
		}
	}
}