/*
 * User: Andreas
 * Date: 17.09.2018
 * Time: 15:57
 */
using System;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public class CheckBox : GameObject, IRenderable, ITick
	{
		public bool Checked;

		readonly CheckBoxType type;
		readonly Action<bool> action;

		bool mouseOnBox;

		public CheckBox(CPos pos, bool @checked, CheckBoxType type, Action<bool> onTicked) : base(pos, type.Default)
		{
			Checked = @checked;
			this.type = type;
			action = onTicked;
		}

		public override void Render()
		{
			if (mouseOnBox && MouseInput.isLeftDown)
			{
				type.Click.setPosition(Position);
				type.Click.Render();
			}
			else
			{
				if (!Checked)
				{
					Renderable.setPosition(Position);
					base.Render();
				}
				else
				{
					type.Checked.setPosition(Position);
					type.Checked.Render();
				}
			}
		}

		public override void Tick()
		{
			base.Tick();

			checkMouse();
		}

		void checkMouse()
		{
			var mousePosition = MouseInput.WindowPosition;

			mouseOnBox = mousePosition.X > Position.X - type.Width && mousePosition.X < Position.X + type.Width && mousePosition.Y > Position.Y - type.Height && mousePosition.Y < Position.Y + type.Height;

			if (MouseInput.isLeftClicked && mouseOnBox)
			{
				Checked = !Checked;
				action?.Invoke(Checked);
			}
		}
	}
}
