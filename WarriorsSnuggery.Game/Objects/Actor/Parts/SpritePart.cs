using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class SpritePartInfo : PartInfo
	{
		public readonly Texture[] Textures;

		[Require, Desc("The texture details.")]
		public readonly TextureInfo Texture;

		[Desc("If true, a random start frame of the animation will be picked.")]
		public readonly bool StartRandom = false;

		[Desc("Count of facings that the sprite has.")]
		public readonly int Facings = 1;

		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		[Desc("Offset of the sprite.")]
		public readonly CPos Offset;

		[Desc("Use Sprite as preview in e.g. the editor.")]
		public readonly bool UseAsPreview;

		[Desc("Color to multiply the sprite with.")]
		public readonly Color Color = Color.White;

		[Desc("Random Color to add to the sprite.", "Alpha will not be applied.")]
		public readonly Color ColorVariation = Color.Black;

		[Desc("Determines whether to ignore ambient light.", "This can be used for 'glow in the dark' effects.")]
		public readonly bool IgnoreAmbience;

		public SpritePartInfo(PartInitSet set) : base(set)
		{
			Textures = Texture.GetTextures();
		}

		public override ActorPart Create(Actor self)
		{
			return new SpritePart(self, this);
		}
	}

	public class SpritePart : ActorPart, IPartRenderable, ITick
	{
		readonly SpritePartInfo info;

		readonly BatchRenderable[] renderables;
		BatchRenderable renderable;
		readonly Color variation;
		Color cachedColor;
		TextureFlags cachedFlags;

		int currentFacing;
		float angle;

		public SpritePart(Actor self, SpritePartInfo info) : base(self)
		{
			this.info = info;
			renderables = new BatchRenderable[info.Facings];
			var frameCountPerIdleAnim = info.Textures.Length / info.Facings;

			if (frameCountPerIdleAnim * info.Facings != info.Textures.Length)
				throw new InvalidNodeException($"Idle Frame '{info.Textures.Length}' count cannot be matched with the given Facings '{info.Facings}'.");

			for (int i = 0; i < renderables.Length; i++)
			{
				var anim = new Texture[frameCountPerIdleAnim];
				for (int x = 0; x < frameCountPerIdleAnim; x++)
					anim[x] = info.Textures[i * frameCountPerIdleAnim + x];

				if (anim.Length == 0)
					throw new InvalidNodeException("Animation Frame count is zero. Make sure you set the bounds properly.");

				if (anim.Length == 1 || info.Texture.Randomized)
				{
					var index = 0;
					if (info.Texture.Randomized)
						index = self.World.Game.SharedRandom.Next(anim.Length);

					renderables[i] = new BatchObject(anim[index]);
				}
				else
				{
					renderables[i] = new BatchSequence(anim, info.Texture.Tick, startRandom: info.StartRandom);
				}
			}

			if (info.ColorVariation != Color.Black)
			{
				var random = Program.SharedRandom;
				variation = new Color((float)(random.NextDouble() - 0.5f) * info.ColorVariation.R, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.G, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.B, 0f);
			}
			cachedColor = info.Color + variation;
			cachedFlags = info.IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None;

			self.ZOffset = info.Offset.Z;
		}

		public BatchRenderable GetRenderable(ActionType actions, int facing)
		{
			if (info.Condition != null && !info.Condition.True(self))
				return null;

			return renderables[facing];
		}

		public void Render()
		{
			if (renderable != null)
			{
				if (!self.OnGround)
				{
					renderable.SetPosition(self.GraphicPositionWithoutHeight + info.Offset);
					renderable.SetColor(Color.Shadow);
					renderable.Render();
				}

				renderable.SetPosition(self.GraphicPosition + info.Offset);
				renderable.SetColor(cachedColor);
				renderable.SetTextureFlags(cachedFlags);
				renderable.Render();
			}
		}

		public void Tick()
		{
			if (self.Angle != angle)
			{
				angle = self.Angle;
				currentFacing = Angle.ToFacing(angle, info.Facings);
			}
			var last = renderable;
			renderable = GetRenderable(self.Actions, currentFacing);

			if (last == null && renderable is BatchSequence sequence)
				sequence.Reset();

			renderable?.Tick();
		}

		public void SetColor(Color color)
		{
			cachedColor = color * (info.Color + variation);
		}

		public void SetTextureFlags(TextureFlags flags)
		{
			cachedFlags = (info.IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None) | flags;
		}
	}
}
