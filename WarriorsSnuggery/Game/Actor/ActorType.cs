using WarriorsSnuggery.Graphics;
using System.Linq;
using System.Collections.Generic;
using WarriorsSnuggery.Objects.Parts;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly PhysicsPartInfo Physics;
		public readonly PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(PhysicsPartInfo physics, PlayablePartInfo playable, PartInfo[] partInfos)
		{
			Playable = playable;
			Physics = physics;
			PartInfos = partInfos;
		}

		public ITexture GetPreviewSprite()
		{
			// Get sprites here
			var rawimage = PartInfos.Where(s => s is SpritePartInfo).FirstOrDefault(s => (s as SpritePartInfo).UseAsPreview);
			if (rawimage != null)
			{
				var image = rawimage as SpritePartInfo;
				return TextureManager.Sprite(new TextureInfo(image.Name, TextureType.ANIMATION, 0, image.Dimensions.X, image.Dimensions.Y))[0];
			}
			var rawsprite = PartInfos.Where(s => s is AnimatedSpritePartInfo).FirstOrDefault(s => (s as AnimatedSpritePartInfo).UseAsPreview);
			if (rawsprite != null)
			{
				var image = rawsprite as AnimatedSpritePartInfo;
				return TextureManager.Sprite(new TextureInfo(image.Name, TextureType.ANIMATION, image.Tick, image.Dimensions.X, image.Dimensions.Y))[0];
			}

			return TextureManager.Texture("questionmark");
		}
	}

}
