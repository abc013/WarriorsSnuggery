using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery
{
	public enum CursorType
	{
		DEFAULT,
		SELECT,
		MONEY,
		ATTACK,
		NONE
	}

	public class Cursor : IRenderable
	{
		readonly BatchObject @default;
		readonly BatchObject select;
		readonly BatchObject money;
		readonly BatchObject attack;

		public CursorType Current;

		public Cursor()
		{
			@default = new BatchObject(UISpriteManager.Get("cursor_default")[0]);
			select = new BatchObject(UISpriteManager.Get("cursor_select")[0]);
			money = new BatchObject(UISpriteManager.Get("cursor_money")[0]);
			attack = new BatchObject(UISpriteManager.Get("cursor_attack")[0]);

			Current = CursorType.NONE;
		}

		public void Render()
		{
			if (Settings.DeveloperMode)
			{
				const int radius = 64;
				var color = Current == CursorType.ATTACK ? Color.Red : Color.Blue;

				ColorManager.DrawQuad(UIPos.Zero, radius, Color.Cyan);
				ColorManager.DrawQuad(MouseInput.WindowPosition, radius, color);
				return;
			}

			switch (Current)
			{
				case CursorType.DEFAULT:
					@default.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
					@default.Render();
					break;
				case CursorType.SELECT:
					select.SetPosition(MouseInput.WindowPosition);
					select.Render();
					break;
				case CursorType.MONEY:
					money.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
					money.Render();
					break;
				case CursorType.ATTACK:
					attack.SetPosition(MouseInput.WindowPosition);
					attack.Render();
					break;
				default:
					break;
			}
		}
	}
}
