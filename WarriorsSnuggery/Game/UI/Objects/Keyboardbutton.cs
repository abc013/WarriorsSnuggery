using System;

namespace WarriorsSnuggery.Objects
{
	public class KeyboardButton : IDisposable, ITickRenderable, IPositionable
	{
		public virtual CPos Position
		{
			get { return position; }
			set
			{
				position = value;

				released.SetPosition(position);
				pressed.SetPosition(position);
				keyDisplay.SetPosition(position);
			}
		}
		CPos position;

		public virtual CPos Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;

				released.SetRotation(rotation.ToAngle());
				pressed.SetRotation(rotation.ToAngle());
				keyDisplay.SetRotation(rotation.ToAngle());
			}
		}
		CPos rotation;

		public virtual float Scale
		{
			get { return scale; }
			set
			{
				scale = value;

				released.SetScale(scale);
				pressed.SetScale(scale);
				keyDisplay.SetScale(scale);
			}
		}
		float scale = 1f;

		readonly ImageRenderable released;
		readonly ImageRenderable pressed;
		readonly TextRenderable keyDisplay;

		readonly bool changeable;
		int blinkTick;

		bool mouseOnBox;
		public bool Selected;

		public char Key;

		public KeyboardButton(CPos position, char key, TextureInfo buttons, IFont font, Color color, bool changeable = true)
		{
			Key = key;
			this.changeable = changeable;
			var textures = buttons.GetTextures();

			released = new ImageRenderable(textures[0]);
			released.SetPosition(position);
			pressed = new ImageRenderable(textures[1]);
			pressed.SetPosition(position);
			keyDisplay = new TextRenderable(position, font, key, color);

			Position = position;
		}

		public void Render()
		{
			if(KeyInput.IsKeyDown(Key + ""))
			{
				keyDisplay.SetPosition(Position + new CPos(50,50,0));
				pressed.Render();
			}
			else
			{
				keyDisplay.SetPosition(Position + new CPos(50,-100,0));
				released.Render();
			}
			if (blinkTick < 10)
				keyDisplay.Render();
		}

		public void Tick()
		{
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
						keyDisplay.Char = Key;
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

		public void Dispose()
		{
			keyDisplay.Dispose();
		}
	}
}
