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

			renderable = new StaticBatchRenderable(Position.ToCPos(), VAngle.Zero, type.Texture);
			if (type.Overlay != null)
			{
				overlay = new BatchSequence(type.Overlay, startRandom: true);
				overlay.SetPosition(Position.ToCPos());
			}
			edges = new StaticBatchRenderable[4];
			corners = new StaticBatchRenderable[4];
		}

		public void Tick()
		{
			if (!world.Game.Editor && Type.Particles != null)
			{
				if (Program.SharedRandom.NextDouble() <= Type.ParticleProbability)
					world.Add(Type.Particles.Create(world, Position.ToCPos()));
			}

			if (overlay == null)
				return;

			if (Type.UnifyOverlayTick)
				overlay.SetTick((int)Window.GlobalTick);
			else
				overlay.Tick();
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
			overlay?.Render();
		}

		public bool CheckVisibility()
		{
			return CheckVisibility(false);
		}

		public bool CheckVisibility(bool checkEdges = false)
		{
			renderable.Visible = CameraVisibility.IsVisible(Position);
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

			bool overlapVisible(MPos pos)
			{
				var terrain = world.TerrainLayer.Terrain[pos.X, pos.Y].Type;
				return !(terrain.ID == Type.ID || terrain.OverlapHeight > Type.OverlapHeight);
			};

			bool overlapBlockingWall(MPos wallPos)
			{
				var wall = world.WallLayer.Walls[wallPos.X, wallPos.Y];

				return wall != null && wall.Type.BlocksTerrainOverlap;
			};

			var bounds = world.Map.Bounds;
			var wallPosition = Position * new MPos(2, 1);

			var isEdgeTop = Position.Y == 0 || overlapBlockingWall(wallPosition + new MPos(1, 0));
			var isEdgeRight = Position.X >= bounds.X - 1 || overlapBlockingWall(wallPosition + new MPos(2, 0));
			var isEdgeBottom = Position.Y >= bounds.Y - 1 || overlapBlockingWall(wallPosition + new MPos(1, 1));
			var isEdgeLeft = Position.X == 0 || overlapBlockingWall(wallPosition);

			// Edges
			edgesVisible[0] = !isEdgeTop && overlapVisible(new MPos(Position.X, Position.Y - 1));
			edgesVisible[1] = !isEdgeRight && overlapVisible(new MPos(Position.X + 1, Position.Y));
			edgesVisible[2] = !isEdgeBottom && overlapVisible(new MPos(Position.X, Position.Y + 1));
			edgesVisible[3] = !isEdgeLeft && overlapVisible(new MPos(Position.X - 1, Position.Y));

			// Corners
			cornersVisible[0] = (!isEdgeRight && !isEdgeTop) && overlapVisible(new MPos(Position.X + 1, Position.Y - 1));
			cornersVisible[1] = (!isEdgeRight && !isEdgeBottom) && overlapVisible(new MPos(Position.X + 1, Position.Y + 1));
			cornersVisible[2] = (!isEdgeLeft && !isEdgeBottom) && overlapVisible(new MPos(Position.X - 1, Position.Y + 1));
			cornersVisible[3] = (!isEdgeLeft && !isEdgeTop) && overlapVisible(new MPos(Position.X - 1, Position.Y - 1));

			updateRenderables();
		}

		void updateRenderables()
		{
			for (int i = 0; i < 4; i++)
			{
				if (!edgesVisible[i])
				{
					edges[i] = null;
					continue;
				}

				if (edges[i] != null)
					continue;

				if (i % 2 != 0 && Type.VerticalEdgeTexture != null)
					edges[i] = new StaticBatchRenderable(calculateEdgeOffset(i, false), new VAngle(0, 0, i * -90), Type.VerticalEdgeTexture);
				else
					edges[i] = new StaticBatchRenderable(calculateEdgeOffset(i, true), new VAngle(0, 0, i * -90), Type.EdgeTexture);
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

				corners[i] = new StaticBatchRenderable(calculateCornerOffset(i), new VAngle(0, 0, i * -90), Type.CornerTexture);
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
