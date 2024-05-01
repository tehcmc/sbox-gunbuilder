using Sandbox;
using Sandbox.VR;

public sealed class Hand : Component, Component.ITriggerListener
{
	[Property] GameObject ModelGameObject { get; set; }
	[Property] GameObject DummyGameObject { get; set; }

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
		return HandSource == HandSources.Left ? Input.VR.LeftHand : Input.VR.RightHand;
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
		UpdatePose();

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

	internal void AttachModelTo( GameObject gameObject )
	{
		DummyGameObject.SetParent( gameObject, false );
	}

	internal void ResetAttachment()
	{
		DummyGameObject.SetParent( ModelGameObject, false );
	}

	internal Vector3 GetHoldPosition( GrabPoint grabPoint )
	{
		var src = ModelGameObject.Transform.Position;
		return src;
	}

	internal Rotation GetHoldRotation( GrabPoint grabPoint )
	{
		return ModelGameObject.Transform.Rotation;
	}


	// TODO: These should ideally be user-editable, these values only work on the Alyx hands right now
	private static List<string> AnimGraphNames = new()
	{
		"FingerCurl_Thumb",
		"FingerCurl_Index",
		"FingerCurl_Middle",
		"FingerCurl_Ring",
		"FingerCurl_Pinky"
	};

	/// <summary>
	/// Represents a controller to use when fetching skeletal data (finger curl/splay values)
	/// </summary>
	public enum HandSources
	{
		/// <summary>
		/// The left controller
		/// </summary>
		Left,

		/// <summary>
		/// The right controller
		/// </summary>
		Right
	}

	/// <summary>
	/// Which hand should we use to update the parameters?
	/// </summary>
	[Property]
	public HandSources HandSource { get; set; } = HandSources.Left;

	[Property]
	public SkinnedModelRenderer SkinnedModelComponent { get; set; }

	public enum PresetPose
	{
		None,
		Grip,
		GripNoIndex,
		HoldItem,
		Clamp
	}

	public void SetPresetPose( PresetPose pose )
	{
		if ( !Game.IsRunningInVR ) return;

		var source = (HandSource == HandSources.Left) ? Sandbox.Input.VR.LeftHand : Sandbox.Input.VR.RightHand;

		SkinnedModelComponent.Set( "BasePose", 1 );
		SkinnedModelComponent.Set( "bGrab", true );
		SkinnedModelComponent.Set( "GrabMode", 1 );

		var x = 0;

		for ( FingerValue v = FingerValue.ThumbCurl; v <= FingerValue.PinkyCurl; ++v )
		{
			SkinnedModelComponent.Set( AnimGraphNames[(int)v], source.GetFingerValue( v ) );

			if ( pose == PresetPose.Grip || pose == PresetPose.GripNoIndex )
			{
				SkinnedModelComponent.Set( AnimGraphNames[(int)v], 1.0f );
			}

			if ( ( pose == PresetPose.GripNoIndex ) && v == FingerValue.IndexCurl )
			{
				SkinnedModelComponent.Set( AnimGraphNames[(int)v], source.GetFingerValue( v ) );
			}

			if ( pose == PresetPose.HoldItem )
			{
				SkinnedModelComponent.Set( AnimGraphNames[(int)v], 0.1f + ( x * 0.1f ) );
			}

			x++;
		}

		if ( pose == PresetPose.Clamp )
		{
			SkinnedModelComponent.Set( "FingerCurl_Thumb", 0.5f );
			SkinnedModelComponent.Set( "FingerCurl_Index", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Middle", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Ring", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Pinky", 0.8f );

		}
	}

	private void UpdatePose()
	{
		if ( !SkinnedModelComponent.IsValid() )
			return;

		if ( IsHolding() )
		{
			CurrentGrabPoint.UpdateHandPose( this );
		}
		else
		{
			SetPresetPose( PresetPose.None );
		}
	}
}
