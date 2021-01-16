using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Terrain : ITickRenderable, ICheckVisible
	{
		readonly StaticBatchRenderable renderable;
		readonly BatchSequence overlay;
		readonly StaticBatchRenderable[] edges, corners;

		readonly bool[] edgesVisible = new bool[4];
		static readonly CPos[] edgePositions = new CPos[4]
		{
			new CPos(0, -1024, 0),
			new CPos(1024, 0, 0),
			new CPos(0, 1024, 0),
			new CPos(-1024, 0, 0)
		};
		readonly bool[] cornersVisible = new bool[4];
		static readonly CPos[] cornerPositions = new CPos[4]
		{
			new CPos(1024, -1024, 0),
			new CPos(1024, 1024, 0),
			new CPos(-1024, 1024, 0),
			new CPos(-1024, -1024, 0)
		};

		readonly World world;
		public readonly MPos Position;
		public readonly TerrainType Type;
		bool firstChecked;

		public Terrain(World world, MPos position, TerrainType type)
		{
			this.world = world;
			Position = position;
			Type = type;

			renderable = new StaticBatchRenderable(Position.ToCPos(), VAngle.Zero, type.Texture, Color.White);
			if (type.Texture_Overlay != null)
			{
				overlay = new BatchSequence(type.Texture_Overlay, Color.White, type.Overlay.Tick, startRandom: true);
				overlay.SetPosition(Position.ToCPos());
			}
			edges = new StaticBatchRenderable[4];
			corners = new StaticBatchRenderable[4];
		}

		public void Tick()
		{
			overlay?.Tick();
		}

		public void Render()
		{
			if (!renderable.Visible)
				return;

			if (Type.Overlaps)
			{
				for (int i = 0; i < 4; i++)
				{
					if (edgesVisible[i] && edges[i] != null)
						edges[i].PushToBatchRenderer();
				}

				for (int i = 0; i < 4; i++)
				{
					if (cornersVisible[i] && corners[i] != null)
						corners[i].PushToBatchRenderer();
				}
			}
			renderable.PushToBatchRenderer();
			overlay?.PushToBatchRenderer();
		}

		public bool CheckVisibility()
		{
			return CheckVisibility(false);
		}

		public bool CheckVisibility(bool checkEdges = false)
		{
			renderable.Visible = VisibilitySolver.IsVisible(Position);
			if (!firstChecked || checkEdges)
			{
				CheckEdgeVisibility();
				firstChecked = true;
			}

			return renderable.Visible;
		}

		public void CheckEdgeVisibility()
		{
			if (!Type.Overlaps)
				return;

			// Special case: Map size is 1x1
			var visible = world.Map.Bounds.X != 1 && world.Map.Bounds.Y != 1;
			for (int i = 0; i < 4; i++)
			{
				edgesVisible[i] = visible;
				cornersVisible[i] = visible;
			}

			if (!visible)
				return;

			bool isEdgeLeft = Position.X == 0;
			bool isEdgeRight = Position.X >= world.Map.Bounds.X - 1;
			bool isEdgeTop = Position.Y == 0;
			bool isEdgeBottom = Position.Y >= world.Map.Bounds.Y - 1;
			if (isEdgeRight)
			{
				edgesVisible[1] = false;
				cornersVisible[0] = false;
				cornersVisible[1] = false;

				var terrainLeft = world.TerrainLayer.Terrain[Position.X - 1, Position.Y].Type;
				edgesVisible[3] = !(terrainLeft.ID == Type.ID || terrainLeft.OverlapHeight > Type.OverlapHeight);
			}
			else if (isEdgeLeft)
			{
				edgesVisible[3] = false;
				cornersVisible[2] = false;
				cornersVisible[3] = false;
			}
			else
			{
				var terrainLeft = world.TerrainLayer.Terrain[Position.X - 1, Position.Y].Type;
				edgesVisible[3] = !(terrainLeft.ID == Type.ID || terrainLeft.OverlapHeight > Type.OverlapHeight);
				var terrainRight = world.TerrainLayer.Terrain[Position.X + 1, Position.Y].Type;
				edgesVisible[1] = !(terrainRight.ID == Type.ID || terrainRight.OverlapHeight > Type.OverlapHeight);
			}

			if (isEdgeBottom)
			{
				edgesVisible[2] = false;
				cornersVisible[1] = false;
				cornersVisible[2] = false;
				var terrainTop = world.TerrainLayer.Terrain[Position.X, Position.Y - 1].Type;
				edgesVisible[0] = !(terrainTop.ID == Type.ID || terrainTop.OverlapHeight > Type.OverlapHeight);
			}
			else if (isEdgeTop)
			{
				edgesVisible[0] = false;
				cornersVisible[0] = false;
				cornersVisible[3] = false;
				var terrainBottom = world.TerrainLayer.Terrain[Position.X, Position.Y + 1].Type;
				edgesVisible[2] = !(terrainBottom.ID == Type.ID || terrainBottom.OverlapHeight > Type.OverlapHeight);
			}
			else
			{
				var terrainTop = world.TerrainLayer.Terrain[Position.X, Position.Y - 1].Type;
				edgesVisible[0] = !(terrainTop.ID == Type.ID || terrainTop.OverlapHeight > Type.OverlapHeight);
				var terrainBottom = world.TerrainLayer.Terrain[Position.X, Position.Y + 1].Type;
				edgesVisible[2] = !(terrainBottom.ID == Type.ID || terrainBottom.OverlapHeight > Type.OverlapHeight);
			}

			if (!isEdgeRight && !isEdgeTop)
			{
				var terrainRightUp = world.TerrainLayer.Terrain[Position.X + 1, Position.Y - 1].Type;
				cornersVisible[0] = !(terrainRightUp.ID == Type.ID || terrainRightUp.OverlapHeight > Type.OverlapHeight);
			}

			if (!isEdgeRight && !isEdgeBottom)
			{
				var terrainRightBottom = world.TerrainLayer.Terrain[Position.X + 1, Position.Y + 1].Type;
				cornersVisible[1] = !(terrainRightBottom.ID == Type.ID || terrainRightBottom.OverlapHeight > Type.OverlapHeight);
			}

			if (!isEdgeLeft && !isEdgeBottom)
			{
				var terrainLeftBottom = world.TerrainLayer.Terrain[Position.X - 1, Position.Y + 1].Type;
				cornersVisible[2] = !(terrainLeftBottom.ID == Type.ID || terrainLeftBottom.OverlapHeight > Type.OverlapHeight);
			}

			if (!isEdgeLeft && !isEdgeTop)
			{
				var terrainLeftUp = world.TerrainLayer.Terrain[Position.X - 1, Position.Y - 1].Type;
				cornersVisible[3] = !(terrainLeftUp.ID == Type.ID || terrainLeftUp.OverlapHeight > Type.OverlapHeight);
			}

			for (int i = 0; i < 4; i++)
			{
				if (!edgesVisible[i])
				{
					edges[i] = null;
					continue;
				}

				if (edges[i] != null)
					continue;

				if (i % 2 != 0 && Type.Texture_Edge2 != null)
					edges[i] = new StaticBatchRenderable(calculateEdgeOffset(i, false), new VAngle(0, 0, i * -90), Type.Texture_Edge2, Color.White);
				else
					edges[i] = new StaticBatchRenderable(calculateEdgeOffset(i, true), new VAngle(0, 0, i * -90), Type.Texture_Edge, Color.White);
			}

			for (int i = 0; i < 4; i++)
			{
				if (!cornersVisible[i])
				{
					corners[i] = null;
					continue;
				}

				if (corners[i] != null)
					continue;

				corners[i] = new StaticBatchRenderable(calculateCornerOffset(i), new VAngle(0, 0, i * -90), Type.Texture_Corner, Color.White);
			}
		}

		CPos calculateEdgeOffset(int rot, bool isHorizontal)
		{
			var position = Position.ToCPos() + edgePositions[rot];

			var offset = isHorizontal ? Type.EdgeOffset : Type.VerticalEdgeOffset;
			if (rot % 2 != 0)
				return position + (rot == 1 ? new CPos(-offset.Y, 0, 0) : new CPos(offset.Y, 0, 0));

			return position + (rot == 0 ? new CPos(0, offset.Y, 0) : new CPos(0, -offset.Y, 0));
		}

		CPos calculateCornerOffset(int rot)
		{
			var position = Position.ToCPos() + cornerPositions[rot];
			if (rot % 2 != 0)
				return position + (rot == 1 ? -Type.CornerOffset : Type.CornerOffset);

			return position + (rot == 0 ? new CPos(-Type.CornerOffset.X, Type.CornerOffset.Y, 0) : new CPos(Type.CornerOffset.X, -Type.CornerOffset.Y, 0));
		}
	}
}
