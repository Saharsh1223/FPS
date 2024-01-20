using UnityEngine;

public class Recoil : MonoBehaviour
{
	private Vector3 currentRotation;
	private Vector3 targetRotation;
	
	[HideInInspector] public float recoilX;
	[HideInInspector] public float recoilY;
	[HideInInspector] public float recoilZ;
	
	[HideInInspector] public float snappiness;
	[HideInInspector] public float returnSpeed;
	
	private void Update()
	{
		targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
		currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
		transform.localRotation = Quaternion.Euler(currentRotation);
	}
	
	public void RecoilFire()
	{
		Vector3 rotationVector = new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
		targetRotation += rotationVector;
	}
}