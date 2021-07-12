//By Carson Rueber

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;
using Unity.Jobs;
using Unity.Collections;
using System.Collections;
using OSGeo.OSR;
using OSGeo.GDAL;
using OSGeo.OGR;
using NLua;
using SimpleFileBrowser;

//Test class
public class Main : MonoBehaviour {
	//Texture scaling factor
	int pixelSize = 4;

	//The background image
	MovableRawImage backgroundMovableImage;

	Simulation simulation;

	Text statisticsEditLabel;

	//Slider/button combos
	Slider alphaSlider;
	Text alphaText;

	Slider gammaSlider;
	Text gammaText;

	Slider betaSlider;
	Text betaText;

	Slider spreadRateSlider;
	Text spreadRateText;

	Text r0Text;

	//Toggles
	Toggle drawInfectedToggle;
	Toggle drawRecoveredToggle;
	Toggle drawDeadToggle;

	Toggle drawProportionToggle;

	bool loadedSimulation = false;

	void Start() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		Application.targetFrameRate = 60;

		//Init GDAL
		Gdal.AllRegister();
		Gdal.SetCacheMax((int)System.Math.Pow(2, 30));
		Osr.SetPROJSearchPath(Application.streamingAssetsPath + "\\proj");

		//Set up render space
		string shapeFilePath = Application.streamingAssetsPath + "/USA_Reprojected.shp";

		Projection.setRenderSpaceByShapeFile(shapeFilePath);
		Projection.pixelSize = pixelSize;

		//Init the Lua singleton
		LuaSingleton.initLua();


		//Find some unity components
		backgroundMovableImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();
		statisticsEditLabel = GameObject.Find("Canvas/StatisticsEditLabel").GetComponent<Text>();


		r0Text = GameObject.Find("Canvas/R0Text").GetComponent<Text>();

		alphaSlider = GameObject.Find("Canvas/AlphaSlider").GetComponent<Slider>();
		alphaText = GameObject.Find("Canvas/AlphaText").GetComponent<Text>();

		gammaSlider = GameObject.Find("Canvas/GammaSlider").GetComponent<Slider>();
		gammaText = GameObject.Find("Canvas/GammaText").GetComponent<Text>();

		betaSlider = GameObject.Find("Canvas/BetaSlider").GetComponent<Slider>();
		betaText = GameObject.Find("Canvas/BetaText").GetComponent<Text>();

		spreadRateSlider = GameObject.Find("Canvas/SpreadRateSlider").GetComponent<Slider>();
		spreadRateText = GameObject.Find("Canvas/SpreadRateText").GetComponent<Text>();

		drawInfectedToggle = GameObject.Find("Canvas/DrawInfectedToggle").GetComponent<Toggle>();
		drawRecoveredToggle = GameObject.Find("Canvas/DrawRecoveredToggle").GetComponent<Toggle>();
		drawDeadToggle = GameObject.Find("Canvas/DrawDeadToggle").GetComponent<Toggle>();

		drawProportionToggle = GameObject.Find("Canvas/DrawProportionToggle").GetComponent<Toggle>();


		StartCoroutine("loadSimulation");

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to run the Main.cs start function");
	}

	IEnumerator loadSimulation() {
		//Get some Lua functions
		LuaFunction needDataFolder = (LuaFunction)LuaSingleton.lua["RasterUtilities.needDataFolder"];
		LuaFunction createDataDirectoryStructure = (LuaFunction)
			LuaSingleton.lua["RasterUtilities.createDataDirectoryStructure"];

		if ((bool)needDataFolder.Call()[0]) {
			do {
				Debug.Log("In the Loop");
				yield return FileBrowser.WaitForLoadDialog(
					FileBrowser.PickMode.Folders, false, null, null, "Select Data Folder"
				);
			} while (!FileBrowser.Success);

			LuaSingleton.lua["RasterDataFolderLocation"] = FileBrowser.Result[0];

			//This should probably be outside this part, as the above is for when we need a data folder,
			//but this is more for when the data folder needs stuff in it
			createDataDirectoryStructure.Call();
		}


		int width = Screen.width / pixelSize;
		int height = Screen.height / pixelSize;

		double preprocessTime = 0;
		double textureLoadTime = 0;

		//Load in the population data
		Texture2D[] populationTextures = new Texture2D[(int)Population.PopulationCount];

		for (int q = 0; q < populationTextures.Length; q++) {
			RasterHandler rasterHandler = new RasterHandler(RasterType.Population, q);

			double localStartTime = Time.realtimeSinceStartupAsDouble;

			rasterHandler.preprocessData();

			preprocessTime += Time.realtimeSinceStartupAsDouble - localStartTime;
			localStartTime = Time.realtimeSinceStartupAsDouble;

			populationTextures[q] = rasterHandler.loadToTexture(width, height);

			textureLoadTime += Time.realtimeSinceStartupAsDouble - localStartTime;

			populationTextures[q].Apply();

			yield return null;
		}
		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			new Texture2D[] { populationTextures[(int)Population.FullPopulation] }
		);

		backgroundMovableImage.texture = simulation.drawTexture;

		//Log time
		Debug.Log("Preprocess time: " + preprocessTime);
		Debug.Log("Texture load time: " + textureLoadTime);

		loadedSimulation = true;
	}
	

	//Some vars for testing purposes

	//Doing things every x seconds
	float lastSimTime = -100.0f;
	float lastStatsTime = -100.0f;
	bool autoPlay = false;
	//The demographic we are currently looking at statistics for
	int targetDemographic = (int)Population.FullPopulation;

	private unsafe void Update() {
		if (!loadedSimulation) return;
		//Sliders and buttons

		//r0
		r0Text.text = "r0: " + (simulation.data.beta / simulation.data.gamma).ToString("f2");

		//Alpha, beta, gamma
		alphaText.text = "α: " + alphaSlider.value.ToString("f2");
		simulation.data.alpha = alphaSlider.value;

		betaText.text = "β: " + betaSlider.value.ToString("f2");
		simulation.data.beta = betaSlider.value;

		gammaText.text = "γ: " + gammaSlider.value.ToString("f2");
		simulation.data.gamma = gammaSlider.value;

		spreadRateText.text = "sr: " + spreadRateSlider.value.ToString("f2");
		simulation.data.spreadRate = spreadRateSlider.value;

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
			targetDemographic = (int)Population.PopulationCount - 1;
		if (targetDemographic >= (int)Population.PopulationCount)
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
				//Can't mess with the cells if the simulation is going
				if (simulation.simulationIsRunning)
					simulation.endTick();
				cell.infected[targetDemographic]+= 80.0f;
				cell.susceptible[targetDemographic]-= 80.0f;
				simulation.readCells[index] = cell;
			}
		}

		//Step once on space pressed
		if (Input.GetKeyDown(KeyCode.Space) && !autoPlay) {
			simulation.tickSimulation();
		}

		//Tick the simulation every now and then
		if (Time.realtimeSinceStartup - lastSimTime >= 0.05f && autoPlay) {
			lastSimTime = Time.realtimeSinceStartup;

			if (simulation.simulationIsRunning)
				simulation.endTick();

			simulation.beginTick();
		}

		//Toggle autoplay on q pressed
		if (Input.GetKeyDown(KeyCode.Q)) autoPlay = !autoPlay;
	}

	void OnDestroy() {
		if (simulation.simulationIsRunning) simulation.endTick();
		simulation.deleteNativeArrays();
	}

	//Updates the statistics label based on the pixel the mouse is over, and targetDemographic
	int lastIndex = -1;
	unsafe void updateStatisticsLabel() {
		float totalSusceptible = 0.0f;
		float totalInfected = 0.0f;
		float totalRecovered = 0.0f;
		float totalExposed = 0.0f;

		//Pixel coord on the draw texture
		Vector2 pixel = backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (!simulation.cellIsValid(index) && simulation.cellIsValid(lastIndex))
			index = lastIndex;

		if (simulation.cellIsValid(index)) {
			lastIndex = index;
			Simulation.Cell cell = simulation.readCells[index];

			//Gather statistics for the entire thing
			if (Time.realtimeSinceStartup - lastStatsTime >= 0.08f) {
				lastStatsTime = Time.realtimeSinceStartup;
				totalSusceptible = 0;
				totalInfected = 0;
				totalRecovered = 0;
				totalExposed = 0;
				for (int q = 0; q < simulation.readCells.Length; q++) {
					Simulation.Cell readCell = simulation.readCells[q];
					totalSusceptible += readCell.susceptible[targetDemographic];
					totalInfected += readCell.infected[targetDemographic];
					totalRecovered += readCell.recovered[targetDemographic];
					totalExposed += readCell.exposed[targetDemographic];
				}

				//Set the string to the statistics
				string finalString =
					((Population)targetDemographic).ToString() + "\n" +
					cell.susceptible[targetDemographic].ToString("F3") + "\n" +
					cell.infected[targetDemographic].ToString("F3") + "\n" +
					cell.recovered[targetDemographic].ToString("F3") + "\n" +
					cell.exposed[targetDemographic].ToString("F3") + "\n" +
					"Totals:" + "\n" +
					totalSusceptible.ToString("F3") + "\n" +
					totalInfected.ToString("F3") + "\n" +
					totalRecovered.ToString("F3") + "\n" +
					totalExposed.ToString("F3") + "\n";
				statisticsEditLabel.text = finalString;
			}
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