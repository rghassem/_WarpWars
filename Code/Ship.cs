using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Explodable))]
public class Ship : Selectable {
	
	public const float MINIMUM_WARP_RANGE = 50;
	readonly Color COLONIZATION_TARGETER_COLOR = Color.yellow;
	readonly string WARP_BUTTON_TEXT = "Warp";

	
	public float range;
	public float speed;
	
	Waypoint waypoint;
	Vector3 moveStartPosition;
	
	const float epsilon = 0.01f;
	
	//TODO: Use the LineDrawer here
	List<ColonyTarget> colonyTargets;
	List<VectorLine> colonizationTargets;
	
	ILine minimumWarpCirce;
	
	UIManager.IGUI warpButton;
	readonly Vector2 COLONIZE_BUTTON_DIMENSIONS = new Vector2(100, 30);
	readonly float COLONIZE_BUTTON_PLANET_OFFSET_MULTIPLIER = 1.5f;
	readonly string COLONIZE_BUTTON_TEXT = "Colonize";
	
	public static Ship Spawn(Vector3 position, Player owner)
	{
		GameObject prefab = Resources.Load("Space_Shooter", typeof(GameObject)) as GameObject;
		GameObject instance = Instantiate(prefab) as GameObject;
		Ship ship = instance.GetComponent<Ship>();
		instance.transform.position = position;
		owner.Register(ship);
		ship.Initialize();
		return ship;
	}
	
	enum ShipState
	{
		IDLE,
		WARPING,
		TURN_FINISHED
	}
	
	ShipState state;
	
	/// <summary>
	/// Initialize this instance.Used in place of the Start event
	/// </summary>
	private void Initialize()
	{
		state = ShipState.IDLE;
		shouldDeselectOnClickAway = false;
		colonyTargets = new List<ColonyTarget>();
		colonizationTargets = new List<VectorLine>();
		ScanForPlanets();
		
		//Create ui objects
		warpButton = ui.CreateButton(WARP_BUTTON_TEXT, Engage);
		
		//Create lines
		minimumWarpCirce = GetComponent<LineDrawer>().CreateCircle(gameObject, MINIMUM_WARP_RANGE);
		minimumWarpCirce.Hide();
	}
	
	public bool IsWarping() { return state == ShipState.WARPING; }
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();
		
		//Handle moving the ship
		if(state == ShipState.WARPING)
		{
			Vector3 movementVector = waypoint.transform.position - transform.position;
			if(movementVector.sqrMagnitude > epsilon || movementVector.sqrMagnitude < -epsilon)
			{
				Vector3 maxMove = movementVector.normalized * speed * Time.deltaTime;
				Vector3 realMovementVector = (maxMove.sqrMagnitude > movementVector.sqrMagnitude)
												? movementVector
												: maxMove;
				transform.Translate(realMovementVector,Space.World);
			}
			else 
				DropOutOfWarp();
		}
		
		//Handle input
		else if(isSelected && state != ShipState.TURN_FINISHED)
		{
			if(IsMouseDown())
				HandleMouseDown(GetMousePositon());
			
			if(Input.GetKeyDown(KeyCode.W))
			{
				Engage();
			}
		}		
	}
	
	
	void HandleMouseDown(Vector3 position)
	{
		if(!collider.bounds.Contains(position))
		{
			GameObject clickedObject = GetObjectUnderMouse();
			if (clickedObject != null &&
				clickedObject.GetComponent<Planet>() != null)
			{
				if(waypoint != null)
					waypoint.Hide();
			}
			else 
			{
				transform.LookAt(position);
				if(Vector3.Distance(position, transform.position) < MINIMUM_WARP_RANGE)
					SetWaypoint(position, false);
				else if(!ui.IsMouseInGui()) 
					SetWaypoint(position, true);
			}
		}
	}
	
	void OnObjectDeselect()
	{
		if(waypoint != null)
		{
			waypoint.Hide();
		}
		warpButton.IsVisible = false;
		UnHighlightColonizeTargets();
		minimumWarpCirce.Hide();
	}
	
	void OnObjectSelect()
	{
		if(waypoint != null && Vector3.Distance(waypoint.transform.position, transform.position) > MINIMUM_WARP_RANGE + epsilon)
		{
			waypoint.Show();
		}
		warpButton.IsVisible = true;
		HighlightColonizeTargets();
		minimumWarpCirce.Show();
	}
	
	void OnDamage(GameObject attacker)
	{
		if(attacker != gameObject)
		{
			GetComponent<Explodable>().Explode();
			Destroy(gameObject);
		}
	}
	
	void OnShockwaveComplete()
	{
		state = ShipState.IDLE;
	}
	
	protected override void OnDestroy()
	{		
		if(warpButton != null)
			ui.DeleteButton(warpButton);
		
		if(waypoint != null)
			Destroy(waypoint.gameObject);
		
		base.OnDestroy();
	}
	
	/// <summary>
	/// Fly the ship to the waypoint coordinates
	/// </summary>
	public void Engage()
	{
		if(!owner.hasCurrentTurn)
			return;
		
		if(waypoint != null)
		{
			state = ShipState.WARPING;
			LockSelection(0);
			moveStartPosition = transform.position;
		}
		
		if(colonyTargets.Count > 0)
			ClearColonizationPlanets();
	}
	
	void DropOutOfWarp()
	{
		state = ShipState.TURN_FINISHED;
		Destroy(waypoint.gameObject);
		float distance = Mathf.Abs(Vector3.Distance(transform.position, moveStartPosition));
		ReleaseEnergy(distance);
		ClearSelection(true);
		ScanForPlanets();
	}
	
	void SetWaypoint(Vector3 position, bool makeVisible)
	{
		//Create a waypoint if we have not got one
		if(waypoint == null)
		{
			waypoint = Waypoint.Spawn(this);
		}
		
		if(waypoint.transform.position != position)
			waypoint.SetPosition(position); //update the position of the waypoint BEFORE it is shown.
		
		if(makeVisible)
			waypoint.Show();
		else waypoint.Hide();
	}
	
	void ReleaseEnergy(float distanceTraveled)
	{
		Vector3 frontPoint = transform.position + transform.forward * 10;
		Shockwave.Spawn(transform.position, frontPoint, gameObject, distanceTraveled);
	}
	
	#region Colonization Logic
	
	public bool CheckColonyRange(Planet planet)
	{
		return Vector3.Distance(planet.transform.position, transform.position) <= MINIMUM_WARP_RANGE;
	}

	
	public void ScanForPlanets()
	{
		ClearColonizationPlanets();
		Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, MINIMUM_WARP_RANGE);
		foreach(Collider nearbyObject in nearbyObjects)
		{
			Planet planet = nearbyObject.GetComponent<Planet>();
			if(planet != null)
			{
				ReadyColonization(planet);
			}
		}
	}
	
	void ReadyColonization(Planet planet)
	{
		if(planet.CanColonize())
		{			
			colonyTargets.Add(new ColonyTarget(planet, 
				VectorLine.SetLine3D(Color.cyan, transform.position, planet.transform.position),
				ShowColonizablePlanets(planet)
			));
		}
	}
	
	
	void ClearColonizationPlanets()
	{
		for(int i = 0; i < colonyTargets.Count; i++)
		{
			VectorLine line = colonyTargets[i].line;
			ui.DeleteButton(colonyTargets[i].colonizeButton);
			VectorLine.Destroy(ref line);
		}
		colonyTargets = new List<ColonyTarget>();
	}
	
	public void Colonize(Planet planet)
	{
		ClearColonizationPlanets();
		UnHighlightColonizeTargets();
		planet.Colonize(owner);
		
		//Let any other ships in colonization range know that this ship was colonized
		Collider[] nearbyObjects = Physics.OverlapSphere(planet.transform.position, MINIMUM_WARP_RANGE); 
		foreach (Collider nearbyObject in nearbyObjects)
		{
			Ship nearbyShip = nearbyObject.GetComponent<Ship>();
			if(nearbyShip != null)
				nearbyShip.ScanForPlanets();
		}
		
		Destroy(gameObject);
	}
	

	
	void HighlightColonizeTargets()
	{
		List<Planet> currentTargetedPlanets = new List<Planet>();
		foreach(ColonyTarget colonyTarget in colonyTargets)
		{
			Planet planet = colonyTarget.planet;
			colonizationTargets.Add( DrawTarget(planet.transform.position, Planet.TARGETER_RADIUS, COLONIZATION_TARGETER_COLOR) );
			currentTargetedPlanets.Add(planet);
			colonyTarget.line.SetWidths(new float[] { colonyTarget.line.lineWidth * 3 });
			colonyTarget.colonizeButton.IsVisible = true;
		}
	}
	
	void UnHighlightColonizeTargets()
	{
		for(int i = 0; i < colonizationTargets.Count; i++)
		{
			VectorLine line = colonizationTargets[i];
			VectorLine.Destroy(ref line);
		}
		colonizationTargets = new List<VectorLine>();
		
		foreach(ColonyTarget colonyTarget in colonyTargets)
		{
			colonyTarget.line.SetWidths( new int[]{1} );
			colonyTarget.colonizeButton.IsVisible = false;
		}
	}
	
	UIManager.IGUI ShowColonizablePlanets(Planet planet)
	{
		Vector3 planetPos = planet.transform.position;
		float offset = planet.transform.localScale.z * COLONIZE_BUTTON_PLANET_OFFSET_MULTIPLIER;
		Vector3 worldSpaceButtonPosition = new Vector3(planetPos.x, planetPos.y, planetPos.z - offset);
		return ui.CreateButton(
			worldSpaceButtonPosition, COLONIZE_BUTTON_DIMENSIONS.x, COLONIZE_BUTTON_DIMENSIONS.y,
			COLONIZE_BUTTON_TEXT, () => Colonize(planet));
	}
	
	struct ColonyTarget
	{
		public Planet planet;
		public VectorLine line;
		public UIManager.IGUI colonizeButton;
		
		public ColonyTarget(Planet thePlanet, VectorLine targetingLine, UIManager.IGUI buttonToColonize)
		{
			planet = thePlanet;
			line = targetingLine;
			colonizeButton = buttonToColonize;
		}
	}
	
	#endregion
}
