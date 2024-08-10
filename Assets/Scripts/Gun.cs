using System.Collections;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
	[Header("References")]
	public GunData gunData;
	[SerializeField] private Transform cam;
	[SerializeField] private Recoil recoil;
	[SerializeField] private ParticleSystem muzzleFlash;
	[SerializeField] private AudioSource shootAudio;
	[SerializeField] private AudioSource reloadAudio;
	[SerializeField] private TMP_Text gunText;
	
	[Header("Animation")]
	[SerializeField] private Animator anim;
	
	private float timeSinceLastShot;
	
	private void Start()
	{
		GunInput.shootInput += Shoot;
		GunInput.reloadInput += StartReload;
		
		gunData.currentAmmo = gunData.magSize;
		gunData.totalAmmo = gunData.magSize;
	}
	
	private bool CanShoot() => !gunData.isReloading && timeSinceLastShot > 1f / (gunData.fireRate / 60);
	
	private void OnDisable() => gunData.isReloading = false;
	
	private void StartReload()
	{
		if (!gunData.isReloading && this.gameObject.activeSelf)
		{
			StartCoroutine(Reload());
		}
	}
	
	private IEnumerator Reload()
	{
		// Calculate how much ammo is needed to fill the magazine
		int missingAmmo = gunData.magSize - gunData.currentAmmo;

		// If there's no missing ammo, we can't reload
		if (missingAmmo <= 0)
		{
			Debug.Log("Can't reload right now.");
			yield break;
		}
		
		gunData.isReloading = true;
		anim.CrossFadeInFixedTime(gunData.name + "_Reload", 0f);
		reloadAudio.Play();

		Debug.Log("Reloading");

		// Wait for the reload time to finish
		yield return new WaitForSeconds(gunData.reloadTime);
		
		// Assuming you have a total ammo pool from which to draw, we refill the magazine
		if (gunData.totalAmmo >= missingAmmo)
		{
			// Full reload possible
			gunData.currentAmmo += missingAmmo;
			gunData.totalAmmo -= missingAmmo;
		}
		else
		{
			// Partial reload if not enough total ammo
			gunData.currentAmmo += gunData.totalAmmo;
			gunData.totalAmmo = 0;
		}
		
		gunData.isReloading = false;
	}
	
	private void Shoot()
	{
		if (gunData.currentAmmo > 0)
		{
			if (CanShoot())
			{
				if (Physics.Raycast(cam.position, transform.forward, out RaycastHit hitInfo, gunData.maxDistance))
				{
					IDamagable damagable = hitInfo.transform.GetComponent<IDamagable>();
					damagable?.Damage(gunData.damage);
				}
				
				gunData.currentAmmo--;
				timeSinceLastShot = 0;
				OnGunShot(hitInfo);
			}
		}
	}
	
	private void Update()
	{
		timeSinceLastShot += Time.deltaTime;
		
		recoil.recoilX = gunData.recoilX;
		recoil.recoilY = gunData.recoilY;
		recoil.recoilZ = gunData.recoilZ;
		recoil.snappiness = gunData.snappiness;
		recoil.returnSpeed = gunData.returnSpeed;
		
		gunText.text = gunData.currentAmmo + "/" + gunData.magSize + " (" + gunData.totalAmmo + ")";
	}

	private void OnGunShot(RaycastHit hitInfo)
	{
		Debug.Log("Shot!");
		anim.CrossFadeInFixedTime(gunData.name + "_Shoot", 0f);
		recoil.RecoilFire();
		muzzleFlash.Play();
		shootAudio.PlayOneShot(shootAudio.clip);
		
		if (hitInfo.rigidbody != null)
			hitInfo.rigidbody.AddForce((hitInfo.collider.gameObject.transform.position - hitInfo.point).normalized * gunData.shootForce, ForceMode.VelocityChange);
	}
	
	// private IEnumerator LerpCoroutine(Vector3 pos1, Vector3 pos2, Transform objectToMove, float duration)
	// {
	// 	for (float t = 0f; t < duration; t += Time.deltaTime)
	// 	{
	// 		objectToMove.position = Vector3.Lerp(pos1, pos2, t / duration);
	// 		yield return 0;
	// 	}
	// 	objectToMove.position = pos2;
	// }
	
	// private IEnumerator FloatLerp(float a, float b, float floatToChange, float duration)
	// {
	// 	for (float t = 0f; t < duration; t += Time.deltaTime)
	// 	{
	// 		floatToChange = Mathf.Lerp(a, b, t / duration);
	// 		yield return 0;
	// 	}
	// 	floatToChange = b;
	// }
}
