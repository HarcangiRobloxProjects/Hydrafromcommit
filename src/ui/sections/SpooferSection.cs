using HydraMenu.assets;
using HydraMenu.modules;
using System;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : Section
	{
		public SpooferSection() : base("Spoofer") { }

		public override void Render()
		{
			GUILayout.Label("Version Spoofer:");
			ModuleManager.spoofVersion.Enabled = GUILayout.Toggle(ModuleManager.spoofVersion.Enabled, "Enable Version Spoofing");

			GUILayout.Label($"Spoofed Version: {GameAssets.BroadcastVersions[ModuleManager.spoofVersion.SpoofedVersion]} ({ModuleManager.spoofVersion.SpoofedVersion})");
			ModuleManager.spoofVersion.SpoofedVersion = Controls.HorizontalListSlider(GameAssets.BroadcastVersions, ModuleManager.spoofVersion.SpoofedVersion);

			ModuleManager.spoofVersion.UseModdedProtocol = GUILayout.Toggle(ModuleManager.spoofVersion.UseModdedProtocol, "Use Modded Protocol");

			GUILayout.Label($"Spoof Mod Registration: {GameAssets.ModRegistrationGuids[CurrentModRegistration.ModRegistrationGuidString]}");
			CurrentModRegistration.ModRegistrationGuidString = Controls.HorizontalListSlider(GameAssets.ModRegistrationGuids, CurrentModRegistration.ModRegistrationGuidString);

			GUILayout.Space(5);
			GUILayout.Label("Level Spoofer:");

			ModuleManager.spoofLevel.Enabled = GUILayout.Toggle(ModuleManager.spoofLevel.Enabled, "Enabled");
			GUILayout.Label($"Spoofed Level: {ModuleManager.spoofLevel.SpoofedLevel}");
			ModuleManager.spoofLevel.SpoofedLevel = (uint)GUILayout.HorizontalSlider(ModuleManager.spoofLevel.SpoofedLevel, 1, 200);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("-100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel - 100);
			}

			if(GUILayout.Button("-10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel - 10);
			}

			if(GUILayout.Button("+10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel + 10);
			}

			if(GUILayout.Button("+100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel + 100);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("Send Level Update"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(ModuleManager.spoofLevel.SpoofedLevel - 1);
				Hydra.notifications.Send("Level Updater", $"Your level has been changed to {ModuleManager.spoofLevel.SpoofedLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("Platform Spoofer:");

			GUILayout.Label($"Spoofed Platform: {GameAssets.GetPlatformName(ModuleManager.spoofDevice.SpoofedPlatform)}");
			ModuleManager.spoofDevice.SpoofedPlatform = (Platforms)Controls.HorizontalListSlider(GameAssets.PlayerPlatforms, (int)ModuleManager.spoofDevice.SpoofedPlatform);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// Do we really need to have an upper bounds on the level value?
			// I doubt anyone will press the +100 that much anyway
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			ModuleManager.spoofLevel.SpoofedLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}