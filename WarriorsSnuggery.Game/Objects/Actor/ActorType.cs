using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Parts;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects
{
	public class ActorType
	{
		public readonly PhysicsPartInfo Physics;
		public readonly PlayablePartInfo Playable;

		public readonly PartInfo[] PartInfos;

		public ActorType(PartInfo[] partInfos)
		{
			Physics = (PhysicsPartInfo)partInfos.FirstOrDefault(p => p is PhysicsPartInfo);
			Playable = (PlayablePartInfo)partInfos.FirstOrDefault(p => p is PlayablePartInfo);
			PartInfos = partInfos;
		}

		public Texture GetPreviewSprite()
		{
			// Get sprites here
			var rawimage = PartInfos.Where(s => s is SpritePartInfo).FirstOrDefault(s => (s as SpritePartInfo).UseAsPreview);
			if (rawimage != null)
			{
				var image = rawimage as SpritePartInfo;
				return image.Textures[0];
			}
			var rawsprite = PartInfos.Where(s => s is AnimatedSpritePartInfo).FirstOrDefault(s => (s as AnimatedSpritePartInfo).UseAsPreview);
			if (rawsprite != null)
			{
				var image = rawsprite as AnimatedSpritePartInfo;
				return image.Textures[0];
			}

			return RuleLoader.Questionmark[0];
		}

		public override string ToString()
		{
			return ActorCreator.GetName(this);
		}
	}

}
