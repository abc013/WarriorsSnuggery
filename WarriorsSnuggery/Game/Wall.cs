using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject
	{
		public readonly MPos LayerPosition;
		readonly CPos renderPosition;
		readonly WallLayer layer;

		public readonly WallType Type;
		readonly BatchObject damaged1;
		readonly BatchObject damaged2;

		readonly bool isHorizontal;
		public int Health
		{
			get { return health; }
			set
			{
				if (Type.Invincible)
					return;

				health = value;
			}
		}

		int health;
		float healthPercentage
		{
			get { return health / (float)Type.Health; }
		}

		public Wall(MPos position, WallLayer layer, WallType type) : base(position.ToCPos(), new BatchObject(type.GetTexture(position.X % 2 != 0), Color.White), type.Blocks ? new Physics.SimplePhysics(position.ToCPos() / new CPos(2, 1, 1), 0, position.X % 2 != 0 ? WarriorsSnuggery.Physics.Shape.LINE_HORIZONTAL : WarriorsSnuggery.Physics.Shape.LINE_VERTICAL, 512, 512, type.Height) : new Physics.SimplePhysics(position.ToCPos() / new CPos(2, 1, 1), 0, WarriorsSnuggery.Physics.Shape.NONE, 0, 0, 0))
		{
			LayerPosition = position;
			this.layer = layer;

			var pos = position.ToCPos() / new CPos(2, 1, 1);
			Position = pos + new CPos(0, -512, 0);
			isHorizontal = position.X % 2 != 0;
			if (isHorizontal)
			{
				Physics.Position += new CPos(0, 512, 0);
				renderPosition = pos + new CPos(-512, -1536, 0);
			}
			else
			{
				Position += new CPos(0, 0, 2048);
				Physics.Position += new CPos(0, 1024, 0);
				renderPosition = pos + new CPos(-83, -512, 0);
			}
			Type = type;
			health = type.Health;

			if (Type.DamagedImage1 != null)
				damaged1 = new BatchObject(type.GetDamagedTexture(isHorizontal, false), Color.White);
			if (Type.DamagedImage2 != null)
				damaged2 = new BatchObject(type.GetDamagedTexture(isHorizontal, true), Color.White);
		}

		public override void Render()
		{
			if (!Type.Invincible && healthPercentage < 0.75f)
			{
				if (healthPercentage >= 0.25f)
				{
					if (damaged1 != null)
					{
						damaged1.SetPosition(renderPosition);
						damaged1.PushToBatchRenderer();
					}
					else
					{
						Renderable.SetPosition(renderPosition);
						base.Render();
					}
				}
				else
				{
					if (damaged2 != null)
					{
						damaged2.SetPosition(renderPosition);
						damaged2.PushToBatchRenderer();
					}
					else if (damaged1 != null)
					{
						damaged1.SetPosition(renderPosition);
						damaged1.PushToBatchRenderer();
					}
					else
					{
						Renderable.SetPosition(renderPosition);
						base.Render();
					}
				}
			}
			else
			{
				Renderable.SetPosition(renderPosition);
				base.Render();
			}
		}

		public void Damage(int damage)
		{
			if (Type.Invincible)
				return;

			health -= (int)Math.Round(damage * Type.DamagePenetration);

			if (health <= 0)
			{
				Dispose();
				layer.Remove(LayerPosition);
			}
		}

		public override void CheckVisibility()
		{
			Renderable.Visible = VisibilitySolver.IsVisibleIgnoringBounds(new MPos(LayerPosition.X / 2, LayerPosition.Y - 1));

			if (!isHorizontal)
				Renderable.Visible |= VisibilitySolver.IsVisibleIgnoringBounds(new MPos(LayerPosition.X / 2, LayerPosition.Y));
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
