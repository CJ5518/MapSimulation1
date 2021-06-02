//The only file to survive the purge

using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
//using OSGeo.OGR;

public class MapTest : MonoBehaviour {

	ShapeFileRenderer shapeFileRenderer;

	Slider thicknessSlider;

	Texture2D finalTexture = null;
	bool wantDraw = true;

	MovableRawImage movableRawImage;
	void Start() {
		Application.targetFrameRate = 60;

		shapeFileRenderer = new ShapeFileRenderer(
			@"C:\Users\carso\Desktop\DataPart2\USA2.shp", GameObject.Find("Canvas/Background").transform
		);

		thicknessSlider = GameObject.Find("Canvas/ThicknessSlider").GetComponent<Slider>();
		thicknessSlider.value = 1.0f;
		thicknessSlider.onValueChanged.AddListener(OnThicknessSliderValueChanged);

		movableRawImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();

		//Generate the texture
		//Good to keep it at the screen aspect ratio
		double startTime = Time.realtimeSinceStartupAsDouble;

		int factor = 1;
		finalTexture = new Texture2D(1920 / factor, 1080 / factor, TextureFormat.RGBA32, false);
		for (int x = 0; x < finalTexture.width; x++) {
			for (int y = 0; y < finalTexture.height; y++) {
				bool isInShape = IsPointInPolygon(shapeFileRenderer.projectedShapes[3], new Vector2(x * factor, y * factor));


				finalTexture.SetPixel(x, y, isInShape ? Color.white : Color.clear);
			}
		}
		finalTexture.filterMode = FilterMode.Point;
		finalTexture.Apply();
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " time");
	}

	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}
	void OnGUI() {
		if (finalTexture != null && Event.current.type.Equals(EventType.Repaint) && wantDraw) {
			Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), finalTexture);
		}
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Q)) {
			wantDraw = !wantDraw;
		}
		Vector2 coord = movableRawImage.getLocalPositionInRectangle(Input.mousePosition);
		//Vector2 coord = Input.mousePosition;
		GameObject.Find("Canvas/Text").GetComponent<Text>().text = IsPointInPolygon(shapeFileRenderer.projectedShapes[3], coord) ? "inside" : "outside";
	}

	//https://stackoverflow.com/a/14998816
	public static bool IsPointInPolygon(Vector2[] polygon, Vector2 testPoint) {
		bool result = false;
		int j = polygon.Length - 1;
		for (int i = 0; i < polygon.Length; i++) {
			if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y) {
				if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x) {
					result = !result;
				}
			}
			j = i;
		}
		return result;
	}
}