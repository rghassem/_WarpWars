using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineDrawer))]
public class Waypoint : WarpWarsBehavior {
	
	private readonly Color IN_RANGE_COLOR = Color.green;
	private readonly Color OUT_OF_RANGE_COLOR = Color.yellow;
	private readonly Color ENEMY_COLOR = Color.red;
	private const int LINE_WIDTH = 4;
	
	static Material wavePredictorMat = Resources.Load("ShockwavePredictor") as Material;
		
	public static Waypoint Spawn(Ship theShip)
	{
		GameObject prefab = Resources.Load("WaypointPrefab", typeof(GameObject)) as GameObject;
		GameObject instance = Instantiate(prefab) as GameObject;
		Waypoint waypoint = instance.GetComponent<Waypoint>();
		waypoint.ship = theShip;
		waypoint.showLine = false;
		return waypoint;
	}
	
	public Ship ship;
	public bool showLine;
	
	
	private GameObject blastCone;
	private Vector3? prevFrameShipPosition = null;
	private Vector3? prevFramePosition = null;
	
	ILine line;
	
	// Update is called once per frame
	void Update () 
	{
		if(showLine)
		{			
			UpdateLine();
		}
	}
	
	void OnDestroy()
	{
		EraseWaveReach();
	}
	
	public void SetPosition(Vector3 position)
	{
		transform.position = position;
		transform.LookAt( (position - ship.transform.position) * 2 );
	}
	
	public void Hide()
	{
		renderer.enabled = false;
		showLine = false;
		if(line != null)
			line.Hide();
		EraseWaveReach();
	}
	
	/// <summary>
	/// Show the waypoint and all its lines. Must be called after ship position and rotation are set
	/// </summary>
	public void Show()
	{
		renderer.enabled = true;
		if(line == null)
			UpdateLine();
		showLine = true;
		line.Show();
		DrawWaveReach(ship.transform.position);
	}
	
	void UpdateLine()
	{		
		Vector3 currentPosition = transform.position;
		float distance = Mathf.Abs(Vector3.Distance(ship.transform.position, currentPosition));
		Color lineColor;
		if(ship.owner.hasCurrentTurn)
			lineColor = (distance > ship.range) ? OUT_OF_RANGE_COLOR : IN_RANGE_COLOR;
		else
			lineColor = ENEMY_COLOR;
		
		if(line == null)
		{
			line = GetComponent<LineDrawer>().CreateLine(ship.gameObject, lineColor, LINE_WIDTH);
		}
		else if (line.color != lineColor)
			line.color = lineColor;
	}
	
	
	void DrawWaveReach(Vector3 shipPosition)
	{
		if(shipPosition == prevFrameShipPosition && transform.position == prevFramePosition)
			return;
		
		prevFrameShipPosition = shipPosition;
		prevFramePosition = transform.position;
		
		Vector3 currentPosition = transform.position;
		float distance = Mathf.Abs(Vector3.Distance(shipPosition, currentPosition));
		
		//Create gameobject, and relevent components
		MeshFilter filter;
		MeshRenderer renderer;
		Mesh mesh;
		if(blastCone == null)
		{
			blastCone = new GameObject("BlastWaveForecast");
			filter = blastCone.AddComponent<MeshFilter>();
			renderer = blastCone.AddComponent<MeshRenderer>();
			mesh = new Mesh();
			filter.mesh = mesh;
		}
		else
		{
			filter = blastCone.GetComponent<MeshFilter>();
			renderer = blastCone.GetComponent<MeshRenderer>();
			mesh = filter.mesh;
			mesh.Clear();
		}
		
		//define vertices of cone
		float range = Shockwave.ComputeBlastReach(distance);
		Vector3 center = transform.position;
		Vector3 right = transform.position + (ship.transform.right + ship.transform.forward ).normalized * range;
		Vector3 left = transform.position + (-ship.transform.right + ship.transform.forward).normalized * range;
		
		List<Vector3> arc = new List<Vector3>();
		List<int> triangles = new List<int>();
		
		float arcPointDensity = (int)range/5 + (int)range % 3 + 3; //must be multiple of 3
		arc.Add(center);
		arc.Add(left);
		Vector3 arcPoint;
		for(float interp = 2; interp < arcPointDensity + 1; interp++)
		{
			triangles.Add((int)interp - 1); //previous vertex
			triangles.Add(0);          //center point
			
			arcPoint = Vector3.Slerp(left - center, right - center, interp/arcPointDensity) + center;
			arc.Add(arcPoint);
			triangles.Add((int)interp);    //new vertex
		}
		
		mesh.vertices = arc.ToArray();
		mesh.triangles = triangles.ToArray();
		
		//assign uvs (code from unity reference sample)
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        int i = 0;
        while (i < uvs.Length) 
		{
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
            i++;
        }
		//and normals
		Vector3[] normals = new Vector3[vertices.Length];
		while (i < normals.Length) 
		{
            normals[i] = new Vector3(0, 1, 0);
            i++;
        }
		
        mesh.uv = uvs;
		mesh.normals = normals;
		
		renderer.material = wavePredictorMat;
	}
	
	void EraseWaveReach()
	{
		if(blastCone != null)
			Destroy(blastCone);
		prevFrameShipPosition = null;
	}
	
}
