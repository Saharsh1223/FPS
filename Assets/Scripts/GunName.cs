using TMPro;
using UnityEngine;

public class GunName : MonoBehaviour
{
	[SerializeField] private WeaponSwitcher weaponSwitcher;
	[SerializeField] private TMP_Text gunText;
	
	private void Update()
	{
		if (weaponSwitcher.weapons.Length <= 0)
		{
			gunText.text = "";
			return;
		}
		
		gunText.text = weaponSwitcher.weapons[weaponSwitcher.selectedWeapon].name;
	}
}
