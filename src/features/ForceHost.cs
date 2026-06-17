using HarmonyLib;
using InnerNet;

namespace HydraMenu.features
{
	internal class ForceHost
	{
		private static uint prevHostId;

		private static bool enabled = false;
		public static bool Enabled
		{
			get { return enabled; }
			set
			{
				if(value == enabled) return;
				enabled = value;

				if(value)
				{
					BanHost();
				}
			}
		}

		private static void BanHost()
		{
			if(!Enabled) return;

			if(AmongUsClient.Instance.ClientId == AmongUsClient.Instance.HostId)
			{
				Enabled = false;
				return;
			}

			prevHostId = AmongUsClient.Instance.GetHost().Character.NetId;
			Hydra.Log.LogMessage($"Attempting to ban {AmongUsClient.Instance.HostId}");

			Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
			batch.UseAnticheatBypass();
			batch.QueueCheckName(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.PlayerName);
			batch.FinishBatch();
		}

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
		class OnPlayerLeave
		{
			static void Postfix(ClientData data)
			{
				Hydra.Log.LogMessage($"Recieved disconnect for {data.PlayerName}");
				PlayerControl player = data.Character;
				if(!Enabled || player == null) return;

				Hydra.Log.LogMessage($"Is not null, player id {player.OwnerId}, prev host id {prevHostId}");

				if(AmongUsClient.Instance.ClientId == AmongUsClient.Instance.HostId)
				{
					Hydra.notifications.Send("Force Host", $"You are now the host of the lobby", 5);
					Enabled = false;
					return;
				}

				if(player.NetId == prevHostId)
				{
					Hydra.notifications.Send("Force Host", $"Banned {player.Data.PlayerName} from the game", 3);
					BanHost();
				}
			}
		}
	}
}