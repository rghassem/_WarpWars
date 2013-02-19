using UnityEngine;
using System.Collections;

public class WarpWarsBehavior : MonoBehaviour 
{	
	protected Game game;
	protected UIManager ui;

	
	protected virtual void Awake()
	{
		GameObject god = GameObject.FindGameObjectWithTag("God");
		game = god.GetComponent<Game>();
		ui = god.GetComponent<UIManager>();
	}
	
	#region Utility functions
	
	/// <summary>
	/// True if the left mouse button is held down, within the game viewport
	/// </summary>
	public bool IsMouseDown()
	{
		return Input.GetMouseButton(0) && Camera.mainCamera.pixelRect.Contains(Input.mousePosition);
	}
	
	/// <summary>
	/// True if the left mouse button was clicked is frame, within the game viewport
	/// </summary>
	public bool IsMouseClicked()
	{
		return Input.GetMouseButtonDown(0) && Camera.mainCamera.pixelRect.Contains(Input.mousePosition);
	}
	
	public Vector3 GetMousePositon()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		
		if( Camera.main.isOrthoGraphic )
		{
			mousePosition = new Vector3(mousePosition.x, 0, mousePosition.z);
			return mousePosition;
		}
		
		Vector3 cameraPosition = Camera.main.transform.position;
		
		Vector3 directionToWorld = (mousePosition - cameraPosition).normalized;
				
		Plane playingField = new  Plane(Vector3.up, Vector3.zero); //the plane y = 0;
		Ray cameraToWorldByMouse = new Ray(cameraPosition, directionToWorld);
		float projectionDistance;
		playingField.Raycast(cameraToWorldByMouse, out projectionDistance);
		
		Vector3 result = cameraPosition + (directionToWorld * projectionDistance);
		return result;
	}
	
	public GameObject GetObjectUnderMouse()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		float cameraHeight = Camera.main.transform.position.y;
		Vector3 aboveMousePosition = new Vector3(mousePosition.x, cameraHeight + 10, mousePosition.z);
		
		RaycastHit hitInfo;
		Physics.Raycast(aboveMousePosition, mousePosition - aboveMousePosition, out hitInfo);
		
		if(hitInfo.collider != null)
			return hitInfo.collider.gameObject;
		else return null;
	}
	
	public Vector2 Make2D(Vector3 v3)
	{
		return new Vector2(v3.x, v3.z);
	}
	
	#endregion
	
	#region Line Drawing Functions
	
	protected VectorLine DrawTarget(Vector3 targetPosition, float radius, Color color)
	{
		Vector3 upperLeft, upperRight, lowerLeft, lowerRight;
		upperLeft = targetPosition + new Vector3( -radius, 0, radius);
		upperRight = targetPosition + new Vector3( radius, 0, radius);
		lowerLeft = targetPosition + new Vector3( -radius, 0, -radius);
		lowerRight = targetPosition + new Vector3( radius, 0, -radius);

		float sideLength = Vector3.Distance(upperLeft, upperRight);
		VectorLine target = VectorLine.SetLine3D(color, 
			upperLeft, 
			upperLeft + (upperRight - upperLeft).normalized * sideLength/4, 
			upperLeft + (upperRight - upperLeft).normalized * 3 * sideLength/4,
			upperRight, 
			upperRight + (lowerRight - upperRight).normalized * sideLength/4,
			upperRight + (lowerRight - upperRight).normalized * 3 *sideLength/4, 
			lowerRight, 
			lowerRight + (lowerLeft - lowerRight).normalized * sideLength/4, 
			lowerRight + (lowerLeft - lowerRight).normalized * 3 * sideLength/4, 
			lowerLeft,
			lowerLeft + (upperLeft - lowerLeft).normalized * sideLength/4, 
			lowerLeft + (upperLeft - lowerLeft).normalized * 3 * sideLength/4, 
			upperLeft
		); 
		
		target.SetWidths(new float[]{
			1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1
		});
		
		return target;
	}
	
	#endregion
	
}
