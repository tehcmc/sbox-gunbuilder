using Sandbox;

public sealed class WeaponMagazine : Component
{
	[RequireComponent] Interactable Interactable { get; set; }

	[Property] public int BulletCount { get; set; } = 30;
	[Property] public int BulletCapacity { get; set; } = 30;

	[Property] public bool IsInfiniteAmmo { get; set; } = false;

	public bool TakeBullet()
	{
		if ( IsInfiniteAmmo ) return true;

		if ( BulletCount == 0 ) return false;

		BulletCount--;

		return true;
	}
}
