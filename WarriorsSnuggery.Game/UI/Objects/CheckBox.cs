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

				SelectableBounds = new UIPos((int)(value * 256), (int)(value * 256));
				Bounds = SelectableBounds;
			}
		}

		public bool Checked;

		readonly CheckBoxType type;
		readonly Action<bool> action;

		public CheckBox(string typeName, bool @checked = false, Action<bool> onTicked = null) : this(CheckBoxCache.Types[typeName], @checked, onTicked) { }

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
				UIUtils.PlayClickSound();
				Checked = !Checked;
				action?.Invoke(Checked);
			}
		}

		public override void Render()
		{
			var renderable = Checked ? type.Checked : type.Default;

			if (ContainsMouse && MouseInput.IsLeftDown)
				renderable = type.Click;

			renderable.SetPosition(Position);
			renderable.SetScale(Scale);
			renderable.SetRotation(Rotation);
			renderable.Render();
		}
	}
}
