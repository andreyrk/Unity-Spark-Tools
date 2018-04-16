using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[JsonObject (MemberSerialization.OptOut)]
public class SparkColor
{
	public float r = 1f;
	public float g = 1f;
	public float b = 1f;
	public float a = 1f;

	public SparkColor (Color color) {
		this.r = color.r;
		this.g = color.g;
		this.b = color.b;
		this.a = color.a;
	}
}