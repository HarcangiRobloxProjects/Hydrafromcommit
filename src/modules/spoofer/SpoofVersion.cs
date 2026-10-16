using HarmonyLib;

namespace HydraMenu.modules.spoofer
{
	internal class SpoofVersion : Module
	{
		public SpoofVersion() : base("SpoofVersion") { }

		// Do not save this value to the config
		// If a user saves their config, and later updates their game, the protocol version of the old game version will be used
		// resulting in being unable to join lobbies
		internal int SpoofedVersion { get; set; } = Constants.GetBroadcastVersion();
		public bool UseModdedProtocol { get; set; } = false;

		// https://github.com/Innersloth-LLC/AmongUsModdingInformation#host-only-mod-registration-25-modded-flag
		// Adding the constant value 25 to the current protocol version will result in the lobby being in host-authoritative mode
		private readonly int MODDED_PROTOCOL_CONSTANT = 25;

		private static SpoofVersion Instance
		{
			get { return ModuleManager.spoofVersion; }
		}

		[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
		class BroadcastVersion
		{
			static bool Prefix(ref int __result)
			{
				// Starting a local lobby or entering Freeplay will bug out if we are using a spoofed version
				if(!Instance.Enabled || AmongUsClient.Instance == null || AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return true;

				__result = Instance.SpoofedVersion;
				if(Instance.UseModdedProtocol) __result += Instance.MODDED_PROTOCOL_CONSTANT;

				return false;
			}
		}

		[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
		class MarkVersionModded
		{
			static bool Prefix(ref bool __result)
			{
				if(!Instance.Enabled || !Instance.UseModdedProtocol) return true;

				__result = true;
				return false;
			}
		}
	}
}