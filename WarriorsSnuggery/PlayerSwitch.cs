using WarriorsSnuggery.Objects;
using WarriorsSnuggery.Objects.Particles;

namespace WarriorsSnuggery
{
	public sealed class PlayerSwitch
	{
		// TODO unhardcode value
		const int switchTime = 120;

		readonly World world;
		readonly ParticleType particleType;

		readonly ActorType type;
		readonly CPos position;
		public readonly float RelativeHP;

		int timeRemaining;

		public PlayerSwitch(World world, ActorType to)
		{
			this.world = world;
			particleType = ParticleCreator.Types["beam"];

			type = to;
			position = world.LocalPlayer.Position;
			RelativeHP = world.LocalPlayer.Health != null ? world.LocalPlayer.Health.RelativeHP : 1;

			timeRemaining = switchTime;
		}

		public void Tick()
		{
			if (timeRemaining-- == 0)
			{
				var newActor = ActorCreator.Create(world, type, position, Actor.PlayerTeam, isPlayer: true);

				if (newActor.Health != null)
					newActor.Health.RelativeHP = RelativeHP;

				world.FinishPlayerSwitch(newActor);
			}
			else
			{
				for (int i = 0; i < (int)((1 - timeRemaining / (float)switchTime) * 3) + 1; i++)
				{
					var random = Program.SharedRandom;

					var x = random.Next(640) - 320;
					var y = random.Next(640) - 320;

					world.Add(ParticleCreator.Create(particleType, position + new CPos(x, y, 0), 0, Program.SharedRandom));
				}
			}
		}
	}
}
