using System.Linq;

namespace WarriorsSnuggery.Objects
{
	class RocketWeapon : Weapon
	{
		public RocketWeapon(World world, WeaponType type, CPos origin, CPos target) : base(world, type, origin, target)
		{
			TargetActor = world.Actors.FirstOrDefault(a => a.Position.DistToXY(target) < 1024);
		}

		public RocketWeapon(World world, WeaponType type, Actor origin, CPos target) : base(world, type, origin, target)
		{
			TargetActor = world.Actors.FirstOrDefault(a => (origin != null && a.Team != origin.Team) && a.Position.DistToXY(target) < 1024);
		}

		public override void Tick()
		{
			base.Tick();
			
			if (TargetActor != null)
				Target = TargetActor.Position;
		}
	}
}
