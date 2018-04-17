using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameSparks.Api.Messages;
using GameSparks.Api.Responses;

public class MatchmakingPanel : SparkBehaviour
{
	string status;
	string[] playerNames = new string[] { "None." };

	private void OnGUI ()
	{
		// Start GUI centering
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();

		// GUI content
		GUILayout.Label ("Status: " + status);
		GUILayout.Label ("Players: " + string.Join (", ", playerNames));

		if (GUILayout.Button ("Find match")) {
            // Your match code goes in this next function
			SparkManager.instance.FindMatch ("TEST_1", 0);
		}

		GUI.contentColor = Color.black;

		// End GUI centering
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndArea ();
	}
	private void OnTryMatchSuccess (MatchmakingResponse response)
	{
		Debug.Log ("On try match success.");
	}

	private void OnTryMatchError (MatchmakingResponse response)
	{
		Debug.Log ("On try match error.");
	}

	private void OnMatchFound (MatchFoundMessage message)
	{
		Debug.Log ("On match found.");

		status = "Match found! Trying to join...";

		playerNames = message.Participants.Select(x => x.DisplayName).ToArray ();

		SparkManager.instance.StartMatch (message);
	}

	private void OnMatchNotFound (MatchNotFoundMessage message)
	{
		Debug.Log ("On match not found.");

		status = "Match not found.";
	}

	private void OnMatchUpdated (MatchUpdatedMessage message) {
		Debug.Log ("On match updated.");

		playerNames = message.Participants.Select (x => x.DisplayName).ToArray ();
	}

	private void OnMatchStart ()
	{
		Debug.Log ("On match start.");

		SceneManager.LoadScene ("Stage");
	}

	private void OnMatchEnd ()
	{
		Debug.Log ("On match end.");
	}
}
