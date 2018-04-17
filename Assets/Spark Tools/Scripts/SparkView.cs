using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using GameSparks.Api.Messages;
using GameSparks.Core;
using GameSparks.Api.Responses;
using GameSparks.RT;
using Newtonsoft.Json;

[Serializable]
[AddComponentMenu("Spark Tools/Spark View")]
[RequireComponent(typeof(SparkIdentity))]
public sealed class SparkView : MonoBehaviour
{
    public Guid netGuid = Guid.Empty;

    public enum Ownership
	{
		Fixed
	}

	public Ownership ownership;

    private List<SparkRPC> RPC_Buffer = new List<SparkRPC>();
    private List<SparkRPC> LocalRPC_Buffer = new List<SparkRPC>();

    /// <summary>
    /// Returns true in case this SparkView's peer is the local player.
    /// </summary>
    /// <value><c>true</c> if is local player; otherwise, <c>false</c>.</value>
    public bool IsLocalPlayer {
		get {
			if (sparkPeer.id != 0 && SparkManager.instance.sparkNetwork.PeerId.HasValue) {
				return sparkPeer.id == (int)SparkManager.instance.sparkNetwork.PeerId;
			} else {
				return sparkPeer.id == 0;
			}
		}
	}

	/// <summary>
	/// The peer that owns this SparkView.
	/// </summary>
	public SparkPeer sparkPeer = new SparkPeer ("None.", "0", 0);

	public int sendRate = 10;
	private float sendTime;

	private Dictionary<SparkBehaviour, SparkStream> streams;
	private SparkMessageInfo info;

	public List<SparkBehaviour> observedBehaviours;

	public GameSparksRT.DeliveryIntent observeMethod;

	#region Internal Logic

	private void Awake ()
	{
		observedBehaviours.RemoveAll (x => x == null);
        observedBehaviours.ForEach(x => x.sparkView = this);
	}

	private void Start ()
	{
		sendTime = sendRate;

		streams = new Dictionary<SparkBehaviour, SparkStream> ();
		info = new SparkMessageInfo (sparkPeer);
	}

	private void OnEnable ()
	{
		if (SparkManager.instance) {
			SparkManager.instance.Update_SparkViews ();
		}
	}

	private void OnDisable ()
	{
		if (SparkManager.instance) {
			SparkManager.instance.Update_SparkViews ();
		}
	}

	private void Update ()
	{
		if (!SparkManager.instance.isAvailable) {
			return;
		}

		sendTime += Time.deltaTime;

		if (sendTime >= 1f / sendRate) {
			sendTime = 0;

			foreach (SparkBehaviour behaviour in observedBehaviours) {
				if (behaviour.netGuid == Guid.Empty) {
					continue;
				}

				SparkStream stream;
	
				if (!streams.TryGetValue (behaviour, out stream)) {
					stream = new SparkStream (behaviour.netGuid, observeMethod, true);
					streams [behaviour] = stream;
				}

				SendEvent_OnSerializeSparkView (behaviour, stream, info);

				stream.Call("Finish", info);
			}
		}
	}

	#endregion

	#region Player Logic

	/// <summary>
	/// Sends the event player connect.
	/// </summary>
	/// <param name="peers">Peers.</param>
	public void SendEvent_PlayerConnect (params SparkPeer[] peers)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
			foreach (SparkPeer peer in peers) {
                behaviour.Call("OnPlayerConnect", peer);
			}
		}
	}

	/// <summary>
	/// Sends the event player disconnect.
	/// </summary>
	/// <param name="peers">Peers.</param>
	public void SendEvent_PlayerDisconnect (params SparkPeer[] peers)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
			foreach (SparkPeer peer in peers) {
                behaviour.Call("OnPlayerDisconnect", peer);
			}
		}
	}

	#endregion

	#region Authentication Logic

	/// <summary>
	/// Sends the event register success.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_RegisterSuccess (RegistrationResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnRegisterSuccess", response);
		}
	}

	/// <summary>
	/// Sends the event register error.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_RegisterError (RegistrationResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnRegisterError", response);
		}
	}

	/// <summary>
	/// Sends the event login success.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_LoginSuccess (AuthenticationResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnLoginSuccess", response);
		}
	}

	/// <summary>
	/// Sends the event login error.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_LoginError (AuthenticationResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnLoginError", response);
		}
	}

	#endregion

	#region Matchmaking Logic

	/// <summary>
	/// Sends the event try match success.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_TryMatchSuccess (MatchmakingResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnTryMatchSuccess", response);
		}
	}

	/// <summary>
	/// Sends the event try match error.
	/// </summary>
	/// <param name="response">Response.</param>
	public void SendEvent_TryMatchError (MatchmakingResponse response)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnTryMatchError", response);
		}
	}

	/// <summary>
	/// Sends the event match found.
	/// </summary>
	/// <param name="message">Message.</param>
	public void SendEvent_MatchFound (MatchFoundMessage message)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnMatchFound", message);
		}
	}

	/// <summary>
	/// Sends the event match not found.
	/// </summary>
	/// <param name="message">Message.</param>
	public void SendEvent_MatchNotFound (MatchNotFoundMessage message)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnMatchNotFound", message);
		}
	}

	/// <summary>
	/// Sends the event match updated.
	/// </summary>
	/// <param name="message">Message.</param>
	public void SendEvent_MatchUpdated (MatchUpdatedMessage message)
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnMatchUpdated", message);
		}
	}

	/// <summary>
	/// Sends the event match start.
	/// </summary>
	public void SendEvent_MatchStart ()
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnMatchStart");
		}
	}

	/// <summary>
	/// Sends the event match end.
	/// </summary>
	public void SendEvent_MatchEnd ()
	{
		foreach (SparkBehaviour behaviour in observedBehaviours) {
            behaviour.Call("OnMatchEnd");
		}
	}

    #endregion

    #region RPC

    /// <summary>
    /// Local RPC call.
    /// </summary>
    /// <param name="methodName">Method name.</param>
    /// <param name="targetPlayer">Target player.</param>
    /// <param name="isBuffered">If set to <c>true</c> is buffered.</param>
    /// <param name="parameters">Parameters.</param>
    private void LocalRPC(string methodName, SparkTargets targets, bool isBuffered, params object[] parameters)
    {
        LocalRPC(methodName, SparkManager.instance.GetReceivers(targets), isBuffered, parameters);
    }

    private void LocalRPC(string methodName, int[] receiverIds, bool isBuffered, params object[] parameters)
    {
        SendEvent_RPC(netGuid, SparkManager.OpCode.SparkView_LocalRPC, methodName, receiverIds, isBuffered, true, parameters);
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
    private void SendEvent_RPC(Guid netGuid, SparkManager.OpCode code, string methodName, int[] receiverIds, bool isBuffered, bool isLocal, params object[] parameters)
    {
        SparkRPC rpc = new SparkRPC(netGuid, methodName, receiverIds, SparkManager.instance.LocalPlayer, parameters);

        using (RTData data = RTData.Get())
        {
            data.SetString(1, SparkExtensions.Serialize(rpc));

            SparkManager.instance.sparkNetwork.SendData(Convert.ToInt32(code), GameSparksRT.DeliveryIntent.RELIABLE, data, receiverIds);
        }

        if (isBuffered)
        {
            if (isLocal)
            {
                LocalRPC_Buffer.Add(rpc);
                LocalRPC("Add_LocalRPC_Buffer", SparkTargets.Others, false, rpc);
            }
            else
            {
                RPC_Buffer.Add(rpc);
                LocalRPC("Add_RPC_Buffer", SparkTargets.Others, false, rpc);
            }
        }
    }

    /// <summary>
    /// Adds the RPC to the RPC buffer.
    /// </summary>
    /// <param name="rpc">RPC.</param>
    private void Add_RPC_Buffer(SparkRPC rpc)
    {
        RPC_Buffer.Add(rpc);
    }

    /// <summary>
    /// Adds the RPC to the local RPC buffer.
    /// </summary>
    /// <param name="rpc">RPC.</param>
    private void Add_LocalRPC_Buffer(SparkRPC rpc)
    {
        LocalRPC_Buffer.Add(rpc);
    }

    /// <summary>
    /// Removes the RPC from the RPC buffer
    /// </summary>
    /// <param name="sender">Sender.</param>
    private void Remove_RPC_Buffer(SparkPeer sender)
    {
        RPC_Buffer.RemoveAll(x => x.Sender == sender);
    }

    /// <summary>
    /// Removes the RPC from the local RPC buffer.
    /// </summary>
    /// <param name="sender">Sender.</param>
    private void Remove_LocalRPC_Buffer(SparkPeer sender)
    {
        LocalRPC_Buffer.RemoveAll(x => x.Sender == sender);
    }

    private void RPC_Trigger()
    {
        foreach (SparkRPC rpc in RPC_Buffer)
        {
            foreach (SparkBehaviour behaviour in observedBehaviours)
            {
                behaviour.RPC(rpc.MethodName, rpc.ReceiverIds, false, rpc.Parameters);
            }
        }
    }

    private void LocalRPC_Trigger()
    {
        foreach (SparkRPC rpc in LocalRPC_Buffer)
        {
            LocalRPC(rpc.MethodName, rpc.ReceiverIds, false, rpc.Parameters);
        }
    }

    #endregion

    #region Data Logic

    /// <summary>
    /// Sends the event on instantiate.
    /// </summary>
    public void SendEvent_OnInstantiate()
    {
        foreach (SparkBehaviour behaviour in observedBehaviours)
        {
            behaviour.Call("OnInstantiate");
        }
    }

    /// <summary>
    /// Sends the event on serialize spark view.
    /// </summary>
    /// <param name="behaviour">Behaviour.</param>
    /// <param name="stream">Stream.</param>
    /// <param name="info">Info.</param>
    private void SendEvent_OnSerializeSparkView(SparkBehaviour behaviour, SparkStream stream, SparkMessageInfo info)
    {
        behaviour.Call("OnSerializeSparkView", stream, info);
    }

    /// <summary>
    /// Raises the packet event.
    /// </summary>
    /// <param name="packet">Packet.</param>
    public void OnPacket(RTPacket packet)
    {

        switch ((SparkManager.OpCode)Enum.ToObject(typeof(SparkManager.OpCode), packet.OpCode))
        {
            case SparkManager.OpCode.Sync:

                SparkStream stream = SparkExtensions.Deserialize<SparkStream>(packet.Data.GetString(1));
                SparkMessageInfo info = SparkExtensions.Deserialize<SparkMessageInfo>(packet.Data.GetString(2));

                foreach (SparkBehaviour behaviour in observedBehaviours)
                {
                    if (stream.NetGuid == behaviour.netGuid)
                    {
                        SendEvent_OnSerializeSparkView(behaviour, stream, info);
                    }
                }

                break;
            case SparkManager.OpCode.SparkView_RPC:

                // RPC handling

                SparkRPC rpc = SparkExtensions.Deserialize<SparkRPC>(packet.Data.GetString(1));
                
                foreach (SparkBehaviour behaviour in observedBehaviours)
                {
                    if (behaviour.netGuid == rpc.NetGuid)
                    {
                        behaviour.Call(rpc.MethodName, rpc.Parameters);
                    }
                }

                break;
            case SparkManager.OpCode.SparkView_LocalRPC:
                SparkRPC localRpc = SparkExtensions.Deserialize<SparkRPC>(packet.Data.GetString(1));

                if (localRpc.NetGuid == netGuid)
                {
                    this.Call(localRpc.MethodName, localRpc.Parameters);
                }

                break;
        }
    }

	#endregion
}
