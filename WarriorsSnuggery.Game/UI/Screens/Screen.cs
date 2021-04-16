using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Screens
{
	public abstract class Screen : ITickRenderable
	{
		protected readonly UITextLine Title;

		readonly List<UIObject> content = new List<UIObject>();
		protected void Add(UIObject @object) => content.Add(@object);
		protected void Remove(UIObject @object) => content.Remove(@object);

		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new UITextLine(FontManager.Papyrus24, TextOffset.MIDDLE) { Scale = 1.2f };
			Title.SetText(title);

			this.darkness = new Color(0, 0, 0, darkness);
		}

		public virtual bool CursorOnUI()
		{
			return false;
		}

		public virtual void Show() { }

		public virtual void Hide() { }

		public virtual void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt) { }

		public virtual void Tick()
		{
			foreach (var @object in content)
				@object.Tick();
		}

		public virtual void Render()
		{
			if (darkness.A != 0)
				ColorManager.DrawFullscreenRect(darkness);
			Title.Render();

			foreach (var @object in content)
				@object.Render();
		}

		public virtual void DebugRender()
		{
			foreach (var @object in content)
				@object.DebugRender();
		}
	}
}
