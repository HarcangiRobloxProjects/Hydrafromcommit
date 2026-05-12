using AmongUs.GameOptions;
using HydraMenu.features;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : ISection
	{
		public RolesSection()
		{
			name = "Roles";
		}

		private RoleTypes selectedRole = RoleTypes.Crewmate;

		// The RoleTypes enum has some weird gaps, like everything from Crewmate (0) to Tracker (10) is normal, but then Detective is 12 and Viper is 18
		// https://www.innersloth.com/2026-roadmap-part-1/
		// The Among Us 2026 roadmap does state that there are currently 15 prototype roles in the works,
		// could these gaps be attributed to roles that have not been added to the retail version of the game?
		public readonly Dictionary<byte, RoleTypes> roles = new Dictionary<byte, RoleTypes>()
		{
			{ 0, RoleTypes.Crewmate },
			{ 1, RoleTypes.Impostor },
			{ 2, RoleTypes.Scientist},
			{ 3, RoleTypes.Engineer},
			{ 4, RoleTypes.GuardianAngel },
			{ 5, RoleTypes.Shapeshifter },
			{ 6, RoleTypes.Noisemaker },
			{ 7, RoleTypes.Phantom },
			{ 8, RoleTypes.Tracker },
			{ 9, RoleTypes.Detective },
			{ 10, RoleTypes.Viper },
			{ 11, RoleTypes.CrewmateGhost },
			{ 12, RoleTypes.ImpostorGhost }
		};

		public override void Render()
		{
			Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, "Vent As Crewmate");
			Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, "Move In Vents");

			Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, "Sabotage As Crewmate");
			Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, "Allow Sabotaging In Vents As Imposter");

			Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, "Disable Shapeshift Animation");
			// Roles.DisablePhantomEndAnimation = GUILayout.Toggle(Roles.DisablePhantomEndAnimation, "Disable Phantom End Animation");

			GUILayout.Label($"Change role to: {selectedRole}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("Apply Role" + (AmongUsClient.Instance.AmHost ? "" : " (Local)")) && PlayerControl.LocalPlayer)
			{
				Hydra.Log.LogInfo($"Updating role to {selectedRole}");
				UpdateRole(selectedRole);

				if(AmongUsClient.Instance.AmHost)
				{
					Hydra.Log.LogInfo("Since we are host, we can send the SetRole RPC to sync the new role to the server");
					PlayerControl.LocalPlayer.RpcSetRole(selectedRole, true);
				}

				Hydra.notifications.Send("Update Role", $"Your role has been updated to {selectedRole}.");
			}
			GUILayout.EndHorizontal();
		}

		public static void UpdateRole(RoleTypes role)
		{
			bool isGhost = RoleManager.IsGhostRole(role);

			// When a player turns into the ghost, the PlayerControl::CoSetRole function hides the report button. This function then calls the RoleManager::SetRole function we call here
			// This means when we are changing between normal or ghost roles, the report button will not properly be added/removed, so we have to reimplement it here
			// We also cannot use PlayerControl::CoSetRole directly as it prevents in-game roles being overriden by non-ghosts ones (we could just patch it and disable overriding, however a blackout occurs when the game starts)
			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);
		}
	}
}