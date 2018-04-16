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

[JsonObject(MemberSerialization.OptOut)]
public sealed class SparkPeer
{
	public string displayName;
	public string networkId;
	public int id;

	public SparkPeer (string displayName, string networkId, int id)
	{
		this.displayName = displayName;
		this.networkId = networkId;
		this.id = id;
	}
}