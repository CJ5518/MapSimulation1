//By Carson Rueber

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using OSGeo.OSR;
using OSGeo.GDAL;
using NLua;
using SimpleFileBrowser;

//Main class
public class Main : MonoBehaviour {
	//Pixel size for the texture
	int pixelSize = 4;
	const int framerate = 60;

	const float simulationTicksPerSecond = 20.0f;
	const float statisticsUpdatesPerSecond = 20.0f;

	//The background image
	MovableRawImage backgroundMovableImage;

	Simulation simulation;

	Text statisticsEditLabel;

	//Slider/text combos
	SliderTextCombo alphaSliderText;
	SliderTextCombo gammaSliderText;
	SliderTextCombo spreadRateSliderText;
	SliderTextCombo sigmaSliderText;
	SliderTextCombo deltaSliderText;

	Slider contactProbabilitySlider;
	Slider infectionRateSlider;
	Text betaText;

	Text r0Text;

	//Toggles
	Toggle drawInfectedToggle;
	Toggle drawRecoveredToggle;
	Toggle drawDeadToggle;
	Toggle moveZombiesToggle;

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

		alphaSliderText = GameObject.Find("Canvas/AlphaSliderText").GetComponent<SliderTextCombo>();
		gammaSliderText = GameObject.Find("Canvas/GammaSliderText").GetComponent<SliderTextCombo>();
		sigmaSliderText = GameObject.Find("Canvas/SigmaSliderText").GetComponent<SliderTextCombo>();
		deltaSliderText = GameObject.Find("Canvas/DeltaSliderText").GetComponent<SliderTextCombo>();
		spreadRateSliderText = GameObject.Find("Canvas/SpreadRateSliderText").GetComponent<SliderTextCombo>();

		infectionRateSlider = GameObject.Find("Canvas/InfectionRateSlider").GetComponent<Slider>();
		contactProbabilitySlider = GameObject.Find("Canvas/ContactProbabilitySlider").GetComponent<Slider>();
		betaText = GameObject.Find("Canvas/BetaText").GetComponent<Text>();

		drawInfectedToggle = GameObject.Find("Canvas/DrawInfectedToggle").GetComponent<Toggle>();
		drawRecoveredToggle = GameObject.Find("Canvas/DrawRecoveredToggle").GetComponent<Toggle>();
		drawDeadToggle = GameObject.Find("Canvas/DrawDeadToggle").GetComponent<Toggle>();

		drawProportionToggle = GameObject.Find("Canvas/DrawProportionToggle").GetComponent<Toggle>();
		moveZombiesToggle = GameObject.Find("Canvas/MoveZombiesToggle").GetComponent<Toggle>();


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

		//Alpha, beta, gamma, etc.
		simulation.data.beta = infectionRateSlider.value * contactProbabilitySlider.value;

		simulation.data.alpha = alphaSliderText.slider.value;
		simulation.data.gamma = gammaSliderText.slider.value;
		simulation.data.sigma = sigmaSliderText.slider.value;
		simulation.data.delta = deltaSliderText.slider.value;
		simulation.data.spreadRate = spreadRateSliderText.slider.value;

		//Toggles
		simulation.data.drawRecovered = drawRecoveredToggle.isOn;
		simulation.data.drawInfected = drawInfectedToggle.isOn;
		simulation.data.drawDead = drawDeadToggle.isOn;

		simulation.data.drawProportion = drawProportionToggle.isOn;
		simulation.data.moveZombies = moveZombiesToggle.isOn;


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
				simulation.endTick();
				cell.infected[targetDemographic]+= 1.0f;
				cell.susceptible[targetDemographic]-= 1.0f;
				simulation.readCells[index] = cell;
			}
		}

		//Step once on space pressed
		if (Input.GetKeyDown(KeyCode.Space) && !autoPlay) {
			simulation.tickSimulation();
		}

		//Tick the simulation every now and then
		if (Time.realtimeSinceStartup - lastSimTime >= 1.0f / simulationTicksPerSecond  && autoPlay) {
			lastSimTime = Time.realtimeSinceStartup;

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
		float totalVaccinated = 0.0f;
		double totalPeople = 0.0;

		//Pixel coord on the draw texture
		Vector2 pixel = backgroundMovableImage.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (!simulation.cellIsValid(index) && simulation.cellIsValid(lastIndex))
			index = lastIndex;

		if (simulation.cellIsValid(index)) {
			lastIndex = index;
			Simulation.Cell cell = simulation.readCells[index];

			//Gather statistics for the entire thing
			if (Time.realtimeSinceStartup - lastStatsTime >= 1.0f / statisticsUpdatesPerSecond) {
				lastStatsTime = Time.realtimeSinceStartup;
				totalSusceptible = 0;
				totalInfected = 0;
				totalRecovered = 0;
				totalExposed = 0;
				totalPeople = 0;
				totalVaccinated = 0;
				for (int q = 0; q < simulation.readCells.Length; q++) {
					Simulation.Cell readCell = simulation.readCells[q];
					totalSusceptible += readCell.susceptible[targetDemographic];
					totalInfected += readCell.infected[targetDemographic];
					totalRecovered += readCell.recovered[targetDemographic];
					totalExposed += readCell.exposed[targetDemographic];
					totalVaccinated += readCell.vaccinated[targetDemographic];
					totalPeople += (double)readCell.numberOfPeople[targetDemographic];
				}

				//Set the string to the statistics
				string finalString =
					((Population)targetDemographic).ToString() + "\n" +
					cell.susceptible[targetDemographic].ToString("F3") + "\n" +
					cell.vaccinated[targetDemographic].ToString("F3") + "\n" +
					cell.infected[targetDemographic].ToString("F3") + "\n" +
					cell.recovered[targetDemographic].ToString("F3") + "\n" +
					cell.exposed[targetDemographic].ToString("F3") + "\n" +
					"Totals:" + "\n" +
					totalSusceptible.ToString("F3") + "\n" +
					totalVaccinated.ToString("F3") + "\n" +
					totalInfected.ToString("F3") + "\n" +
					totalRecovered.ToString("F3") + "\n" +
					totalExposed.ToString("F3") + "\n";
				statisticsEditLabel.text = finalString;
			}
		}
	}
}