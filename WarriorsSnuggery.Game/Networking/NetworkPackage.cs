using System;
using System.Net.Sockets;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public class NetworkPackage
	{
		public readonly NetworkPackageType Type;
		public readonly int Length;
		public readonly byte[] Content;

		public NetworkPackage(NetworkStream stream)
		{
			var header = new byte[5];
			stream.Read(header);

			Type = (NetworkPackageType)header[0];
			Length = BitConverter.ToInt32(header, 1);

			Content = new byte[Length];
			stream.Read(Content);
		}

		public NetworkPackage(NetworkPackageType type, byte[] content)
		{
			Type = type;
			Length = content.Length;
			Content = content;
		}

		public byte[] AsBytes()
		{
			var data = new byte[5 + Length];
			data[0] = (byte)Type;
			Array.Copy(BitConverter.GetBytes(Length), 0, data, 1, 4);
			Array.Copy(Content, 0, data, 5, Length);

			return data;
		}

		public IOrder ToOrder()
		{
			return Type switch
			{
				NetworkPackageType.CHAT => new ChatOrder(Content),
				NetworkPackageType.PAUSE => new PauseOrder(Content),
				NetworkPackageType.PARTYMODE => new PartyModeOrder(Content),
				_ => throw new NotSupportedException($"Can't convert PackageType {Type} to a valid order.")
			};
		}
	}
}
