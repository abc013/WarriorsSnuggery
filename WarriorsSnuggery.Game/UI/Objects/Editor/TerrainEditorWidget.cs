using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects.Editor
{
	public sealed class TerrainEditorWidget : UIPositionable, IDisableTooltip, ITick, IRenderable
	{
		public override UIPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new UIPos(0, 2048);
			}
		}

		readonly PanelList list;

		public TerrainType CurrentType { get; private set; }

		public TerrainEditorWidget() : base()
		{
			list = new PanelList(new UIPos(2048, 4096), new UIPos(512, 512), "wooden");
			foreach (var a in TerrainCache.Types.Values)
				list.Add(new PanelListItem(new BatchObject(a.Texture), new UIPos(512, 512), a.ID.ToString(), new string[0], () => CurrentType = a));
		}

		public void DisableTooltip()
		{
			list.DisableTooltip();
		}

		public void Render()
		{
			list.Render();
		}

		public override void DebugRender()
		{
			list.DebugRender();
		}

		public void Tick()
		{
			list.Tick();
		}
	}
}
