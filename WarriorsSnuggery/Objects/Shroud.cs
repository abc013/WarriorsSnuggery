﻿using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Shroud : PositionableObject
	{
		static BatchObject shroudRenderable;

		public static void Load()
		{
			shroudRenderable = new BatchObject(RuleLoader.ShroudTexture[0], Color.White);
		}

		readonly MPos pos;

		bool shroudRevealed;
		public bool StateAchieved => shroudRevealed && Uncovered || !shroudRevealed && Covered;

		public bool Uncovered => alpha == 0f;
		public bool Covered => alpha == 1f;
		float alpha = 1f;

		public Shroud(MPos pos) : base(new CPos(pos.X * 512 - 256, pos.Y * 512 - 256, 0), null)
		{
			this.pos = pos;
		}

		public bool ChangeState(bool revealed)
		{
			var value = shroudRevealed ^ revealed;

			shroudRevealed = revealed;

			return value;
		}

		public override void Tick()
		{
			if (shroudRevealed && !Uncovered)
			{
				alpha -= 0.1f;

				if (alpha < 0f)
					alpha = 0f;
			}
			else if (!shroudRevealed && !Covered)
			{
				alpha += 0.1f;

				if (alpha > 1f)
					alpha = 1f;
			}
		}

		public override void Render()
		{
			if (Uncovered)
				return;

			shroudRenderable.SetPosition(new CPos(pos.X * 512 - 256, pos.Y * 512 - 256, 0));
			shroudRenderable.SetColor(new Color(1f, 1f, 1f, alpha));

			shroudRenderable.PushToBatchRenderer();
		}
	}
}
