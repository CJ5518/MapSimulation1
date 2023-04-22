using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//Singleton to manage a simulation instance and all that it entails
//Most everything related to the simulation that's in Main should be in here.
public class SimulationManager {
	public static Simulation simulation;
	public static Main main;
	public static SimulationCanvas simulationCanvas;
	public static SimulationStats stats {
		get {
			if (m_stats != null) {
				return m_stats;
			} else {
				m_stats = new SimulationStats();
				return m_stats;
			}
		}
	}

	private static Texture2D mainTexture;
	private static Texture2D secondTexture;

	public static GameObject objectWithMeshRenderer = null;

	//Stuff set externally so don't worry about it
	public static GameObject settingsPanel;
	public static ParameterPanel parameterPanel;
	//The game object with the panel might want to look into this
	public static GameObject ParametersPanel;
	public static ColorSettingsPanel colorSettingsPanel;

	public static bool goodToGo = false;

	//Private instances of the setters/getters
	private static SimulationStats m_stats;

	public static void Initialize() {
		double startTime = Time.realtimeSinceStartupAsDouble;

		//Set up render space
		Projection.setRenderSpaceByShapeFile(DataHandler.usaShapeFilepath);

		//Init those things that which need to be inited
		objectWithMeshRenderer = GameObject.Find("USAMesh");

		loadSimulation();
		targetTickTime = 1.0f / _targetTps;

		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to run the Main.cs start function");
	}

	bool hasPlacedAZombie = false;

	//Loads the simulation/raster data
	private static void loadSimulation() {
		double startTime = Time.realtimeSinceStartupAsDouble;
		
		//Load in the data layers

		//Load in the population data
		Texture2D populationTexture = new Texture2D(2,2);
		populationTexture.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Images/Population.png"));

		//Load in the elevation data
		Texture2D elevationTexture = new Texture2D(2,2);
		elevationTexture.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Images/Elevation.png"));

		//Load in the vacc rate data
		Texture2D vaccRateTexture = new Texture2D(2,2);
		vaccRateTexture.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Images/VaccRate.png"));

		//Load in the water data
		Texture2D waterTexture = new Texture2D(2,2);
		waterTexture.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Images/Water.png"));

		//Load in the road data
		Texture2D roadsTexture = new Texture2D(2,2);
		roadsTexture.LoadImage(System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/Images/Roads.png"));

		//Set up the movement model
		SimulationMovementModel movementModel;
		switch (SimulationSetupData.movementModel) {
			case 0:
			movementModel = new LocalizedGravityMovementModel();
			break;
			case 1:
			movementModel = new CJsMovementModel();
			break;
			default:
			throw new System.Exception(
				"Invalid number sent from script section to Main.cs for movement model, we got" + SimulationSetupData.movementModel
			);
		}
		//Set up the simulation
		simulation = new Simulation(
			populationTexture,
			elevationTexture,
			vaccRateTexture,
			waterTexture,
			roadsTexture,
			new Texture2D[] { },
			SimulationModelPresets.getPreset(3),
			movementModel
		);

		simulation.useTauLeaping = SimulationSetupData.useStochasticModel;
		
		//Set up peripherals
		stats.init();
		stats.updateStats();
		
		colorSettingsPanel.loadInSettings(ref simulation.model);
		
		//Attach the draw texture to an object
		Material material = objectWithMeshRenderer.GetComponent<MeshRenderer>().material;

		//Put this in our fancy schmancy time thing
		//I'm not sure what that comment means
		colorSettingsPanel.setSimulationColors(ref simulation);
		simulation.tickSimulation();
		stats.updateStats();


		//Set the parameters of the model
		if (SimulationSetupData.useTheseNumbers) {
			SimulationSetupData.mortalityRate = 1.0f - SimulationSetupData.recoveryRate;
			float theNumberToDivideBy = SimulationSetupData.infectionLength;
			simulation.model.parameters[0] = SimulationSetupData.mortalityRate / theNumberToDivideBy;
			simulation.model.parameters[1] = SimulationSetupData.recoveryRate / theNumberToDivideBy;
			simulation.model.parameters[2] = 1.0f / SimulationSetupData.waningImmunity;
			simulation.model.parameters[3] = 0;//SimulationSetupData.vaccinationUptake;
			simulation.model.parameters[4] = 0;//SimulationSetupData.breakthroughRate;
			simulation.model.parameters[5] = 1.0f / SimulationSetupData.latencyEI;
			simulation.model.parameters[6] = (SimulationSetupData.contactRate / 24.0f) * SimulationSetupData.infectionProbability;
		}

		//More peripherals, this time it's parameter sliders
		parameterPanel.loadInSliders(ref simulation.model, ref simulation.movementModel);
		simulationCanvas.UpdateSliderValues();
		
		//Debug the model parameters
		Debug.Log("MortalityRate:" + SimulationSetupData.mortalityRate);
		Debug.Log("recoveryRate:" + SimulationSetupData.recoveryRate);
		Debug.Log("waningImmunity:" + 1.0f/SimulationSetupData.waningImmunity);
		Debug.Log("latencyEI:" + SimulationSetupData.latencyEI + " " + simulation.model.parameters[5]);
		Debug.Log("contactRate:" + SimulationSetupData.contactRate + " " + (SimulationSetupData.contactRate / 24.0f));
		Debug.Log("infectionProbability:" + SimulationSetupData.infectionProbability);
		Debug.Log("Beta: " + simulation.model.parameters[6] + " " + (SimulationSetupData.contactRate / 24.0f) * SimulationSetupData.infectionProbability);
		Debug.Log("Infection Duration: " + SimulationSetupData.infectionLength);

		Debug.Log(simulation.model.parameters[0] + simulation.model.parameters[1] + " " + 1/SimulationSetupData.infectionLength);
		Debug.Log("Movement model id: " + SimulationSetupData.movementModel);
		
		simulation.drawTexture.filterMode = FilterMode.Point;
		mainTexture = new Texture2D(simulation.drawTexture.width, simulation.drawTexture.height, simulation.drawTexture.format, false);
		secondTexture = new Texture2D(simulation.drawTexture.width, simulation.drawTexture.height, simulation.drawTexture.format, false);
		//Copy the textures, maybe not necessary
		mainTexture.SetPixels(simulation.drawTexture.GetPixels());
		mainTexture.Apply();
		secondTexture.SetPixels(simulation.drawTexture.GetPixels());
		secondTexture.Apply();
		material.SetTexture("_MainTex", mainTexture);
		material.SetTexture("_SecondTex", secondTexture);

		Debug.Log(simulation.useTauLeaping ? "Tau leaping" : "Deterministic");
		Debug.Log("took " + (Time.realtimeSinceStartupAsDouble - startTime) +
			" seconds to load the simulation");
	}

	//Target amount of ticks per second, ONLY used at the start
	//Why is it public then? I don't know
	public static float _targetTps = 5.0f;

	//Set the target tps with this
	public static float TargetTps {
		get {
			return _targetTps;
		}

		set {
			_targetTps = value;
			targetTickTime = 1.0f / _targetTps;
		}
	}

	static float targetTickTime;

	//Some vars for testing purposes
	public static int tickCountThisSecond = 0;
	//Negative Zero
	static float aStartTime = -0.0f;
	static float previousTickStart = 0.0f;
	static float nextTickTime = 0.0f;
	static bool showTexture = false;
	public static void Update() {
		//Initialize a part of the TPS counter. could proly do this in Start() no?
		//Negative Zero
		if (aStartTime == -0.0f)
			aStartTime = Time.realtimeSinceStartup;

		if (Input.GetKeyDown(KeyCode.I)) {
			ParametersPanel.SetActive(!ParametersPanel.activeSelf);
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			settingsPanel.SetActive(!settingsPanel.activeSelf);
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			showTexture = !showTexture;
		}


		//Step once on space pressed
		//Because that's a useful feature
		if (Input.GetKeyDown(KeyCode.Space)) {
			simulation.tickSimulation();
			stats.updateStats();
			simulationCanvas.UpdateCanvas();
			simulationCanvas.UpdateSliderValues();

			//debugSurroundings(startingCellIdx);
		}

		//HELLO THIS IS THE COMMENT THE OTHER ONE TOLD YOU ABOUT
		//THIS IS A RETURN STATEMENT, IN CASE IT WASN'T OBVIOUS
		if (_targetTps <= 0.0f)
			return;

		//Runs the simulation constantly, one tick after the other
		if (!simulation.SimulationIsRunning && goodToGo && (nextTickTime == 0.0f || Time.realtimeSinceStartup + 0.01f >= nextTickTime)) {
			simulation.endTick();

			//Some of the rest of this should be wrapped up in a function and tied to the simulation event
			if (previousTickStart != 0.0f) {
				nextTickTime = Time.realtimeSinceStartup + targetTickTime;
			}
			
			if (simulation.runCount % 1 == 0)
				stats.updateStats();
			
			//TODO this is really a test case to verify that the number of people doesn't change because it shouldn't
			//Debug.Log(stats.globalTotals.numberOfPeople);
			colorSettingsPanel.setSimulationColors(ref simulation);
			simulationCanvas.UpdateCanvas();
			simulationCanvas.UpdateSliderValues();


			//Start the next tick
			simulation.beginTick();
			previousTickStart = Time.realtimeSinceStartup;

			Material material = objectWithMeshRenderer.GetComponent<MeshRenderer>().material;
			material.SetFloat("_TextureLerpValue", 0.0f);
			
			//Swap the main and second textures
			Graphics.CopyTexture(secondTexture, mainTexture);
			Graphics.CopyTexture(simulation.drawTexture, secondTexture);
			
			//Debug.Log("Tick finito");
			tickCountThisSecond++;
			if (Time.realtimeSinceStartup - aStartTime >= 1.0f) {
				//Debug.Log("TPS: " + (tickCount / (Time.realtimeSinceStartup - aStartTime)));
				tickCountThisSecond = 0;
				aStartTime = Time.realtimeSinceStartup;
			}
			//In the case where we are just waiting in between ticks
		} else if (!(nextTickTime == 0.0f || Time.realtimeSinceStartup + 0.01f >= nextTickTime)) {
			Material material = objectWithMeshRenderer.GetComponent<MeshRenderer>().material;
			
			material.SetFloat("_TextureLerpValue", 1.0f - ((nextTickTime - Time.realtimeSinceStartup) / targetTickTime));
		}

		//PLEASE LOOK FOR THE COMMENT ABOVE THE ABOVE BLOCK
		//FOR THE LOVE OF GOD
	}

	public static void Delete() {
		
	}
}