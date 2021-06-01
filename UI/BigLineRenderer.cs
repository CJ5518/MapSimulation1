// Decompiled with JetBrains decompiler
// Type: BigLineRenderer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BigLineRenderer {
	public GameObject gameObject;
	public List<GameObject> lineRenderers;
	public const int maxPointCount = 5000;
	private int currentLineIndex;

	public BigLineRenderer() {
		this.lineRenderers = new List<GameObject>();
		this.gameObject = new GameObject(nameof(BigLineRenderer), new System.Type[1]
		{
	  typeof (RectTransform)
		});
		this.createNewLineRenderer();
	}

	public void AddPoint(Vector2 line) {
		if (this.currentLineIndex >= maxPointCount)
			this.createNewLineRenderer();
		UILineRenderer component1 = this.lineRenderers[this.lineRenderers.Count - 1].GetComponent<UILineRenderer>();
		if (this.lineRenderers.Count > 1 && this.currentLineIndex == 0) {
			UILineRenderer component2 = this.lineRenderers[this.lineRenderers.Count - 2].GetComponent<UILineRenderer>();
			component1.Points[0] = component2.Points[maxPointCount - 1];
			++this.currentLineIndex;
		}
		component1.Points[this.currentLineIndex] = line;
		++this.currentLineIndex;
	}

	public void finishAdding() {
		UILineRenderer component = this.lineRenderers[this.lineRenderers.Count - 1].GetComponent<UILineRenderer>();
		Vector2 point = component.Points[this.currentLineIndex - 1];
		for (int currentLineIndex = this.currentLineIndex; currentLineIndex < maxPointCount; ++currentLineIndex)
			component.Points[currentLineIndex] = point;
	}

	private void createNewLineRenderer() {
		GameObject lineRendererObj = new GameObject("LineRenderer", new System.Type[] { typeof(UILineRenderer) });
		lineRendererObj.transform.SetParent(gameObject.transform);
		lineRendererObj.GetComponent<RectTransform>().position = new Vector2(0.0f, 0.0f);
		UILineRenderer lineRenderer = lineRendererObj.GetComponent<UILineRenderer>();

		lineRenderer.Points = new Vector2[maxPointCount];
		lineRenderer.lineList = false;
		lineRenderer.lineThickness = 1.0f;
		lineRenderer.BezierMode = UILineRenderer.BezierType.None;
		lineRenderer.ImproveResolution = ResolutionMode.None;

		lineRenderers.Add(lineRendererObj);
		currentLineIndex = 0;
	}
}
