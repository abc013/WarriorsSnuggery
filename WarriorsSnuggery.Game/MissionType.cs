namespace WarriorsSnuggery
{
	public enum MissionType
	{
		MAIN_MENU,
		STORY,
		STORY_MENU,
		NORMAL,
		NORMAL_MENU,
		TUTORIAL,
		TUTORIAL_MENU,
		TEST
	}

	public static class MissionTypeExts
	{
		public static bool IsMenu(this MissionType type) => type == MissionType.MAIN_MENU || type == MissionType.STORY_MENU || type == MissionType.NORMAL_MENU || type == MissionType.TUTORIAL_MENU;
		public static bool IsCampaign(this MissionType type) => type == MissionType.STORY || type == MissionType.STORY_MENU || type == MissionType.NORMAL || type == MissionType.NORMAL_MENU;
		public static bool IsTutorial(this MissionType type) => type == MissionType.TUTORIAL || type == MissionType.TUTORIAL_MENU;

		public static MissionType GetMenuType(this MissionType type)
		{
			if (type == MissionType.STORY || type == MissionType.STORY_MENU)
				return MissionType.STORY_MENU;

			if (type == MissionType.NORMAL || type == MissionType.NORMAL_MENU)
				return MissionType.NORMAL_MENU;

			if (type == MissionType.TUTORIAL || type == MissionType.TUTORIAL_MENU)
				return MissionType.TUTORIAL_MENU;

			return type;
		}

		public static MissionType GetCampaignType(this MissionType type)
		{
			if (type == MissionType.STORY || type == MissionType.STORY_MENU)
				return MissionType.STORY;

			if (type == MissionType.NORMAL || type == MissionType.NORMAL_MENU)
				return MissionType.NORMAL;

			if (type == MissionType.TUTORIAL || type == MissionType.TUTORIAL_MENU)
				return MissionType.TUTORIAL;

			return type;
		}
	}
}
