using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineDrawer))]
public class Selectable : WarpWarsBehavior {

//Selection constants
	const float POST_SELECTION_LOCK_TIME = 0.2f;
	
	//Selection controls
	
	protected static Selectable selectedObject;
	protected static Selectable previousSelectedObject;
	public static bool selectionLocked;
	
	protected bool shouldDeselectOnClickAway = true;
	
	public bool isSelected;
	
	public Player owner;
	
	ILine selectionIndicator;

	private bool clickedThisFrame = false;
	
	#region Events
	void OnMouseDown()
	{
		clickedThisFrame = true;
		if(isSelected)
			ClearSelection(false);
		else
			SetSelection(this);
	}
	
	protected virtual void Start()
	{
		selectionIndicator = CreateSelectionIndicator();
	}
	
	protected virtual void OnDestroy()
	{
		ClearSelectionIndicators();
		if(owner != null)
			owner.Unregister(this);
	}
	
	protected virtual void Update()
	{
		//Deselect if the mouse is clicked somewhere else
		if(isSelected && shouldDeselectOnClickAway && !clickedThisFrame && IsMouseClicked() && !ui.IsMouseInGui())
		{
			ClearSelection(false);
		}
		
		clickedThisFrame = false;
	}
	
	#endregion
	
	
	#region Selection Controls
	
	public void ClearSelection(bool forceLock)
	{
		if(selectionLocked && !forceLock)
			return;
		
		if(selectedObject != null)
		{
			if(selectionLocked)
				UnlockSelection();
			selectedObject.isSelected = false;
			selectedObject.ClearSelectionIndicators();
			selectedObject.SendMessage("OnObjectDeselect", SendMessageOptions.DontRequireReceiver);
			selectedObject = null;
		}
		Highlighter.instance.SetHighlight(null);
	}
	
	public void SetSelection(Selectable thing)
	{
		if(selectionLocked)
			return;
		
		previousSelectedObject = selectedObject;
		ClearSelection(true);
		selectedObject = thing;
		isSelected = true;
		Highlighter.instance.SetHighlight(gameObject);
		DrawSelectionIndicators();
		BroadcastMessage("OnObjectSelect", SendMessageOptions.DontRequireReceiver);
		LockSelection(POST_SELECTION_LOCK_TIME);
	}
	
	/// <summary>
	/// Locks the selection for the specified time. Time = 0 means indefinetly.
	/// </summary>
	public void LockSelection(float time)
	{
		selectionLocked = true;
		if(time > 0)
		{
			Invoke("UnlockSelection", time);
		}
	}
			
	public void UnlockSelection()
	{
		selectionLocked = false;	
	}
	
	#endregion
	
	#region Selection Line Drawing
	
	/// <summary>
	/// Creates the indicator that shows as this ship. Must use the LineDrawer component to do it.
	/// </summary>
	protected virtual ILine CreateSelectionIndicator()
	{
		ILine iline = GetComponent<LineDrawer>().CreateCircle(gameObject, collider.bounds.extents.x/2, Color.green, 1);
		iline.Hide();
		return iline;
	}
	
	void DrawSelectionIndicators()
	{
		selectionIndicator.Show();
	}
	
	void ClearSelectionIndicators()
	{
		selectionIndicator.Hide();
	}
	
	#endregion
	
}
