using Sandbox.VR;

public sealed class Hand : Component
{
	[RequireComponent] VRHand VRHand { get; set; }

	GrabPoint CurrentGrabPoint { get; set; }

	const float flDeadzone = 0.25f;
	
	/// <summary>
	/// Is the hand trigger down?
	/// </summary>
	/// <returns></returns>
	bool IsTriggerDown()
	{
		// For debugging purposes
		if ( !Game.IsRunningInVR ) return Input.Down( "Attack1" );

		var src = VRHand.HandSource == VRHand.HandSources.Left ? Input.VR.LeftHand : Input.VR.RightHand;

		return src.Grip.Value > flDeadzone;
	}

	/// <summary>
	/// Look for a grab point
	/// </summary>
	/// <returns></returns>
	GrabPoint FindGrabPoint()
	{
		var objects = Scene.FindInPhysics( BBox.FromPositionAndSize( Transform.Position, 8 ) );

		foreach ( var obj in objects )
		{
			var grabPoints = obj.Root.Components.GetAll<GrabPoint>( FindMode.EnabledInSelfAndDescendants );
			grabPoints = grabPoints.OrderBy( x => x.Transform.Position.Distance( Transform.Position ) );

			var closest = grabPoints.FirstOrDefault();
			return closest;
		}
		return null;
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
		if ( CurrentGrabPoint?.Interactable?.StopInteract( CurrentGrabPoint, this ) ?? false )
		{
			CurrentGrabPoint = null;
		}
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		if ( IsTriggerDown() )
		{
			var grabPoint = FindGrabPoint();
			if ( !grabPoint.IsValid() )
				return;

			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineBBox( BBox.FromPositionAndSize( Transform.Position, 2 ) );

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
}
