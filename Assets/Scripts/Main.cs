﻿using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;

//TODO:
//Maybe lean more/less on width/height being in projection
//See where we calculate airport index in it's constructor
//As that's where this thought comes from

//Main class
public class Main : MonoBehaviour {
	const int framerate = 60;
	public GameObject ParametersPanel;

	public SimulationCanvas simulationCanvas;
	public Simulation simulation;

	public GameObject settingsPanel;

	public ParameterPanel parameterPanel;
	public ColorSettingsPanel colorSettingsPanel;

	//Event we emit, called in unity's own OnDestroy
	public UnityEvent onMainDestroy;
	
	bool loadedSimulation = false;
	bool hasPlacedAZombie = false;
	float targetTickTime;

	void Start() {
		Application.targetFrameRate = framerate;

		string[] args = System.Environment.GetCommandLineArgs();
		GlobalSettings.initFromCommandLine(args);
		
		onMainDestroy = new UnityEvent();

		//Set things externally
		SimulationManager.parameterPanel = parameterPanel;
		SimulationManager.colorSettingsPanel = colorSettingsPanel;
		SimulationManager.settingsPanel = settingsPanel;
		SimulationManager.ParametersPanel = ParametersPanel;
		SimulationManager.simulationCanvas = simulationCanvas;
		SimulationManager.main = this;

		SimulationManager.Initialize();
		simulation = SimulationManager.simulation;
	}
	
	public int xCoordSub = 5;
	public int widthSub = 33;
	public int yCoordSub = 9;
	public int heightSub = 32;

	private unsafe void Update() {
		SimulationManager.Update();
		if (!hasPlacedAZombie && Application.isBatchMode) {
			dropZombieAtIndex(45908);
		}
		SimulationManager.simulationCanvas.getRealCoordFromSimCoord(new Vector2(0,0));
		Debug.Log(SimulationManager.stats.dtElapsed);
	}
	void OnDestroy() {
		simulation.endTick();
		simulation.deleteNativeArrays();
		onMainDestroy.Invoke();
	}

	//Print out some debug information about a cell and it's neighbors
	public int startingCellIdx = 0;
	public void debugSurroundings(int index) {
		int[] res = simulation.getNeighborIndices(index);
		float comingToUs = 0.0f;
		for (int q = 0; q < res.Length; q++) {
			comingToUs += simulation.movementModel.getCellSpreadValue(res[q], index, simulation);
			Debug.Log($"{q}={res[q]} {simulation.readCells[res[q]].state.state[simulation.model.droppingStateIdx]} {simulation.readCells[res[q]].state.state[simulation.model.startingStateIdx]} +{simulation.movementModel.getCellSpreadValue(index, res[q], simulation)}");
		}
		Debug.Log($"-1={index} {simulation.readCells[index].state.state[simulation.model.droppingStateIdx]} {simulation.readCells[index].state.state[simulation.model.startingStateIdx]} +{comingToUs}");
	}

	//Functions called by other scripts

	public static void ExitProgram() {
		Application.Quit();
	}

	public static void ResetScene() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public static void ReturnToSetup() {
		SceneManager.LoadScene("LandonSetup0");
	}

	public void dropZombieAtMousePosition() {
		//Pixel coord on the draw texture
		Vector2 pixel = SimulationCanvas.getPixelFromScreenCoord(Input.mousePosition);
		int index = simulation.coordToIndex(pixel);

		const int zombieCount = 30;

		dropZombieAtIndex(index, zombieCount);
	}

	public void dropZombieAtIndex(int index, int zombieCount = 30) {
		if (simulation.cellIsValid(index)) {

			Simulation.Cell cell = simulation.readCells[index];
			
			
			//Kill cells on whatever now
			if (cell.state.state[simulation.model.startingStateIdx] >= zombieCount) 
			{
				//Can't mess with the cells if the simulation is going
				simulation.endTick();
				cell.state.state[simulation.model.droppingStateIdx] += zombieCount;
				cell.state.state[simulation.model.startingStateIdx] -= zombieCount;
				simulation.readCells[index] = cell;
				hasPlacedAZombie = true;
				SimulationManager.goodToGo = true;
				startingCellIdx = index;
			}
		}
	}
}