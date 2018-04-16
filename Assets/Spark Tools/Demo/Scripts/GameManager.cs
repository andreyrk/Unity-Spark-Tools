using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SparkBehaviour
{
	public GameObject prefab;

	private void Start() {
		SparkManager.instance.InstantiatePrefab (prefab);
	}
		
	// Connect
	private void OnPlayerConnect (SparkPeer peer)
	{
		Debug.Log ("On player connect.");
	}

	// Disconnect
	private void OnPlayerDisconnect (SparkPeer peer)
	{
		Debug.Log ("On player disconnect.");
	}
}
