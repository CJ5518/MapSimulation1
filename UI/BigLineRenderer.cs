//By Carson Rueber
//Max point count could very likely be 10666
//ENTIRELY untested in its new state

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

//Renders one very large line
//Best used for large shapes, as no "islands" are permitted
public class BigLineRenderer {
	//Our game object, all the lineRenderers get parented to it, has a RectTransform
	public GameObject gameObject;
	public List<GameObject> lineRenderers;
	//The maximum number of points before needing to make a new lineRenderer
	public const int maxPointCount = 5000;
	//Index of the next point
	private int currentPointIndex = 0;

	//Default constructor
	public BigLineRenderer() {
		lineRenderers = new List<GameObject>();
		gameObject = new GameObject("BigLineRenderer", new System.Type[] { typeof(RectTransform) });
		createNewLineRenderer();
	}

	//Add a point to the line
	public void addPoint(Vector2 point) {
		//If we need to make a new line renderer, do so
		if (currentPointIndex >= maxPointCount) {
			createNewLineRenderer();
		}
		UILineRenderer currentComponent = lineRenderers[lineRenderers.Count - 1].GetComponent<UILineRenderer>();
		//If this is the first point in a new line renderer
		if (lineRenderers.Count > 1 && currentPointIndex == 0) {
			//Now we need to set the first point to the last point of the previous line renderer
			UILineRenderer previousComponent = lineRenderers[lineRenderers.Count - 2].GetComponent<UILineRenderer>();
			currentComponent.Points[0] = previousComponent.Points[maxPointCount - 1];
			currentPointIndex++;
		}
		//Set the point
		currentComponent.Points[currentPointIndex] = point;
		currentPointIndex++;
	}

	//Call this function when you're finished so that it'll be finished
	public void finishAdding() {
		UILineRenderer component = lineRenderers[lineRenderers.Count - 1].GetComponent<UILineRenderer>();
		//The last point we added
		Vector2 point = component.Points[currentPointIndex - 1];

		//Set the remaining points to the last point we added
		for (int q = currentPointIndex; q < maxPointCount; q++) {
			component.Points[q] = point;
		}
	}

	private void createNewLineRenderer() {
		//Create the game object
		GameObject lineRendererObj = new GameObject("LineRenderer", new System.Type[] { typeof(UILineRenderer) });
		lineRendererObj.transform.SetParent(gameObject.transform);
		lineRendererObj.GetComponent<RectTransform>().position = new Vector2(0.0f, 0.0f);
		UILineRenderer lineRenderer = lineRendererObj.GetComponent<UILineRenderer>();

		//Set properties
		lineRenderer.Points = new Vector2[maxPointCount];
		lineRenderer.lineList = false;
		lineRenderer.lineThickness = 1.0f;
		lineRenderer.BezierMode = UILineRenderer.BezierType.None;
		lineRenderer.ImproveResolution = ResolutionMode.None;

		//Add it to the list and reset currentLineIndex while we're here
		lineRenderers.Add(lineRendererObj);
		currentPointIndex = 0;
	}
}
