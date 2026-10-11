using AmongUs.GameOptions;
using System.Collections.Generic;

namespace HydraMenu.assets
{
	internal class GameAssets
	{
		// The RoleTypes enum has some weird gaps, everything from Crewmate (0) to Tracker (10) is normal, but then Detective is 12, Viper is 18, and Spirit Guide is 21
		// https://www.innersloth.com/2026-roadmap-part-1/
		// The Among Us 2026 roadmap does state that there are currently 15 prototype roles in the works,
		// could these gaps be attributed to roles that have not been added to the retail version of the game?
		public static readonly List<RoleTypes> RolesList = new List<RoleTypes>()
		{
			RoleTypes.Crewmate,
			RoleTypes.Scientist,
			RoleTypes.Engineer,
			RoleTypes.Noisemaker,
			RoleTypes.Tracker,
			RoleTypes.Detective,
			RoleTypes.Judge,
			RoleTypes.Impostor,
			RoleTypes.Shapeshifter,
			RoleTypes.Phantom,
			RoleTypes.Viper,
			RoleTypes.CrewmateGhost,
			RoleTypes.GuardianAngel,
			RoleTypes.SpiritGuide,
			RoleTypes.ImpostorGhost
		};

		public static readonly SortedList<int, string> BroadcastVersions = new SortedList<int, string>()
		{
			{ 50632950, "16.1.0" },
			{ 50643450, "17.1" },
			{ 50647000, "17.1.2" },
			{ 50645050, "17.2" },
			{ 50652900, "17.2.1" },
			{ 50653700, "17.2.2" },
			{ 50652400, "17.3" },
			{ 50656300, "17.4" },
			{ 50663350, "18.0" },
			{ 50663400, "18.0 (build 7348)" },
			{ 50663650, "19.0 (build 7488)" }, // iOS
			{ 50663600, "19.0 (build 7489)" } // PC
		};
	}
}