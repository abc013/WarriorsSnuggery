using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects;

namespace WarriorsSnuggery.UI.Objects.Editor
{
	public class TerrainEditorWidget : UIObject
	{
		public override CPos Position
		{
			get => base.Position;
			set
			{
				base.Position = value;

				list.Position = value + new CPos(0, 2048, 0);
			}
		}

		readonly PanelList list;

		public TerrainType CurrentType { get; private set; }

		public TerrainEditorWidget() : base()
		{
			list = new PanelList(new MPos(2048, 4096), new MPos(512, 512), "wooden");
			foreach (var a in TerrainCreator.Types.Values)
				list.Add(new PanelItem(new BatchObject(a.Texture, Color.White), new MPos(512, 512), a.ID.ToString(), new string[0], () => CurrentType = a));
		}

		public void DisableTooltip()
		{
			list.DisableTooltip();
		}

		public override void Render()
		{
			list.Render();
		}

		public override void DebugRender()
		{
			list.DebugRender();
		}

		public override void Tick()
		{
			list.Tick();
		}
	}
}
