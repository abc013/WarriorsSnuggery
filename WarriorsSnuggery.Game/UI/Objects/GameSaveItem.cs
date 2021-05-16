using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Objects
{
	class GameSaveItem : PanelItem
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				name.Position = value - new CPos(3072, 512, 0);
				score.Position = value - new CPos(3072, 0, 0);
				level.Position = value - new CPos(-1152, 0, 0);
			}
		}

		public readonly GameSave Save;
		public bool Selected;

		readonly UITextLine name;
		readonly UITextLine score;
		readonly UITextLine level;

		public GameSaveItem(GameSave save, int width, Action action) : base(new BatchObject(UISpriteManager.Get("UI_save")[0]), new MPos(width, 1024), save.Name, new[] { Color.Grey + "Difficulty: " + save.Difficulty, Color.Grey + "Money: " + save.Money }, action)
		{
			var pos = CPos.Zero;

			Save = save;
			Scale *= 2;

			name = new UITextLine(FontManager.Default)
			{
				Position = pos - new CPos(3072, 512, 0)
			};
			name.SetText(save.Name);

			score = new UITextLine(FontManager.Default)
			{
				Position = pos - new CPos(3072, 0, 0),
				Color = Color.Yellow
			};
			score.SetText(save.CalculateScore());

			level = new UITextLine(FontManager.Header)
			{
				Position = pos - new CPos(-1152, 0, 0),
				Scale = 1.4f
			};
			if (save.Level >= save.FinalLevel)
				level.Color = new Color(0, 200, 0);
			level.SetText(save.Level + "/" + save.FinalLevel);
		}

		public override void Render()
		{
			base.Render();
			if (Visible)
			{
				name.Scale += ContainsMouse ? 0.04f : -0.04f;

				if (name.Scale > 1.28f)
					name.Scale = 1.28f;

				if (name.Scale < 1f)
					name.Scale = 1f;

				name.Render();
				score.Render();
				level.Render();
			}

			SetColor(Selected ? new Color(1.5f, 1.5f, 1.5f) : Color.White);
		}

		public override void Tick()
		{
			base.Tick();

			if (ContainsMouse && MouseInput.IsLeftClicked)
				Selected = true;
			else if (MouseInput.IsLeftClicked)
				Selected = false;
		}
	}
}
