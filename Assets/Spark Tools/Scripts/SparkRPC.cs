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
public class SparkRPC
{
    public Guid NetGuid { get; private set; }
    public string MethodName { get; private set; }
	public object[] Parameters { get; private set; }
	public int[] ReceiverIds { get; private set; }
	public SparkPeer Sender { get; private set; }

	public SparkRPC (Guid netGuid, string methodName, int[] receiverIds, SparkPeer sender, object[] parameters)
	{
        this.NetGuid = netGuid;
		this.MethodName = methodName;
		this.ReceiverIds = receiverIds;
		this.Sender = sender;
		this.Parameters = parameters;
	}
}