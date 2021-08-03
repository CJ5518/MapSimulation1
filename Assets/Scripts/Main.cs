//By Carson Rueber

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using OSGeo.OSR;
using OSGeo.OGR;
using OSGeo.GDAL;
using NLua;
using SimpleFileBrowser;

//TODO:
//Maybe lean more/less on width/height being in projection
//See where we calculate airport index in it's constructor
//As that's where this thought comes from

//Main class
public class Main : MonoBehaviour {
	const int framerate = 60;

	const float simulationTicksPerSecond = 10.0f;

	public Simulation simulation;
	public SimulationCanvas simulationCanvas;

	public GameObject objectWithMeshRenderer = null;
	

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

		//Init the Lua singleton
		LuaSingleton.initLua();

		StartCoroutine("loadSimulation");

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to run the Main.cs start function");
	}

	//Loads the simulation/raster data
	IEnumerator loadSimulation() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		//Load in the population data
		Texture2D[] populationTextures = new Texture2D[(int)Population.PopulationCount];
		RasterHandler rasterHandler;

		for (int q = 0; q < populationTextures.Length; q++) {
			rasterHandler = new RasterHandler(RasterType.Population, q);

			populationTextures[q] = rasterHandler.loadToTexture();

			yield return null;
		}

		//Load in the elevation data
		rasterHandler = new RasterHandler(RasterType.Elevation, null);
		Texture2D elevationTexture = rasterHandler.loadToTexture();

		//Load in the vacc rate data
		rasterHandler = new RasterHandler(RasterType.VaccRate, null);
		Texture2D vaccRateTexture = rasterHandler.loadToTexture();

		
		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			elevationTexture,
			vaccRateTexture,
			new Texture2D[] { }
		);

		simulationCanvas.UpdateSliderValues();
		
		//Attach the draw texture to an object
		Material material = objectWithMeshRenderer.GetComponent<MeshRenderer>().material;
		MeshRenderer meshRenderer = objectWithMeshRenderer.GetComponent<MeshRenderer>();
		
		material.SetTexture("_MainTex", simulation.drawTexture);

		//We're done
		loadedSimulation = true;

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to load the simulation");
	}
	

	//Some vars for testing purposes

	//Doing things every x seconds
	float lastSimTime = -100.0f;
	bool autoPlay = true;
	//The demographic we are currently looking at statistics for
	public int targetDemographic = (int)Population.FullPopulation;

	private unsafe void Update() {
		//Don't do anything until the simulation has been loaded
		if (!loadedSimulation) return;

		//Horribly laggy
		simulationCanvas.UpdateCanvas();

		//Make sure targetDemographic is in range
		if (targetDemographic < 0)
			targetDemographic = (int)Population.PopulationCount - 1;
		if (targetDemographic >= (int)Population.PopulationCount)
			targetDemographic = 0;

		simulation.data.drawDemographic = targetDemographic;

		//Draw elevation on key press because I'm lazy
		if (Input.GetKeyDown(KeyCode.Alpha1)) 
			simulation.data.drawElevation = !simulation.data.drawElevation;
		//Reset the simulation on key press because I continue to be lazy
		if (Input.GetKeyDown(KeyCode.Alpha2)) 
			simulation.reset();

		//Pixel coord on the draw texture
		Vector2 pixel = SimulationCanvas.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (simulation.cellIsValid(index)) {
			
			Simulation.Cell cell = simulation.readCells[index];
			
			//Kill cells on click
			if (Input.GetMouseButtonDown(0) &&
				EventSystem.current.currentSelectedGameObject == null &&
				cell.susceptible[targetDemographic] >= 1.0f) 
			{
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
		if (Time.realtimeSinceStartup - lastSimTime >= (1.0f / simulationTicksPerSecond)  && autoPlay) {
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