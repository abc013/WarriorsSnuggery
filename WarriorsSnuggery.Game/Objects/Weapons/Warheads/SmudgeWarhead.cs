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

		[Desc("Also spawn smudge if bullet explodes while flying.")]
		public readonly bool SpawnInAir = true;

		public SmudgeWarhead(List<TextNode> nodes)
		{
			TypeLoader.SetValues(this, nodes);
		}

		public void Impact(World world, Weapon weapon, Target target)
		{
			if (world.TerrainAt(target.Position) != null && world.TerrainAt(target.Position).Type.SpawnSmudge && (SpawnInAir || weapon.Height == 0))
				world.SmudgeLayer.Add(new Smudge(new CPos(target.Position.X, target.Position.Y, 0), new BatchSequence(Texture), DissolveDuration, StartDissolve));
		}
	}
}
