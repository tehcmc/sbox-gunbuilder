using Sandbox;

public sealed class Attachable : Component
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[RequireComponent] public Collider Collider { get; set; }

	[Property] public SoundEvent AttachSound { get; set; }

	internal void OnAttach( AttachmentPoint attachmentPoint )
	{
		if ( AttachSound is not null )
			Sound.Play( AttachSound, Transform.Position );
	}
}
