using UnityEngine;

public class GameManager : SparkBehaviour
{
	public GameObject prefab;

	private void Start() {
		SparkManager.instance.InstantiatePrefab (prefab);
	}

    private void OnGameStart()
    {
        Debug.Log("The game has started! This instance is the master player.");
    }
		
	private void OnPlayerConnect (SparkPeer peer)
	{
		Debug.Log ("On player connect.");
	}
    
	private void OnPlayerDisconnect (SparkPeer peer)
	{
		Debug.Log ("On player disconnect.");
	}
}
