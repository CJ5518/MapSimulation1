using UnityEngine;
using ChartUtil;
using TMPro;
using System.Collections.Generic;

//Manages the single cell scene
//Generates the UI, runs the simulation, and displays the results on a graph
public class SingleCellScript : MonoBehaviour {
	//The main chart we're updating
	public Chart chart;
	public ParameterSlider templateSlider;
	public TMP_Text templateTextHeader;
	private SimulationModel model;
	private DiseaseState state;

	private List<ParameterSlider> stateParameterSliders; 
	private List<ParameterSlider> paramsParemeterSliders;

	//Unity functions

	void Start() {
		Application.targetFrameRate = 60;
		model = SimulationModelPresets.recovBackToSus();
		generateUI();
		initChart();
	}

	//Called by the start button, set in editor
	public void startSimulation() {
		//Reset the chart
		initChart();

		//Init the disease state
		state = new DiseaseState(model.compartmentCount);
		state.setToZero();
		for (int q = 0; q < stateParameterSliders.Count; q++) {
			state.state[q] = (int)stateParameterSliders[q].scaledValue;
		}

		//Load in the model parameters
		for (int q = 0; q < paramsParemeterSliders.Count; q++) {
			model.parameters[q] = paramsParemeterSliders[q].scaledValue;
		}

		//Run some ticks TEMP
		writeStateDataToChart();
		for (int q = 0; q < 100; q++) {
			tickSimulation(0.5f);
			writeStateDataToChart();
		}
		chart.UpdateChart();
	}
	
	//Tick the simulation by some amount of time
	void tickSimulation(float dt) {
		state = SimulationAlgorithms.basicTick(state, ref model, dt);
	}

	//Puts the state data in the chart
	void writeStateDataToChart() {
		for (int q = 0; q < model.compartmentCount; q++) {
			chart.chartData.series[q].data.Add(new Data(state.state[q], state.dt));
		}
		chart.chartData.categories.Add(state.dt.ToString());
	}

	//generates the UI based on the current model
	void generateUI() {
		//Make objects
		stateParameterSliders = new List<ParameterSlider>();
		paramsParemeterSliders = new List<ParameterSlider>();

		//Starting population
		makeHeader("Starting Populations");

		for (int q = 0; q < model.compartmentCount; q++) {
			ParameterSlider clone = cloneSlider();
			clone.textPrefix = model.compartmentInfoArray[q].longName + ": ";
			clone.significantDigits = 0;
			clone.wholeNumbers = true;
			clone.minValue = 0;
			clone.maxValue = 300;
			clone.value = 0;
			if (model.startingStateIdx == q) {
				clone.value = 200;
			}
			stateParameterSliders.Add(clone);
			clone.gameObject.SetActive(true);
		}

		//Parameter slider section
		makeHeader("Parameters");

		for (int q = 0; q < model.reactionCount; q++) {
			ParameterSlider clone = cloneSlider();
			clone.textPrefix = model.parameterInfoArray[q].longName + ": ";
			clone.significantDigits = 2;
			clone.wholeNumbers = false;
			clone.maxValue = 1.0f;
			clone.minValue = 0.0f;
			clone.value = 0.5f;
			clone.gameObject.SetActive(true);
			paramsParemeterSliders.Add(clone);
		}
	}

	//Initialize the chart based on the current model
	//Set the series count and names, colors, etc.
	void initChart() {
		chart.chartData.series.Clear();
		for (int q = 0; q < model.compartmentCount; q++) {
			Series series = new Series();
			series.colorIndex = -1;
			series.name = model.compartmentInfoArray[q].longName;
			chart.chartData.series.Add(series);
		}
		chart.chartData.categories = new List<string>();
	}

	//Clones and sets the template header to whatever text you want
	TMP_Text makeHeader(string text) {
		TMP_Text startingPopulationHeader = GameObject.Instantiate(templateTextHeader, templateTextHeader.transform.parent);
		startingPopulationHeader.text = text;
		startingPopulationHeader.gameObject.SetActive(true);
		return startingPopulationHeader;
	}

	//Clones the slider object and sets its parent to the template slider's parent
	ParameterSlider cloneSlider() {
		return GameObject.Instantiate(templateSlider, templateSlider.transform.parent);
	}

}
