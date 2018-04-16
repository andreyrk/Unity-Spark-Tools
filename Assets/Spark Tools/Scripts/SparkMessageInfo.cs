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
public sealed class SparkMessageInfo
{
	public SparkPeer Sender { get; private set; }

	public SparkMessageInfo (SparkPeer sender)
	{
		this.Sender = sender;
	}
}
