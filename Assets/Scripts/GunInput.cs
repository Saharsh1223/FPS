using System;
using UnityEngine;

public class GunInput : MonoBehaviour
{
	public static Action shootInput;
	public static Action reloadInput;
	
	[SerializeField] private Transform WeaponObject;
	
	[SerializeField] private KeyCode reloadKey = KeyCode.R;
	[SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
	
	private void Update()
	{
		Gun activeGun = WeaponObject.gameObject.GetComponentInChildren<Gun>(false);
		
		if (activeGun.gunData.autoShoot)
		{
			if (Input.GetMouseButton(0) && !activeGun.gunData.isReloading)
				shootInput?.Invoke();
		}
		else
		{
			if (Input.GetMouseButtonDown(0) && !activeGun.gunData.isReloading)
				shootInput?.Invoke();
		}
		
		if (Input.GetKeyDown(reloadKey) || activeGun.gunData.currentAmmo <= 0)
			reloadInput?.Invoke();
	}
}
