using Sandbox.VR;

public partial class Hand : Component, Component.ITriggerListener
{
	[Property] GameObject ModelGameObject { get; set; }
	[Property] GameObject DummyGameObject { get; set; }

	/// <summary>
	/// Which object are we hovering our hand over right now?
	/// This doesn't mean HOLDING, it means hovered.
	/// </summary>
	GrabPoint HoveredGrabPoint { get; set; }

	/// <summary>
	/// The current grab point that this hand is holding. This means the grip is down, and we're actively holding an interactable.
	/// </summary>
	GrabPoint CurrentGrabPoint { get; set; }

	/// <summary>
	/// The input deadzone, so holding ( flDeadzone * 100 ) percent of the grip down means we've got the grip / trigger down.
	/// </summary>
	const float flDeadzone = 0.25f;

	/// <summary>
	/// Is the hand grip down?
	/// </summary>
	/// <returns></returns>
	public bool IsGripDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return Input.Down( "Attack2" );

		var src = GetController();
		if ( src is null ) return false;

		return src.Grip.Value > flDeadzone;
	}

	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	public bool IsTriggerDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return Input.Down( "Attack1" );

		var src = GetController();
		if ( src is null ) return false;

		return src.Trigger.Value > flDeadzone;
	}

	public VRController GetController()
	{
		return HandSource == HandSources.Left ? Input.VR?.LeftHand : Input.VR?.RightHand;
	}

	public bool IsDown( GrabPoint.GrabInputType inputType )
	{
		return inputType switch
		{
			GrabPoint.GrabInputType.Grip => IsGripDown(),
			GrabPoint.GrabInputType.Trigger => IsTriggerDown(),
			_ => false
		};
	}

	/// <summary>
	/// Try to grab a grab point.
	/// </summary>
	/// <param name="grabPoint"></param>
	void StartGrabbing( GrabPoint grabPoint )
	{
		// If we're already grabbing this thing, don't bother.
		if ( CurrentGrabPoint == grabPoint ) return;

		// Input type match
		if ( !IsDown( grabPoint.GrabInput ) ) return;

		// Only if we succeed to interact with the interactable, take hold of the object.
		if ( grabPoint.Interactable.Interact( grabPoint, this ) )
		{
			CurrentGrabPoint = grabPoint;
		}
	}

	/// <summary>
	/// Stop grabbing something.
	/// </summary>
	public void StopGrabbing()
	{
		// If we can release the object (which can fail!), clear the current grab point.
		if ( CurrentGrabPoint?.Interactable?.StopInteract( CurrentGrabPoint ) ?? false )
		{
			CurrentGrabPoint = null;
		}
	}

	private void UpdateTrackedLocation()
	{
		var controller = GetController();
		if ( controller is null ) return;

		var tx = controller.Transform;
		tx = tx.Add( Vector3.Forward * -2f, false );

		Transform.World = tx;
	}

	protected override void OnUpdate()
	{
		UpdateTrackedLocation();
		UpdatePose();

		if ( IsProxy ) return;

		if ( IsGripDown() || IsTriggerDown() )
		{
			if ( !HoveredGrabPoint.IsValid() ) return;
			StartGrabbing( HoveredGrabPoint );
		}
		else
		{
			StopGrabbing();
		}
	}

	/// <summary>
	/// Is this hand holding something right now?
	/// </summary>
	/// <returns></returns>
	internal bool IsHolding()
	{
		return CurrentGrabPoint.IsValid();
	}
	
	/// <summary>
	/// Attaches the hand model to a grab point.
	/// </summary>
	/// <param name="gameObject"></param>
	internal void AttachModelToGrabPoint( GameObject gameObject )
	{
		DummyGameObject.SetParent( gameObject, false );
	}

	/// <summary>
	/// Detaches the hand model from the grab point, puts it back on our hand.
	/// </summary>
	internal void DetachModelFromGrabPoint()
	{
		DummyGameObject.SetParent( ModelGameObject, false );
	}

	// Not sure what purpose this'll really serve soon.
	internal Vector3 GetHoldPosition( GrabPoint grabPoint )
	{
		var src = ModelGameObject.Transform.Position;
		return src;
	}

	// Not sure what purpose this'll really serve soon.
	internal Rotation GetHoldRotation( GrabPoint grabPoint )
	{
		return ModelGameObject.Transform.Rotation;
	}

	/// <summary>
	/// Called when we overlap with another trigger in the world.
	/// </summary>
	/// <param name="other"></param>
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		// Did we find a grab point that'll become eligible to grab?
		if ( other.Components.Get<GrabPoint>( FindMode.EnabledInSelf ) is { } grabPoint )
		{
			HoveredGrabPoint = grabPoint;
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		// Did we find a grab point that'll become eligible to grab?
		if ( other.Components.Get<GrabPoint>( FindMode.EnabledInSelf ) is { } grabPoint )
		{
			if ( HoveredGrabPoint == grabPoint )
				HoveredGrabPoint = null;
		}
	}
}
