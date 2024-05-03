using Sandbox;

public sealed class WeaponMagazine : Component
{
	/// <summary>
	/// The interactable that belongs to this weapon magazine. It'll always exist.
	/// </summary>
	[RequireComponent] public Interactable Interactable { get; set; }

	/// <summary>
	/// The attachable that belongs to this weapon magazine. It'll also always exist.
	/// </summary>
	[RequireComponent] public Attachable Attachable { get; set; }

	/// <summary>
	/// The model renderer for this mag.
	/// </summary>
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	private int bulletCount = 30;
	/// <summary>
	/// How many bullets are in this gun?
	/// </summary>
	[Property] public int BulletCount
	{
		get => bulletCount;
		set
		{
			if ( bulletCount == value ) return;
			bulletCount = value;

			Renderer?.SetBodyGroup( AmmoBodygroup, bulletCount > 0 ? 0 : 1 );
		}
	}

	/// <summary>
	/// What's the ammo capacity for this gun?
	/// </summary>
	[Property] public int BulletCapacity { get; set; } = 30;

	/// <summary>
	/// Mainly for development - but negates needing ammo in the magazine to fire a gun.
	/// </summary>
	[Property] public bool IsInfiniteAmmo { get; set; } = false;

	[Property] public int AmmoBodygroup { get; set; } = 0;

	/// <summary>
	/// Does this magazine have any ammo?
	/// </summary>
	public bool HasAmmo
	{
		get => BulletCount > 0;
	}

	/// <summary>
	/// Tries to take a bullet from the magazine. Called when firing a gun.
	/// </summary>
	/// <returns></returns>
	public bool TakeBullet()
	{
		if ( IsInfiniteAmmo ) return true;

		if ( BulletCount == 0 ) return false;

		BulletCount--;

		return true;
	}
}
