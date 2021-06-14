//By Carson Rueber

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
public class Main : MonoBehaviour {
	//Texture scaling factor
	int pixelSize = 4;


	//Our shape file renderer
	ShapeFileRenderer shapeFileRenderer;

	//Controls the thickness of the lines of the shape file renderer
	Slider thicknessSlider;

	//The background image
	MovableRawImage backgroundMovableImage;

	Simulation simulation;

	Text editInfoLabel;

	void Start() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		Application.targetFrameRate = 60;

		string shapeFilePath = @"F:\Data\shp\USA_Reprojected.shp";

		//Set up render space
		Projection.setRenderSpaceByShapeFile(shapeFilePath);

		//Set up the shape file renderer
		shapeFileRenderer = new ShapeFileRenderer(
			shapeFilePath, GameObject.Find("Canvas/Background").transform
		);

		//Find some unity components
		backgroundMovableImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();
		editInfoLabel = GameObject.Find("Canvas/EditInfoLabel").GetComponent<Text>();
		thicknessSlider = GameObject.Find("Canvas/ThicknessSlider").GetComponent<Slider>();

		//Set up said components
		thicknessSlider.value = 1.0f;
		thicknessSlider.onValueChanged.AddListener(OnThicknessSliderValueChanged);

		//Init GDAL
		Gdal.AllRegister();
		Gdal.SetCacheMax((int)System.Math.Pow(2, 30));
		Osr.SetPROJSearchPath(Application.streamingAssetsPath + "\\proj");

		Texture2D[] populationTextures = new Texture2D[(int)PopulationRasterHandler.PopulationType.PopulationTypeCount];

		int width = Screen.width / pixelSize;
		int height = Screen.height / pixelSize;

		for (int q = 0; q < populationTextures.Length; q++) {
			RasterHandler rasterHandler = new PopulationRasterHandler((PopulationRasterHandler.PopulationType)q);
			populationTextures[q] = rasterHandler.loadToTexture(width, height);
			populationTextures[q].Apply();
		}
		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			new Texture2D[] { populationTextures[(int)PopulationRasterHandler.PopulationType.FullPopulation]}
		);

		backgroundMovableImage.texture = simulation.drawTexture;

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + " seconds to run the start function");
	}


	void OnThicknessSliderValueChanged(float value) {
		shapeFileRenderer.bigLineListRenderer.setLineThickness(value);
	}

	//Some vars for testing purposes
	float lastSimTime = -100.0f;
	float lastStatsTime = -100.0f;
	bool autoPlay = false;
	float totalSusceptible, totalInfected, totalRecovered, totalDead = 0.0f;
	private void Update() {

		//Tick the simulation every now and then
		if (Time.realtimeSinceStartup - lastSimTime >= 0.1f && autoPlay) {
			lastSimTime = Time.realtimeSinceStartup;
			simulation.tickSimulation();
		}

		if (Input.GetKeyDown(KeyCode.Space)) {
			simulation.tickSimulation();
		}
		if (Input.GetKeyDown(KeyCode.Q)) autoPlay = !autoPlay;

		Vector2 pixel = backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (simulation.cellIsValid(index)) {
			//Report statistics

			Simulation.Cell cell = simulation.readCells[index];

			//Gather statistics for the entire thing
			if (Time.realtimeSinceStartup - lastStatsTime >= 0.01f) {
				lastStatsTime = Time.realtimeSinceStartup;
				totalSusceptible = 0.0f;
				totalInfected = 0.0f;
				totalRecovered = 0.0f;
				totalDead = 0.0f;
				for (int q = 0; q < simulation.readCells.Length; q++) {
					Simulation.Cell readCell = simulation.readCells[q];
					totalSusceptible += readCell.susceptible;
					totalInfected += readCell.infected;
					totalRecovered += readCell.recovered;
					totalDead += readCell.dead;
				}
			}

			//Set the string to the statistics
			string finalString =
				Mathf.FloorToInt(cell.susceptible + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.infected + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.recovered + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.dead + 0.5f) + "\n" +
				"Totals:" + "\n" +
				Mathf.FloorToInt(totalSusceptible + 0.5f) + "\n" +
				Mathf.FloorToInt(totalInfected + 0.5f) + "\n" +
				Mathf.FloorToInt(totalRecovered + 0.5f) + "\n" +
				Mathf.FloorToInt(totalDead + 0.5f) + "\n";
			editInfoLabel.text = finalString;

			//Kill cells on click
			if (Input.GetMouseButtonDown(0)) {
				cell.infected++;
				cell.susceptible--;
				simulation.readCells[index] = cell;
			}
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