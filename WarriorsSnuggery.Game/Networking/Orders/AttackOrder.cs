using System;

namespace WarriorsSnuggery.Networking.Orders
{
    internal class AttackOrder : IOrder
    {
        const NetworkPackageType type = NetworkPackageType.ATTACK;

        public bool Immediate => false;

        public readonly CPos Target;

        public AttackOrder(CPos target)
        {
            Target = target;
        }

        public AttackOrder(byte[] data)
        {
            int x = BitConverter.ToInt32(data, 0);
            int y = BitConverter.ToInt32(data, 4);
            int z = BitConverter.ToInt32(data, 8);

            Target = new CPos(x, y, z);
        }

        public NetworkPackage GeneratePackage()
        {
            var array = new byte[4 * 3];
            BitConverter.GetBytes(Target.X).CopyTo(array, 0);
            BitConverter.GetBytes(Target.Y).CopyTo(array, 4);
            BitConverter.GetBytes(Target.Z).CopyTo(array, 8);

            return new NetworkPackage(type, array);
        }
    }
}
