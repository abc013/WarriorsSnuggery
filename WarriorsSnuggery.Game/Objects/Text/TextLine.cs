using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public class TextLine : IPositionable, ITickRenderable
	{
		public readonly TextOffset Offset;

		readonly Font font;
		readonly List<TextRenderable> chars = new List<TextRenderable>();

		public bool Visible = true;

		public string Text { get; private set; }

		public virtual CPos Position
		{
			get => position;
			set
			{
				position = value;

				if (!string.IsNullOrWhiteSpace(Text))
					setCharPositions(font.GetWidth(Text));
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get => rotation;
			set
			{
				rotation = value;

				foreach (var c in chars)
					c.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get => scale;
			set
			{
				scale = value;

				foreach (var c in chars)
					c.SetScale(scale);
			}
		}
		float scale = 1f;

		public Color Color
		{
			get => color;
			set
			{
				color = value;

				foreach (TextRenderable @char in chars)
					@char.SetColor(color);
			}
		}
		Color color = Color.White;

		public MPos Bounds { get; private set; }

		public TextLine(CPos pos, Font font, TextOffset type = TextOffset.LEFT)
		{
			Position = pos;
			this.font = font;
			Offset = type;
		}

		/// <summary>
		/// In this method, you can write Colors down. They will be translated.
		/// </summary>
		public void WriteText(object obj, bool add = false, bool colored = true)
		{
			var colorPairs = new Dictionary<int, Color>();
			var text = obj.ToString();
			var currentIndex = 0;

			while (colored && text.IndexOf("COLOR(", currentIndex) >= 0)
			{
				var index = text.IndexOf("COLOR(");
				var endindex = text.Remove(0, index).IndexOf(')');

				if (endindex < 0)
				{
					currentIndex = index + 6;
					continue;
				}

				// If nothing comes after this color, then just discard it and stop
				if (endindex + 1 >= text.Length - index)
				{
					text = text.Remove(index, endindex + 1);
					break;
				}

				var color = recognizeColor(text.Remove(0, index).Remove(endindex + 1), out var success);

				if (!success)
				{
					currentIndex = index + 6;
					continue;
				}

				colorPairs.Add(index, color);

				text = text.Remove(index, endindex + 1);
			}

			var width = 0;
			if (add)
			{
				width += font.GetWidth(Text);

				Text += text.ToString();

				var color = Color.White;
				int charlength = chars.Count;
				for (int i = chars.Count; i < Text.Length; i++)
				{
					if (colorPairs.ContainsKey(i - charlength))
						color = colorPairs[i - charlength];

					chars.Add(new TextRenderable(Position, font, Text[i], color, width));
					width += font.GetWidth(Text[i]);
				}
			}
			else
			{
				Text = text;

				var color = Color.White;
				for (int i = 0; i < text.Length; i++)
				{
					if (colorPairs.ContainsKey(i))
						color = colorPairs[i];

					var @char = text[i];
					if (chars.Count <= i)
						chars.Add(new TextRenderable(Position, font, @char, color, width));
					else
					{
						var localchar = chars[i];
						localchar.SetPosition(Position, width);
						localchar.SetCharacter(text[i]);
						localchar.SetColor(color);
					}
					width += font.GetWidth(text[i]);
				}

				while (text.Length < chars.Count)
					chars.Remove(chars[^1]);
			}

			setCharPositions(width);
		}

		Color recognizeColor(string text, out bool success)
		{
			success = true;

			text = text.Remove(0, 6);
			text = text.Replace(')', ' ');

			var values = text.Split('|');

			if (values.Length != 3 && values.Length != 4)
			{
				success = false;
				return Color.Black;
			}

			try
			{
				success &= float.TryParse(values[0], out var r);
				success &= float.TryParse(values[1], out var g);
				success &= float.TryParse(values[2], out var b);
				var a = 1.0f;
				if (values.Length == 4)
					success &= float.TryParse(values[3], out a);
				return new Color(r, g, b, a);
			}
			catch (Exception e)
			{
				throw new InvalidNodeException("Unable to create Textcolor.", e);
			}
		}

		public void SetText(object @new)
		{
			Text = @new.ToString();

			var width = 0;
			for (int i = 0; i < Text.Length; i++)
			{
				var @char = Text[i];
				if (chars.Count <= i)
					chars.Add(new TextRenderable(Position, font, @char, Color, width));
				else
				{
					var localchar = chars[i];
					localchar.SetColor(Color);
					localchar.SetCharacter(Text[i]);
				}
				width += font.GetWidth(Text[i]);
			}

			while (Text.Length < chars.Count)
				chars.Remove(chars[^1]);

			setCharPositions(width);
		}

		public void AddText(object text)
		{
			var width = font.GetWidth(Text);

			Text += text.ToString();

			for (int i = chars.Count; i < Text.Length; i++)
			{
				chars.Add(new TextRenderable(Position, font, Text[i], Color, width));
				width += font.GetWidth(Text[i]);
			}

			setCharPositions(width);
		}

		void setCharPositions(int maxWidth)
		{
			Bounds = new MPos(maxWidth, font.Height);

			var width = 0;
			switch (Offset)
			{
				case TextOffset.LEFT:
					width += font.Width / 4;

					break;
				case TextOffset.MIDDLE:
					for (int i = 0; i < (chars.Count - 1); i++)
						width -= font.GetWidth(Text[i]);
					width /= 2;
					break;
				case TextOffset.RIGHT:
					for (int i = 0; i < chars.Count - 1; i++)
						width -= font.GetWidth(Text[i]);
					break;
			}

			for (int i = 0; i < chars.Count; i++)
			{
				chars[i].SetPosition(Position, width);
				width += font.GetWidth(Text[i]);
			}
		}

		public virtual void Render()
		{
			if (!Visible)
				return;

			foreach (TextRenderable @char in chars)
				@char.Render();
		}

		public virtual void Tick() { }
	}
}
