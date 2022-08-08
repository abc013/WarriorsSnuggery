using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class AnimatedSpritePartInfo : PartInfo
	{
		public readonly Texture[] Textures;

		[Require, Desc("Name of the texture file.")]
		public readonly PackageFile Name;

		[Require, Desc("Size of a single frame.")]
		public readonly MPos Dimensions;

		[Desc("Count of facings that the animation has.")]
		public readonly int Facings = 1;

		[Desc("Frame rate (Speed of animation).")]
		public readonly int Tick = 20;

		[Desc("If true, a random start frame of the animation will be picked.")]
		public readonly bool StartRandom = false;

		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition = new Condition("True");

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

		public AnimatedSpritePartInfo(PartInitSet set) : base(set)
		{
			if (Name != null)
				Textures = new TextureInfo(Name, TextureType.ANIMATION, Dimensions, Tick).GetTextures();
		}

		public override ActorPart Create(Actor self)
		{
			return new AnimatedSpritePart(self, this);
		}
	}

	public class AnimatedSpritePart : ActorPart, IPartRenderable, ITick, ITickInEditor, INoticeMove, INoticeAttack
	{
		readonly AnimatedSpritePartInfo info;

		readonly BatchSequence[] renderables;
		BatchSequence renderable;
		int currentFacing;
		readonly Color variation;
		Color cachedColor;
		TextureFlags cachedFlags;
		float angle;

		public AnimatedSpritePart(Actor self, AnimatedSpritePartInfo info) : base(self)
		{
			this.info = info;
			renderables = new BatchSequence[info.Facings];
			var frameCountPerIdleAnim = info.Textures.Length / info.Facings;

			if (frameCountPerIdleAnim * info.Facings != info.Textures.Length)
				throw new InvalidNodeException($"Idle Frame '{info.Textures.Length}' count cannot be matched with the given Facings '{info.Facings}'.");

			for (int i = 0; i < renderables.Length; i++)
			{
				var anim = new Texture[frameCountPerIdleAnim];
				for (int x = 0; x < frameCountPerIdleAnim; x++)
					anim[x] = info.Textures[i * frameCountPerIdleAnim + x];

				renderables[i] = new BatchSequence(anim, info.Tick, startRandom: info.StartRandom);
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

		public void OnMove(CPos old, CPos speed)
		{
			currentFacing = Angle.ToFacing(angle, info.Facings);
		}

		public void OnAttack(CPos target, Weapon weapon)
		{
			currentFacing = Angle.ToFacing(angle, info.Facings);
		}

		public void Tick()
		{
			if (self.Angle != angle)
			{
				angle = self.Angle;
				currentFacing = Angle.ToFacing(angle, info.Facings);
			}
			var last = renderable;
			renderable = (BatchSequence)GetRenderable(self.Actions, currentFacing);

			if (last == null)
				renderable?.Reset();

			renderable?.Tick();
		}

		public void Render()
		{
			if (renderable == null)
				return;

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
