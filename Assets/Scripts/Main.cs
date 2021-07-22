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

//Main class
public class Main : MonoBehaviour {
	//Pixel size for the texture
	const int pixelSize = 4;
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
		Projection.pixelSize = pixelSize;

		//Init the Lua singleton
		LuaSingleton.initLua();


		DataSource x;

		StartCoroutine("loadSimulation");

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to run the Main.cs start function");
	}

	//Loads the simulation/raster data
	IEnumerator loadSimulation() {
		int width = Screen.width / pixelSize;
		int height = Screen.height / pixelSize;

		//Load in the population data
		Texture2D[] populationTextures = new Texture2D[(int)Population.PopulationCount];
		RasterHandler rasterHandler;

		for (int q = 0; q < populationTextures.Length; q++) {
			rasterHandler = new RasterHandler(RasterType.Population, q);

			populationTextures[q] = rasterHandler.loadToTexture(width, height);

			populationTextures[q].Apply();

			yield return null;
		}

		//Load in the elevation data
		//TODO:
		//Actually do something with this
		rasterHandler = new RasterHandler(RasterType.Elevation, null);
		Texture2D elevationTexture = rasterHandler.loadToTexture(width, height);
		elevationTexture.Apply();
		
		
		//Set up the simulation
		simulation = new Simulation(
			populationTextures,
			elevationTexture,
			new Texture2D[] { }
		);
		simulationCanvas.UpdateSliderValues();


		if (objectWithMeshRenderer != null) {
			Material material = objectWithMeshRenderer.GetComponent<MeshRenderer>().material;
			MeshRenderer meshRenderer = objectWithMeshRenderer.GetComponent<MeshRenderer>();
			
			material.SetTexture("_MainTex", simulation.drawTexture);
		}
		loadedSimulation = true;
	}
	

	//Some vars for testing purposes

	//Doing things every x seconds
	float lastSimTime = -100.0f;
	bool autoPlay = true;
	//The demographic we are currently looking at statistics for
	public int targetDemographic = (int)Population.FullPopulation;

	private unsafe void Update() {
		if (!loadedSimulation) return;

		simulationCanvas.UpdateCanvas();
		simulation.data.drawRecovered = true;

		//Make sure targetDemographic is in range
		if (targetDemographic < 0)
			targetDemographic = (int)Population.PopulationCount - 1;
		if (targetDemographic >= (int)Population.PopulationCount)
			targetDemographic = 0;

		simulation.data.drawDemographic = targetDemographic;

		//Draw elevation on key press because I'm lazy
		if (Input.GetKeyDown(KeyCode.Alpha1)) 
			simulation.data.drawElevation = !simulation.data.drawElevation;

		//Pixel coord on the draw texture
		Vector2 pixel = simulationCanvas.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		if (simulation.cellIsValid(index)) {
			
			Simulation.Cell cell = simulation.readCells[index];
			
			//Kill cells on click
			if (
				Input.GetMouseButtonDown(0) &&
				EventSystem.current.currentSelectedGameObject == null &&
				cell.susceptible[targetDemographic] >= 1.0f) {
				//Can't mess with the cells if the simulation is going
				simulation.endTick();
				cell.infected[targetDemographic]+= 1.0f;
				cell.susceptible[targetDemographic]-= 1.0f;
				simulation.readCells[index] = cell;
			}
		}

		//Step once on space pressed
		if (Input.GetKeyDown(KeyCode.Space) && !autoPlay) {
			simulation.tickSimulation(1);
		}

		//Tick the simulation every now and then
		if (Time.realtimeSinceStartup - lastSimTime >= (1.0f / simulationTicksPerSecond)  && autoPlay) {
			lastSimTime = Time.realtimeSinceStartup;

			simulation.endTick();

			simulation.beginTick(1);
		}

		//Toggle autoplay on q pressed
		if (Input.GetKeyDown(KeyCode.Q)) autoPlay = !autoPlay;
	}

	void OnDestroy() {
		if (simulation.simulationIsRunning) simulation.endTick();
		simulation.deleteNativeArrays();
	}
}