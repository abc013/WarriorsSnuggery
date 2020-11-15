using System;
using System.Net.Sockets;

namespace WarriorsSnuggery.Networking
{
	public enum PackageType : byte
	{
		WELCOME = 0x0,
		MESSAGE = 0x1,
		GOODBYE = 0x2,
		ERROR = 0x3
	}

	public class NetworkPackage
	{
		public readonly PackageType Type;
		public readonly int Length;
		public readonly byte[] Content;

		public NetworkPackage(NetworkStream stream)
		{
			var header = new byte[5];
			stream.Read(header);

			Type = (PackageType)header[0];
			Length = BitConverter.ToInt32(header, 1);

			Content = new byte[Length];
			stream.Read(Content);
		}

		public NetworkPackage(PackageType type, byte[] content)
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
	}
}
