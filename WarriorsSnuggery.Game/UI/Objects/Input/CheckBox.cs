using System;

namespace WarriorsSnuggery.UI.Objects
{
	public class CheckBox : UIPositionable, ITick, IRenderable
	{
		public float Scale
		{
			get => scale;
			set
			{
				scale = value;

				SelectableBounds = new UIPos((int)(value * 256), (int)(value * 256));
				Bounds = SelectableBounds;
			}
		}
		float scale;

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

		public void Tick()
		{
			if (MouseInput.IsLeftClicked && UIUtils.ContainsMouse(this))
			{
				UIUtils.PlayClickSound();
				Checked = !Checked;
				action?.Invoke(Checked);
			}
		}

		public void Render()
		{
			var renderable = Checked ? type.Checked : type.Default;

			if (MouseInput.IsLeftDown && UIUtils.ContainsMouse(this))
				renderable = type.Click;

			renderable.SetPosition(Position);
			renderable.SetScale(Scale);
			renderable.Render();
		}
	}
}
