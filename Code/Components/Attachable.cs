using Sandbox;

public sealed class Attachable : Component
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[RequireComponent] public Collider Collider { get; set; }

	[Property] public SoundEvent AttachSound { get; set; }
	[Property] public SoundEvent DetachSound { get; set; }

	internal void OnAttach( AttachmentPoint attachmentPoint )
	{
		if ( AttachSound is not null )
			Sound.Play( AttachSound, Transform.Position );
	}

	internal void OnDetach( AttachmentPoint attachmentPoint )
	{
		if ( DetachSound is not null )
			Sound.Play( DetachSound, Transform.Position );
	}
}
