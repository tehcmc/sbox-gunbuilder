using Sandbox;

public partial class AttachmentPoint : Component, Component.ExecuteInEditor, Component.ITriggerListener, Component.ICollisionListener
{
	[Property] public Model GuideModel { get; set; }
	[Property] public TagSet AcceptedTags { get; set; } = new TagSet();

	[Property] public Collider Trigger { get; set; }

	public Attachable CurrentAttachable { get; private set; }

	protected override void DrawGizmos()
	{
		if ( GuideModel is not null )
		{
			Gizmo.Draw.Color = Color.Orange.WithAlpha( 0.4f );
			Gizmo.Draw.Model( GuideModel );
		}
	}

	protected bool IsValidToAttach( Attachable attachable )
	{
		if ( CurrentAttachable.IsValid() ) return false;

		var attachableFwd = attachable.Transform.Rotation.Forward;
		var thisFwd = Transform.Rotation.Forward;
		var dot = attachableFwd.Dot( thisFwd );

		// Facing the same way
		if ( dot > 0f ) return true;

		return false;
	}

	protected void Attach( Attachable attachable )
	{
		attachable.Interactable.ClearAll();

		attachable.OnAttach( this );
		attachable.Rigidbody.MotionEnabled = false;
		attachable.Rigidbody.Enabled = false;

		CurrentAttachable = attachable;
	}

	protected void Detach()
	{
		// TODO: do it
	}

	protected override void OnUpdate()
	{
		if ( CurrentAttachable.IsValid() )
		{
			CurrentAttachable.Transform.Position = this.GameObject.Transform.Position;
			CurrentAttachable.Transform.Rotation = this.GameObject.Transform.Rotation;
		}
	}

	public void TryAttach( Attachable attachable )
	{
		Log.Info( $"Trigger entered attachmnent point {attachable}" );
		{
			if ( !IsValidToAttach( attachable ) )
			{
				return;
			}

			{
				Attach( attachable );
			}
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Root.Components.Get<Attachable>( FindMode.EnabledInSelfAndDescendants ) is { } attachable )
		{
			TryAttach( attachable );
		}
	}

	void ICollisionListener.OnCollisionStart( Collision other )
	{
		Log.Info( other.Other.GameObject );
		if ( other.Other.GameObject.Root.Components.Get<Attachable>( FindMode.EnabledInSelfAndDescendants ) is { } attachable )
		{
			TryAttach( attachable );
		}
	}

	void ITriggerListener.OnTriggerExit( Collider other )
	{
	}
}
