using System;
using System.Collections.Generic;

namespace WarriorsSnuggery.Objects
{
	public class TextLine : PhysicsObject, IPositionable
	{
		public enum OffsetType
		{
			RIGHT,
			MIDDLE,
			LEFT
		}
		readonly OffsetType offset;
		readonly IFont font;
		readonly List<TextRenderable> chars = new List<TextRenderable>();

		public bool Visible = true;
		public string String;
		protected Color color = Color.White;

		public override CPos Position {
			get
			{
				return base.Position;
			}
			set
			{
				base.Position = value;
				if (font == null)
					return;

				setCharPositions();
			}
		}

		public override CPos Rotation {
			get
			{
				return base.Rotation;
			}
			set
			{
				base.Rotation = value;
				if (font == null)
					return;

				for(int i = 0; i < chars.Count; i++)
				{
					chars[i].SetRotation(value.ToAngle());
				}
			}
		}

		public override float Scale {
			get {
				return base.Scale;
			}
			set {
				base.Scale = value;
				if (font == null)
					return;

				for(int i = 0; i < chars.Count; i++)
				{
					chars[i].SetScale(value);
				}
			}
		}

		public TextLine(CPos pos, IFont font, OffsetType type = OffsetType.LEFT) : base(pos)
		{
			Position += new CPos(0,64,0);
			this.font = font;
			offset = type;
		}

		public void SetColor(Color color, bool updateText = true)
		{

			this.color = color;
			if (updateText)
			{
				foreach(TextRenderable @char in chars)
				{
					@char.SetColor(color);
				}
			}
		}

		/// <summary>
		/// In this method, you can write Colors down. They will be translated.
		/// </summary>
		public void WriteText(object obj, bool add = false, bool colored = true)
		{
			var text = obj.ToString();

			Dictionary<int, Color> colorPairs = new Dictionary<int, Color>();

			while(text.IndexOf("COLOR(") >= 0 && colored)
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
					{
						color = colorPairs[i];
					}
					var @char = text[i];
					if (chars.Count <= i)
						chars.Add(new TextRenderable(Position, font, @char, color, width));
					else
					{
						var localchar = chars[i];
						localchar.Color = color;
						localchar.Char = text[i];
					}
					width += charWidth(text[i]);
				}

				while (text.Length < chars.Count)
				{
					chars[chars.Count - 1].Dispose();
					chars.Remove(chars[chars.Count - 1]);
				}
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
					{
						color = colorPairs[i - charlength];
					}
					chars.Add(new TextRenderable(Position, font, String[i], color, width));
					width += charWidth(String[i]) + 1;
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
			for(int i = 0; i < String.Length; i++)
			{
				var @char = String[i];
				if (chars.Count <= i)
					chars.Add(new TextRenderable(Position, font, @char, color, width));
				else
				{
					var localchar = chars[i];
					localchar.Color = color;
					localchar.Char = String[i];
				}
				width += charWidth(String[i]);
			}

			while (String.Length < chars.Count)
			{
				chars[chars.Count - 1].Dispose();
				chars.Remove(chars[chars.Count - 1]);
			}

			setCharPositions();
		}

		public void AddText(object text)
		{
			String += text.ToString();

			int width = 0;
			for (int i = 0; i < chars.Count; i++)
				width += charWidth(String[i]);

			for(int i = chars.Count; i < String.Length; i++)
			{
				chars.Add(new TextRenderable(Position, font, String[i], color, width));
				width += charWidth(String[i]) + 1;
			}

			setCharPositions();
		}

		void setCharPositions()
		{
			int width = 0;
			switch(offset)
			{
				case OffsetType.MIDDLE:
					for (int i = 0; i < (chars.Count - 1); i++)
						width -= (charWidth(String[i]) + 1);
					width /= 2;
					break;
				case OffsetType.RIGHT:
					for (int i = 0; i < chars.Count - 1; i++)
						width += -(charWidth(String[i]) + 1);
					break;
			}
			for(int i = 0; i < chars.Count; i++)
			{
				chars[i].SetPosition(base.Position.ToVector() + new OpenTK.Vector4((width + 1) * TextRenderable.SizeMultiplier, 0,0,0));
				width += charWidth(String[i]) + 1;
			}
		}

		int charWidth(char @char)
		{
			if (@char != ' ')
				return font.CharWidth[TextureManager.Characters.IndexOf(@char)];

			return font.MaxSize.X / 2;
		}

		public override void Render()
		{
			if (!Visible || Disposed)
				return;

			foreach(TextRenderable @char in chars)
			{
				@char.Render();
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach(TextRenderable @char in chars)
				@char.Dispose();
		}
	}
}
