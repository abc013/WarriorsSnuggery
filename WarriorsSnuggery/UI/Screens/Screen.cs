using OpenToolkit.Windowing.Common.Input;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI
{
	public abstract class Screen : ITickRenderable
	{
		protected readonly UITextLine Title;

		protected readonly List<UIObject> Content = new List<UIObject>();

		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new UITextLine(CPos.Zero, FontManager.Papyrus24, TextOffset.MIDDLE);
			Title.SetText(title);
			Title.Scale = 1.2f;

			this.darkness = new Color(0, 0, 0, darkness);
		}

		public virtual bool CursorOnUI()
		{
			return false;
		}

		public virtual void Show() { }

		public virtual void Hide() { }

		public virtual void KeyDown(Key key, bool isControl, bool isShift, bool isAlt) { }

		public virtual void Tick()
		{
			foreach (var content in Content)
				content.Tick();
		}

		public virtual void Render()
		{
			ColorManager.DrawFullscreenRect(darkness);
			Title.Render();

			foreach (var content in Content)
				content.Render();
		}

		public virtual void DebugRender()
		{
			foreach (var content in Content)
				content.DebugRender();
		}
	}
}
