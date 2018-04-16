using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Converters;

public static class SparkExtensions
{
	public static object Call (this object o, string methodName, params object[] parameters)
	{
		MethodInfo method = o.GetType ().GetMethod (methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

		if (method != null) {
			return method.Invoke (o, parameters);
		}

		return null;
	}

	public static JsonSerializerSettings jsonSettings = new JsonSerializerSettings {
		TypeNameHandling = TypeNameHandling.All,    
		ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Converters = new List<JsonConverter> () { new SparkJsonConverter (), new StringEnumConverter() }	
	};

	public static string Serialize (object obj) {
		return JsonConvert.SerializeObject (obj, jsonSettings);
	}

	public static T Deserialize<T> (string json) {
		return JsonConvert.DeserializeObject<T> (json, jsonSettings);
	}
}