using Sandbox.VR;

public sealed class Hand : Component
{
	[RequireComponent] VRHand VRHand { get; set; }

	protected bool IsTriggerDown()
	{
		if ( VRHand.HandSource == VRHand.HandSources.Left)
		{
			return Input.VR.LeftHand.Grip.Value > 0.25f;
		}

		return Input.VR.RightHand.Grip.Value > 0.25f;
	}

	GrabPoint LastGrabPoint { get; set; }
	GameObject RootObject { get; set; }

	protected GrabPoint FindGrabPoint()
	{
		var objects = Scene.FindInPhysics( BBox.FromPositionAndSize( Transform.Position, 16 ) );

		foreach ( var obj in objects )
		{
			if ( obj.Root.Components.Get<GrabPoint>( FindMode.EnabledInSelfAndDescendants ) is { } grabPoint )
			{
				return grabPoint;
			}
		}

		return null;
	}

	void Grab( GrabPoint grabPoint )
	{
		if ( LastGrabPoint == grabPoint ) return;

		LastGrabPoint = grabPoint;
		RootObject = LastGrabPoint.GameObject.Root;

		RootObject.SetParent( this.GameObject, true );
	}

	void Release()
	{
		if ( LastGrabPoint.IsValid() )
		{
			RootObject.SetParent( null );
			RootObject = null;
			LastGrabPoint = null;
			Log.Info( "Released" );
		}
	}

	protected override void OnUpdate()
	{
		if ( IsTriggerDown() )
		{
			var grabPoint = FindGrabPoint();
			if ( !grabPoint.IsValid() )
			{
				return;
			}

			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineBBox( BBox.FromPositionAndSize( Transform.Position, 2 ) );

			Grab( grabPoint );
		}
		else
		{
			Release();
		}
	}
}
