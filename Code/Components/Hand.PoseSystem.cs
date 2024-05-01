using Sandbox.VR;

public partial class Hand
{
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
	[Property] public HandSources HandSource { get; set; } = HandSources.Left;

	[Property] public SkinnedModelRenderer SkinnedModelComponent { get; set; }

	/// <summary>
	/// A preset pose. This is fucking shit, but I don't think it matters for this game.
	/// </summary>
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

		// Get our controller inputs
		var source = GetController();

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

			if ( (pose == PresetPose.GripNoIndex) && v == FingerValue.IndexCurl )
			{
				SkinnedModelComponent.Set( AnimGraphNames[(int)v], source.GetFingerValue( v ) );
			}

			if ( pose == PresetPose.HoldItem )
			{
				SkinnedModelComponent.Set( AnimGraphNames[(int)v], 0.1f + (x * 0.1f) );
			}

			x++;
		}

		// -_-
		if ( pose == PresetPose.Clamp )
		{
			SkinnedModelComponent.Set( "FingerCurl_Thumb", 0.5f );
			SkinnedModelComponent.Set( "FingerCurl_Index", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Middle", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Ring", 0.4f );
			SkinnedModelComponent.Set( "FingerCurl_Pinky", 0.8f );
		}
	}

	/// <summary>
	/// Designed to run every Update, will update the pose of the hand.
	/// </summary>
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
