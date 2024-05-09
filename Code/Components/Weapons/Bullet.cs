/// <summary>
/// The bullet's caliber. Dictates whether or not a magazine or a bullet is compatible with a weapon.
/// </summary>
public enum BulletCaliber
{
	None,
	[Title( "5.56x45mm" )]
	FiveFiveSix,
	[Title( "9mm" )]
	Nine,
	[Title( "12ga" )]
	TwelveGauge
}

/// <summary>
/// An instance of a bullet. This is purely data to store in a list/stack.
/// </summary>
public partial record Bullet
{
	/// <summary>
	/// A bodygroup key value.
	/// </summary>
	public class BodyGroupKeyValue
	{
		[KeyProperty] public int Index { get; set; }
		[KeyProperty] public int Value { get; set; }
	}

	/// <summary>
	/// Has this bullet been spent?
	/// </summary>
	[Property, Group( "Data" )] public bool IsSpent { get; set; }

	/// <summary>
	/// The bullet's caliber.
	/// </summary>
	[Property, Group( "Setup" )] public BulletCaliber Caliber { get; set; } = BulletCaliber.FiveFiveSix;

	/// <summary>
	/// The prefab to spawn if we want to instantiate a world version of this bullet.
	/// </summary>
	[Property, Group( "Setup" )] public Model WorldModel { get; set; }

	/// <summary>
	/// The bodygroup set to use on a spawned bullet when spawning a bullet in the world.
	/// </summary>
	[Property, Group( "Setup" )] public BodyGroupKeyValue SpentCasingBodygroup { get; set; }

	/// <summary>
	/// Creates a bullet component in the world (can be picked up)
	/// </summary>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	/// <returns></returns>
	public BulletComponent CreateInWorld( Vector3 position, Rotation rotation = default )
	{
		var go = new GameObject();
		go.Transform.Position = position;
		go.Transform.Rotation = rotation;

		// Create the bullet
		var comp = go.Components.Create<BulletComponent>();
		comp.Bullet = this;

		// Create the model
		var mdl = go.Components.Create<ModelRenderer>();
		mdl.Model = WorldModel;

		if ( IsSpent )
		{
			mdl.SetBodyGroup( SpentCasingBodygroup.Index, SpentCasingBodygroup.Value );
		}

		// Create the model collider
		var collider = go.Components.Create<ModelCollider>();
		collider.Model = WorldModel;

		// Create the rigidbody (so it can move)
		go.Components.Create<Rigidbody>();

		return comp;
	}
}

public partial class BulletComponent : Component, IAmmoSource
{
	[Property] public Bullet Bullet { get; set; } = new();

	public Stack<Bullet> Bullets
	{
		get
		{
			var stack = new Stack<Bullet>();
			stack.Push( Bullet );
			return stack;
		}
	}

	public IEnumerable<Bullet> Pop( int amount = 1 )
	{
		yield return Bullet;
	}
}
