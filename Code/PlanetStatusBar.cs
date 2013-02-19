using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetStatusBar : MonoBehaviour {
	
	static readonly int BAR_WIDTH_PIXELS = 20;
	static readonly int BAR_HEIGHT_PIXELS = 10;
	static readonly int BAR_SEPERATOR_PIXELS = 2;
	static readonly float PLANET_SEPERATOR_METERS = 5;
	static readonly int BARS_PER_HORIZONTAL = 4;
	static readonly Color BAR_FILL_COLOR = Color.green;
	static readonly Color BAR_EMPTY_COLOR = new Color(0.1f, 0.1f, 0.1f, 1);
	
	public GameObject barTemplate;
	
	Planet thePlanet;
	List<GameObject> bars;
	
	Transform planetTransform;	
	Transform cachedTransform;
	float verticalPlanetOffset;
	float horizontalPlanetOffset;
	int filledBars = 0;
	
	public static PlanetStatusBar CreateStatusBar(Planet planet)
	{
		GameObject planetGui = GameObject.FindWithTag("PlanetGUI");
		GameObject planetBarPrefab = Resources.Load("PlanetStatusBar/StatusBar") as GameObject;
		GameObject planetBar = Instantiate(planetBarPrefab) as GameObject;
		planetBar.transform.parent = planetGui.transform;
		
		PlanetStatusBar psb = planetBar.GetComponent<PlanetStatusBar>();
		psb.thePlanet = planet;
		psb.planetTransform = planet.transform;
		psb.cachedTransform = psb.transform; //cache a reference, since it must be accessed every frame
		psb.verticalPlanetOffset = planet.collider.bounds.extents.x;
		psb.horizontalPlanetOffset = BAR_WIDTH_PIXELS / 2;
		psb.bars = new List<GameObject>();	
		
		return psb;
	}
		
	void Update () 
	{			
		//update bars to match planet heath and population
		bool changeNeeded = bars.Count != thePlanet.health || filledBars != thePlanet.population;
		while(bars.Count != thePlanet.health)
		{
			 if(bars.Count > thePlanet.health)
				RemoveBar();
			 else
				AddBar();
		}
		if(changeNeeded)
		{
			SetBarFill(thePlanet.population);
			horizontalPlanetOffset = ( Mathf.Min(bars.Count, BARS_PER_HORIZONTAL) * (BAR_WIDTH_PIXELS + BAR_SEPERATOR_PIXELS) ) / 2;
		}
		
		//update position to match planet screen position 
		float actualVerticalOffset = Mathf.Ceil(bars.Count / (float)BARS_PER_HORIZONTAL) * verticalPlanetOffset + PLANET_SEPERATOR_METERS;
		Vector3 worldCoordinates = planetTransform.position + new Vector3(0,0,actualVerticalOffset);
		Vector3 desiredPosition = Camera.main.WorldToViewportPoint (worldCoordinates) 
								- Camera.main.ScreenToViewportPoint( new Vector3(horizontalPlanetOffset, 0, 0)); 
		cachedTransform.position = desiredPosition; 

	}
	
	private void AddBar()
	{
		GameObject bar = Instantiate(barTemplate) as GameObject;
		bar.transform.parent = gameObject.transform;
		GUITexture barGT = bar.GetComponent<GUITexture>();
		
		float left, top;
		left = bars.Count * (BAR_WIDTH_PIXELS + BAR_SEPERATOR_PIXELS) % ( (BAR_WIDTH_PIXELS + BAR_SEPERATOR_PIXELS)*BARS_PER_HORIZONTAL );
		top = (bars.Count >= BARS_PER_HORIZONTAL) ? -BAR_HEIGHT_PIXELS - BAR_SEPERATOR_PIXELS : 0;
		barGT.pixelInset = new Rect( left, top, BAR_WIDTH_PIXELS, BAR_HEIGHT_PIXELS);
		
		Transform barFillTrans = bar.transform.GetChild(0);
		GUITexture barFill = barFillTrans.GetComponent<GUITexture>();
		barFill.pixelInset = barGT.pixelInset;
		
		bars.Add(bar);
	}
	
	private void RemoveBar()
	{
		GameObject bar = bars[bars.Count - 1];
		bars.RemoveAt(bars.Count - 1);
		Destroy(bar);
	}
	
	private void SetBarFill(int barsToFill)
	{
		if(barsToFill > bars.Count)
			barsToFill = bars.Count;
		for(int i = 0; i < bars.Count; i++)
		{
			if( i < barsToFill )
				FillBar(bars[i], BAR_FILL_COLOR);
			else
				FillBar(bars[i], BAR_EMPTY_COLOR);
		}
		filledBars = barsToFill;
	}
	
	private void FillBar(GameObject bar, Color fill)
	{
		Transform barFillTrans = bar.transform.GetChild(0);
		GUITexture barFill = barFillTrans.GetComponent<GUITexture>();
		barFill.color = fill;
	}
		
}
