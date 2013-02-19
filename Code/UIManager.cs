using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIManager : WarpWarsBehavior {
	
	#region Main
	/// <summary>
	/// The bounding boxes of every gui currently rendered. Used to check against mouse position.
	/// Reset after every OnGui call.
	/// </summary>
	List<Rect> activeGuiBounds;
	
	List<GuiElement> guis;
	
	protected override void Awake()
	{
		base.Awake();
		guis = new List<GuiElement>(); 
	}
	
	public bool IsMouseInGui()
	{
		foreach(Rect bounds in activeGuiBounds)
		{
			if( bounds.Contains(Input.mousePosition) )
				return true;
		}
		return false;
	}	
	
	/// <summary>
	/// Creates a button to show in the default central location. Returns GUI inteface for the button
	/// </summary>
	public IGUI CreateButton(string buttonText, Action buttonPressAction)
	{
		CenterButton button = new CenterButton(buttonText, buttonPressAction);
		button.IsVisible = false;
		guis.Add(button);
		return button;
	}
	
	/// <summary>
	/// Creates a world positioned button. Returns GUI inteface for the button
	/// </summary>
	public IGUI CreateButton(Vector3 position, float width, float height, string buttonText, Action buttonPressAction)
	{
		WorldPositionedButton button = new WorldPositionedButton(position, width, height, buttonText, buttonPressAction);
		button.IsVisible = false;
		guis.Add(button);
		return button;
	}
	
	public void DeleteButton(IGUI button)
	{
		(button as GuiElement).deleteFlag = true; //mark for clean up in the next gui phase.
	}
	
	void OnGUI()
	{
		activeGuiBounds = new List<Rect>();
				
		if(showPlayerStart)
			ShowPlayerStart();
		
		List<GuiElement> doomedGuis = new List<GuiElement>();
		
		for(int i = 0; i< guis.Count; i++ )
		{
			GuiElement gui = guis[i];
			if(gui.deleteFlag) //clean up guis flagged for deletion
			{
				doomedGuis.Add(gui);
			}
			//Display visible guis
			else if(gui.IsVisible)
			{
				activeGuiBounds.Add(gui.GetBounds());
				gui.Display();
			}
		}
		
		//Sweep up unnessecary gui elements
		foreach( GuiElement doomedGui in doomedGuis )
		{
			guis.Remove(doomedGui);
		}
		
	}
	#endregion
	
	#region Player Instructions
	public bool showPlayerStart;	
	
	void ShowPlayerStart()
	{
		GUI.Label(new Rect( Screen.width/2 - 50, 0, 100, 50), 
						game.currentPlayer.playerName + "'s Turn!");
	}
	#endregion
	
	
	#region GUI Definitions
	
	public interface IGUI
	{
		bool IsVisible {get; set;}
		
		
	}
	
	private abstract class GuiElement : IGUI
	{
		public bool IsVisible {get; set;}
		
		public bool deleteFlag = false;
		
		public abstract void Display();
		
		public abstract Rect GetBounds(); 
	}
	
	
	private class CenterButton : GuiElement
	{
		protected string buttonText;
		protected Action buttonPressAction;
		
		//constants definitions
		static readonly float mainButtonWidth = 200;
		static readonly float mainButtonHeight = 50;
		

		
		public CenterButton(string text, Action pressAction)
		{
			buttonText = text;
			buttonPressAction = pressAction;
		}
		
		public override void Display()
		{		
			Rect mainButtonBounds = new Rect(Screen.width/2 - mainButtonWidth/2, Screen.height/4 - mainButtonHeight/2,
													mainButtonWidth, mainButtonHeight);
			
			Rect mainButtonBoundsGuiSpace = new Rect(mainButtonBounds);
			mainButtonBoundsGuiSpace.y = Screen.height - mainButtonBoundsGuiSpace.y;
		
			if(GUI.Button(mainButtonBoundsGuiSpace, buttonText))
			{
				buttonPressAction();
			}
		}
		
		public override Rect GetBounds()
		{
			//This is mainButtonBounds, adjusted for point inclusion detection. Used in ShowCentralButton
			Rect screenSpaceMainButtonBounds = new Rect( Screen.width/2 - mainButtonWidth/2,
			(Screen.height/4 - mainButtonHeight/2) - mainButtonHeight, mainButtonWidth, mainButtonHeight );
			
			return screenSpaceMainButtonBounds;
		}
		
	}
	
	
	
	private class WorldPositionedButton : CenterButton
	{
		Vector3 targetPosition;
		float width;
		float height;
		
		public WorldPositionedButton(Vector3 position, float buttonWidth, float buttonHeight, string text, Action pressAction)
			:base(text, pressAction)
		{
			targetPosition = position;
			width = buttonWidth;
			height = buttonHeight;
		}
		
		public override void Display()
		{		
			Vector2 screenSpaceButtonPosition = Camera.main.WorldToScreenPoint(targetPosition);
			screenSpaceButtonPosition.Set(screenSpaceButtonPosition.x - width/2, screenSpaceButtonPosition.y);
						
			Vector2 guiSpaceButtonPosition = new Vector2(screenSpaceButtonPosition.x, Screen.height-screenSpaceButtonPosition.y);
			Rect bounds = new Rect( guiSpaceButtonPosition.x, guiSpaceButtonPosition.y, 
								width, height);
			
			if(GUI.Button(bounds, buttonText))
			{
				buttonPressAction();
			}
		}
		
		public override Rect GetBounds()
		{
			Vector2 screenSpaceButtonPosition = Camera.main.WorldToScreenPoint(targetPosition);
			Rect screenSpaceBounds = new Rect(screenSpaceButtonPosition.x, screenSpaceButtonPosition.y - height,  width, height);			
			return screenSpaceBounds;
		}
		
	}
	
	#endregion

	
}
