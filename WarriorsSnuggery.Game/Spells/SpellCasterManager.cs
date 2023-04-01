using WarriorsSnuggery.Loader;
using WarriorsSnuggery.Objects.Actors;

namespace WarriorsSnuggery.Spells
{
	public class SpellCasterManager : ITick, ILoadable, ISaveable
	{
		public readonly SpellCaster[] Casters;

		public SpellCasterManager(Player player)
		{
			Casters = new SpellCaster[SpellCasterCache.Types.Count];

			var i = 0;
			foreach (var casterType in SpellCasterCache.Types.Values)
				Casters[i++] = new SpellCaster(player, casterType);
		}

		public void Load(TextNodeInitializer initializer)
		{
			foreach (var caster in Casters)
			{
				if (initializer.ContainsRule(caster.Type.InnerName))
					caster.Load(initializer.MakeInitializerWith(caster.Type.InnerName));
			}
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

		public bool Activate(int caster, Actor actor)
		{
			return Casters[caster].Activate(actor);
		}

		public bool Unlocked(int caster)
		{
			return Casters[caster].Unlocked();
		}

		public TextNodeSaver Save()
		{
			var saver = new TextNodeSaver();
			foreach (var caster in Casters)
				saver.AddChildren(caster.Type.InnerName, caster.Save(), true);

			return saver;
		}
	}
}
