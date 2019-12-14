using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Conditions;
using WarriorsSnuggery.Objects.Weapons;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class AnimatedSpritePartInfo : PartInfo
	{
		public readonly IImage[] Textures;

		[Desc("Name of the texture file.")]
		public readonly string Name;

		[Desc("Size of a single frame.")]
		public readonly MPos Dimensions;

		[Desc("Count of facings that the animation has.")]
		public readonly int Facings = 1;

		[Desc("Frame rate (Speed of animation)")]
		public readonly int Tick = 10;

		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition = new Condition("True");

		[Desc("Offset of the sprite.")]
		public readonly CPos Offset;

		[Desc("Use Sprite as preview in e.g. the editor.")]
		public readonly bool UseAsPreview;

		public override ActorPart Create(Actor self)
		{
			return new AnimatedSpritePart(self, this);
		}

		public AnimatedSpritePartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			if (Name != null)
				Textures = SpriteManager.AddTexture(new TextureInfo(Name, TextureType.ANIMATION, Tick, Dimensions.X, Dimensions.Y));
		}
	}

	public class AnimatedSpritePart : RenderablePart
	{
		readonly AnimatedSpritePartInfo info;

		readonly IImageSequenceRenderable[] renderables;
		int currentFacing;

		public AnimatedSpritePart(Actor self, AnimatedSpritePartInfo info) : base(self)
		{
			this.info = info;
			renderables = new IImageSequenceRenderable[info.Facings];
			var frameCountPerIdleAnim = info.Textures.Length / info.Facings;

			if (frameCountPerIdleAnim * info.Facings != info.Textures.Length)
				throw new YamlInvalidNodeException(string.Format(@"Idle Frame '{0}' count cannot be matched with the given Facings '{1}'.", info.Textures.Length, info.Facings));

			for (int i = 0; i < renderables.Length; i++)
			{
				var anim = new IImage[frameCountPerIdleAnim];
				for (int x = 0; x < frameCountPerIdleAnim; x++)
					anim[x] = info.Textures[i * frameCountPerIdleAnim + x];

				renderables[i] = new IImageSequenceRenderable(anim, tick: info.Tick);
			}
		}

		public override int FacingFromAngle(float angle)
		{
			float part = (float)(2f * Math.PI) / info.Facings;

			int facing = (int)Math.Round(angle / part);
			if (facing >= info.Facings)
				facing = 0;

			return facing;
		}

		public override GraphicsObject GetRenderable(ActorAction action, int facing)
		{
			if (info.Condition != null && !info.Condition.True(self))
				return null;

			return renderables[facing];
		}

		public override void OnMove(CPos old, CPos speed)
		{
			currentFacing = FacingFromAngle(self.Angle);
		}

		public override void OnAttack(CPos target, Weapon weapon)
		{
			currentFacing = FacingFromAngle(self.Angle);
		}

		public override void Render()
		{
			var renderable = GetRenderable(self.CurrentAction, currentFacing);
			if (renderable != null)
			{
				if (self.Height > 0)
				{
					MasterRenderer.RenderShadow = true;
					MasterRenderer.UniformHeight(self.Height);

					renderable.SetPosition(self.GraphicPositionWithoutHeight);
					renderable.Render();

					MasterRenderer.RenderShadow = false;
					Program.CheckGraphicsError("RenderShadow");
				}

				self.Offset = info.Offset; // TODO replace by proper rendering
				renderable.SetPosition(self.GraphicPosition);
				renderable.Render();
			}
		}
	}
}
