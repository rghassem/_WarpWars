using UnityEngine;
using System.Collections;

public class TopDownCamera : WarpWarsBehavior {
	
	private const float SCROLL_WHEEL_SENSITIVITY = 100;
	private const float BORDER_TRACKING_TRIGGER_RADIUS = 50;
	private const float DEFAULT_MOVE_PIXELS_PER_SECOND = 300;
	
	public float tilt;
	public float distanceOffGround;
	public float moveSpeed;
	
	Transform cachedTransform;
	
	// Use this for initialization
	void Start () {
		//Get the camera's tilt set
		cachedTransform = transform;
		moveSpeed = DEFAULT_MOVE_PIXELS_PER_SECOND;
		cachedTransform.rotation = Quaternion.Euler(90 - tilt, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		
		HandleInput(); 
			
		Vector3 ground = new Vector3(cachedTransform.position.x, 0, cachedTransform.position.z);
		cachedTransform.position = ground + Vector3.up * distanceOffGround;
	}
	
	public void SetZoom(float deltaDistance)
	{
		if(camera.isOrthoGraphic)
			camera.orthographicSize = Mathf.Max(camera.orthographicSize + deltaDistance, 1) ;
		else
			distanceOffGround = Mathf.Max(distanceOffGround + deltaDistance, 1);
	}
	
	public void Track(Vector3 direction)
	{
		transform.position +=  direction * moveSpeed * Time.deltaTime;
	}
	
	void HandleInput()
	{
		SetZoom( SCROLL_WHEEL_SENSITIVITY * -Input.GetAxis("Mouse ScrollWheel") );
		
		//Create rectangle bounding everything in the space except for a thin border (of size BORDER_TRACKING_TRIGGER_RADIUS)
		Rect noTrackSpaceBounds = new Rect(
			Camera.mainCamera.pixelRect.x + BORDER_TRACKING_TRIGGER_RADIUS,
			Camera.mainCamera.pixelRect.y + BORDER_TRACKING_TRIGGER_RADIUS,
			Camera.mainCamera.pixelRect.width - BORDER_TRACKING_TRIGGER_RADIUS * 2,
			Camera.mainCamera.pixelRect.height - BORDER_TRACKING_TRIGGER_RADIUS * 2
		);
		
		Vector2 mousePosition = Input.mousePosition;
		
		if( Camera.mainCamera.pixelRect.Contains(mousePosition) && !noTrackSpaceBounds.Contains(mousePosition) )
		{
			Vector3 trackVector = (GetMousePositon() - Camera.mainCamera.transform.position).normalized; //use world space mouse position
			Track(trackVector);
		}
	}

}
