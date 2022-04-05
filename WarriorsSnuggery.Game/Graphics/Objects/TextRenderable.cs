using OpenTK.Mathematics;
using System.Collections.Generic;

namespace WarriorsSnuggery.Graphics
{
	class TextRenderable : BatchRenderable
	{
		public readonly TextOffset Offset;
		public readonly Font Font;

		readonly List<CharRenderable> chars = new List<CharRenderable>();

		public string Text { get; private set; } = string.Empty;

		public TextRenderable(Font font, TextOffset offset = TextOffset.LEFT) : base(new Vertex[0])
		{
			Font = font;
			Offset = offset;
		}

		public void SetText(params string[] args)
		{
			if (args.Length == 0)
			{
				SetText((object)string.Empty);
				return;
			}

			SetText((object)args[0]);

			for (int i = 1; i < args.Length; i++)
				AddText((object)args[i]);
		}

		public void SetText(object obj)
		{
			var text = obj.ToString();
			var colorPairs = getColors(ref text);

			setText(text, colorPairs);
		}

		void setText(string text, Dictionary<int, Color> colorPairs)
		{
			chars.Clear();

			Text = text;

			var color = Color;
			for (int i = 0; i < text.Length; i++)
			{
				if (Text[i] == '\n')
					continue;

				if (colorPairs.ContainsKey(i))
					color = colorPairs[i];

				chars.Add(new CharRenderable(Font, text[i], color));
			}

			CacheOutdated = true;
		}

		public void AddText(params string[] args)
		{
			for (int i = 0; i < args.Length; i++)
				AddText((object)args[i]);
		}

		public void AddText(object obj)
		{
			var text = '\n' + obj.ToString();
			var colorPairs = getColors(ref text);

			addText(text, colorPairs);
		}

		void addText(string text, Dictionary<int, Color> colorPairs)
		{
			var offset = Text.Length;
			Text += text;

			var color = Color;
			for (int i = offset; i < Text.Length; i++)
			{
				if (Text[i] == '\n')
					continue;

				if (colorPairs.ContainsKey(i - offset))
					color = colorPairs[i - offset];

				chars.Add(new CharRenderable(Font, Text[i], color));
			}

			CacheOutdated = true;
		}

		Dictionary<int, Color> getColors(ref string text)
		{
			var colorPairs = new Dictionary<int, Color>();
			var currentIndex = 0;

			while (text.IndexOf("COLOR(", currentIndex) >= 0)
			{
				var index = text.IndexOf("COLOR(");
				var endindex = text.IndexOf(')') + 1;

				if (endindex < 0)
				{
					currentIndex = index + 6;
					continue;
				}

				// If nothing comes after this color, then just discard it and stop
				if (endindex >= text.Length)
				{
					text = text[..index];
					break;
				}

				var success = Color.FromString(text[index..endindex], out var color);

				if (!success)
				{
					currentIndex = endindex;
					continue;
				}

				colorPairs.Add(index, color);

				text = text.Remove(index, endindex - index);
			}

			return colorPairs;
		}

		public override void Render()
		{
			if (!Visible || string.IsNullOrWhiteSpace(Text))
				return;

			if (CacheOutdated)
			{
				updateCharRenderables();
				CacheOutdated = false;
			}

			foreach (var @char in chars)
				@char.Render();
		}

		void updateCharRenderables()
		{
			var height = 0;

			int getOffset(string line)
            {
				if (string.IsNullOrEmpty(line))
					return 0;

				var width = Font.Measure(line).width;
				var offset = Font.Measure(line[0]).width / 2;
				if (Offset == TextOffset.MIDDLE)
					offset = -width / 2;
				else if (Offset == TextOffset.RIGHT)
					offset = -width;

				return offset;
			}

			var nextLine = Text.IndexOf('\n');
			var width = getOffset(nextLine < 0 ? Text : Text[..nextLine]);

			int charIndex = 0;
			for (int i = 0; i < Text.Length; i++)
			{
				if (Text[i] == '\n')
				{
					height += Font.MaxHeight + Font.HeightGap;
					if (i + 1 < Text.Length)
					{
						nextLine = Text.IndexOf('\n', i + 1);
						width = getOffset(nextLine < 0 ? Text[(i + 1)..] : Text[(i + 1)..nextLine]);
					}

					continue;
				}

				chars[charIndex].SetScale(Scale);
				chars[charIndex++].SetPosition(Position + new Vector3(width * Scale.X / 1024f, -height * Scale.Y / 1024f, 0));
				width += Font.Measure(Text[i]).width;
			}
		}
	}
}
