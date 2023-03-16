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

			var i = 0;
			foreach (var caster in SpellCasterCache.Types.Values)
				Casters[i++] = new SpellCaster(game, caster);
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
