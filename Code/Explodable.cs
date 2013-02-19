using UnityEngine;
using System.Collections;

public class Explodable : WarpWarsBehavior {

	public ParticleSystem explosionEffect;
	
	public void Explode()
	{
		ParticleSystem explosion = Instantiate(explosionEffect) as ParticleSystem;
		explosion.transform.position = transform.position;
		explosion.loop = false;
		explosion.Play();
		Destroy(explosion.gameObject, explosion.duration);
	}
}
