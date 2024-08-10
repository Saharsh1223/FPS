using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponSwitcher : MonoBehaviour
{
	[Header("References")]
	public Transform[] weapons;
	[SerializeField] private List<GameObject> weaponPrefabs;
	[SerializeField] private Transform[] weaponParents;
	[SerializeField] private KeyCode dropKey = KeyCode.Q;
	[SerializeField] private KeyCode pickupKey = KeyCode.E;
	[SerializeField] private TMP_Text pickupText;
	[Space]
	[SerializeField] private Camera cam;
	[Space]
	[SerializeField] private LayerMask weaponLayer;
	[SerializeField] private Transform orientation;

	[Header("Settings")]
	[SerializeField] private float switchTime;
	[SerializeField] private float dropForce;
	[SerializeField] private float pickupRange;

	public int selectedWeapon;
	private float timeSinceLastSwitch;
	private bool[] weaponFired;

	private void Start()
	{
		InitializeWeapons();
		// Check if there are any weapons in the list before selecting one
		if (weapons.Length > 0)
		{
			Select(selectedWeapon);
		}
		else
		{
			Debug.Log("No weapons found. Player starts with no weapons.");
		}
		timeSinceLastSwitch = 0f;
	}

	private void InitializeWeapons()
	{
		weaponFired = new bool[weapons.Length];
		for (int i = 0; i < weapons.Length; i++)
		{
			weaponFired[i] = false; // Initialize all weapons as not fired
		}
	}

	private void Update()
	{
		int previousSelectedWeapon = selectedWeapon;
		
		// Setup PickupText
		RaycastHit hit;
		Vector3 raycastOrigin = cam.transform.position;
		Vector3 raycastDirection = cam.transform.forward;

		if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, pickupRange, weaponLayer))
		{
			if (hit.collider.CompareTag("Untagged"))
			{
				pickupText.text = "PICKUP (E)";
			}
		}
		else
		{
			pickupText.text = "";
		}

		// Drop weapon
		if (Input.GetKeyDown(dropKey))
		{
			DropWeapon(selectedWeapon);
			return; // Do not proceed further in this frame
		}

		// Pickup weapon
		if (Input.GetKeyDown(pickupKey))
		{
			PickupWeapon();
			return; // Do not proceed further in this frame
		}

		for (int i = 0; i < weapons.Length; i++)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1 + i) && timeSinceLastSwitch >= switchTime)
			{
				selectedWeapon = i;
				Select(selectedWeapon);
				if (previousSelectedWeapon > -1)
					ResetWeaponFired(previousSelectedWeapon); // Reset the firing state of the previously selected weapon
			}
		}

		timeSinceLastSwitch += Time.deltaTime;
	}

	private void Select(int weaponIndex)
	{
		for (int i = 0; i < weapons.Length; i++)
		{
			bool isSelected = (i == weaponIndex);
			weapons[i].gameObject.SetActive(isSelected);
		}

		timeSinceLastSwitch = 0f;

		OnWeaponSelected();
	}

	private void ResetWeaponFired(int weaponIndex)
	{
		weaponFired[weaponIndex] = false; // Reset the firing state of the specified weapon
	}

	private void DropWeapon(int weaponIndex)
	{
		Debug.Log("Dropping weapon...");
		
		string n = weapons[weaponIndex].parent.name;
		string nm = weapons[weaponIndex].name;
		int wIndex = 0;
		
		switch (n)
		{
			case "Weapon1":
				wIndex = 0;
				break;
			case "Weapon2":
				wIndex = 1;
				break;
			case "Weapon3":
				wIndex = 2;
				break;
			case "Weapon4":
				wIndex = 3;
				break;
			case "Weapon5":
				wIndex = 4;
				break;
		}

		Debug.Log("Weapon index: " + wIndex);

		// Instantiate the dropped weapon prefab
		GameObject droppedWeapon = Instantiate(weaponPrefabs[wIndex], weapons[weaponIndex].parent.transform.position, Quaternion.identity);
		
		String name = droppedWeapon.name;
		
		droppedWeapon.name = name.Replace("(Clone)", "");
		droppedWeapon.tag = "DroppedWeapon";
		
		//Set gun data
		if (weapons[weaponIndex].name != "Grappling Gun")
		{
			DroppedGunData droppedGunData = droppedWeapon.GetComponent<DroppedGunData>();
			Gun weaponGun = weapons[weaponIndex].GetComponent<Gun>();
			
			droppedGunData.damage = weaponGun.gunData.damage;
			droppedGunData.maxDistance = weaponGun.gunData.maxDistance;
			
			droppedGunData.autoShoot = weaponGun.gunData.autoShoot;
			
			droppedGunData.recoilX = weaponGun.gunData.recoilX;
			droppedGunData.recoilY = weaponGun.gunData.recoilY;
			droppedGunData.recoilZ = weaponGun.gunData.recoilZ;
			
			droppedGunData.snappiness = weaponGun.gunData.snappiness;
			droppedGunData.returnSpeed = weaponGun.gunData.returnSpeed;
			
			droppedGunData.shootForce = weaponGun.gunData.shootForce;
			
			droppedGunData.currentAmmo = weaponGun.gunData.currentAmmo;
			droppedGunData.totalAmmo = weaponGun.gunData.totalAmmo;
			droppedGunData.magSize = weaponGun.gunData.magSize;
			
			droppedGunData.fireRate = weaponGun.gunData.fireRate;
			droppedGunData.reloadTime = weaponGun.gunData.reloadTime;
			
			Debug.Log("Weapon properties successfully set to dropped weapon.");
		}

		// Get the Rigidbody component of the dropped weapon
		Rigidbody droppedWeaponRigidbody = droppedWeapon.GetComponent<Rigidbody>();
		if (droppedWeaponRigidbody != null)
		{
			// Apply a force in the forward direction
			droppedWeaponRigidbody.AddForce(orientation.forward * dropForce, ForceMode.Impulse);
		}

		// Disable the original weapon
		weapons[weaponIndex].gameObject.SetActive(false);

		// Remove the weapon from the weapons array
		List<Transform> weaponList = new List<Transform>(weapons);
		weaponList.RemoveAt(weaponIndex);
		weapons = weaponList.ToArray();

		// Reorder the keys
		ReorderKeys();

		// Update the selected weapon index if necessary
		if (selectedWeapon >= weapons.Length)
			selectedWeapon = weapons.Length - 1;
		Select(selectedWeapon);
		
		Debug.Log("Successfully dropped " + nm + "!");
	}


	private void PickupWeapon()
	{
		RaycastHit hit;
		Vector3 raycastOrigin = cam.transform.position;
		Vector3 raycastDirection = cam.transform.forward;

		Debug.DrawRay(raycastOrigin, raycastDirection * 10f, Color.blue); // Draw a debug raycast

		if (Physics.Raycast(raycastOrigin, raycastDirection, out hit, pickupRange, weaponLayer))
		{
			if (hit.collider.CompareTag("Untagged"))
			{
				Debug.Log("Picking up weapon...");
				// Re-enable the dropped weapon
				hit.collider.gameObject.SetActive(true);

				// Add the dropped weapon to the weapons array
				string weaponName = hit.collider.transform.parent.parent.name;
				
				Debug.Log(weaponName);
				
				Transform weapon = gameObject.transform;
				
				foreach (Transform weaponParent in weaponParents)
				{
					if (weaponParent.Find(weaponName) != null)
					{
						weapon = weaponParent.Find(weaponName);
						Debug.Log("Weapon Found!");
						break;
					}
				}

				Array.Resize(ref weapons, weapons.Length + 1);
				weapons[weapons.Length - 1] = weapon;
				
				// Set the gun data
				if (weapon.name != "Grappling Gun")
				{
					Gun weaponGun = weapon.GetComponent<Gun>();
					DroppedGunData droppedGunData = hit.collider.transform.parent.parent.GetComponent<DroppedGunData>();
					
					weaponGun.gunData.damage = droppedGunData.damage;
					weaponGun.gunData.maxDistance = droppedGunData.maxDistance;
					
					weaponGun.gunData.autoShoot = droppedGunData.autoShoot;
					
					weaponGun.gunData.recoilX = droppedGunData.recoilX;
					weaponGun.gunData.recoilY = droppedGunData.recoilY;
					weaponGun.gunData.recoilZ = droppedGunData.recoilZ;
					
					weaponGun.gunData.snappiness = droppedGunData.snappiness;
					weaponGun.gunData.returnSpeed = droppedGunData.returnSpeed;
					
					weaponGun.gunData.shootForce = droppedGunData.shootForce;
					
					weaponGun.gunData.currentAmmo = droppedGunData.currentAmmo;
					weaponGun.gunData.totalAmmo = droppedGunData.totalAmmo;
					weaponGun.gunData.magSize = droppedGunData.magSize;
					
					weaponGun.gunData.fireRate = droppedGunData.fireRate;
					weaponGun.gunData.reloadTime = droppedGunData.reloadTime;
					
					Debug.Log("Weapon properties successfully inherited from dropped weapon.");
				}

				// Reorder the keys
				ReorderKeys();

				// Select the newly picked up weapon
				selectedWeapon = weapons.Length - 1;
				Select(selectedWeapon);
				
				Debug.Log("Successfully picked up " + weapon.name + "!");
				
				// Delete the prefab of the dropped weapon
				Destroy(hit.collider.transform.parent.parent.gameObject);
			}
		}
		else
		{
			Debug.Log("Raycast did not hit anything.");
		}
	}

	private void ReorderKeys()
	{
		for (int i = 0; i < weapons.Length; i++)
		{
			KeyCode key = KeyCode.Alpha1 + i;
			if (i != selectedWeapon)
			{
				if (Input.GetKeyDown(key))
				{
					if (i < selectedWeapon)
						selectedWeapon--; // Adjust selected weapon index if necessary
				}
			}
		}
	}

	private void OnWeaponSelected() {}
}
