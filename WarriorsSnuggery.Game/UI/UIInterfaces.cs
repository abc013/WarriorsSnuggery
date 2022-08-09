using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WarriorsSnuggery.UI
{
	interface IDebugRenderable
	{
		public void DebugRender();
	}

	interface ICheckKeys
	{
		public void KeyDown(Keys key, bool isControl, bool isShift, bool isAlt);
	}
}
