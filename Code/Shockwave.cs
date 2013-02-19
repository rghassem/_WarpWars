using UnityEngine;
using System.Collections;

public class Shockwave : WarpWarsBehavior {
	
	public static readonly float VALID_HIT_TARGET_MAX_ANGLE = 45; 
	const float SCALING_SPEED = 100;

	
	public ParticleSystem shockParticleEffect; 
	private GameObject activeParticleSystem;
	
	public static Shockwave Spawn(Vector3 position, Vector3 pointInFront, GameObject spawner, float blastRadius)
	{
		GameObject prefab = Resources.Load("Shockwave", typeof(GameObject)) as GameObject;
		GameObject instance = Instantiate(prefab) as GameObject;
		Shockwave shockwave = instance.GetComponent<Shockwave>();
		shockwave.targetScale = ComputeBlastReach( blastRadius * 2 );
		shockwave.creator = spawner;
		shockwave.transform.position = position;
		shockwave.transform.LookAt(pointInFront, Camera.mainCamera.transform.position - position);
		instance.SetActive(true);
		return shockwave;
	}
	
	public static float ComputeBlastReach(float travelDistance)
	{
		return travelDistance;
	}
	
	void Start()
	{
		ParticleSystem shockEffect = Instantiate(shockParticleEffect, transform.position,transform.rotation) as ParticleSystem;
		activeParticleSystem = shockEffect.gameObject;
		
		shockEffect.startSpeed = SCALING_SPEED / 2;
		ParticleSystem waveEffect =  shockEffect.transform.GetChild(0).GetComponent<ParticleSystem>();
		waveEffect.startSpeed = SCALING_SPEED / 2;
		
		shockEffect.Stop();
		shockEffect.Clear();
		shockEffect.Play();
	}
	
	float targetScale;
	GameObject creator;
	
	// Update is called once per frame
	void Update () {
		if(transform.localScale.magnitude < (Vector3.one * targetScale).magnitude)
			transform.localScale += (Vector3.one * SCALING_SPEED * Time.deltaTime);
		else
		{
			creator.SendMessage("OnShockwaveComplete", SendMessageOptions.DontRequireReceiver);
			game.NextTurn();
			Destroy(gameObject); //self destruct when peak is reached
			Destroy(activeParticleSystem);
		}
		//TODO: persist and fade out for a little first
	}
	
	void OnTriggerEnter(Collider collider)
	{
		Vector3 vectorToTarget = transform.position - collider.transform.position;
		float angle = Vector3.Angle(vectorToTarget, -transform.forward);
		if(angle < VALID_HIT_TARGET_MAX_ANGLE) //Maybe add the radius of the ships collider
		{
			collider.SendMessage("OnDamage", creator, SendMessageOptions.DontRequireReceiver); 
		}
	}
	
}
