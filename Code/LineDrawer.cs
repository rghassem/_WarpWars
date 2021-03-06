using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour {
	
	readonly Color DEFAULT_COLOR = Color.green;
	readonly int DEFAULT_WIDTH = 1;
	
	List<Drawing> lines;
	
	#region Interface
	
	#region Line Creation
	public ILine CreateLine(GameObject start, GameObject end, Color color, int width)
	{
		Line line = new Line(start, end, color, width);
		lines.Add(line);
		return line;
	}
	public ILine CreateLine(GameObject start, GameObject end) {return CreateLine(start, end, DEFAULT_COLOR, DEFAULT_WIDTH); }	
	public ILine CreateLine(GameObject end){ return CreateLine(gameObject, end); }
	public ILine CreateLine(GameObject end, Color color, int width){ return CreateLine(gameObject, end, color, width); }

	public ILine CreateLine(Vector3 startPoint, Vector3 endPoint, Color color, int width)
	{
		Line line = new Line(startPoint, endPoint, color, width);
		lines.Add(line);
		return line;
	}
	#endregion
	
	public ILine CreateCircle(GameObject origin, float radius, Color color, int width )
	{
		Circle circle =  new Circle(origin, radius, color, width);
		lines.Add(circle);
		return circle;
	}
	public ILine CreateCircle(GameObject origin, float radius){ return CreateCircle(origin, radius, DEFAULT_COLOR, DEFAULT_WIDTH); }
	
	public ILine CreateTargetReticule(GameObject targetObject, float radius, Color color, int width)
	{
		TargetReticule target = new TargetReticule(targetObject, radius, color, width);
		lines.Add(target);
		return target;
	}
	
	public ILine CreateTargetReticule(GameObject targetObject, float radius)
		{ return CreateTargetReticule(targetObject, radius, DEFAULT_COLOR, DEFAULT_WIDTH); }
	
	public void Erase()
	{
		foreach(Drawing line in lines)
		{
			line.Cleanup();
		}
		lines = new List<Drawing>();
	}
	
	#endregion
	
	
	#region Events
	// Use this for initialization
	void Awake () {
		lines = new List<Drawing>();
	}
	
	// Update is called once per frame
	void Update () {
		foreach(Drawing line in lines)
		{
			line.Update();
		}
	}
	
	void OnDestroy () 
	{
		Erase();
	}
	#endregion
	
	
	#region ILines
	
	abstract class Drawing : ILine
	{
		protected VectorLine vectorLine;
		protected bool isHidden;
		
		//Methods for use by LineDrawer
		public abstract void Update();
		
		public virtual void Cleanup()
		{
			VectorLine.Destroy(ref vectorLine);
		}
		
		//Implement ILine interface
		public abstract void SetWidth(int newWidth);
		
		public virtual Color color
		{
			get {return vectorLine.color;}
			set {vectorLine.SetColor(value);}
		}
		
		public virtual void Show()
		{
			vectorLine.active = true;
			isHidden = false;
		}
		
		public virtual void Hide()
		{
			vectorLine.active = false;
			isHidden = true;
		}
	}
		
	class Line : Drawing
	{
		public GameObject origin;
		public GameObject target;
		Vector3 prevStart;
		Vector3 prevEnd;
		
		public Line(GameObject start, GameObject end, Color color, int width)
			:this(start.transform.position, end.transform.position, color, width)
		{
			origin = start;
			target = end;
		}
		
		public Line(Vector3 start, Vector3 end, Color color, int width)
		{
			vectorLine =  VectorLine.SetLine3D(color, start, end);
			vectorLine.SetWidths( new int[] {width} );	
			prevStart = start;
			prevEnd = end;
		}
		
		
		public override void Update()
		{
			if(isHidden)
				return;
			if(origin == null && target == null)
				return;
			
			Vector3 newStart = vectorLine.points3[0];
			Vector3 newEnd = vectorLine.points3[1];
			
			if(origin != null )
				newStart = origin.transform.position;
			
			if(target != null)
				newEnd = target.transform.position;
			
			if(newEnd == prevEnd && newStart == prevStart)
				return;
			
			if(origin != null || target != null)
			{
				vectorLine.points3 = new Vector3[] {newStart, newEnd};
			}
		}
		
		public override void Cleanup()
		{
			VectorLine.Destroy(ref vectorLine);
		}
		
		//Implement ILine interface
		
		public override Color color
		{
			get {return vectorLine.color;}
			set {vectorLine.SetColor(value);}
		}
		
		
		public override void Show()
		{
			vectorLine.active = true;
			isHidden = false;
		}
		
		public override void Hide()
		{
			vectorLine.active = false;
			isHidden = true;
		}
		
		public override void SetWidth(int newWidth)
		{
			vectorLine.SetWidths(new int[] {newWidth});
		}
	}
	
	
	class Circle :Drawing
	{		
		const int MAX_SEGMENTS = 15;
		int segments;
		GameObject origin;
		Vector3 prevPosition;
		float radius;
		Color circleColor;

		public Circle(Vector3 origin, float radius, Color theColor, int theWidth)
		{
			segments = Mathf.Max((int)radius, MAX_SEGMENTS);
			vectorLine = new VectorLine("SelectCircle", new Vector3[segments * 2], theColor, null, theWidth);
			vectorLine.MakeCircle(origin, Vector3.up, radius, segments);
			prevPosition = origin;
			circleColor = theColor;
			this.radius = radius;
			vectorLine.Draw3D();
		}
		
		public Circle(GameObject theOrigin, float radius, Color color, int width)
			:this(theOrigin.transform.position, radius, color, width)
		{
			origin = theOrigin;
		}
		
		public override void Update()
		{
			if(isHidden)
				return;
			if(origin == null)
				return;
			if(prevPosition == origin.transform.position)
				return;
			
			vectorLine.MakeCircle(origin.transform.position, Vector3.up, radius, segments); 
			vectorLine.Draw3D();
		}
		
		public override void Cleanup()
		{
			VectorLine.Destroy(ref vectorLine);
		}
		
		public override Color color
		{
			get {return circleColor;}
			set {
					vectorLine.SetColor(value);
					circleColor = value;
				}
		}
		
		
		public override void Show()
		{
			vectorLine.active = true;
			isHidden = false;
		}
		
		public override void Hide()
		{
			vectorLine.active = false;
			isHidden = true;
		}
		
		public override void SetWidth(int newWidth)
		{
			vectorLine.SetWidths(new int[] {newWidth});
		}
		
	}
	
	class TargetReticule : Drawing
	{
		float radius;
		GameObject target;
		Vector3 prevPosition;
		
		public TargetReticule(GameObject target, float radius, Color color, int width)
		{
			vectorLine = VectorLine.SetLine3D(color, GetPoints(target.transform.position, radius, color));
			this.target = target;
			this.radius = radius;
			this.color = color;
			SetWidth(width);
			prevPosition = target.transform.position;
		}
		
		public override void Update ()
		{
			if(isHidden)
				return;
			if(target == null)
				return;
			if(prevPosition == target.transform.position)
				return;
			
			vectorLine.points3 = GetPoints(target.transform.position, radius, color);
		}
		
		Vector3[] GetPoints(Vector3 targetPosition, float radius, Color color)
		{
			Vector3 upperLeft, upperRight, lowerLeft, lowerRight;
			upperLeft = targetPosition + new Vector3( -radius, 0, radius);
			upperRight = targetPosition + new Vector3( radius, 0, radius);
			lowerLeft = targetPosition + new Vector3( -radius, 0, -radius);
			lowerRight = targetPosition + new Vector3( radius, 0, -radius);
	
			float sideLength = Vector3.Distance(upperLeft, upperRight);
			return new Vector3[] { 
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
			};
		}
	
		public override void SetWidth(int newWidth)
		{
			vectorLine.SetWidths(new float[]{
					newWidth, 0, newWidth, newWidth, 0, newWidth, newWidth, 0, newWidth, newWidth, 0, newWidth
				});
		}
	}
	
	#endregion
}

public interface ILine
{
	Color color {get; set;}
	
	void Show();
	void Hide();
	void SetWidth(int newWidth);
}



