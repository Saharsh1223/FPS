using UnityEngine;

public class Target : MonoBehaviour, IDamagable
{
	public float health = 100f;
	
	public void Damage(float damage)
	{
		health -= damage;
		
		if (health <= 0)
			Destroy(gameObject);
	}
}
