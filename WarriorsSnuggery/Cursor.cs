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
		readonly ImageRenderable @default;
		readonly ImageRenderable select;
		readonly ImageRenderable money;
		readonly ImageRenderable attack;

		public CursorType Current;

		public Cursor()
		{
			@default = new ImageRenderable(TextureManager.Texture("cursor_default"));
			select = new ImageRenderable(TextureManager.Texture("cursor_select"));
			money = new ImageRenderable(TextureManager.Texture("cursor_money"));
			attack = new ImageRenderable(TextureManager.Texture("cursor_attack"));

			Current = CursorType.NONE;
		}

		public void Render()
		{
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
					attack.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
					attack.Render();
					break;
				default:
					break;
			}
		}
	}
}
