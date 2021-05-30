using System;
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

		[Desc("Name of the texture file.")]
		public readonly string Name;

		[Desc("Size of a single frame.")]
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

	public class AnimatedSpritePart : RenderablePart, ITick, ITickInEditor, INoticeMove, INoticeAttack
	{
		readonly AnimatedSpritePartInfo info;

		readonly BatchSequence[] renderables;
		BatchSequence renderable;
		int currentFacing;
		readonly Color variation;
		Color cachedColor;
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

			self.ZOffset = info.Offset.Z;
		}

		public override int FacingFromAngle(float angle)
		{
			var part = Angle.MaxRange / info.Facings;

			var facing = (int)Math.Round(angle / part);
			if (facing >= info.Facings)
				facing = 0;

			return facing;
		}

		public override BatchSequence GetRenderable(ActionType actions, int facing)
		{
			if (info.Condition != null && !info.Condition.True(self))
				return null;

			return renderables[facing];
		}

		public void OnMove(CPos old, CPos speed)
		{
			currentFacing = FacingFromAngle(self.Angle);
		}

		public void OnAttack(CPos target, Weapon weapon)
		{
			currentFacing = FacingFromAngle(self.Angle);
		}

		public void Tick()
		{
			if (self.Angle != angle)
			{
				angle = self.Angle;
				currentFacing = FacingFromAngle(angle);
			}
			var last = renderable;
			renderable = GetRenderable(self.Actions, currentFacing);

			if (last == null)
				renderable?.Reset();

			renderable?.Tick();
		}

		public override void Render()
		{
			if (renderable == null)
				return;

			if (self.Height > 0)
			{
				renderable.SetPosition(self.GraphicPositionWithoutHeight + info.Offset);
				renderable.SetColor(Color.Shadow);
				renderable.PushToBatchRenderer();
			}

			renderable.SetPosition(self.GraphicPosition + info.Offset);
			renderable.SetColor(cachedColor);
			renderable.PushToBatchRenderer();
		}

		public override void SetColor(Color color)
		{
			cachedColor = color * (info.Color + variation);
		}
	}
}
