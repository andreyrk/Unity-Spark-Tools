using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameSparks.Api.Messages;
using GameSparks.Core;
using GameSparks.Api.Responses;
using GameSparks.RT;
using Newtonsoft.Json;

[AddComponentMenu("Spark Tools/")]
public sealed class SparkManager : MonoBehaviour
{
	public static SparkManager instance;

	public enum OpCode
	{
		Sync = 1,
		RPC = 2,
		LocalRPC = 3,
        SparkView_RPC = 4,
        SparkView_LocalRPC = 5
	}

	[HideInInspector]
	public GameSparksRTUnity sparkNetwork;

	[HideInInspector]
	public SparkMatch sparkMatch;

	[HideInInspector]
	public List<SparkView> sparkViews = new List<SparkView> ();

	[HideInInspector]
	public bool isAvailable;

	[HideInInspector]
	public bool IsMasterPlayer {
		get {
			return LocalPlayer == MasterPlayer;
		}
	}

	public SparkPeer MasterPlayer {
		get {
			return sparkMatch.FindBy_Id (sparkMatch.peerList.Min (x => x.id));
		}
	}

	[HideInInspector]
	public SparkPeer LocalPlayer {
		get {
			return sparkMatch.FindBy_Id ((int)sparkNetwork.PeerId);
		}
	}

	[HideInInspector]
	public bool IsReady { get; private set; }

	private List<SparkRPC> RPC_Buffer = new List<SparkRPC> ();
	private List<SparkRPC> LocalRPC_Buffer = new List<SparkRPC> ();

	private void Awake ()
	{
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (this.gameObject);
		} else {
			Destroy (gameObject);
		}

		sparkNetwork = gameObject.AddComponent<GameSparksRTUnity> ();

		SceneManager.sceneLoaded += OnSceneLoaded;
		SceneManager.sceneUnloaded += OnSceneUnloaded;

		MatchFoundMessage.Listener = OnMatchFound;
		MatchNotFoundMessage.Listener = OnMatchNotFound;
		MatchUpdatedMessage.Listener = OnMatchUpdated;

		GS.GameSparksAvailable += (isAvailable) => {
			this.isAvailable = isAvailable;
		};

		Update_SparkViews ();
	}

	private void OnDestroy ()
	{
		GS.Disconnect ();
	}

	/// <summary>
	/// Updates the spark views.
	/// </summary>
	public void Update_SparkViews ()
	{
		sparkViews = GameObject.FindObjectsOfType<SparkView> ().ToList ();
	}

	/// <summary>
	/// Starts the match.
	/// </summary>
	/// <param name="message">Message.</param>
	public void StartMatch (MatchFoundMessage message)
	{
		sparkMatch = new SparkMatch (message);

		foreach (MatchFoundMessage._Participant participant in message.Participants) {
			SparkPeer peer = new SparkPeer (participant.DisplayName, participant.Id, participant.PeerId.Value);

			sparkMatch.peerList.Add (peer);
		}

		sparkNetwork.Configure (message,
			OnPlayerConnect,
			OnPlayerDisconnect,
			OnReady,
			OnPacket
		);

		sparkNetwork.Connect ();
	}

	/// <summary>
	/// Raises the ready event.
	/// </summary>
	/// <param name="isReady">If set to <c>true</c> is ready.</param>
	private void OnReady (bool isReady)
	{
		this.IsReady = isReady;
	
		if (isReady) {
			foreach (SparkView view in sparkViews) {
				view.SendEvent_MatchStart ();
			}
		} else {
			foreach (SparkView view in sparkViews) {
				view.SendEvent_MatchEnd ();
			}
		}
	}

	#region Scene Events

	/// <summary>
	/// Raises the scene loaded event.
	/// </summary>
	/// <param name="scene">Scene.</param>
	/// <param name="mode">Mode.</param>
	private void OnSceneLoaded (Scene scene, LoadSceneMode mode)
	{
		Update_SparkViews ();

		if (IsReady) {
			foreach (SparkView view in sparkViews) {
				if (view.IsLocalPlayer) {
					view.SendEvent_PlayerConnect (sparkMatch.peerList.ToArray ());
				}
			}
		}
	}

	/// <summary>
	/// Raises the scene unloaded event.
	/// </summary>
	/// <param name="scene">Scene.</param>
	private void OnSceneUnloaded (Scene scene)
	{
		
	}

	#endregion

	#region Player Events

	/// <summary>
	/// Raises the player connect event.
	/// </summary>
	/// <param name="peerId">Peer identifier.</param>
	private void OnPlayerConnect (int peerId)
	{
		foreach (SparkView view in sparkViews)
        {
            view.Call("RPC_Trigger");
            view.Call("LocalRPC_Trigger");

            view.SendEvent_PlayerConnect (sparkMatch.FindBy_Id (peerId));
		}

		foreach (SparkRPC rpc in LocalRPC_Buffer) {
			LocalRPC (rpc.MethodName, rpc.ReceiverIds, false, rpc.Parameters);
		}
	}

    /// <summary>
    /// Raises the player disconnect event.
    /// </summary>
    /// <param name="peerId">Peer identifier.</param>
    private void OnPlayerDisconnect(int peerId)
    {
        SparkPeer peer = sparkMatch.FindBy_Id(peerId);

        foreach (SparkView view in sparkViews)
        {
            if (view.sparkPeer.id == peerId)
            {
                view.SendEvent_PlayerDisconnect(peer);
            }
        }

        if (IsMasterPlayer)
        {
            LocalRPC("Remove_LocalRPC_Buffer", SparkTargets.Others, false, peer);
            LocalRPC("Remove_RPC_Buffer", SparkTargets.Others, false, peer);

            Remove_LocalRPC_Buffer(peer);
            Remove_RPC_Buffer(peer);
        }

        for (int i = sparkViews.Count - 1; i >= 0; i--)
        {
            if (sparkViews[i].sparkPeer.id == peerId)
            {
                Destroy(sparkViews[i].gameObject);
            }
        }

        if (IsMasterPlayer)
        {
            foreach (SparkView view in sparkViews)
            {
                view.Call("LocalRPC", "Remove_LocalRPC_Buffer", SparkTargets.Others, false, peer);
                view.Call("LocalRPC", "Remove_RPC_Buffer", SparkTargets.Others, false, peer);
            }
        }
    }

	#endregion

	#region Authentication

	/// <summary>
	/// Registers with the specified userName, displayName and password.
	/// </summary>
	/// <param name="userName">User name.</param>
	/// <param name="displayName">Display name.</param>
	/// <param name="password">Password.</param>
	public void Register (string userName, string displayName, string password)
	{
		new GameSparks.Api.Requests.RegistrationRequest ()
			.SetDisplayName (displayName)
			.SetPassword (password)
			.SetUserName (userName)
			.Send (OnRegisterSuccess, OnRegisterError);
	}

	/// <summary>
	/// Raises the register success event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnRegisterSuccess (RegistrationResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_RegisterSuccess (response);
		}
	}

	/// <summary>
	/// Raises the register error event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnRegisterError (RegistrationResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_RegisterError (response);
		}
	}

	/// <summary>
	/// Logs in with the specified userName, displayName and password.
	/// </summary>
	/// <param name="userName">User name.</param>
	/// <param name="password">Password.</param>
	public void Login (string userName, string displayName, string password)
	{
		new GameSparks.Api.Requests.AuthenticationRequest ()
			.SetUserName (userName)
			.SetPassword (password)
			.Send (OnLoginSuccess, OnLoginError);
	}

	/// <summary>
	/// Logs in anonymously with the specified userName and displayName.
	/// </summary>
	/// <param name="userName">User name.</param>
	/// <param name="displayName">Display name.</param>
	public void Guest(string userName, string displayName) {
		new GameSparks.Api.Requests.DeviceAuthenticationRequest ()
			.SetDisplayName (displayName)
			.Send (OnLoginSuccess, OnLoginError);
	}

	/// <summary>
	/// Raises the login success event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnLoginSuccess (AuthenticationResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_LoginSuccess (response);
		}
	}

	/// <summary>
	/// Raises the login error event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnLoginError (AuthenticationResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_LoginError (response);
		}
	}

	#endregion

	#region Matchmaking

	/// <summary>
	/// Finds the match.
	/// </summary>
	/// <param name="matchCode">Match code.</param>
	/// <param name="skill">Skill.</param>
	public void FindMatch (string matchCode, int skill)
	{
		new GameSparks.Api.Requests.MatchmakingRequest ()
			.SetMatchShortCode (matchCode)
			.SetSkill (0)
			.Send (OnTryMatchSuccess, OnTryMatchError);
	}

	/// <summary>
	/// Raises the try match success event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnTryMatchSuccess (MatchmakingResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_TryMatchSuccess (response);
		}
	}

	/// <summary>
	/// Raises the try match error event.
	/// </summary>
	/// <param name="response">Response.</param>
	private void OnTryMatchError (MatchmakingResponse response)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_TryMatchError (response);
		}
	}

	/// <summary>
	/// Raises the match found event.
	/// </summary>
	/// <param name="message">Message.</param>
	private void OnMatchFound (MatchFoundMessage message)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_MatchFound (message);
		}
	}

	/// <summary>
	/// Raises the match not found event.
	/// </summary>
	/// <param name="message">Message.</param>
	private void OnMatchNotFound (MatchNotFoundMessage message)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_MatchNotFound (message);
		}
	}

	/// <summary>
	/// Raises the match updated event.
	/// </summary>
	/// <param name="message">Message.</param>
	private void OnMatchUpdated (MatchUpdatedMessage message)
	{
		foreach (SparkView view in sparkViews) {
			view.SendEvent_MatchUpdated (message);
		}

		foreach (MatchUpdatedMessage._Participant participant in message.Participants) {

			SparkPeer peer = new SparkPeer (participant.DisplayName, participant.Id, (int)participant.PeerId);

			if (message.AddedPlayers.Contains (participant.Id)) {
				sparkMatch.peerList.Add (peer);
			}

			if (message.RemovedPlayers.Contains (participant.Id)) {
				sparkMatch.peerList.Remove (peer);
			}
		}
	}

	#endregion

	#region RPC

	public int[] GetReceivers(SparkTargets targets) {
		if (targets == SparkTargets.Not_Master) {
			List<int> receiverIds = sparkMatch.peerList.Select (x => x.id).ToList ();
			receiverIds.Remove (MasterPlayer.id);

			return receiverIds.ToArray ();
		}

		if (targets == SparkTargets.Master) {
			return new int[] { MasterPlayer.id };
		}

		if (targets == SparkTargets.Others) {
			List<int> receiverIds = sparkMatch.peerList.Select (x => x.id).ToList ();
			receiverIds.Remove (LocalPlayer.id);

			return receiverIds.ToArray ();
		}

		return null;
	}

	/// <summary>
	/// Local RPC call.
	/// </summary>
	/// <param name="methodName">Method name.</param>
	/// <param name="targetPlayer">Target player.</param>
	/// <param name="isBuffered">If set to <c>true</c> is buffered.</param>
	/// <param name="parameters">Parameters.</param>
	private void LocalRPC (string methodName, SparkTargets targets, bool isBuffered, params object[] parameters)
	{
		LocalRPC (methodName, GetReceivers (targets), isBuffered, parameters);
	}

	private void LocalRPC (string methodName, int[] receiverIds, bool isBuffered, params object[] parameters)
	{
		SendEvent_RPC (OpCode.LocalRPC, methodName, receiverIds, isBuffered, parameters);
	}

	/// <summary>
	/// Sends the RPC.
	/// </summary>
	/// <param name="code">Code.</param>
	/// <param name="methodName">Method name.</param>
	/// <param name="targetPlayer">Target player.</param>
	/// <param name="isBuffered">If set to <c>true</c> is buffered.</param>
	/// <param name="isLocal">If set to <c>true</c> is local.</param>
	/// <param name="parameters">Parameters.</param>
	private void SendEvent_RPC (OpCode code, string methodName, int[] receiverIds, bool isBuffered, params object[] parameters)
	{
		SparkRPC rpc = new SparkRPC (Guid.Empty, methodName, receiverIds, LocalPlayer, parameters);

		using (RTData data = RTData.Get ()) {
			data.SetString (1, SparkExtensions.Serialize (rpc));

			sparkNetwork.SendData (Convert.ToInt32 (code), GameSparksRT.DeliveryIntent.RELIABLE, data, receiverIds);
		}
			
		if (isBuffered) {
        	LocalRPC_Buffer.Add (rpc);
			LocalRPC ("Add_LocalRPC_Buffer", SparkTargets.Others, false, rpc);
		}
	}

	/// <summary>
	/// Adds the RPC to the RPC buffer.
	/// </summary>
	/// <param name="rpc">RPC.</param>
	private void Add_RPC_Buffer (SparkRPC rpc)
	{
		RPC_Buffer.Add (rpc);
	}

	/// <summary>
	/// Adds the RPC to the local RPC buffer.
	/// </summary>
	/// <param name="rpc">RPC.</param>
	private void Add_LocalRPC_Buffer (SparkRPC rpc)
	{
		LocalRPC_Buffer.Add (rpc);
	}

	/// <summary>
	/// Removes the RPC from the RPC buffer
	/// </summary>
	/// <param name="sender">Sender.</param>
	private void Remove_RPC_Buffer (SparkPeer sender)
	{
		RPC_Buffer.RemoveAll (x => x.Sender == sender);
	}

	/// <summary>
	/// Removes the RPC from the local RPC buffer.
	/// </summary>
	/// <param name="sender">Sender.</param>
	private void Remove_LocalRPC_Buffer (SparkPeer sender)
	{
		LocalRPC_Buffer.RemoveAll (x => x.Sender == sender);
	}

    private void LocalRPC_Trigger()
    {
        foreach (SparkRPC rpc in LocalRPC_Buffer)
        {
            LocalRPC(rpc.MethodName, rpc.ReceiverIds, false, rpc.Parameters);
        }
    }

    #endregion

    #region Instantiation

    public GameObject InstantiatePrefab (GameObject prefab, bool isBuffered = true)
	{
		return TryInstantiate (prefab, prefab.transform.position, prefab.transform.rotation, isBuffered);
	}

	public GameObject InstantiatePrefab (GameObject prefab, Vector3 position, bool isBuffered = true)
	{
		return TryInstantiate (prefab, position, prefab.transform.rotation, isBuffered);
	}

	public GameObject InstantiatePrefab (GameObject prefab, Quaternion rotation, bool isBuffered = true)
	{
		return TryInstantiate (prefab, prefab.transform.position, rotation, isBuffered);
	}
		
	public GameObject InstantiatePrefab (GameObject prefab, Vector3 position, Quaternion rotation, bool isBuffered = true)
	{
		return TryInstantiate (prefab, position, rotation, isBuffered);
	}

	private GameObject TryInstantiate(GameObject prefab, Vector3 position, Quaternion rotation, bool isBuffered = true) {
		SparkIdentity identity = prefab.GetComponent<SparkIdentity> ();
		SparkPeer sender = sparkMatch.peerList.Find (x => x.id == sparkNetwork.PeerId);

		List<Guid> guids = new List<Guid> ();
        foreach (SparkBehaviour behaviour in prefab.GetComponentsInChildren<SparkBehaviour>())
        {
            guids.Add(Guid.NewGuid());
        }
        foreach (SparkView view in prefab.GetComponentsInChildren<SparkView>())
        {
            guids.Add(Guid.NewGuid());
        }

        GameObject instantiate = NetworkInstantiate (identity.uniqueId, isBuffered, position, rotation, sender, guids.ToArray ());
		LocalRPC ("NetworkInstantiate", SparkTargets.Others, isBuffered, identity.uniqueId, isBuffered, position, rotation, sender, guids.ToArray ());

		return instantiate;
	}

	public Dictionary<Guid, SparkBehaviour> behaviours = new Dictionary<Guid, SparkBehaviour> ();
		
	private GameObject NetworkInstantiate (string uniqueId, bool isBuffered, Vector3 position, Quaternion rotation, SparkPeer sender, Guid[] guids)
	{
		GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject> ();

		foreach (GameObject instantiate in gameObjects) {
			
			SparkIdentity identity = instantiate.GetComponent<SparkIdentity> ();

			if (identity != null && identity.uniqueId == uniqueId) {
				GameObject obj = GameObject.Instantiate (instantiate, position, rotation);

				int count = 0;
				foreach (SparkBehaviour behaviour in obj.GetComponentsInChildren<SparkBehaviour>()) {
					behaviour.netGuid = guids [count++];
				}

				foreach (SparkView view in obj.GetComponentsInChildren<SparkView>()) {
					view.sparkPeer = sender;
					view.SendEvent_OnInstantiate ();
                    view.netGuid = guids[count++];
				}

				return obj;
			}
		}

		throw new Exception ("A GameObject with this SparkIdentity ID was not found.");
	}

	#endregion

	#region Data Logic

	/// <summary>
	/// Raises the packet event.
	/// </summary>
	/// <param name="packet">Packet.</param>
	private void OnPacket (RTPacket packet)
	{
        switch ((OpCode)Enum.ToObject(typeof(OpCode), packet.OpCode))
        {
            case OpCode.Sync:

                // Synchronization handling

                foreach (SparkView view in sparkViews)
                {
                    view.OnPacket(packet);
                }

                break;
            case OpCode.RPC:

                // RPC handling

                SparkRPC rpc = SparkExtensions.Deserialize<SparkRPC>(packet.Data.GetString(1));

                foreach (SparkView view in sparkViews)
                {
                    if (rpc.ReceiverIds.Contains(view.sparkPeer.id))
                    {
                        foreach (SparkBehaviour behaviour in view.observedBehaviours)
                        {
                            behaviour.Call(rpc.MethodName, rpc.Parameters);
                        }
                    }
                }

                break;

            case OpCode.LocalRPC:

                SparkRPC localRpc = SparkExtensions.Deserialize<SparkRPC>(packet.Data.GetString(1));

                this.Call(localRpc.MethodName, localRpc.Parameters);

                break;
            case OpCode.SparkView_RPC:

                // RPC handling

                foreach (SparkView view in sparkViews)
                {
                    view.OnPacket(packet);
                }

                break;

            case OpCode.SparkView_LocalRPC:
                
                foreach (SparkView view in sparkViews)
                {
                    view.OnPacket(packet);
                }

                break;
        }
    }

	#endregion
}
