using System.Collections.Generic;
using WarriorsSnuggery.Graphics;
using WarriorsSnuggery.Loader;

namespace WarriorsSnuggery.Objects.Weapons.Warheads
{
	public class SmudgeWarhead : IWarhead
	{
		[Desc("Texture of the Smudge that will be left behind from impact.")]
		public readonly TextureInfo Texture;

		[Desc("How long it will need to dissolve the smudge.")]
		public readonly int DissolveDuration = 120;

		[Desc("Start dissolving the smudge immediately.")]
		public readonly bool StartDissolve = false;

		public SmudgeWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);

			if (Texture != null)
				SpriteManager.AddTexture(Texture);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (world.TerrainAt(target.Position) != null && world.TerrainAt(target.Position).Type.SpawnSmudge)
				world.SmudgeLayer.Add(new Smudge(new CPos(target.Position.X, target.Position.Y, 0), new BatchSequence(Texture.GetTextures(), Color.White, Texture.Tick), DissolveDuration, StartDissolve));
		}
	}
}
