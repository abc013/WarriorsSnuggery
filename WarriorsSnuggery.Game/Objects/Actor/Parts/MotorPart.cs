namespace WarriorsSnuggery.Objects.Actors.Parts
{
	[Desc("Attach this to an actor to allow self-propelled movement.", "This part requires the Mobile part to work.")]
	public class MotorPartInfo : PartInfo
	{
		[Desc("Speed of the Actor.", "This is unused currently. Modify Acceleration and Mobile's Friction values instead to regulate speed.")]
		public readonly int Speed;
		[Desc("Time it takes to prepare for moving.")]
		public readonly int PreparationDelay;
		[Desc("Time it takes to cool down before being able to do something else.")]
		public readonly int CooldownDelay;
		[Desc("Acceleration of the Actor. If 0, Speed is used.")]
		public readonly int Acceleration;
		[Desc("Acceleration to use for the vertical axis.")]
		public readonly int HeightAcceleration;

		public MotorPartInfo(PartInitSet set) : base(set)
		{
			if (Acceleration == 0)
				Acceleration = Speed;
		}
	}

	public class MotorPart : ActorPart, ITick, INoticeStop
	{
		readonly MotorPartInfo info;

		bool movedThisTick;
		bool accelerationOrdered;
		float angle;
		int prep;

		public MotorPart(Actor self, MotorPartInfo info) : base(self)
		{
			this.info = info;
		}

		public void Tick()
		{
			if (accelerationOrdered && (--prep <= 0 || self.DoesAction(ActionType.MOVE)))
				accelerateSelf();

			movedThisTick = false;
		}

		public void AccelerateSelf(float angle)
		{
			accelerationOrdered = true;
			this.angle = angle;

			if (prep > 0)
				return;

			if (!self.DoesAction(ActionType.MOVE) && info.PreparationDelay > 0)
			{
				prep = info.PreparationDelay;
				self.AddAction(ActionType.PREPARE_MOVE, info.PreparationDelay);
				return;
			}

			accelerateSelf();
		}

		void accelerateSelf()
		{
			prep = 0;
			accelerationOrdered = false;

			if (movedThisTick)
				return;

			if (!self.DoesAction(ActionType.MOVE) && info.PreparationDelay > 0 && !self.DoesAction(ActionType.PREPARE_MOVE))
				return; // Preparation has been canceled

			self.Push(angle, info.Acceleration);

			movedThisTick = true;
		}

		public void AccelerateHeightSelf(bool up)
		{
			self.Lift(up ? info.HeightAcceleration : -info.HeightAcceleration);
		}

		public void OnStop()
		{
			self.AddAction(ActionType.END_MOVE, info.CooldownDelay);
		}
	}
}
