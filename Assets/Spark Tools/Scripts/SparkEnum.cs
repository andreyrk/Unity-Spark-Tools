using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[JsonObject(MemberSerialization.OptOut)]
public class SparkEnum
{
    public System.Type enumType;
    public string enumString;

    public SparkEnum(System.Type enumType, string enumString)
    {
        this.enumType = enumType;
        this.enumString = enumString;
    }
}