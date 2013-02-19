using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Planet : Selectable {
	
	readonly float SHIP_SPAWN_DISTANCE_FROM_PLANET = 30;
	public static readonly float TARGETER_RADIUS = 10;
	readonly string BUILD_SHIP_BUTTON_TEXT = "Build Ship";
	
	/// <summary>
	/// The health of the planet. When it declines to zero, the planet is destroyed
	/// </summary>
	public int health;
	
	/// <summary>
	/// The population on the planet. Limited by the planet's health. Can be traded for ships.
	/// </summary>
	public int population;
	
	PlanetStatusBar statusBar;
	
	UIManager.IGUI buildShipsButton;
	
	protected override void Start () 
	{
		base.Start();
		statusBar = PlanetStatusBar.CreateStatusBar(this);
		buildShipsButton = ui.CreateButton(BUILD_SHIP_BUTTON_TEXT, BuildShips);
	}
	
	protected override void Update()
	{
		base.Update();	
	}
	
	protected override void OnDestroy()
	{
		if(statusBar != null)
			Destroy(statusBar.gameObject);
	}
	
	void OnObjectSelect()
	{
		if(population > 0)
			buildShipsButton.IsVisible = true;
		
	}
	
	void OnObjectDeselect()
	{
		buildShipsButton.IsVisible = false;
		//CancelColonization();
	}
	
	
	void OnDamage(GameObject attacker)
	{
		health -= 1;
		if(health == 0)
		{
			GetComponent<Explodable>().Explode();
			Destroy(gameObject);
		}
		else
		{
			population = Mathf.Min(population, health);
		}
	}
	
	public void BuildShips()
	{
		Vector3 spawnPosition = new Vector3(SHIP_SPAWN_DISTANCE_FROM_PLANET, 0, SHIP_SPAWN_DISTANCE_FROM_PLANET);
		float angle = 360 / population;
		Vector3 planetPosition = transform.position;

		for(int i = population; i > 0; i--)
		{
			Ship.Spawn(planetPosition + 
				spawnPosition, game.currentPlayer);
			//rotate spawnPosition arround position by angle
			spawnPosition = Quaternion.Euler(0, angle, 0) * spawnPosition;
		}
		
		population = 0;
		buildShipsButton.IsVisible = false;
		
		NofifyNearbyShipsOfColonyStatusChange();
	}
	
	public bool CanColonize()
	{
		return population == 0;
	}
	
	public void Colonize(Player newOwner)
	{	
		population += 1;
		NofifyNearbyShipsOfColonyStatusChange();
		newOwner.Register(this);
	}
	
	void NofifyNearbyShipsOfColonyStatusChange()
	{
		//Let any other ships in colonization range know that this ship was colonized
		Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, Ship.MINIMUM_WARP_RANGE); 
		foreach (Collider nearbyObject in nearbyObjects)
		{
			Ship nearbyShip = nearbyObject.GetComponent<Ship>();
			if(nearbyShip != null)
				nearbyShip.ScanForPlanets();
		}
	}
	
	#region 
	protected override ILine CreateSelectionIndicator()
	{				
		ILine iline = GetComponent<LineDrawer>().CreateTargetReticule(gameObject, TARGETER_RADIUS);
		iline.Hide();
		return iline;
	}
		
	#endregion
	
}
