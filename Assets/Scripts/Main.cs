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

	//The background image
	public MovableRawImage backgroundMovableImage;

	public Simulation simulation;
    public SimulationCanvas simulationCanvas;
	

	bool loadedSimulation = false;

	void Start() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		Application.targetFrameRate = framerate;

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

		//Find a unity component
		backgroundMovableImage = GameObject.Find("Canvas/Background").GetComponent<MovableRawImage>();

		StartCoroutine("loadSimulation");

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to run the Main.cs start function");
	}

	//Loads the simulation/raster data
	IEnumerator loadSimulation() {
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
		Debug.Log("Loaded the population data");
		//Dataset elevation = Gdal.Open("F:\\Data\\tif\\Elevation\\test.tif", Access.GA_ReadOnly);

		LuaFunction warpVrt = (LuaFunction)LuaSingleton.lua["RasterUtilities.warpVrt"];
		Dataset output = (Dataset)warpVrt.Call("F:\\Data\\tif\\Elevation\\Elevation.vrt", "F:\\Data\\tif\\Elevation\\output.tif", "average")[0];
		output.Dispose();

		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			new Texture2D[] { populationTextures[(int)Population.FullPopulation] }
		);
		backgroundMovableImage.texture = simulation.drawTexture;
        simulationCanvas.UpdateSliderValues();
		//Log time
		Debug.Log("Preprocess time: " + preprocessTime);
		Debug.Log("Texture load time: " + textureLoadTime);

		loadedSimulation = true;
	}
	

	//Some vars for testing purposes

	//Doing things every x seconds
	float lastSimTime = -100.0f;
	bool autoPlay = false;
	//The demographic we are currently looking at statistics for
	public int targetDemographic = (int)Population.FullPopulation;

	private unsafe void Update() {
		if (!loadedSimulation) return;

        simulationCanvas.UpdateCanvas();

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

		simulationCanvas.updateStatisticsLabel();

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

}