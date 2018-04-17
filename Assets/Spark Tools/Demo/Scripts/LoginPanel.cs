using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameSparks;
using GameSparks.Core;
using GameSparks.Api;
using GameSparks.Api.Responses;

public class LoginPanel : SparkBehaviour
{

	public string status;

	public string userName;
	public string displayName;
	public string password;

	private void Awake ()
	{
		// Gets cached authentication data from previous sessions
		userName = PlayerPrefs.GetString ("Authentication_UserName", userName);
		displayName = PlayerPrefs.GetString ("Authentication_DisplayName", displayName);
		password = PlayerPrefs.GetString ("Authentication_Password", password);
	}

	private void Start ()
	{
		GS.GameSparksAvailable += (isAvailable) => {
			if (isAvailable) {
				status = "Connected.";
			} else {
				GS.Reset ();
				status = "Disconnected.";
			}
		};
	}

	private void OnGUI ()
	{
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));

		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();

		userName = GUILayout.TextField (userName);
		displayName = GUILayout.TextField (displayName);
		password = GUILayout.PasswordField (password, '*');

		if (GUILayout.Button ("Register")) {
			CacheData ();
			SparkManager.instance.Register (displayName, userName, password);
		}

		if (GUILayout.Button ("Login")) {
			CacheData ();
			SparkManager.instance.Login (userName, displayName, password);
		}

		if (GUILayout.Button ("Guest")) {
			CacheData ();
			SparkManager.instance.Guest (userName, displayName);
		}

		GUI.contentColor = Color.black;

		GUILayout.Label ("Status: " + status);

		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();

		GUILayout.EndArea ();
	}

	private void CacheData() {
		PlayerPrefs.SetString ("Authentication_UserName", userName);
		PlayerPrefs.SetString ("Authentication_DisplayName", displayName);
		PlayerPrefs.SetString ("Authentication_Password", password);
	}

	private void OnRegisterSuccess (RegistrationResponse response)
	{
		Debug.Log ("Register success.");

		status = "Register success. Logging in...";

		SparkManager.instance.Login (userName, displayName, password);
	}

	private void OnRegisterError (RegistrationResponse response)
	{
		Debug.Log ("Register error.");

		status = "Register error. Try again.";
	}

	private void OnLoginSuccess(AuthenticationResponse response) {
		Debug.Log ("Login success.");

		status = "Login success.";

		SceneManager.LoadScene ("Matchmaking");
	}

	private void OnLoginError(AuthenticationResponse response) {
		Debug.Log ("Login error.");

		status = "Login error.";
	}
}
