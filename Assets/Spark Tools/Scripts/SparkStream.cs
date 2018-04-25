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
public sealed class SparkStream
{
	public bool IsWriting { get; private set; }
	public bool IsReading { get { return !IsWriting; } }
	public bool IsEmpty { get { return networkVariables.Count == 0; } }

	public Guid NetGuid { get; private set; }

	public int Count { get { return networkVariables.Count; } }

	[JsonProperty]
	private Dictionary<int, object> networkVariables = new Dictionary<int, object> ();
    [JsonIgnore]
    private Dictionary<int, object> previousVariables = new Dictionary<int, object> ();

	[JsonProperty]
	private int sendCount;

	[JsonProperty]
	private int receiveCount;

	private GameSparksRT.DeliveryIntent observeMethod;

	public SparkStream(Guid netGuid, GameSparksRT.DeliveryIntent observeMethod, bool isWriting) {
		this.NetGuid = netGuid;
		this.observeMethod = observeMethod;
		this.IsWriting = isWriting;
	}

	/// <summary>
	/// Returns the next variable and increments the received variables count.
	/// </summary>
	/// <returns>The next.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T ReceiveNext<T> ()
	{
		return GetVariableAt<T> (receiveCount++);
	}

	/// <summary>
	/// Returns the next variable without incrementing the received variables count.
	/// </summary>
	/// <returns>The next.</returns>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T PeekNext<T> ()
	{
		return GetVariableAt<T> (receiveCount);
	}

	/// <summary>
	/// Gets the variable at index.
	/// </summary>
	/// <returns>The <see cref="``0 (owner=[Method SparkStream.GetVariableAt``1(index:System.Int32):``0])"/>.</returns>
	/// <param name="index">Index.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public T GetVariableAt<T>(int index) {
		if (receiveCount <= Count) {
			object returnValue = networkVariables.ElementAt (index).Value;

            if (typeof(SparkColor).IsAssignableFrom(returnValue.GetType()))
            {
                SparkColor sparkColor = (SparkColor)returnValue;
                return (T)(object)new Color(sparkColor.r, sparkColor.g, sparkColor.b, sparkColor.a);
            }

            if (typeof(SparkEnum).IsAssignableFrom(returnValue.GetType()))
            {
                SparkEnum sparkEnum = (SparkEnum)returnValue;
                return (T)Enum.Parse(sparkEnum.enumType, sparkEnum.enumString);
            }

            return (T)returnValue;
		} else {
			Debug.Log ("Default value returned for this 'GetVariableAt'. " + default(T).ToString ());
			return default(T);
		}
	}

	// Bool

	/// <summary>
	/// Sends a boolean.
	/// </summary>
	/// <param name="boolean">If set to <c>true</c> boolean.</param>
	public void SendNext (bool boolean)
	{
		networkVariables.Add (sendCount++, boolean);
	}

	// Byte

	/// <summary>
	/// Sends a byte.
	/// </summary>
	/// <param name="byteValue">Byte value.</param>
	public void SendNext (byte byteValue) {
		networkVariables.Add (sendCount++, byteValue);
	}

	/// <summary>
	/// Sends a signed byte.
	/// </summary>
	/// <param name="sbyteValue">Sbyte value.</param>
	public void SendNext (sbyte sbyteValue) {
		networkVariables.Add (sendCount++, sbyteValue);
	}

	// Char

	/// <summary>
	/// Sends a character.
	/// </summary>
	/// <param name="charValue">Char value.</param>
	public void SendNext (char charValue) {
		networkVariables.Add (sendCount++, charValue);
	}

	// Decimal

	/// <summary>
	/// Sends a decimal.
	/// </summary>
	/// <param name="decimalValue">Decimal value.</param>
	public void SendNext (decimal decimalValue) {
		networkVariables.Add (sendCount++, decimalValue);
	}

	// Double

	/// <summary>
	/// Sends a double.
	/// </summary>
	/// <param name="doubleValue">Double value.</param>
	public void SendNext (double doubleValue) {
		networkVariables.Add (sendCount++, doubleValue);
	}

	// Float

	/// <summary>
	/// Sends a float.
	/// </summary>
	/// <param name="floatValue">Float value.</param>
	public void SendNext (float floatValue) {
		networkVariables.Add (sendCount++, floatValue);
	}

	// Int

	/// <summary>
	/// Sends an int.
	/// </summary>
	/// <param name="int32Value">Int32 value.</param>
	public void SendNext (int int32Value)
	{
		networkVariables.Add (sendCount++, int32Value);
	}

	/// <summary>
	/// Sends an unsigned int.
	/// </summary>
	/// <param name="uint32Value">Uint32 value.</param>
	public void SendNext (uint uint32Value)
	{
		networkVariables.Add (sendCount++, uint32Value);
	}

	// Long

	/// <summary>
	/// Sends a long.
	/// </summary>
	/// <param name="int64Value">Int64 value.</param>
	public void SendNext (long int64Value)
	{
		networkVariables.Add (sendCount++, int64Value);
	}

	/// <summary>
	/// Sends an unsigned long.
	/// </summary>
	/// <param name="uint64Value">Uint64 value.</param>
	public void SendNext (ulong uint64Value)
	{
		networkVariables.Add (sendCount++, uint64Value);
	}

	// Short

	/// <summary>
	/// Sends a short.
	/// </summary>
	/// <param name="int16Value">Int16 value.</param>
	public void SendNext (short int16Value)
	{
		networkVariables.Add (sendCount++, int16Value);
	}

	/// <summary>
	/// Sends a unsigned short.
	/// </summary>
	/// <param name="uint16Value">Uint16 value.</param>
	public void SendNext (ushort uint16Value)
	{
		networkVariables.Add (sendCount++, uint16Value);
	}

	// String

	/// <summary>
	/// Sends a string.
	/// </summary>
	/// <param name="stringValue">String value.</param>
	public void SendNext (string stringValue)
	{
		networkVariables.Add (sendCount++, stringValue);
	}

	// Vector

	/// <summary>
	/// Sends a Vector2.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public void SendNext (Vector2 vector) {
		networkVariables.Add (sendCount++, vector);
	}

	/// <summary>
	/// Sends a Vector3.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public void SendNext (Vector3 vector) {
		networkVariables.Add (sendCount++, vector);
	}

	/// <summary>
	/// Sends a Vector4.
	/// </summary>
	/// <param name="vector">Vector.</param>
	public void SendNext (Vector4 vector) {
		networkVariables.Add (sendCount++, vector);
	}

	// Color

	/// <summary>
	/// Sends a Color.
	/// </summary>
	/// <param name="color">Color.</param>
	public void SendNext (Color color) {
		SparkColor sparkColor = new SparkColor (color);
		networkVariables.Add (sendCount++, sparkColor);
	}

	// Enum

	/// <summary>
	/// Sends an Enum.
	/// </summary>
	/// <param name="enumeration">Enumeration.</param>
	public void SendNext (Enum enumeration) {
        SparkEnum sparkEnum = new SparkEnum(enumeration.GetType(), enumeration.ToString());
		networkVariables.Add (sendCount++, sparkEnum);
	}

	// Object

	/// <summary>
	/// Sends an object.
	/// </summary>
	/// <param name="obj">Object.</param>
	public void SendNext (object obj) {
		networkVariables.Add (sendCount++, obj);
	}

    /// <summary>
    /// Finishes caching data and then sends it over through the network. Used internally.
    /// </summary>
    /// <param name="info">Info.</param>
    private void Finish(SparkMessageInfo info)
    {
        if (IsEmpty)
        {
            return;
        }

        bool equal = networkVariables.OrderBy(pair => pair.Key).SequenceEqual(previousVariables.OrderBy(pair => pair.Key));

        if (equal)
        {
            return;
        }

        IsWriting = false;

        using (RTData data = RTData.Get())
        {
            data.SetString(1, SparkExtensions.Serialize(this));
            data.SetString(2, SparkExtensions.Serialize(info));

            SparkManager.instance.sparkNetwork.SendData(1, observeMethod, data);
        }

        sendCount = 0;
        previousVariables = networkVariables;
        networkVariables.Clear();

        IsWriting = true;
    }
}
