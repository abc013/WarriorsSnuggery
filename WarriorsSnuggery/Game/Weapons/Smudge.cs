using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Smudge : IPositionable, ITickRenderable
	{
		readonly BatchRenderable renderable;

		public virtual CPos Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
				renderable.SetPosition(position);
			}
		}
		CPos position;

		public virtual VAngle Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				rotation = value;
				renderable.SetRotation(rotation);
			}
		}
		VAngle rotation;

		public virtual float Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
				renderable.SetScale(new Vector(scale, scale, scale));
			}
		}
		float scale = 1f;

		public virtual Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
				renderable.SetColor(color);
			}
		}
		Color color = Color.White;

		public bool Disposed;
		public bool IsDissolving;

		readonly int maxDissolve;
		int dissolveTime;

		public Smudge(BatchRenderable renderable, int dissolve)
		{
			this.renderable = renderable;
			maxDissolve = dissolve;
		}

		public void Tick()
		{
			if (IsDissolving)
			{
				if (dissolveTime-- > 0)
					renderable.SetColor(new Color(1f, 1f, 1f, dissolveTime / (float)maxDissolve));
				else
					Disposed = true;
			}
		}

		public void Render()
		{
			renderable.PushToBatchRenderer();
		}

		public void BeginDissolve()
		{
			IsDissolving = true;
			dissolveTime = maxDissolve;
		}
	}
}
