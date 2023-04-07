using WarriorsSnuggery.Conditions;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This will add a sprite to an actor which will be rendered upon call.")]
	public class SpritePartInfo : PartInfo
	{
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

		[Desc("Offset of the sprite regarding depth.")]
		public readonly int ZOffset;

		[Desc("Use Sprite as preview in e.g. the editor.")]
		public readonly bool UseAsPreview;

		[Desc("Color to multiply the sprite with.")]
		public readonly Color Color = Color.White;

		[Desc("Random Color to add to the sprite.", "Alpha will not be applied.")]
		public readonly Color ColorVariation = Color.Black;

		[Desc("Determines whether to ignore ambient light.", "This can be used for 'glow in the dark' effects.")]
		public readonly bool IgnoreAmbience;

		public SpritePartInfo(PartInitSet set) : base(set) { }
	}

	public class SpritePart : ActorPart, INoticeBasicChanges, IRenderable, ITick, ITickInEditor
	{
		readonly SpritePartInfo info;

		readonly BatchRenderable[] renderables;
		readonly ActorObject renderObject = new ActorObject(); // used to prevent code duplication.
		readonly Color variation;

		int currentFacing;
		float angle;
		bool wasVisible;

		public SpritePart(Actor self, SpritePartInfo info) : base(self, info)
		{
			this.info = info;
			renderables = new BatchRenderable[info.Facings];
			var textures = info.Texture.GetTextures();
			var framesPerFacing = textures.Length / info.Facings;

			if (framesPerFacing * info.Facings != textures.Length)
				throw new InvalidNodeException($"Frame '{textures.Length}' count cannot be matched with the given Facings '{info.Facings}'.");

			for (int i = 0; i < renderables.Length; i++)
			{
				var anim = new Texture[framesPerFacing];
				for (int x = 0; x < framesPerFacing; x++)
					anim[x] = textures[i * framesPerFacing + x];

				if (anim.Length == 0)
					throw new InvalidNodeException("Animation Frame count is zero. Make sure you set the bounds properly.");

				if (anim.Length == 1)
					renderables[i] = new BatchObject(anim[0]);
				else
					renderables[i] = new BatchSequence(anim, info.Texture.Tick, startRandom: info.StartRandom);
			}

			if (info.ColorVariation != Color.Black)
			{
				var random = Program.SharedRandom;
				variation = new Color((float)(random.NextDouble() - 0.5f) * info.ColorVariation.R, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.G, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.B, 0f);
			}

			SetPosition(self.Position);
			SetScale(self.Scale);
			SetRotation(self.Rotation);
			SetColor(self.Color);
			SetTextureFlags(self.TextureFlags);

			renderObject.setRenderable(null);
			self.ZOffset = info.ZOffset;
		}

		BatchRenderable getRenderable(ActionType actions, int facing)
		{
			if (info.Condition != null && !info.Condition.True(Self))
				return null;

			return renderables[facing];
		}

		public void Render()
		{
			renderObject.RenderShadow();
			renderObject.Render();
		}

		public void Tick()
		{
			if (Self.Angle != angle)
			{
				angle = Self.Angle;
				currentFacing = Angle.ToFacing(angle, info.Facings);
			}

			var renderable = getRenderable(Self.Actions, currentFacing);
			renderObject.setRenderable(renderable);

			if (wasVisible && renderable is BatchSequence sequence)
				sequence.Reset();

			wasVisible = renderable == null;

			renderObject.Tick();
		}

		public void SetPosition(CPos pos)
		{
			renderObject.Position = pos + info.Offset;
		}

		public void SetScale(float scale)
		{
			renderObject.Scale = scale;
		}

		public void SetRotation(VAngle rotation)
		{
			renderObject.Rotation = rotation;
		}

		public void SetColor(Color color)
		{
			renderObject.Color = color * (info.Color + variation);
		}

		public void SetTextureFlags(TextureFlags flags)
		{
			renderObject.TextureFlags = (info.IgnoreAmbience ? TextureFlags.IgnoreAmbience : TextureFlags.None) | flags;
		}

		class ActorObject : PositionableObject
		{
			internal void setRenderable(BatchRenderable renderable)
			{
				Renderable = renderable;
			}
		}
	}
}
