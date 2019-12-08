using System;

namespace WarriorsSnuggery
{
	public interface ITick
	{
		void Tick();
	}

	public interface IRenderable
	{
		void Render();
	}

	public interface ICheckVisible
	{
		void CheckVisibility();
	}

	public interface ITickRenderable : ITick, IRenderable { }

	public interface ITickRenderDisposable : ITick, IRenderable, IDisposable { }

	public interface IDisableTooltip
	{
		void DisableTooltip();
	}
}
