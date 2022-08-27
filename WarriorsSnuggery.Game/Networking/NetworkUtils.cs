using System.Text;
using WarriorsSnuggery.Networking.Orders;

namespace WarriorsSnuggery.Networking
{
	public static class NetworkUtils
	{
		public const string DefaultAddress = "127.0.0.1";
		public const int DefaultPort = 5050;

		public static byte[] ToBytes(string text)
		{
			return Encoding.ASCII.GetBytes(text);
		}

		public static string ToString(byte[] data)
		{
			return Encoding.ASCII.GetString(data);
		}

		public static IOrder ToOrder(NetworkPackage package)
		{
			return package.Type switch
			{
				PackageType.CHAT => new ChatOrder(package.Content),
				PackageType.PAUSE => new PauseOrder(package.Content),
				PackageType.PARTYMODE => new PartyModeOrder(package.Content),
				// TODO crash
				_ => null,
			};
		}
	}
}
