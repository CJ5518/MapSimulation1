//Re-Written by Carson Rueber
//Entirely untested as of now

using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections;
using System.Collections.Generic;


public class BigLineListRenderer {
	//Our game object, all the lineRenderers get parented to it, has a RectTransform
	public GameObject gameObject;
	public List<GameObject> lineRenderers;
	//The maximum number of points before needing to make a new lineRenderer
	public const int maxPointCount = 10666;
	//Index of the next point
	private int currentPointIndex = 0;
	//Whether or not there is a previous element we need to copy
	private bool arePreviousElements = false;

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

		if (arePreviousElements) {
			lineRenderer.Points[currentPointIndex] = getPreviousElement();
		}
		else { //Generally just the first element of a shape
			arePreviousElements = true;
		}

		//Set the point
		lineRenderer.Points[currentPointIndex] = point;
		currentPointIndex++;
	}

	//Call this function when you're finished with a shape
	public void finishShape() {
		arePreviousElements = false;
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
