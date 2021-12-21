using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.UI.Objects
{
	class EnemyPointer : UIObject
	{
		readonly Game game;
		readonly BatchObject pointer;
		Actor targetedEnemy;
		bool enabled;

		const int enableTick = 60 * 60;
		int showTick;

		public EnemyPointer(Game game)
		{
			this.game = game;
			pointer = new BatchObject(UISpriteManager.Get("UI_enemy_arrow")[0]);
		}

		public override void Tick()
		{
			base.Tick();

			if (game.IsCampaign && !game.IsMenu && showTick++ == enableTick)
				ShowArrow();

			if (enabled)
			{
				if (targetedEnemy != null && targetedEnemy.IsAlive)
					reaimPointer();
				else
					newTarget();
			}
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
			targetedEnemy = game.World.ActorLayer.NonNeutralActors.Find(a => a.Team != Actor.PlayerTeam && a.WorldPart != null && a.WorldPart.KillForVictory);
		}

		void reaimPointer()
		{
			var pos = Camera.LookAt - Camera.CameraPlayerOffset - targetedEnemy.GraphicPosition;

			pointer.Visible = pos.SquaredFlatDist > 6120 * 6120;
			if (!pointer.Visible)
				return;

			var angle = pos.FlatAngle;
			pointer.SetRotation(new VAngle(0, 0, -angle) + new VAngle(0, 0, 270));
			pointer.SetPosition(CPos.FromFlatAngle(angle, 2048) - Camera.CameraPlayerOffset);
		}

		public override void Render()
		{
			base.Render();

			if (targetedEnemy != null)
				pointer.Render();
		}
	}
}
