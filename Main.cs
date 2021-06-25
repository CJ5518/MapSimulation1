//By Carson Rueber

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

	//The background image
	MovableRawImage backgroundMovableImage;

	Simulation simulation;

	Text statisticsEditLabel;

	//Slider/button combos
	Slider r0Slider;
	Text r0Text;

	Slider mortalitySlider;
	Text mortalityText;

	//Toggles
	Toggle drawInfectedToggle;
	Toggle drawRecoveredToggle;
	Toggle drawDeadToggle;

	Toggle drawProportionToggle;


	//The demographic we are currently looking at statistics for
	int targetDemographic = (int)PopulationRasterType.FullPopulation;

	void Start() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		Application.targetFrameRate = 60;

		string shapeFilePath = @"F:\Data\shp\USA_Reprojected.shp";

		//Set up render space
		Projection.setRenderSpaceByShapeFile(shapeFilePath);

		//Find some unity components
		backgroundMovableImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();
		statisticsEditLabel = GameObject.Find("Canvas/StatisticsEditLabel").GetComponent<Text>();

		r0Slider = GameObject.Find("Canvas/R0Slider").GetComponent<Slider>();
		r0Text = GameObject.Find("Canvas/R0Text").GetComponent<Text>();

		mortalitySlider = GameObject.Find("Canvas/MortalitySlider").GetComponent<Slider>();
		mortalityText = GameObject.Find("Canvas/MortalityText").GetComponent<Text>();

		drawInfectedToggle = GameObject.Find("Canvas/DrawInfectedToggle").GetComponent<Toggle>();
		drawRecoveredToggle = GameObject.Find("Canvas/DrawRecoveredToggle").GetComponent<Toggle>();
		drawDeadToggle = GameObject.Find("Canvas/DrawDeadToggle").GetComponent<Toggle>();

		drawProportionToggle = GameObject.Find("Canvas/DrawProportionToggle").GetComponent<Toggle>();


		//Init GDAL
		Gdal.AllRegister();
		Gdal.SetCacheMax((int)System.Math.Pow(2, 30));
		Osr.SetPROJSearchPath(Application.streamingAssetsPath + "\\proj");

		int width = Screen.width / pixelSize;
		int height = Screen.height / pixelSize;

		double preprocessTime = 0;
		double textureLoadTime = 0;

		//Load in the population data
		Texture2D[] populationTextures = new Texture2D[(int)PopulationRasterType.PopulationTypeCount];

		for (int q = 0; q < populationTextures.Length; q++) {
			RasterHandler rasterHandler = new PopulationRasterHandler((PopulationRasterType)q);

			double localStartTime = Time.realtimeSinceStartupAsDouble;

			rasterHandler.preprocessData(pixelSize);

			preprocessTime += Time.realtimeSinceStartupAsDouble - localStartTime;
			localStartTime = Time.realtimeSinceStartupAsDouble;

			populationTextures[q] = rasterHandler.loadToTexture(width, height);

			textureLoadTime += Time.realtimeSinceStartupAsDouble - localStartTime;

			populationTextures[q].Apply();
		}
		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			new Texture2D[] { populationTextures[(int)PopulationRasterType.FullPopulation]}
		);

		backgroundMovableImage.texture = simulation.drawTexture;

		//Log time
		Debug.Log("Preprocess time: " + preprocessTime);
		Debug.Log("Texture load time: " + textureLoadTime);

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) + 
			" seconds to run the Main.cs start function");
	}

	//Some vars for testing purposes
	float lastSimTime = -100.0f;
	float lastStatsTime = -100.0f;
	bool autoPlay = false;
	float totalSusceptible, totalInfected, totalRecovered, totalDead = 0.0f;
	private unsafe void Update() {
		//Sliders and buttons

		//r0
		r0Text.text = "r0: " + r0Slider.value.ToString("f2");
		simulation.data.r0 = r0Slider.value;

		//Mortality
		mortalityText.text = "Mortality Rate: " + mortalitySlider.value.ToString("f2");
		simulation.data.mortalityRate = mortalitySlider.value;

		//Draw toggles
		simulation.data.drawRecovered = drawRecoveredToggle.isOn;
		simulation.data.drawInfected = drawInfectedToggle.isOn;
		simulation.data.drawDead = drawDeadToggle.isOn;

		simulation.data.drawProportion = drawProportionToggle.isOn;


		//Change the targetDemographic on keypress
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			targetDemographic--;
		if (Input.GetKeyDown(KeyCode.RightArrow))
			targetDemographic++;

		//Make sure targetDemographic is in range
		if (targetDemographic < 0)
			targetDemographic = (int)PopulationRasterType.PopulationTypeCount - 1;
		if (targetDemographic >= (int)PopulationRasterType.PopulationTypeCount)
			targetDemographic = 0;

		simulation.data.drawDemographic = targetDemographic;

		updateStatisticsLabel();

		//Pixel coord on the draw texture
		Vector2 pixel = backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (simulation.cellIsValid(index)) {

			Simulation.Cell cell = simulation.readCells[index];

			//Kill cells on click
			if (Input.GetMouseButtonDown(0) && EventSystem.current.currentSelectedGameObject == null) {
				Debug.Log(cell.susceptible[targetDemographic]);
				cell.infected[targetDemographic]++;
				cell.susceptible[targetDemographic]--;
				simulation.readCells[index] = cell;
			}
		}

		//Step once on space pressed
		if (Input.GetKeyDown(KeyCode.Space)) {
			simulation.tickSimulation();
		}
		//Toggle autoplay on q pressed
		if (Input.GetKeyDown(KeyCode.Q)) autoPlay = !autoPlay;

		//Tick the simulation every now and then
		if (Time.realtimeSinceStartup - lastSimTime >= 0.1f && autoPlay) {
			lastSimTime = Time.realtimeSinceStartup;
			simulation.tickSimulation();
		}
	}

	void OnDestroy() {
		simulation.deleteNativeArrays();
	}

	//Updates the statistics label based on the pixel the mouse is over, and targetDemographic
	unsafe void updateStatisticsLabel() {
		//Pixel coord on the draw texture
		Vector2 pixel = backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (simulation.cellIsValid(index)) {
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
					totalSusceptible += readCell.susceptible[targetDemographic];
					totalInfected += readCell.infected[targetDemographic];
					totalRecovered += readCell.recovered[targetDemographic];
					totalDead += readCell.dead[targetDemographic];
				}
			}
			//Set the string to the statistics
			string finalString =
				((PopulationRasterType)targetDemographic).ToString() + "\n" +
				Mathf.FloorToInt(cell.susceptible[targetDemographic] + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.infected[targetDemographic] + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.recovered[targetDemographic] + 0.5f) + "\n" +
				Mathf.FloorToInt(cell.dead[targetDemographic] + 0.5f) + "\n" +
				"Totals:" + "\n" +
				Mathf.FloorToInt(totalSusceptible + 0.5f) + "\n" +
				Mathf.FloorToInt(totalInfected + 0.5f) + "\n" +
				Mathf.FloorToInt(totalRecovered + 0.5f) + "\n" +
				Mathf.FloorToInt(totalDead + 0.5f) + "\n";
			statisticsEditLabel.text = finalString;
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