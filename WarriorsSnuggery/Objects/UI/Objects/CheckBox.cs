using System;

namespace WarriorsSnuggery.UI
{
	public class CheckBox : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;
				type.Click.SetPosition(value);
				type.Default.SetPosition(value);
				type.Checked.SetPosition(value);
			}
		}

		public override VAngle Rotation
		{
			get => base.Rotation;
			set
			{
				base.Rotation = value;
				type.Click.SetRotation(value);
				type.Default.SetRotation(value);
				type.Checked.SetRotation(value);
			}
		}

		public override float Scale
		{
			get => base.Scale;
			set
			{
				base.Scale = value;
				type.Click.SetScale(value);
				type.Default.SetScale(value);
				type.Checked.SetScale(value);
			}
		}

		public bool Checked;

		readonly CheckBoxType type;
		readonly Action<bool> action;

		public CheckBox(CPos pos, bool @checked, CheckBoxType type, Action<bool> onTicked)
		{
			Checked = @checked;
			this.type = type;
			action = onTicked;
			Position = pos;
		}

		public override void Tick()
		{
			CheckMouse(type.Width, type.Height);

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
				type.Click.PushToBatchRenderer();
			}
			else
			{
				if (!Checked)
				{
					type.Default.SetPosition(Position);
					type.Default.PushToBatchRenderer();
				}
				else
				{
					type.Checked.SetPosition(Position);
					type.Checked.PushToBatchRenderer();
				}
			}
		}
	}
}
