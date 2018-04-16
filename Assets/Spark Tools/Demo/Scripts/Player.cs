﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SparkBehaviour
{
	Rigidbody2D body2D;
	SparkView sparkView;

	public float speed;

	private void Awake ()
	{
		body2D = GetComponent<Rigidbody2D> ();
		sparkView = GetComponent<SparkView> ();
	}

	private void FixedUpdate ()
	{
		if (sparkView.IsLocalPlayer) {
			float horizontal = Input.GetAxis ("Horizontal");
			float vertical = Input.GetAxis ("Vertical");

			body2D.velocity = new Vector2 (horizontal, vertical) * speed;
		}
	}
		
	private void OnInstantiate ()
	{
		Debug.Log ("On instantiate.");
	}
}