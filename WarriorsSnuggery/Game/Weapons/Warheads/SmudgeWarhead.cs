using WarriorsSnuggery.Graphics;

namespace WarriorsSnuggery.Objects.Weapons
{
	public class SmudgeWarhead : IWarhead
	{
		[Desc("Texture of the Smudge that will be left behind from impact.")]
		public readonly TextureInfo Texture;

		[Desc("How long it will need to dissolve the smudge.")]
		public readonly int DissolveDuration = 120;

		public SmudgeWarhead(MiniTextNode[] nodes)
		{
			Loader.PartLoader.SetValues(this, nodes);

			if (Texture != null)
				SpriteManager.AddTexture(Texture);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (world.TerrainAt(target.Position) != null && world.TerrainAt(target.Position).Type.SpawnSmudge)
				world.SmudgeLayer.Add(new Smudge(new CPos(target.Position.X, target.Position.Y, -512), new BatchSequence(Texture.GetTextures(), Texture.Tick), DissolveDuration));
		}
	}
}
