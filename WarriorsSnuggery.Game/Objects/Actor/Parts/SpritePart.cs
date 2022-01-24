using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class SpritePartInfo : PartInfo
	{
		public readonly Texture[] Textures;

		[Require, Desc("Name of the texture file.")]
		public readonly string Name;

		[Require, Desc("Size of a single frame.")]
		public readonly MPos Dimensions;

		[Desc("Count of facings that the sprite has.")]
		public readonly int Facings = 1;

		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		[Desc("Offset of the sprite.")]
		public readonly CPos Offset;

		[Desc("use a random image in the sprite sequence.")]
		public readonly bool Random;

		[Desc("Use the n-th image in the sprite sequence.", "crashes if there are less images in the sequence than the offset size.")]
		public readonly int SpriteOffset;

		[Desc("Use Sprite as preview in e.g. the editor.")]
		public readonly bool UseAsPreview;

		[Desc("Color to multiply the sprite with.")]
		public readonly Color Color = Color.White;

		[Desc("Random Color to add to the sprite.", "Alpha will not be applied.")]
		public readonly Color ColorVariation = Color.Black;

		public SpritePartInfo(PartInitSet set) : base(set)
		{
			if (Name != null)
				Textures = new TextureInfo(Name, TextureType.ANIMATION, Dimensions).GetTextures();
		}

		public override ActorPart Create(Actor self)
		{
			return new SpritePart(self, this);
		}
	}

	public class SpritePart : ActorPart, IPartRenderable
	{
		readonly SpritePartInfo info;

		readonly BatchObject[] renderables;
		readonly Color variation;
		Color cachedColor;

		public SpritePart(Actor self, SpritePartInfo info) : base(self)
		{
			this.info = info;
			renderables = new BatchObject[info.Facings];
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

				var index = 0;
				if (info.Random)
					index = self.World.Game.SharedRandom.Next(anim.Length);

				renderables[i] = new BatchObject(anim[index]);
			}

			if (info.ColorVariation != Color.Black)
			{
				var random = Program.SharedRandom;
				variation = new Color((float)(random.NextDouble() - 0.5f) * info.ColorVariation.R, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.G, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.B, 0f);
			}
			cachedColor = info.Color + variation;

			self.ZOffset = info.Offset.Z;
		}

		public int FacingFromAngle(float angle)
		{
			float part = 360f / info.Facings;

			int facing = (int)Math.Round(angle / part);
			if (facing >= info.Facings)
				facing = 0;

			return facing;
		}

		public BatchRenderable GetRenderable(ActionType actions, int facing)
		{
			if (info.Condition != null && !info.Condition.True(self))
				return null;

			return renderables[facing];
		}

		public void Render()
		{
			var renderable = GetRenderable(self.Actions, FacingFromAngle(self.Angle));
			if (renderable != null)
			{
				if (self.Height > 0)
				{
					renderable.SetPosition(self.GraphicPositionWithoutHeight + info.Offset);
					renderable.SetColor(Color.Shadow);
					renderable.Render();
				}

				renderable.SetPosition(self.GraphicPosition + info.Offset);
				renderable.SetColor(cachedColor);
				renderable.Render();
			}
		}

		public void SetColor(Color color)
		{
			cachedColor = color * (info.Color + variation);
		}
	}
}
