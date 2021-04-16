using System;

namespace WarriorsSnuggery.UI.Objects
{
	public class CheckBox : UIObject
	{
		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;

				SelectableBounds = new MPos((int)(value * 256), (int)(value * 256));
				Bounds = SelectableBounds;
			}
		}

		public bool Checked;

		readonly CheckBoxType type;
		readonly Action<bool> action;

		public CheckBox(string typeName, bool @checked = false, Action<bool> onTicked = null) : this(CheckBoxManager.GetType(typeName), @checked, onTicked) { }

		public CheckBox(CheckBoxType type, bool @checked = false, Action<bool> onTicked = null)
		{
			Checked = @checked;
			this.type = type;

			// Set bounds and default scale
			Scale = 1.5f;

			action = onTicked;
		}

		public override void Tick()
		{
			CheckMouse();

			if (MouseInput.IsLeftClicked && ContainsMouse)
			{
				Checked = !Checked;
				action?.Invoke(Checked);
			}
		}

		public override void Render()
		{
			if (ContainsMouse && MouseInput.IsLeftDown)
			{
				type.Click.SetPosition(Position);
				type.Click.SetScale(Scale);
				type.Click.SetRotation(Rotation);
				type.Click.PushToBatchRenderer();
			}
			else
			{
				if (!Checked)
				{
					type.Default.SetPosition(Position);
					type.Default.SetScale(Scale);
					type.Default.SetRotation(Rotation);
					type.Default.PushToBatchRenderer();
				}
				else
				{
					type.Checked.SetPosition(Position);
					type.Checked.SetScale(Scale);
					type.Checked.SetRotation(Rotation);
					type.Checked.PushToBatchRenderer();
				}
			}
		}
	}
}
