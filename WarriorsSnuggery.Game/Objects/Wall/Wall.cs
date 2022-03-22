using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Maps.Layers;
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

		public readonly bool IsHorizontal;

		readonly WallLayer layer;

		public CPos EndPointA => Physics.Position - new CPos(Physics.Boundaries.X, Physics.Boundaries.Y, 0);
		public CPos EndPointB => Physics.Position + new CPos(Physics.Boundaries.X, Physics.Boundaries.Y, 0);

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

			Position = LayerPosition.ToCPos() / new CPos(2, 1, 1);

			ZOffset -= 512;
			if (!IsHorizontal)
				ZOffset += Height;

			if (type.IsOnFloor)
				ZOffset = -2048;

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
			renderable.Render();
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
					layer.Set(WallCache.Create(LayerPosition, layer, Type.WallOnDeath));
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
				newRenderable = Type.SlightDamageTexture != null && damageState != DamageState.HEAVY;

				damageState = DamageState.HEAVY;
			}
			else if (healthPercentage < 0.75f)
			{
				newRenderable = Type.HeavyDamageTexture != null && damageState != DamageState.LIGHT;

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
			var info = Type.Texture;
			if (damageState == DamageState.LIGHT)
			{
				if (Type.SlightDamageTexture != null)
					info = Type.SlightDamageTexture;
			}
			else if (damageState == DamageState.HEAVY)
			{
				if (Type.HeavyDamageTexture != null)
					info = Type.HeavyDamageTexture;
				else if (Type.SlightDamageTexture != null)
					info = Type.SlightDamageTexture;
			}

			renderable = new BatchObject(Type.GetTexture(IsHorizontal, neighborState, info));
			renderable.SetPosition(Position + getTextureOffset(info, IsHorizontal));
			renderable.SetColor(color);
		}

		static CPos getTextureOffset(TextureInfo info, bool horizontal)
		{
			// Assumed wall width: 4px
			var width = horizontal ? info.Width : 4;
			var x = (int)-(Constants.PixelMultiplier * 512 * width);

			var height = info.Height;
			var y = (int)-(Constants.PixelMultiplier * 512 * height) + 512 * (horizontal ? -1 : 1);

			return new CPos(x, y, 0);
		}

		public override bool CheckVisibility()
		{
			renderable.Visible = CameraVisibility.IsVisibleIgnoringBounds(TerrainPosition) || CameraVisibility.IsVisibleIgnoringBounds(TerrainPosition - new MPos(0, 1));

			return renderable.Visible;
		}
	}
}
