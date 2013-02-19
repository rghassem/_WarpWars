using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class Highlighter : WarpWarsBehavior {
	
	const float DISTANCE_FROM_SUBJECT = 10;
	
	public static Highlighter instance;
	
	GameObject subject;
	
	// Use this for initialization
	protected override void Awake () {
		if(instance != null)
			Destroy(instance.gameObject);
		instance = this;
		light.range = DISTANCE_FROM_SUBJECT*2;
	}
	
	void Update()
	{
		if(subject != null)
		{
			Vector3 cameraPos = Camera.mainCamera.transform.position;
			Vector3 subjectPos = subject.transform.position;
			
			Vector3 cameraToSubject = (subjectPos - cameraPos).normalized;
			transform.position = subjectPos + cameraToSubject * DISTANCE_FROM_SUBJECT;
			transform.LookAt(cameraPos);
		}
	}
	
	public void SetHighlight(GameObject target)
	{
		subject = target;
	}
}
