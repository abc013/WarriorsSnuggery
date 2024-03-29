using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Particles;
using WarriorsSnuggery.Physics;

namespace WarriorsSnuggery.Objects
{
	public class Wall : PhysicsObject, ISaveable
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

		// Saved separately
		public readonly WallType Type;

		[Save("Position")]
		public readonly WPos LayerPosition;
		public readonly MPos TerrainPosition;

		public readonly bool IsHorizontal;

		readonly World world;

		public CPos EndPointA => Physics.Position - Physics.Boundaries;
		public CPos EndPointB => Physics.Position + Physics.Boundaries;

		[Save, DefaultValue(0)]
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

		public Wall(WPos position, World world, WallType type)
		{
			this.world = world;

			Type = type;
			health = type.Health;

			LayerPosition = position;

			TerrainPosition = LayerPosition.ToMPos();
			IsHorizontal = LayerPosition.IsHorizontal();

			Position = LayerPosition.ToCPos();

			ZOffset -= 512;
			if (!IsHorizontal)
				ZOffset += type.Height;

			if (type.IsOnFloor)
				ZOffset = -2048;

			Physics = getPhysics(type);
		}

		SimplePhysics getPhysics(WallType type)
		{
			if (!type.Blocks || type.IsOnFloor)
				return SimplePhysics.Empty;

			return new SimplePhysics(this, IsHorizontal ? type.HorizontalPhysicsType : type.VerticalPhysicsType);
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
					world.WallLayer.Set(WallCache.Create(LayerPosition, world, Type.WallOnDeath));
				else
					world.WallLayer.Remove(LayerPosition);

				spawnDamageParticles();
				return;
			}

			checkDamageState();
		}

		void checkDamageState()
		{
			var previousDamageState = damageState;

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

			if (previousDamageState != damageState)
				spawnDamageParticles();
		}

		void spawnDamageParticles()
		{
			if (Type.DebrisParticleCount == 0 || Type.DebrisParticles == null || Physics.IsEmpty)
				return;

			var particleSqrt = (int)Math.Sqrt(Type.DebrisParticleCount);

			var distH = Type.Height / particleSqrt;
			var distI = new CPos(Physics.Boundaries.X * 2 / particleSqrt, Physics.Boundaries.Y * 2 / particleSqrt, 0);

			for (int h = 0; h < particleSqrt; h++)
			{
				var height = h * distH;
				for (int i = 0; i < particleSqrt; i++)
				{
					var position = Physics.Position - new CPos(Physics.Boundaries.X, Physics.Boundaries.Y, height) + distI * i;

					var particle = ParticleCache.Create(world, Type.DebrisParticles, position);
					particle.ZOffset += Type.Height / 2;
					world.Add(particle);
				}
			}
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

			Renderable = new BatchObject(Type.GetTexture(IsHorizontal, neighborState, info));
			Renderable.SetPosition(Position + getTextureOffset(info, IsHorizontal));
			Renderable.SetColor(Color);
			Renderable.SetTextureFlags(TextureFlags);
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

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			saver.AddSaveFields(this);
			saver.Add(nameof(Type), Type.ID);

			return saver;
		}
	}
}
