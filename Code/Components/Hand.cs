using Sandbox.VR;
using System.Diagnostics;
using System.Numerics;

public partial class Hand : Component, Component.ITriggerListener
{
	[Property] GameObject ModelGameObject { get; set; }
	[Property] GameObject DummyGameObject { get; set; }

	/// <summary>
	/// Which objects are we hovering our hand over right now?
	/// This doesn't mean HOLDING, it means hovered.
	/// </summary>
	HashSet<IGrabbable> HoveredGrabPoints = new();

	/// <summary>
	/// The current grab point that this hand is holding. This means the grip is down, and we're actively holding an interactable.
	/// </summary>
	IGrabbable CurrentGrabPoint { get; set; }

	/// <summary>
	/// The input deadzone, so holding ( flDeadzone * 100 ) percent of the grip down means we've got the grip / trigger down.
	/// </summary>
	const float flDeadzone = 0.25f;

	/// <summary>
	/// What the velocity?
	/// </summary>
	public Vector3 Velocity { get; set; }

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

	public bool IsDown( GrabInputType inputType )
	{
		return inputType switch
		{
			GrabInputType.Hover => true,
			GrabInputType.Grip => IsGripDown(),
			GrabInputType.Trigger => IsTriggerDown(),
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
			HoveredGrabPoints.Remove( CurrentGrabPoint );
			CurrentGrabPoint = null;
		}
	}

	private void UpdateTrackedLocation()
	{
		var controller = GetController();
		if ( controller is null ) return;

		var tx = controller.Transform;
		// Bit of a hack, but the alyx controllers have a weird origin that I don't care for.
		tx = tx.Add( Vector3.Forward * -2f, false );

		tx = tx.WithRotation( tx.Rotation * Rotation.From( 20, -5, 0 ) );

		var prevPosition = Transform.World.Position;

		Transform.World = tx;

		var newPosition = Transform.World.Position;

		Velocity = (newPosition - prevPosition);
	}

	protected IGrabbable GetPrioritizedGrabPoint()
	{
		if ( CurrentGrabPoint.IsValid() ) return CurrentGrabPoint;

		var points = HoveredGrabPoints.OrderBy( x => x.GameObject.Transform.Position.Distance( Transform.Position ) );
		return points.FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		UpdateTrackedLocation();
		UpdatePose();

		if ( IsProxy ) return;

		if ( CurrentGrabPoint.IsValid() )
		{
			// Auto-detach for hover input type
			if ( CurrentGrabPoint is GrabPoint grabPoint && grabPoint.GrabInput == GrabInputType.Hover )
			{
				// Detach!
				if ( grabPoint.Transform.Position.Distance( Transform.Position ) > 3f )
				{
					StopGrabbing();
					return;
				}
			}
		}

		var grabbable = GetPrioritizedGrabPoint();
		if ( grabbable.IsValid() && grabbable is GrabPoint point && IsDown( point.GrabInput ) )
		{
			StartGrabbing( point );
		}
		else
		{
			StopGrabbing();
		}

		if ( WantsToPoint )
		{
			UpdateRemotePickup();
		}
	}

	public bool WantsToPoint => IsTriggerDown() && !IsHolding();

	[Property] public SkinnedModelRenderer Model { get; set; }

	void UpdateRemotePickup()
	{
		var att = Model.GetAttachment( "ui_pointer" ) ?? default;

		var tr = Scene.Trace.Ray( att.Position, att.Position + att.Forward * 100000f )
			.IgnoreGameObject( GameObject )
			.Run();

		Gizmo.Draw.Color = Color.Red;

		if ( tr.Hit )
		{
			if ( tr.GameObject.Root.Components.Get<BaseInteractable>( FindMode.EnabledInSelfAndDescendants ) is { } interactable )
			{
				Gizmo.Draw.Color = Color.Green;

				var grabPoint = interactable.AllGrabPoints.FirstOrDefault();

				if ( IsDown( grabPoint.GrabInput ) && interactable.Interact( grabPoint, this ) )
				{
					CurrentGrabPoint = grabPoint;
				}
			}
		}

		Gizmo.Draw.Line( tr.StartPosition, tr.EndPosition );
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
	internal Vector3 GetHoldPosition( IGrabbable grabPoint )
	{
		var src = ModelGameObject.Transform.Position;
		return src;
	}

	// Not sure what purpose this'll really serve soon.
	internal Rotation GetHoldRotation( IGrabbable grabPoint )
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
			HoveredGrabPoints.Add( grabPoint );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
		// Did we find a grab point that'll become eligible to grab?
		if ( other.Components.Get<GrabPoint>( FindMode.EnabledInSelf ) is { } grabPoint )
		{
			if ( HoveredGrabPoints.Contains( grabPoint ) )
			{
			//	HoveredGrabPoints.Remove( grabPoint );
			}
		}
	}
}
