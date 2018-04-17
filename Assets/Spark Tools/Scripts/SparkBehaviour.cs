using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using GameSparks.Core;
using GameSparks.Api.Responses;
using GameSparks.RT;

[AddComponentMenu("Spark Tools/Internal/Spark Behaviour")]
[RequireComponent(typeof(SparkIdentity))]
public class SparkBehaviour : MonoBehaviour
{
    [HideInInspector]
    public SparkView sparkView;

    [HideInInspector]
    public Guid netGuid = Guid.Empty;

	#region Player Events

	// Connect
	private void OnPlayerConnect (SparkPeer peer)
	{
		
	}

	// Disconnect
	private void OnPlayerDisconnect (SparkPeer peer)
	{
		
	}

	#endregion

	#region Authentication Events

	// Register
	private void OnRegisterSuccess (RegistrationResponse response)
	{
		
	}

	private void OnRegisterError (RegistrationResponse response)
	{
		
	}

	// Login
	private void OnLoginSuccess (AuthenticationResponse response)
	{
		
	}

	private void OnLoginError (AuthenticationResponse response)
	{
		
	}

	#endregion

	#region Matchmaking Events

	private void OnTryMatchSuccess (MatchmakingResponse response)
	{
		
	}

	private void OnTryMatchError (MatchmakingResponse response)
	{
		
	}

	private void OnMatchFound (MatchFoundMessage message)
	{
		
	}

	private void OnMatchNotFound (MatchNotFoundMessage message)
	{
		
	}

	private void OnMatchUpdated (MatchUpdatedMessage message)
	{
		
	}

	private void OnMatchStart ()
	{
		
	}

	private void OnMatchEnd ()
	{
		
	}

	#endregion

	#region Data Events

	private void OnInstantiate ()
	{
		
	}

	// Serialize
	private void OnSerializeSparkView (SparkStream stream, SparkMessageInfo info)
	{
		
	}

    #endregion

    #region RPC

    /// <summary>
	/// RPC call.
	/// </summary>
	/// <param name="methodName">Method name.</param>
	/// <param name="targetPlayer">Target player.</param>
	/// <param name="isBuffered">If set to <c>true</c> is buffered.</param>
	/// <param name="parameters">Parameters.</param>
	public void RPC(string methodName, SparkTargets targets, bool isBuffered, params object[] parameters)
    {
        RPC(methodName, SparkManager.instance.GetReceivers(targets), isBuffered, parameters);
    }

    public void RPC(string methodName, int[] receiverIds, bool isBuffered, params object[] parameters)
    {
        if (sparkView)
        {
            sparkView.Call("SendEvent_RPC", netGuid, SparkManager.OpCode.SparkView_RPC, methodName, receiverIds, isBuffered, false, parameters);
        }
    }

    #endregion
}
