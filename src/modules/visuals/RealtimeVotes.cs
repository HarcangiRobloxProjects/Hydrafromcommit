using HydraMenu.network;
using InnerNet;
using System.Linq;
using UnityEngine;

namespace HydraMenu.modules.visuals
{
	internal class RealtimeVotes : Module
	{
		public RealtimeVotes() : base("RealtimeVotes")
		{
			base.Enabled = true;
		}

		private void OnNetObjectSpawn(InnerNetObject netObject)
		{
			if(netObject.SpawnId == (byte)SpawnType.MeetingHud)
			{
				MeetingHud.Instance.SkippedVoting.gameObject.SetActive(true);
			}
		}

		private void OnPlayerCastVote(NetworkedPlayerInfo voter, NetworkedPlayerInfo votee)
		{
			if(MeetingHud.Instance.CurrentState == MeetingHud.MeetingStates.Results) return;

			if(votee != null)
			{
				PlayerVoteArea voteArea = MeetingHud.Instance.playerStates.FirstOrDefault((PlayerVoteArea voteArea) => voteArea.PlayerId == votee.PlayerId);
				if(voteArea == null) return;

				MeetingHud.Instance.BloopAVoteIcon(voter, 0, voteArea.transform);
			}
			else
			{
				MeetingHud.Instance.BloopAVoteIcon(voter, 0, MeetingHud.Instance.SkippedVoting.transform);
			}
		}

		private void OnVotingComplete(MeetingHud.VoterState[] voteStates, NetworkedPlayerInfo exiled, bool isTie, bool wasOverruled)
		{
			// In theory, we could patch MeetingHud::PopulateResults and block its execution to prevent it from re-blooping all the votes
			// however the VoterStates array in the VotingComplete RPC may not actually reflect past vote states in situations where the Force Votes utility was used
			// so our meeting hud would not display the true votes from the VotingComplete RPC
			// TODO: Maybe compare vote states from the serialized MeetingHud object to the one from the VotingComplete RPC and see what has changed?
			ClearBloopedVotes();
		}

		private void BloopAllVotes()
		{
			MeetingHud.Instance.SkippedVoting.gameObject.SetActive(true);

			foreach(PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
			{
				if(!voteArea.DidVote) continue;

				NetworkedPlayerInfo voter = GameData.Instance.GetPlayerById(voteArea.PlayerId);
				NetworkedPlayerInfo votee = GameData.Instance.GetPlayerById(voteArea.VotedForId);
				if(voter == null) continue;

				OnPlayerCastVote(voter, votee);
			}
		}

		private void ClearBloopedVotes()
		{
			MeetingHud.Instance.SkippedVoting.gameObject.SetActive(false);

			foreach(PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
			{
				VoteSpreader spreader = voteArea.GetComponent<VoteSpreader>();
				ClearVoteSpreader(spreader);
			}

			VoteSpreader skipSpreader = MeetingHud.Instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
			ClearVoteSpreader(skipSpreader);
		}

		private void ClearVoteSpreader(VoteSpreader spreader)
		{
			foreach(SpriteRenderer renderer in spreader.Votes)
			{
				renderer.enabled = false;
			}

			spreader.Votes.Clear();
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnNetObjectSpawn += OnNetObjectSpawn;
			EventCoordinator.OnPlayerCastVote += OnPlayerCastVote;
			EventCoordinator.OnVotingComplete += OnVotingComplete;

			if(MeetingHud.Instance != null && MeetingHud.Instance.CurrentState != MeetingHud.MeetingStates.Results)
			{
				BloopAllVotes();
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnNetObjectSpawn -= OnNetObjectSpawn;
			EventCoordinator.OnPlayerCastVote -= OnPlayerCastVote;
			EventCoordinator.OnVotingComplete -= OnVotingComplete;

			if(MeetingHud.Instance != null && MeetingHud.Instance.CurrentState != MeetingHud.MeetingStates.Results)
			{
				ClearBloopedVotes();
			}
		}
	}
}