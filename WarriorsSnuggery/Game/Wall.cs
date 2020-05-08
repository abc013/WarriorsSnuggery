using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject
	{
		enum NeighborState : byte
		{
			NONE = 0,
			TOP_LEFT = 1,
			BOTTOM_RIGHT = 2,
			BOTH = 3
		}

		enum DamageState : byte
		{
			NONE,
			LIGHT,
			HEAVY
		}

		public readonly MPos LayerPosition;
		readonly CPos renderPos;
		readonly WallLayer layer;

		public readonly WallType Type;
		BatchObject renderable;
		Color color = Color.White;

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
				checkDamageState();
			}
		}

		int health;
		float healthPercentage
		{
			get { return health / (float)Type.Health; }
		}

		byte nState;
		NeighborState neighborState;
		DamageState damageState = DamageState.NONE;

		public Wall(MPos position, WallLayer layer, WallType type) : base(position.ToCPos(), null, type.Blocks ? new SimplePhysics(position.ToCPos() / new CPos(2, 1, 1), 0, position.X % 2 != 0 ? Shape.LINE_HORIZONTAL : Shape.LINE_VERTICAL, 512, 512, type.Height) : SimplePhysics.Empty)
		{
			LayerPosition = position;
			this.layer = layer;

			var pos = position.ToCPos() / new CPos(2, 1, 1);
			renderPos = pos;
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
		}

		public override void Render()
		{
			renderable.PushToBatchRenderer();
		}

		public override void SetColor(Color color)
		{
			this.color = color;
			renderable.SetColor(color);
		}

		public void Damage(int damage)
		{
			if (Type.Invincible)
				return;

			health -= damage;

			if (health <= 0)
			{
				Dispose();
				if (Type.WallOnDeath >= 0)
					layer.Set(WallCreator.Create(LayerPosition, layer, Type.WallOnDeath));
				else
					layer.Remove(LayerPosition);

				return;
			}

			checkDamageState();
		}

		void checkDamageState()
		{
			if (Type.Invincible)
				return;

			if (healthPercentage < 0.25f)
			{
				var newRenderable = Type.DamagedImage1 != null && damageState != DamageState.HEAVY;

				damageState = DamageState.HEAVY;

				if (newRenderable)
					setRenderable();
			}
			else if (healthPercentage < 0.75f)
			{
				var newRenderable = Type.DamagedImage2 != null && damageState != DamageState.LIGHT;

				damageState = DamageState.LIGHT;

				if (newRenderable)
					setRenderable();
			}
		}

		public void SetNeighborState(byte nS, bool s1, bool enabled)
		{
			if (!enabled)
			{
				//if (neighborState == NeighborState.NONE)
				//	return;
			}
			else
			{
				//if (neighborState == NeighborState.BOTH)
				//	return;
			}

			if (enabled)
				nState |= nS;
			else
				nState &= (byte)~nS;

			if (enabled)
				neighborState |= s1 ? NeighborState.TOP_LEFT : NeighborState.BOTTOM_RIGHT;
			else
				neighborState &= s1 ? NeighborState.BOTTOM_RIGHT : NeighborState.TOP_LEFT;

			setRenderable();
		}

		void setRenderable()
		{
			switch (damageState)
			{
				case DamageState.NONE:
					renderable = new BatchObject(Type.GetTexture(isHorizontal, (byte)neighborState, nState), Color.White);
					break;
				case DamageState.LIGHT:
					renderable = new BatchObject(Type.GetDamagedTexture(isHorizontal, false, (byte)neighborState, nState), Color.White);
					break;
				case DamageState.HEAVY:
					renderable = new BatchObject(Type.GetDamagedTexture(isHorizontal, true, (byte)neighborState, nState), Color.White);
					break;
			}
			renderable.SetPosition(renderPos);
			renderable.SetColor(color);
		}

		public override void CheckVisibility()
		{
			renderable.Visible = VisibilitySolver.IsVisibleIgnoringBounds(new MPos(LayerPosition.X / 2, LayerPosition.Y - 1));

			if (!isHorizontal)
				renderable.Visible |= VisibilitySolver.IsVisibleIgnoringBounds(new MPos(LayerPosition.X / 2, LayerPosition.Y));
		}
	}
}
