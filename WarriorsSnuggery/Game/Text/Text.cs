using System;

namespace WarriorsSnuggery.Objects
{
	public class Text : GameObject //TODO: remove GameObject
	{
		readonly TextLine[] lines;

		public Text(CPos position, IFont font, TextLine.OffsetType type, params string[] args) : base(position)
		{
			lines = new TextLine[args.Length];

			for(int i = 0; i < args.Length; i++)
			{
				lines[i] = new TextLine(position + new CPos(0, 1024 * i, 0), font, type);
				lines[i].WriteText(args[i]);
			}
		}

		public override void Render()
		{
			foreach (var line in lines)
				line.Render();
		}

		public override void Tick()
		{
			foreach (var line in lines)
				line.Tick();
		}

		public override void Dispose()
		{
			foreach (var line in lines)
				line.Dispose();
		}
	}
}
