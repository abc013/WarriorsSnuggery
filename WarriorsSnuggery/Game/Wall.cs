using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject
	{
		public readonly MPos LayerPosition;
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
				if (value >= Type.Health)
					value = Type.Health;

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
			var renderPos = pos;
			Position = pos + new CPos(0, -512, 0);
			isHorizontal = position.X % 2 != 0;
			if (isHorizontal)
			{
				Physics.Position += new CPos(0, 512, 0);
				renderPos += new CPos(-512, -1536, 0);
			}
			else
			{
				Position += new CPos(0, 0, 2048);
				Physics.Position += new CPos(0, 1024, 0);
				renderPos += new CPos(-83, -512, 0);
			}
			Type = type;
			health = type.Health;

			Renderable.SetPosition(renderPos);
			if (Type.DamagedImage1 != null)
			{
				damaged1 = new BatchObject(type.GetDamagedTexture(isHorizontal, false), Color.White);
				damaged1.SetPosition(renderPos);
			}
			if (Type.DamagedImage2 != null)
			{
				damaged2 = new BatchObject(type.GetDamagedTexture(isHorizontal, true), Color.White);
				damaged2.SetPosition(renderPos);
			}
		}

		public override void Render()
		{
			if (!Type.Invincible && healthPercentage < 0.75f)
			{
				if (healthPercentage >= 0.25f || damaged2 == null)
				{
					if (damaged1 != null)
						damaged1.PushToBatchRenderer();
					else
						base.Render();
				}
				else
					damaged2.PushToBatchRenderer();
			}
			else
				base.Render();
		}

		public override void SetColor(Color color)
		{
			base.SetColor(color);
			if (damaged1 != null)
				damaged1.SetColor(color);
			if (damaged2 != null)
				damaged2.SetColor(color);
		}

		public void Damage(int damage)
		{
			if (Type.Invincible)
				return;

			health -= damage;

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
	}
}
