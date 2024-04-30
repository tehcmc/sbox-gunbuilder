using Sandbox;

public sealed class Attachable : Component
{
	[Property] public Interactable Interactable { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[RequireComponent] public Collider Collider { get; set; }
}
