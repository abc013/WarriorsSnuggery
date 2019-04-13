/*
 * User: Andreas
 * Date: 30.05.2018
 * Time: 15:19
 */
using System;

namespace WarriorsSnuggery.Objects
{
	public class KeyboardButton : GameObject
	{
		readonly ImageRenderable released;
		readonly ImageRenderable pressed;
		readonly TextRenderable KeyOn;

		readonly bool changeable;
		int blinkTick;

		bool mouseOnBox;
		public bool Selected;

		public char Key;

		public KeyboardButton(CPos position, char key, TextureInfo buttons, IFont font, Color color, bool changeable = true) : base(position)
		{
			Key = key;
			this.changeable = changeable;
			var textures = buttons.GetTextures();
			released = new ImageRenderable(textures[0]);
			released.SetPosition(position);
			pressed = new ImageRenderable(textures[1]);
			pressed.SetPosition(position);
			KeyOn = new TextRenderable(position, font, key, color);
		}

		public override void Render()
		{
			if(KeyInput.IsKeyDown(Key + ""))
			{
				KeyOn.SetPosition(Position + new CPos(50,50,0));
				pressed.Render();
			}
			else
			{
				KeyOn.SetPosition(Position + new CPos(50,-100,0));
				released.Render();
			}
			if (blinkTick < 10)
				KeyOn.Render();
		}

		public override void Tick()
		{
			base.Tick();
			checkMouse();
			if (mouseOnBox && MouseInput.isLeftClicked)
				Selected = true;
			else if (MouseInput.isLeftClicked)
			{
				Selected = false;
				blinkTick = 0;
			}

			if (Selected)
			{
				if (blinkTick-- < 0)
					blinkTick = 20;

				foreach(var key in KeyInput.AllKeys)
				{
					if (KeyInput.IsKeyDown(key,0))
					{
						Key = key[0];
						KeyOn.setChar(Key);
						Selected = false;
						blinkTick = 0;
					}
				}
			}
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			var width = 400;
			var height = 400;
			mouseOnBox = mousePosition.X > Position.X - width && mousePosition.X < Position.X + width && mousePosition.Y > Position.Y - height && mousePosition.Y < Position.Y + height;
		}

		public override void Dispose()
		{
			base.Dispose();
			KeyOn.Dispose();
		}
	}
}
