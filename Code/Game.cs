using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {
	
	public Player currentPlayer;
	public int playerCount;
	
	List<Player> players;
	int currentTurnNumber;
	
	// Use this for initialization
	void Start () 
	{
		players = new List<Player>();
		
		GameObject playerObject;
		Player player;
		for(int i = 0; i < playerCount; i++)
		{
			int playerNum = i + 1;
			playerObject = new GameObject("Player " + playerNum);
			player = playerObject.AddComponent<Player>();
			player.playerName = "Player " + playerNum;
			players.Add(player);
			playerObject.transform.parent = transform;
			Vector3 startingShipPosition = new Vector3( i * 100, 0, 0);
			Ship.Spawn(startingShipPosition, player);
		}
		currentPlayer = players[currentTurnNumber];
		currentPlayer.StartTurn();
	}
	
	public void NextTurn()
	{	
		currentTurnNumber++;
		currentTurnNumber = currentTurnNumber % players.Count;
		
		currentPlayer.EndTurn();
		currentPlayer = players[currentTurnNumber];
		currentPlayer.StartTurn();
	}
	
	public void RemoveFromRotation(Player player)
	{
		players.Remove(player);
		if(players.Count == 0)
		{
			currentTurnNumber = 0;
			//TODO: Game over logic (actually players less than 1 is more appropriate)
		}
		else
		{
			currentTurnNumber = currentTurnNumber % players.Count;  
			currentPlayer = players[currentTurnNumber];
			currentPlayer.StartTurn();
		}
	}
}