using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : WarpWarsBehavior {
	
	public string playerName;
	public bool hasCurrentTurn;
	
	List<Ship> ships;
	List<Planet> planets;
	
	protected override void Awake()
	{
		base.Awake();
		ships = new List<Ship>();	
		planets = new List<Planet>();
	}
	
	public void StartTurn()
	{		
		ui.showPlayerStart = true;
		hasCurrentTurn = true;
	}
	
	public void EndTurn()
	{
		hasCurrentTurn = false;
	}
	
	public void Register(Selectable piece)
	{
		if (piece is Ship)
			ships.Add(piece as Ship);
		else if (piece is Planet)
			planets.Add(piece as Planet);
		
		piece.owner = this;
	}
	
	public void Unregister(Selectable piece)
	{
		if (piece is Ship)
			ships.Remove(piece as Ship);
		
		if(ships.Count == 0 && planets.Count == 0)
			game.RemoveFromRotation(this);
	}
	

	
	
	
}
