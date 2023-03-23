using WarriorsSnuggery.Loader;

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

	public interface ISaveable
	{
		TextNodeSaver Save();
	}

	public interface ILoadable
	{
		void Load(TextNodeInitializer init);
	}
}
