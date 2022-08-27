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
	}
}
