/*
 * User: Andreas
 * Date: 30.09.2017
 * 
 */

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

	public interface ITickRenderable : ITick, IRenderable
	{

	}

	public interface IDisableTooltip
	{
		void DisableTooltip();
	}
}
