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
				setCharPositions();
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

			if (add)
				addText(text, colorPairs);
			else
				setText(text, colorPairs);
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

		public void SetText(object text) => setText(text.ToString());

		void setText(string text, Dictionary<int, Color> colorPairs = null)
		{
			Text = text;

			var color = Color;
			for (int i = 0; i < text.Length; i++)
			{
				if (colorPairs != null && colorPairs.ContainsKey(i))
					color = colorPairs[i];

				var @char = text[i];
				if (chars.Count <= i)
					chars.Add(new TextRenderable(font, @char, color));
				else
				{
					var localchar = chars[i];
					localchar.SetCharacter(text[i]);
					localchar.SetColor(color);
				}
			}

			while (text.Length < chars.Count)
				chars.Remove(chars[^1]);

			setCharPositions();
		}

		public void AddText(object text) => addText(text.ToString());

		void addText(string text, Dictionary<int, Color> colorPairs = null)
		{
			Text += text.ToString();

			var color = Color;
			int charlength = chars.Count;
			for (int i = chars.Count; i < Text.Length; i++)
			{
				if (colorPairs != null && colorPairs.ContainsKey(i - charlength))
					color = colorPairs[i - charlength];

				chars.Add(new TextRenderable(font, Text[i], color));
			}

			setCharPositions();
		}

		void setCharPositions()
		{
			if (string.IsNullOrWhiteSpace(Text))
			{
				Bounds = MPos.Zero;
				return;
			}

			var maxWidth = font.GetWidth(Text);
			Bounds = new MPos(maxWidth / 2, font.MaxHeight / 2);

			var width = font.GetWidth(Text[0]) / 2;
			switch (Offset)
			{
				case TextOffset.MIDDLE:
					width -= maxWidth / 2;

					break;
				case TextOffset.RIGHT:
					width -= maxWidth;

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
