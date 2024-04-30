using Sandbox.VR;

public sealed class Hand : Component, Component.ITriggerListener
{
	[RequireComponent] VRHand VRHand { get; set; }

	GrabPoint CurrentGrabPoint { get; set; }

	const float flDeadzone = 0.25f;
	
	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	public bool IsGripDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return Input.Down( "Attack2" );

		var src = GetController();

		return src.Grip.Value > flDeadzone;
	}

	public bool IsTriggerDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return Input.Down( "Attack1" );

		var src = GetController();

		return src.Trigger.Value > flDeadzone;
	}

	VRController GetController()
	{
		return VRHand.HandSource == VRHand.HandSources.Left ? Input.VR.LeftHand : Input.VR.RightHand;
	}

	void Grab( GrabPoint grabPoint )
	{
		if ( CurrentGrabPoint == grabPoint ) return;

		if ( grabPoint.Interactable.Interact( grabPoint, this ) )
		{
			CurrentGrabPoint = grabPoint;
		}
	}

	void Release()
	{
		if ( CurrentGrabPoint?.Interactable?.StopInteract( CurrentGrabPoint ) ?? false )
		{
			CurrentGrabPoint = null;
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( IsGripDown() )
		{
			var grabPoint = HoveredGrabPoint;
			if ( !grabPoint.IsValid() )
				return;

			Grab( grabPoint );
		}
		else
		{
			Release();
		}
	}

	internal bool IsHolding()
	{
		return CurrentGrabPoint.IsValid();
	}

	GrabPoint HoveredGrabPoint { get; set; }

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.Components.Get<GrabPoint>( FindMode.EnabledInSelf ) is { } grabPoint )
		{
			HoveredGrabPoint = grabPoint;

			GetController().TriggerHapticVibration( 0.1f, 0, 0.2f );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		if ( other.Components.Get<GrabPoint>() is { } grabPoint )
		{
			HoveredGrabPoint = null;
		}
	}
}
