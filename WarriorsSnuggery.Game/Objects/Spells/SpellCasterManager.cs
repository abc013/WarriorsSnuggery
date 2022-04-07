namespace WarriorsSnuggery.Spells
{
	public class SpellCasterManager : ITick
	{
		readonly Game game;
		public readonly SpellCaster[] Casters;

		public SpellCasterManager(Game game)
		{
			this.game = game;

			Casters = new SpellCaster[SpellCasterCache.Types.Count];
			for (int i = 0; i < Casters.Length; i++)
				Casters[i] = new SpellCaster(game, SpellCasterCache.Types[i], game.Stats.GetSpellCaster(i));
		}

		public void Tick()
		{
			foreach (var caster in Casters)
				caster.Tick();
		}

		public void CancelActive()
		{
			foreach (var caster in Casters)
				caster.CancelActive();
		}

		public bool Activate(int caster)
		{
			return Casters[caster].Activate(game.World.LocalPlayer);
		}

		public bool Unlocked(int caster)
		{
			return Casters[caster].Unlocked();
		}
	}
}
