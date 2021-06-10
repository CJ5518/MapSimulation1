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

//Test class
public class MapTest : MonoBehaviour {
	//Our shape file renderer
	ShapeFileRenderer shapeFileRenderer;

	//Controls the thickness of the lines of the shape file renderer
	Slider thicknessSlider;

	//Texture testing things
	Texture2D finalTexture = null;
	//The background image
	MovableRawImage movableRawImage;

	PopulationRasterHandler popRaster;

	Simulation simulation;

	void Start() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		Application.targetFrameRate = 60;

		string shapeFilePath = @"C:\Users\carso\Desktop\DataPart2\USA_Final.shp";

		//Set up render space
		Projection.setRenderSpaceByShapeFile(shapeFilePath);

		//Set up the shape file renderer
		shapeFileRenderer = new ShapeFileRenderer(
			shapeFilePath, GameObject.Find("Canvas/Background").transform
		);

		//Set up thickness slider
		thicknessSlider = GameObject.Find("Canvas/ThicknessSlider").GetComponent<Slider>();
		thicknessSlider.value = 1.0f;
		thicknessSlider.onValueChanged.AddListener(OnThicknessSliderValueChanged);

		movableRawImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();

		//Generate the texture

		//Texture scaling factor
		int pixelSize = 4;
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
		


		//Init GDAL
		Gdal.AllRegister();
		Gdal.SetCacheMax((int)System.Math.Pow(2, 30));
		Osr.SetPROJSearchPath(Application.streamingAssetsPath + "\\proj");

		popRaster = new PopulationRasterHandler(PopulationRasterHandler.PopulationType.Youth15To24);
		popRaster.preprocessData(pixelSize);
		finalTexture = popRaster.loadToTexture(finalTexture.width, finalTexture.height);
		finalTexture.filterMode = FilterMode.Point;
		finalTexture.Apply();

		simulation = new Simulation(new Texture2D[] { finalTexture });
		movableRawImage.texture = simulation.drawTexture;

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds to run the start function");
	}


	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}

	float lastSimTime = 0.0f;
	private void Update() {

		//Tick the simulation every half second or so
		if (Time.realtimeSinceStartup - lastSimTime >= 0.5f) {
			lastSimTime = Time.realtimeSinceStartup;
			//simulation.tickSimulation();
		}
		simulation.tickSimulation();

		//Kill cells on click
		if (Input.GetMouseButtonDown(0)) {
			Vector2 pixel = movableRawImage.getPixelFromScreenCoord(Input.mousePosition);
			int index = simulation.coordToIndex(pixel);


			Simulation.Cell cell = simulation.readCells[index];
			cell.health = 0.0f;
			simulation.readCells[index] = cell;
		}

	}

	void OnDestroy() {
		simulation.deleteNativeArrays();
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