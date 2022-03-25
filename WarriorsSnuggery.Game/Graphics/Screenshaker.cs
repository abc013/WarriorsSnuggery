namespace WarriorsSnuggery.Graphics
{
	public static class Screenshaker
	{
		public const int MaxShakeStrength = 512;
		public static int ShakeStrength
        {
			get { return shakeStrength; }
			set { shakeStrength = value > MaxShakeStrength ? MaxShakeStrength : value; }
        }
		static int shakeStrength;

		static int randomShake => Program.SharedRandom.Next(-ShakeStrength, ShakeStrength);
		static public CPos RandomShake => new CPos(randomShake, randomShake, 0);

		public static void DecreaseShake()
        {
			ShakeStrength -= ShakeStrength / 16 + 1;
        }
	}
}
