//By Carson Rueber

using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections;
using System.Collections.Generic;


//Renders some big lines
//Best used for large amounts of small shapes, as this is more ineffecient than its counterpart,
//BigLineRenderer; however, this implementation allows for "islands"
public class BigLineListRenderer {
	//Our game object, all the lineRenderers get parented to it, has a RectTransform
	public GameObject gameObject;
	public List<GameObject> lineRenderers;
	//The maximum number of points before needing to make a new lineRenderer 10666
	public const int maxPointCount = 10666;
	//Index of the next point
	private int currentPointIndex = 0;
	//Number of points that have been added to the shape, not how many vectors are in lineRenderers or anything
	private int linePointCount = 0;

	//Default constructor
	public BigLineListRenderer() {
		lineRenderers = new List<GameObject>();
		gameObject = new GameObject("BigLineListRenderer", new System.Type[] { typeof(RectTransform) });
		createNewLineRenderer();
	}

	//Add a point to the line
	public void addPoint(Vector2 point) {
		//If we need to make a new line renderer, do so
		if (currentPointIndex >= maxPointCount) {
			createNewLineRenderer();
		}
		UILineRenderer lineRenderer = lineRenderers[lineRenderers.Count - 1].GetComponent<UILineRenderer>();

		if (linePointCount >= 2) {
			lineRenderer.Points[currentPointIndex] = getPreviousElement();
			currentPointIndex++;
		}

		//Set the point
		lineRenderer.Points[currentPointIndex] = point;
		currentPointIndex++;

		linePointCount++;
	}

	//Call this function when you're finished with a line
	public void finishLine() {
		linePointCount = 0;
	}

	//Sets the thickness of the lines
	public void setLineThickness(float thickness) {
		for (int q = 0; q < lineRenderers.Count; q++) {
			UILineRenderer lineRenderer = lineRenderers[q].GetComponent<UILineRenderer>();
			lineRenderer.LineThickness = thickness;
		}
	}

	private void createNewLineRenderer() {
		//Create the game object
		GameObject lineRendererObj = new GameObject("LineRenderer", new System.Type[]{ typeof(UILineRenderer) });
		lineRendererObj.transform.SetParent(gameObject.transform);
		lineRendererObj.GetComponent<RectTransform>().position = new Vector2(0.0f, 0.0f);
		UILineRenderer lineRenderer = lineRendererObj.GetComponent<UILineRenderer>();

		//Set properties
		lineRenderer.Points = new Vector2[maxPointCount];
		lineRenderer.lineList = true; //The key difference
		lineRenderer.lineThickness = 1.0f;
		lineRenderer.lineCaps = true;
		lineRenderer.BezierMode = UILineRenderer.BezierType.None;
		lineRenderer.ImproveResolution = ResolutionMode.None;

		//Add it to the list and reset currentLineIndex while we're here
		lineRenderers.Add(lineRendererObj);
		currentPointIndex = 0;
	}

	//Trusts that you'll never call it when there are no previous elements
	private Vector2 getPreviousElement() {
		UILineRenderer lineRenderer;
		//The previous element is the last element of the previous lineRenderer
		if (currentPointIndex == 0 && lineRenderers.Count >= 2) {
			lineRenderer = lineRenderers[lineRenderers.Count - 2].GetComponent<UILineRenderer>();
			return lineRenderer.Points[maxPointCount - 1];
		}

		//Return the previous element
		lineRenderer = lineRenderers[lineRenderers.Count - 1].GetComponent<UILineRenderer>();
		return lineRenderer.Points[currentPointIndex - 1];
	}
}
