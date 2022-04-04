using System;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Conditions;

namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("This will add a light sprite to an actor. This means that the given sprite is rendered as light.")]
	public class LightPartInfo : PartInfo
	{
		public readonly Texture[] Textures;

		[Require, Desc("Name of the texture file.")]
		public readonly PackageFile Name;

		[Desc("Activate only by the following Condition.")]
		public readonly Condition Condition;

		[Desc("Offset of the sprite.")]
		public readonly CPos Offset;

		[Desc("Scale of the sprite.")]
		public readonly float Scale = 1.0f;

		[Desc("Color to multiply the sprite with.")]
		public readonly Color Color = Color.White;

		[Desc("Random Color to add to the sprite.", "Alpha will not be applied.")]
		public readonly Color ColorVariation = Color.Black;

		[Desc("Strength of Light fluctuation. The stronger, the weaker the light can get.")]
		public readonly float FluctuationStrength = 0.0f;

		[Desc("Apply randomness to the color strength to get a more natural look.")]
		public readonly float FluctuationStrengthRandomness = 0.0f;

		[Desc("Frequency of the light fluctuation in ticks.")]
		public readonly int FluctuationSpeed = 0;

		[Desc("Apply random ticks to the Frequency to get a more natural look.")]
		public readonly float FluctuationSpeedRandomness = 0.0f;

		[Desc("Consider world ambient when lighting.", "This means that this light ist weaker if world ambient is bright and stronger if the world ambient is dark.")]
		public readonly bool ConsiderWorldAmbient = true;

		public LightPartInfo(PartInitSet set) : base(set)
		{
			if (Name != null)
				Textures = new TextureInfo(Name).GetTextures();
		}

		public override ActorPart Create(Actor self)
		{
			return new LightPart(self, this);
		}
	}

	public class LightPart : ActorPart, IRenderable, ITick
	{
		readonly LightPartInfo info;

		readonly BatchObject renderable;
		readonly Color variation = Color.Black;
		Color additionalTint = Color.Black;
		float fluctuationTint = 1f;
		float fluctuation;
		float randomFluctuationStrength;

		public LightPart(Actor self, LightPartInfo info) : base(self)
		{
			this.info = info;

			renderable = new BatchObject(info.Textures[0]);

			if (info.ColorVariation != Color.Black)
			{
				var random = Program.SharedRandom;
				variation = new Color((float)(random.NextDouble() - 0.5f) * info.ColorVariation.R, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.G, (float)(random.NextDouble() - 0.5f) * info.ColorVariation.B, 0f);
			}

			if (info.ConsiderWorldAmbient)
			{
				var ambient = WorldRenderer.Ambient;
				static float getSingleTint(float worldTint) => Math.Min(1 / Math.Max(worldTint, float.Epsilon), 3f);
				additionalTint = info.Color * new Color(getSingleTint(ambient.R), getSingleTint(ambient.G), getSingleTint(ambient.B));
				var scale = Math.Max(1, (additionalTint.R + additionalTint.G + additionalTint.B) / 3);
				renderable.SetScale(info.Scale * scale);
			}

			SetColor(Color.White);

			fluctuation = -info.FluctuationSpeed;

			renderable.SetScale(info.Scale);
		}

		public void Tick()
		{
			if (info.FluctuationStrength > 0)
			{
				fluctuation += (1 + ((float)(Program.SharedRandom.NextDouble() - 0.5f) * 2f * info.FluctuationSpeedRandomness));
				if (fluctuation >= info.FluctuationSpeed)
					fluctuation = -info.FluctuationSpeed;

				randomFluctuationStrength += ((float)Program.SharedRandom.NextDouble() - 0.5f) * info.FluctuationStrength;
				randomFluctuationStrength = Math.Clamp(randomFluctuationStrength, -info.FluctuationStrengthRandomness, info.FluctuationStrengthRandomness);

				fluctuationTint = 1 - ((Math.Abs(fluctuation) / info.FluctuationSpeed) + randomFluctuationStrength) * info.FluctuationStrength;
				SetColor(Color.White);
			}
		}

		public void Render()
		{
			if (info.Condition != null && !info.Condition.True(self))
				return;

			renderable.SetPosition(self.GraphicPosition + info.Offset);

			MasterRenderer.SetRenderer(Renderer.LIGHTS);
			renderable.Render();
			MasterRenderer.SetRenderer(Renderer.DEFAULT);
		}

		public void SetColor(Color color)
		{
			var finalColorTint = info.ConsiderWorldAmbient ? (variation - WorldRenderer.Ambient + additionalTint) : variation;
			renderable.SetColor(fluctuationTint * color * (info.Color + finalColorTint));
		}
	}
}
