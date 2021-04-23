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

		public readonly WallType Type;

		public readonly MPos LayerPosition;
		public readonly MPos TerrainPosition;
		readonly CPos renderPos;

		public readonly bool IsHorizontal;

		readonly WallLayer layer;

		public CPos EndPointA => Physics.Position - (IsHorizontal ? new CPos(2 * Physics.Type.RadiusX, Physics.Type.RadiusY, 0) : new CPos(Physics.Type.RadiusX, 2 * Physics.Type.RadiusY, 0));
		public CPos EndPointB => Physics.Position - (IsHorizontal ? new CPos(0, Physics.Type.RadiusY, 0) : new CPos(Physics.Type.RadiusX, 0, 0));

		BatchObject renderable;
		Color color = Color.White;

		public int Health
		{
			get => health;
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
		float healthPercentage => health / (float)Type.Health;

		byte neighborState;
		DamageState damageState = DamageState.NONE;

		public Wall(MPos position, WallLayer layer, WallType type) : base(CPos.Zero, null)
		{
			this.layer = layer;

			Type = type;
			health = type.Health;

			LayerPosition = position;
			TerrainPosition = LayerPosition / new MPos(2, 1);

			IsHorizontal = LayerPosition.X % 2 != 0;

			var pos = LayerPosition.ToCPos() / new CPos(2, 1, 1);
			renderPos = pos;
			Position = pos + new CPos(0, -512, 0);

			if (IsHorizontal)
			{
				renderPos += new CPos(-512, -1536, 0);
			}
			else
			{
				ZOffset += 2048;
				renderPos += new CPos(-83, -512, 0);
			}
			if (type.IsOnFloor)
				ZOffset -= 2048;

			Physics = getPhysics(position, type);
		}

		SimplePhysics getPhysics(MPos position, WallType type)
		{
			if (!type.Blocks || type.IsOnFloor)
				return SimplePhysics.Empty;

			var isHorizontal = position.X % 2 != 0;

			return new SimplePhysics(this, isHorizontal ? type.HorizontalPhysicsType : type.VerticalPhysicsType);
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
			bool newRenderable = false;
			if (healthPercentage < 0.25f)
			{
				newRenderable = Type.DamagedImage1 != null && damageState != DamageState.HEAVY;

				damageState = DamageState.HEAVY;
			}
			else if (healthPercentage < 0.75f)
			{
				newRenderable = Type.DamagedImage2 != null && damageState != DamageState.LIGHT;

				damageState = DamageState.LIGHT;
			}

			if (newRenderable)
				setRenderable();
		}

		public void SetNeighborState(byte nS, bool enabled)
		{
			if (enabled)
				neighborState |= nS;
			else
				neighborState &= (byte)~nS;

			setRenderable();
		}

		void setRenderable()
		{
			switch (damageState)
			{
				case DamageState.NONE:
					renderable = new BatchObject(Type.GetTexture(IsHorizontal, neighborState));
					break;
				case DamageState.HEAVY:
					if (Type.DamagedImage2 != null)
						renderable = new BatchObject(Type.GetDamagedTexture(IsHorizontal, true, neighborState));
					else if (Type.DamagedImage1 != null)
						renderable = new BatchObject(Type.GetDamagedTexture(IsHorizontal, false, neighborState));
					else
						renderable = new BatchObject(Type.GetTexture(IsHorizontal, neighborState));
					break;
				case DamageState.LIGHT:
					if (Type.DamagedImage1 != null)
						renderable = new BatchObject(Type.GetDamagedTexture(IsHorizontal, false, neighborState));
					else
						renderable = new BatchObject(Type.GetTexture(IsHorizontal, neighborState));
					break;
			}
			renderable.SetPosition(renderPos);
			renderable.SetColor(color);
		}

		public override bool CheckVisibility()
		{
			renderable.Visible = VisibilitySolver.IsVisibleIgnoringBounds(TerrainPosition) || VisibilitySolver.IsVisibleIgnoringBounds(TerrainPosition - new MPos(0, 1));

			return renderable.Visible;
		}
	}
}
