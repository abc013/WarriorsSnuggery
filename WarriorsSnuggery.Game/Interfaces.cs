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
		bool CheckVisibility();
	}

	public interface ITickRenderable : ITick, IRenderable { }

	public interface IDisableTooltip
	{
		void DisableTooltip();
	}
}
