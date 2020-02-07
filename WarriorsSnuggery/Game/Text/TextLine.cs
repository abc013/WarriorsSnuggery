using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class TextLine : IPositionable, ITickRenderDisposable
	{
		public enum OffsetType
		{
			RIGHT,
			MIDDLE,
			LEFT
		}
		readonly OffsetType offset;
		readonly Font font;
		readonly List<TextRenderable> chars = new List<TextRenderable>();

		public bool Visible = true;
		public string String;
		protected Color color = Color.White;

		public virtual CPos Position
		{
			get { return position; }
			set
			{
				position = value;

				setCharPositions();
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get { return rotation; }
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
			get { return scale; }
			set
			{
				scale = value;

				foreach (var c in chars)
					c.SetScale(scale);
			}
		}
		float scale = 1f;

		public TextLine(CPos pos, Font font, OffsetType type = OffsetType.LEFT)
		{
			Position = pos;
			this.font = font;
			offset = type;
		}

		public void SetColor(Color color, bool updateText = true)
		{

			this.color = color;
			if (updateText)
			{
				foreach (TextRenderable @char in chars)
					@char.SetColor(color);
			}
		}

		/// <summary>
		/// In this method, you can write Colors down. They will be translated.
		/// </summary>
		public void WriteText(object obj, bool add = false, bool colored = true)
		{
			var text = obj.ToString();

			Dictionary<int, Color> colorPairs = new Dictionary<int, Color>();

			while (text.IndexOf("COLOR(") >= 0 && colored)
			{
				var index = text.IndexOf("COLOR(");
				var endindex = text.Remove(0, index).IndexOf(')');
				var color = recognizeColor(text.Remove(0, index).Remove(endindex + 1));

				colorPairs.Add(index, color);

				text = text.Remove(index, endindex + 1);
			}

			if (!add)
			{
				String = text;
				int width = 0;
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
						localchar.SetColor(color);
						localchar.SetCharacter(text[i]);
					}
					width += charWidth(text[i]);
				}

				while (text.Length < chars.Count)
					chars.Remove(chars[chars.Count - 1]);
			}
			else
			{
				String += text.ToString();

				int width = 0;
				for (int i = 0; i < chars.Count; i++)
					width += charWidth(String[i]);

				int charlength = chars.Count;
				for (int i = chars.Count; i < String.Length; i++)
				{
					if (colorPairs.ContainsKey(i - charlength))
						color = colorPairs[i - charlength];

					chars.Add(new TextRenderable(Position, font, String[i], color, width));
					width += charWidth(String[i]);
				}
			}

			setCharPositions();
		}

		Color recognizeColor(string text)
		{
			text = text.Remove(0, 6);
			text = text.Replace(')', ' ');
			var values = text.Split('|');

			try
			{
				var r = float.Parse(values[0]);
				var g = float.Parse(values[1]);
				var b = float.Parse(values[2]);
				var a = float.Parse(values[3]);
				return new Color(r, g, b, a);
			}
			catch (Exception e)
			{
				throw new YamlInvalidNodeException("Unable to create Textcolor.", e);
			}
		}

		public void SetText(object @new)
		{
			String = @new.ToString();

			int width = 0;
			for (int i = 0; i < String.Length; i++)
			{
				var @char = String[i];
				if (chars.Count <= i)
					chars.Add(new TextRenderable(Position, font, @char, color, width));
				else
				{
					var localchar = chars[i];
					localchar.SetColor(color);
					localchar.SetCharacter(String[i]);
				}
				width += charWidth(String[i]);
			}

			while (String.Length < chars.Count)
				chars.Remove(chars[chars.Count - 1]);

			setCharPositions();
		}

		public void AddText(object text)
		{
			String += text.ToString();

			int width = 0;
			for (int i = 0; i < chars.Count; i++)
				width += charWidth(String[i]);

			for (int i = chars.Count; i < String.Length; i++)
			{
				chars.Add(new TextRenderable(Position, font, String[i], color, width));
				width += charWidth(String[i]);
			}

			setCharPositions();
		}

		void setCharPositions()
		{
			int width = 0;
			switch (offset)
			{
				case OffsetType.MIDDLE:
					for (int i = 0; i < (chars.Count - 1); i++)
						width -= charWidth(String[i]);
					width /= 2;
					break;
				case OffsetType.RIGHT:
					for (int i = 0; i < chars.Count - 1; i++)
						width -= charWidth(String[i]);
					break;
			}
			for (int i = 0; i < chars.Count; i++)
			{
				chars[i].SetPosition(Position, width);
				width += charWidth(String[i]);
			}
		}

		int charWidth(char @char)
		{
			if (@char != ' ')
				return font.GetWidth(@char);

			return font.Info.MaxSize.X / 2 + 1;
		}

		public virtual void Render()
		{
			if (!Visible)
				return;

			foreach (TextRenderable @char in chars)
			{
				@char.Render();
			}
		}

		public virtual void Tick() { }

		public void Dispose()
		{
			Visible = false;
		}
	}
}
