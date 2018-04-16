using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSparks.Api.Messages;
using GameSparks.Core;
using GameSparks.Api.Responses;
using GameSparks.RT;
using Newtonsoft.Json;

[JsonObject (MemberSerialization.OptOut)]
public sealed class SparkMatch
{
	public int portID;
	public string matchID;
	public string hostURL;
	public string acccessToken;

	public List<SparkPeer> peerList = new List<SparkPeer> ();

	public SparkMatch (MatchFoundMessage message)
	{
		portID = message.Port.Value;
		hostURL = message.Host;
		acccessToken = message.AccessToken;
		matchID = message.MatchId;
	}

	public SparkPeer FindBy_Id (int peerId)
	{
		return peerList.Find (x => x.id == peerId);
	}
}