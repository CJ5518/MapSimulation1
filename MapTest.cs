//By Carson Rueber
//The only file to survive the purge

using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Unity.Jobs;
using Unity.Collections;
using OSGeo.OSR;
using OSGeo.GDAL;
using OSGeo.OGR;

/* TODO:
 * Use the Gdal.Warp() function instead of using it outside of the environment
 * Get it to work lmao
 * https://gis.stackexchange.com/questions/132149/down-sample-raster-map-with-average-algorithm
 * 
 */

//Test class
public class MapTest : MonoBehaviour {
	//Our shape file renderer
	ShapeFileRenderer shapeFileRenderer;

	//Controls the thickness of the lines of the shape file renderer
	Slider thicknessSlider;

	//Texture testing things
	Texture2D finalTexture = null;
	bool wantDraw = true;
	//The background image
	MovableRawImage movableRawImage;

	PopulationRasterHandler popRaster;

	Simulation simulation;

	void Start() {
		Application.targetFrameRate = 60;

		//Set up the shape file renderer
		shapeFileRenderer = new ShapeFileRenderer(
			@"C:\Users\carso\Desktop\DataPart2\USA_Final.shp", GameObject.Find("Canvas/Background").transform
		);

		//Set up thickness slider
		thicknessSlider = GameObject.Find("Canvas/ThicknessSlider").GetComponent<Slider>();
		thicknessSlider.value = 1.0f;
		thicknessSlider.onValueChanged.AddListener(OnThicknessSliderValueChanged);

		movableRawImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();

		//Generate the texture
		//Good to keep it at the screen aspect ratio
		double startTime = Time.realtimeSinceStartupAsDouble;

		//Texture scaling factor
		int pixelSize = 1;
		finalTexture = new Texture2D(1920 / pixelSize, 1080 / pixelSize, TextureFormat.RGBA32, false);
		for (int x = 0; x < finalTexture.width; x++) {
			for (int y = 0; y < finalTexture.height; y++) {
				bool isInShape = IsPointInPolygon(
					shapeFileRenderer.renderShapes[2], //FIX: Bit of a hack
					new Vector2(x * pixelSize, y * pixelSize)
				);
				finalTexture.SetPixel(x, y, isInShape ? Color.gray : Color.Lerp(Color.clear, Color.black, 0.1f));
			}
		}
		finalTexture.filterMode = FilterMode.Point;
		finalTexture.Apply();
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds to create the texture");


		//Init GDAL
		Gdal.AllRegister();
		Gdal.SetCacheMax((int)System.Math.Pow(2, 30));
		Osr.SetPROJSearchPath(Application.streamingAssetsPath + "\\proj");

		popRaster = new PopulationRasterHandler();
		popRaster.preprocessData(finalTexture.width, finalTexture.height, shapeFileRenderer);
		finalTexture = popRaster.loadToTexture(finalTexture.width, finalTexture.height, shapeFileRenderer);
		finalTexture.filterMode = FilterMode.Point;


		finalTexture.filterMode = FilterMode.Point;

		//simulation = new Simulation();
		movableRawImage.texture = finalTexture;
	}


	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}
	void OnGUI() {
		if (finalTexture != null && Event.current.type.Equals(EventType.Repaint) && wantDraw) {
			//Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), finalTexture);
		}
	}

	private void Update() {
		//Texture testing to hide/show the texture
		if (Input.GetKeyDown(KeyCode.Q)) {
			wantDraw = !wantDraw;
		}
		Vector2 coord = movableRawImage.getLocalPositionInRectangle(Input.mousePosition);
		//Fix this too, same hack as above
		GameObject.Find("Canvas/Text").GetComponent<Text>().text = 
			IsPointInPolygon(shapeFileRenderer.renderShapes[2], coord) ? "inside" : "outside";
		
		if (Input.GetMouseButtonDown(0)) {
			Vector2 projectedCoords = shapeFileRenderer.renderSpaceToProjection(coord);
			Vector2Double worldCoords = Projection.projectionToLatLongs((Vector2Double)projectedCoords);

			Debug.Log(coord);
			Debug.Log(popRaster.worldToRasterSpace(worldCoords, popRaster.dataset));
		}

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