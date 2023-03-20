using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.UI.Screens
{
	public abstract class Screen : ITick, IRenderable
	{
		protected static int Width => (int)(WindowInfo.UnitWidth * Constants.TileSize);
		protected const int Height = (int)(WindowInfo.UnitHeight * Constants.TileSize);

		protected static int Right => (int)(WindowInfo.UnitWidth * Constants.TileSize / 2);
		protected static int Left => -Right;

		protected const int Top = -(int)(WindowInfo.UnitHeight * Constants.TileSize / 2);
		protected const int Bottom = -Top;

		protected readonly UIText Title;

		readonly List<ITick> tickables = new List<ITick>();
		readonly List<IRenderable> renderables = new List<IRenderable>();
		readonly List<IDebugRenderable> debugRenderables = new List<IDebugRenderable>();
		readonly List<ICheckKeys> keyCheckers = new List<ICheckKeys>();

		readonly Color darkness;

		public Screen(string title, int darkness = 128)
		{
			Title = new UIText(FontManager.Header, TextOffset.MIDDLE, title) { Scale = 1.2f };
			Add(Title);

			this.darkness = Color.Black.WithAlpha(darkness / 255f);
		}

		public virtual bool CursorOnUI()
		{
			return false;
		}

		public virtual void Show() { }

		public virtual void Hide() { }

		public virtual void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt)
		{
			foreach (var @object in keyCheckers)
				@object.KeyDown(key, isControl, isShift, isAlt);
		}

		public virtual void Tick()
		{
			foreach (var @object in tickables)
				@object.Tick();
		}

		public virtual void Render()
		{
			if (darkness.A != 0)
				ColorManager.DrawFullscreenRect(darkness);

			foreach (var @object in renderables)
				@object.Render();
		}

		public virtual void DebugRender()
		{
			foreach (var @object in debugRenderables)
				@object.DebugRender();
		}

		protected void Add(object @object)
		{
			if (@object == null)
				return;

			if (@object is not UIPositionable)
				throw new InvalidOperationException($"Unable to add object of type '{@object.GetType()}' to Screen.");

			if (@object is ITick tick)
				tickables.Add(tick);

			if (@object is IRenderable render)
				renderables.Add(render);

			if (@object is IDebugRenderable debugRender)
				debugRenderables.Add(debugRender);

			if (@object is ICheckKeys checkKey)
				keyCheckers.Add(checkKey);
		}

		protected void Remove(object @object)
		{
			if (@object == null)
				return;

			if (@object is not UIPositionable)
				throw new InvalidOperationException($"Unable to remove object of type '{@object.GetType()}' to Screen.");

			if (@object is ITick tick)
				tickables.Remove(tick);

			if (@object is IRenderable render)
				renderables.Remove(render);

			if (@object is IDebugRenderable debugRender)
				debugRenderables.Remove(debugRender);

			if (@object is ICheckKeys checkKey)
				keyCheckers.Remove(checkKey);
		}
	}
}
