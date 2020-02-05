using System;
using WarriorsSnuggery.Graphics;

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

		public virtual VAngle Rotation
		{
			get { return rotation; }
			set
			{
				rotation = value;

				released.SetRotation(rotation);
				pressed.SetRotation(rotation);
				keyDisplay.SetRotation(rotation);
			}
		}
		VAngle rotation;

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

		readonly BatchObject released;
		readonly BatchObject pressed;
		readonly TextRenderable keyDisplay;

		int blinkTick;

		bool mouseOnBox;
		public bool Selected;

		public char Key;

		public KeyboardButton(CPos position, char key, ITexture[] textures, IFont font, Color color)
		{
			Key = key;

			released = new BatchObject(textures[0], Color.White);
			released.SetPosition(position);
			pressed = new BatchObject(textures[1], Color.White);
			pressed.SetPosition(position);
			keyDisplay = new TextRenderable(position, font, key, color);

			Position = position;
		}

		public void Render()
		{
			if (KeyInput.IsKeyDown(Key + ""))
			{
				keyDisplay.SetPosition(Position + new CPos(50, 50, 0));
				pressed.PushToBatchRenderer();
			}
			else
			{
				keyDisplay.SetPosition(Position + new CPos(50, -100, 0));
				released.PushToBatchRenderer();
			}
			if (blinkTick < 10)
				keyDisplay.Render();
		}

		public void Tick()
		{
			checkMouse();
			if (mouseOnBox && MouseInput.IsLeftClicked)
				Selected = true;
			else if (MouseInput.IsLeftClicked)
			{
				Selected = false;
				blinkTick = 0;
			}

			if (Selected)
			{
				if (blinkTick-- < 0)
					blinkTick = 20;

				foreach (var key in KeyInput.AllKeys)
				{
					if (KeyInput.IsKeyDown(key, 0))
					{
						Key = key[0];
						keyDisplay.SetCharacter(Key);
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

		}
	}
}
