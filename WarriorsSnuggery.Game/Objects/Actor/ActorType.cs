using System.Linq;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Objects.Actors.Parts;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Actors
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

		public Texture GetPreviewSprite(out Color customColor)
		{
			// Get sprites here
			var rawimage = PartInfos.Where(s => s is SpritePartInfo).FirstOrDefault(s => (s as SpritePartInfo).UseAsPreview);
			if (rawimage != null)
			{
				var image = rawimage as SpritePartInfo;
				customColor = image.Color;
				return image.Texture.GetTextures()[0];
			}

			customColor = Color.White;
			return RuleLoader.Questionmark;
		}

		public override string ToString()
		{
			return ActorCache.Types[this];
		}
	}

}
