namespace WarriorsSnuggery.Networking
{
	public enum NetworkPackageType : byte
	{
		WELCOME,
		LOAD,
		GOODBYE,
		ERROR,
		CHAT,
		PAUSE,
		PARTYMODE
	}

	/* TODO: NetworkPackageTypes should look like this:
	 * - WELCOME (Everything before login, check password, check rule checksum, etc.)
	 * - LOAD (Load game information, also load game tick)
	 * - GOODBYE (Logout of client or closing of server)
	 * - ERROR (Error occured on one side of conneciton)
	 * - DIFF (New difference in ...)
	 * - ... all other orders
	 * 
	 * What are orders for?
	 * Orders tell the structure of a NetworkPackage.
	 */
}
