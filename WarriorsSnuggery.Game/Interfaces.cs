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

	public interface IDisableTooltip
	{
		void DisableTooltip();
	}
}
