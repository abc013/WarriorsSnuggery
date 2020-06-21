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
			@default = new BatchObject(UITextureManager.Get("cursor_default")[0], Color.White);
			select = new BatchObject(UITextureManager.Get("cursor_select")[0], Color.White);
			money = new BatchObject(UITextureManager.Get("cursor_money")[0], Color.White);
			attack = new BatchObject(UITextureManager.Get("cursor_attack")[0], Color.White);

			Current = CursorType.NONE;
		}

		public void Render()
		{
			switch (Current)
			{
				case CursorType.DEFAULT:
					@default.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
					@default.PushToBatchRenderer();
					break;
				case CursorType.SELECT:
					select.SetPosition(MouseInput.WindowPosition);
					select.PushToBatchRenderer();
					break;
				case CursorType.MONEY:
					money.SetPosition(MouseInput.WindowPosition + new CPos(240, 240, 0));
					money.PushToBatchRenderer();
					break;
				case CursorType.ATTACK:
					attack.SetPosition(MouseInput.WindowPosition);
					attack.PushToBatchRenderer();
					break;
				default:
					break;
			}
		}
	}
}
