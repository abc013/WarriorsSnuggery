using System;
using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class SpritePartInfo : PartInfo
	{
		[Desc("Name of the texture file.")]
		public readonly string Name;

		[Desc("Size of a single frame.")]
		public readonly MPos Dimensions;

		[Desc("Count of facings that the sprite has.")]
		public readonly int Facings = 1;

		[Desc("Condition when the sprite is rendered.", "Possible: ATTACKING, MOVING, IDLING, ALL")]
		public readonly ActorAction Condition;

		[Desc("Offset of the sprite.")]
		public readonly CPos Offset;

		[Desc("use a random image in the sprite sequence.")]
		public readonly bool Random;

		[Desc("Use the n-th image in the sprite sequence.", "crashes if there are less images in the sequence than the offset size.")]
		public readonly int SpriteOffset;

		[Desc("Use Sürote as preview in e.g. the editor.")]
		public readonly bool UseAsPreview;

		public override ActorPart Create(Actor self)
		{
			return new SpritePart(self, this);
		}

		public SpritePartInfo(MiniTextNode[] nodes) : base(nodes)
		{
			foreach (var node in nodes)
			{
				switch (node.Key)
				{
					case "Name":
						Name = node.Value;
						break;
					case "Dimensions":
						Dimensions = node.ToMPos();
						break;
					case "Facings":
						Facings = node.ToInt();
						break;
					case "Condition":
						Condition = (ActorAction)node.ToEnum(typeof(ActorAction));
						break;
					case "Offset":
						Offset = node.ToCPos();
						break;
					case "Random":
						Random = node.ToBoolean();
						break;
					case "SpriteOffset":
						SpriteOffset = node.ToInt();
						break;
					case "UseAsPreview":
						UseAsPreview = node.ToBoolean();
						break;
					default:
						throw new YamlUnknownNodeException(node.Key, "RegenerationPart");
				}
			}
		}
	}

	public class SpritePart : RenderablePart
	{
		readonly SpritePartInfo info;

		readonly ImageRenderable[] renderables;

		public SpritePart(Actor self, SpritePartInfo info) : base(self)
		{
			this.info = info;
			var textures = TextureManager.Sprite(new TextureInfo(info.Name, TextureType.ANIMATION, 0, info.Dimensions.X, info.Dimensions.Y));
			renderables = new ImageRenderable[info.Facings];
			var frameCountPerIdleAnim = textures.Length / info.Facings;

			if (frameCountPerIdleAnim * info.Facings != textures.Length)
				throw new YamlInvalidNodeException(string.Format(@"Idle Frame '{0}' count cannot be matched with the given Facings '{1}'.", textures.Length, info.Facings));

			for (int i = 0; i < renderables.Length; i++)
			{
				var anim = new ITexture[frameCountPerIdleAnim];
				for (int x = 0; x < frameCountPerIdleAnim; x++)
					anim[x] = textures[i * frameCountPerIdleAnim + x];

				if (anim.Length == 0)
					throw new YamlInvalidNodeException(string.Format(@"Animation Frame count is zero. Make sure you set the bounds properly."));

				if (info.Random)
				{
					var ran = self.World.Game.SharedRandom.Next(anim.Length);
					renderables[i] = new ImageRenderable(anim[ran]);
				}
				else
				{
					renderables[i] = new ImageRenderable(anim[0]);
				}
			}
		}

		public override int FacingFromAngle(float angle)
		{
			float part = 360f / info.Facings;

			int facing = (int)Math.Round(angle / part);
			if (facing >= info.Facings)
				facing = 0;

			return facing;
		}

		public override GraphicsObject GetRenderable(ActorAction action, int facing)
		{
			if (info.Condition != ActorAction.ALL && action != info.Condition)
				return null;

			return renderables[facing];
		}

		public override void Render()
		{
			var renderable = GetRenderable(self.CurrentAction, FacingFromAngle(self.Angle));
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
				Program.CheckGraphicsError("Render");
			}
		}
	}
}
