using AmongUs.GameOptions;
using HarmonyLib;
using HydraMenu.network;
using InnerNet;

namespace HydraMenu.modules.host
{
	internal class DisableGameEnd : Module
	{
		public DisableGameEnd() : base("DisableGameEnd") { }

		private static DisableGameEnd Instance
		{
			get { return ModuleManager.disableGameEnd; }
		}

		private bool ShouldDespawnGameManager
		{
			get { return GameManager.Instance != null && !AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame && AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started; }
		}

		// If we are the host, we can simply block our client from sending the EndGame message
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.RpcEndGame))]
		class OnEndGameHost
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// If we are not the host, then we have to get clever
		// The host checks if the game should end in two main points: GameManager::FixedUpdate, which checks if any end criteria is met every single frame
		// and PlayerControl::ReportDeadBody, which checks if the game should end if all tasks are completed
		// Non-hosts have no authority over how the hosts handle ReportDeadBody messages, but we can influence the GameManager net object
		// The GameManager net object is responsible for handling much of the current game state, such as the lobby's settings, the HnS timer, role selection, game end, etc
		// If we can despawn the GameManager net object, then we can prevent GameManager::FixedUpdate from running and checking if the game should end
		// In theory this should break major portions of the game, as the client relies on GameManger for handling much of the game state
		// however Unity has a nice quirk known as fake null, where destroying a Unity object will destroy it on the unmanaged side (as in the engine), and attempting to compare it to null will return true
		// but the object will still technically exist on the managed side and you can access its properties until it is collected by the garbage collector (https://docs.unity3d.com/6000.1/Documentation/Manual/null-reference-exception.html)
		// so other clients will still be able to function properly with the fake-null GameManager, just with GameManager::FixedUpdate no longer running, and the object no longer being networked
		private void OnGameLoad()
		{
			if(!ShouldDespawnGameManager) return;

			Hydra.Log.LogMessage($"Game has fully loaded, despawning GameManager");
			GameManager.Instance.Despawn();
		}

		private void OnNetObjectSpawn(InnerNetObject netObject)
		{
			if(netObject.SpawnId != (uint)SpawnType.GameManager || !ShouldDespawnGameManager) return;

			Hydra.Log.LogMessage($"A new instance of GameManager was spawned, despawning...");
			netObject.Despawn();
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameLoad += OnGameLoad;
			EventCoordinator.OnNetObjectSpawn += OnNetObjectSpawn;

			if(AmongUsClient.Instance != null && ShouldDespawnGameManager)
			{
				Hydra.Log.LogMessage($"NoGameEnd was enabled in a Local lobby as non-host, despawning GameManager");
				GameManager.Instance.Despawn();
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameLoad -= OnGameLoad;
			EventCoordinator.OnNetObjectSpawn -= OnNetObjectSpawn;

			// Even after respawning in an instance of GameManager, the game remains in a buggy state
			if(AmongUsClient.Instance != null && GameManager.Instance == null && !AmongUsClient.Instance.AmHost && AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame)
			{
				Hydra.Log.LogMessage($"NoGameEnd was disabled in a Local lobby as non-host, respawning GameManager");
				IGameOptions cloneOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);

				GameManager netObjParent = GameManagerCreator.CreateGameManager(GameOptionsManager.Instance.CurrentGameOptions.GameMode);
				netObjParent.LogicOptions.SetGameOptions(cloneOptions);

				BatchedMessage batch = new BatchedMessage();
				batch.QueueSpawn(netObjParent, -2, SpawnFlags.None);
				batch.FinishBatch();
			}
		}
	}
}