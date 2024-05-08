/// <summary>
/// A grab point. This has to be on an interactable somewhere. This'll be something like a handle on a gun.
/// Or just a point where we can hold something.
/// </summary>
public partial class GrabPoint : Component
{
	[Property] public Action OnGrabStartEvent { get; set; }
	[Property] public Action OnGrabEndEvent { get; set; }

	/// <summary>
	/// The collider in question
	/// </summary>
	[Property] public Collider Collider { get; set; }

	/// <summary>
	/// The linked interactable
	/// </summary>
	[Property] public BaseInteractable Interactable { get; set; }

	/// <summary>
	/// The primary grab point
	/// </summary>
	[Property] public GrabPoint PrimaryGrabPoint { get; set; }

	/// <summary>
	/// The hand pose that we'll use when a hand is attached to this grab point.
	/// </summary>
	[Property, Title( "Hand Preset" )] public HandPreset Preset { get; set; } = HandPreset.GripWithoutIndexFinger;

	[Property] public bool PresetDebugging { get; set; } = false;

	/// <summary>
	/// What's our grabbing input type?
	/// </summary>
	public enum GrabInputType
	{
		Grip,
		Trigger,
		Hover
	}

	/// <summary>
	/// What's our grabbing input type?
	/// </summary>
	[Property] public GrabInputType GrabInput { get; set; } = GrabInputType.Grip;

	/// <summary>
	/// Is this a secondary grab point? Has to be linked to a primary grab point for it to work.
	/// </summary>
	public bool IsSecondaryGrabPoint => PrimaryGrabPoint.IsValid();

	/// <summary>
	/// Is this a primary grab point?
	/// </summary>
	public bool IsPrimaryGrabPoint => !IsSecondaryGrabPoint;

	/// <summary>
	/// What hand is this grab point currently being held by?
	/// Make this a method at some point so we can control access.
	/// </summary>
	public Hand HeldHand { get; set; }

	/// <summary>
	/// Is this grab point being held by a player?
	/// </summary>
	public bool IsBeingHeld => HeldHand.IsValid();

	public void OnStartGrabbing()
	{
		OnGrabStartEvent?.Invoke();
	}

	public void OnStopGrabbing()
	{
		OnGrabEndEvent?.Invoke();
	}

	/// <summary>
	/// Can we stop grabbing?
	/// </summary>
	/// <param name="interactable"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool CanStopGrabbing( BaseInteractable interactable, Hand hand )
	{
		return true;
	}

	/// <summary>
	/// Can we grab right now?
	/// </summary>
	/// <param name="interactable"></param>
	/// <param name="hand"></param>
	/// <returns></returns>
	public bool CanStartGrabbing( BaseInteractable interactable, Hand hand )
	{
		// TODO: Don't allow holding a secondary grab point if the primary grab point isn't being held already.
		// if ( IsSecondaryGrabPoint ) return PrimaryGrabPoint.IsBeingHeld;
		return true;
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Cyan.WithAlpha( 0.2f );
		Gizmo.Draw.Model( "models/hands/alyx_hand_right.vmdl" );
	}

	/// <summary>
	/// Called normally by the hand, will update the pose of a hand to match the data we give it.
	/// </summary>
	/// <param name="hand"></param>
	public virtual void UpdateHandPose( Hand hand )
	{
		hand.ApplyHandPreset( Preset );
	}

	SkinnedModelRenderer handGuide;
	private void RecreateHandGuide()
	{
		handGuide?.GameObject?.Destroy();

		var go = new GameObject();

		var sm = go.Components.Create<SkinnedModelRenderer>();
		sm.Model = Model.Load( "models/hands/alyx_hand_right.vmdl" );
		handGuide = sm;

		go.SetParent( this.GameObject, false );

		sm.Set( "BasePose", 1 );
		sm.Set( "bGrab", true );
		sm.Set( "GrabMode", 1 );

		if ( Preset is not null )
		{
			Preset.Apply( sm );
		}
	}

	protected override void OnUpdate()
	{
		if ( Preset is not null && handGuide.IsValid() )
		{
			Preset.Apply( handGuide );
		}
	}

	protected override void OnStart()
	{
		if ( PresetDebugging )
		{
			RecreateHandGuide();
		}
	}
}
