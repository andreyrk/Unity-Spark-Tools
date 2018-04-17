using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkTransformSyncer : SparkBehaviour {

    public enum Syncer
    {
        LocalPlayer,
        MasterPlayer
    }

	public enum Mode
	{
		Transform,
		Rigidbody,
		Rigidbody2D
	}

	public enum InterpolateOption {
		None,
		Lerp,
		Slerp
	}

	public enum ExtrapolateOption {
		None,
		Extrapolate
	}

    public Syncer syncer;

    public bool syncPosition;
	public InterpolateOption positionInterpolate;
	public ExtrapolateOption positionExtrapolate;

    public bool syncScale;
    public InterpolateOption scaleInterpolate;
    public ExtrapolateOption scaleExtrapolate;

    public bool syncRotation;
    public InterpolateOption rotationInterpolate;
    public ExtrapolateOption rotationExtrapolate;

	public bool teleport;
	public float teleportDistance;

    [HideInInspector]
    Vector3 previousPosition;
    [HideInInspector]
    Vector3 nextPosition;

    [HideInInspector]
    Vector3 previousScale;
    [HideInInspector]
    Vector3 nextScale;

    [HideInInspector]
    Vector3 previousRotation;
    [HideInInspector]
    Vector3 nextRotation;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;

    private void Awake ()
	{

	}

	private void Start() {
        previousPosition = transform.position;
        nextPosition = transform.position;

        previousScale = transform.localScale;
        nextScale = transform.localScale;

        previousRotation = transform.eulerAngles;
        nextRotation = transform.eulerAngles;
	}

    private void Update() {
        if ((sparkView.IsLocalPlayer && syncer == Syncer.LocalPlayer) || (SparkManager.instance.IsMasterPlayer && syncer == Syncer.MasterPlayer))
        {
            return;
        }

        syncTime += Time.deltaTime;

        // Syncing

        if (syncPosition)
        {
            switch (positionInterpolate)
            {
                case InterpolateOption.None:
                    transform.position = nextPosition;
                    break;
                case InterpolateOption.Lerp:
                    transform.position = Vector3.Lerp(previousPosition, nextPosition, syncTime / syncDelay);
                    break;
                case InterpolateOption.Slerp:
                    transform.position = Vector3.Slerp(previousPosition, nextPosition, syncTime / syncDelay);
                    break;
            }

            if (teleport)
            {
                if (Vector3.Distance(transform.position, nextPosition) >= teleportDistance)
                {
                    transform.position = nextPosition;
                }
            }
        }

        if (syncScale)
        {
            switch (scaleInterpolate)
            {
                case InterpolateOption.None:
                    transform.localScale = nextPosition;
                    break;
                case InterpolateOption.Lerp:
                    transform.localScale = Vector3.Lerp(previousScale, nextScale, syncTime / syncDelay);
                    break;
                case InterpolateOption.Slerp:
                    transform.localScale = Vector3.Slerp(previousScale, nextScale, syncTime / syncDelay);
                    break;
            }
        }

        if (syncRotation)
        {
            switch (rotationInterpolate)
            {
                case InterpolateOption.None:
                    transform.eulerAngles = nextScale;
                    break;
                case InterpolateOption.Lerp:
                    transform.eulerAngles = Vector3.Lerp(previousRotation, nextRotation, syncTime / syncDelay);
                    break;
                case InterpolateOption.Slerp:
                    transform.eulerAngles = Vector3.Slerp(previousRotation, nextRotation, syncTime / syncDelay);
                    break;
            }
        }
    }

	private void OnSerializeSparkView (SparkStream stream, SparkMessageInfo info)
	{
        if (stream.IsWriting)
        {
            if ((sparkView.IsLocalPlayer && syncer == Syncer.LocalPlayer) || (SparkManager.instance.IsMasterPlayer && syncer == Syncer.MasterPlayer))
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.localScale);
                stream.SendNext(transform.eulerAngles);
            }
        }
        else
        {
            previousPosition = transform.position;
            nextPosition = stream.ReceiveNext<Vector3>();

            previousScale = transform.localScale;
            nextScale = stream.ReceiveNext<Vector3>();

            previousRotation = transform.eulerAngles;
            nextRotation = stream.ReceiveNext<Vector3>();

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;
        }
	}
}
