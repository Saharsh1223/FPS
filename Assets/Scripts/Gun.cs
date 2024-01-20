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
	[SerializeField] private TMP_Text gunText;
	
	// [Header("Aim References - Ignore if not aimable")]
	// [SerializeField] private Transform aimPos;
	// [SerializeField] private Transform startPos;
	// [SerializeField] private Transform posObj;
	
	[Header("Animation")]
	[SerializeField] private Animator anim;
	
	private float timeSinceLastShot;
	
	
	private void Start()
	{
		GunInput.shootInput += Shoot;
		GunInput.reloadInput += StartReload;
	}
	
	private bool CanShoot() => !gunData.isReloading && timeSinceLastShot > 1f / (gunData.fireRate / 60);
	
	private void OnDisable() => gunData.isReloading = false;
	
	private void StartReload()
	{
		if (!gunData.isReloading && this.gameObject.activeSelf)
		{
			StartCoroutine(Reload());
			Debug.Log("Reloading");
		}
	}
	
	private IEnumerator Reload()
	{
		gunData.isReloading = true;
		anim.CrossFadeInFixedTime(gunData.name + "_Reload", 0f);
		
		yield return new WaitForSeconds(gunData.reloadTime);
		
		gunData.currentAmmo = gunData.magSize;
		
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
		
		gunText.text = gunData.currentAmmo + "/" + gunData.magSize;
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
