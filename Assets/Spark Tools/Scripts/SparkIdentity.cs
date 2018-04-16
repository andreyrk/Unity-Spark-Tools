using UnityEngine;
using System.Collections;

public class SparkIdentityAttribute : PropertyAttribute { }

[AddComponentMenu("Spark Tools/Spark Identity")]
public class SparkIdentity : MonoBehaviour
{
	public static uint count = 1;

	[SparkIdentityAttribute]
	public string uniqueId;
}
