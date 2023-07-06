using UnityEngine;
using UnityEngine.Events;
using ShapeImporter;
using System.Collections.Generic;
using Aspose.Gis;
using Aspose.Gis.Geometries;
using ChartUtil;
using System.IO;


public class SimulationStats {
	public int lastStatsUpdate = -1;

	//Some settings
	//Store at minimum 20 data points
	public int graphTimeRangeMin = 20;
	//Store at maximum 25 data points
	public int graphTimeRangeMax = 25;

	public const string relativeStateShapefilePath = "/USA_States_Expanded.shp";
	//The shapes of every state
	public List<Vector2[]> stateShapes;
	//Which state has which cells
	public List<List<int>> stateIndices;
	//
	public int[] indexToState;
	//State names
	public List<string> stateNames;

	//The actual statistics
	public List<GameObject> charts;
	public GameObject usaChartObj;
	public DiseaseState globalTotals;

	public float dtElapsed {
		get {
			return globalTotals.dt;
		}
	}

	//Events
	public UnityEvent<int> infectionReachesState = new UnityEvent<int>();
	public UnityEvent infectionDiesOut = new UnityEvent();
	//Used to prevent the above event from firing too many times
	private bool invokedDeathOut = false;


	public SimulationStats() {
		//Init the state hovering
		stateShapes = new List<Vector2[]>();
		stateNames = new List<string>();

		using (VectorLayer layer = VectorLayer.Open(Application.streamingAssetsPath + relativeStateShapefilePath, Drivers.Shapefile)) {
			if (layer == null) {
				Logger.LogError("Error loading US shapefile");
			}
			foreach (Feature feature in layer) {
				if (feature.Geometry.GeometryType == GeometryType.Polygon) {
					string name = feature.GetValue<string>("NAME");
					Polygon statePoly = (Polygon)feature.Geometry;
					ILinearRing stateShape = statePoly.ExteriorRing;
					Vector2[] thisStateArray = new Vector2[stateShape.Count];
					for (int q = 0; q < stateShape.Count; q++) {
						thisStateArray[q] = new Vector2((float)stateShape[q].X, (float)stateShape[q].Y);
					}
					stateShapes.Add(thisStateArray);
					stateNames.Add(name);
				}
			}
		}

	}

	//Init stat tracking 
	public void init() {
		Simulation simulation = SimulationManager.simulation;
		SimulationManager.main.onZombieDropped.AddListener(onZombieDropped);
		stateIndices = new List<List<int>>();
		for (int q = 0; q < stateNames.Count; q++) {
			stateIndices.Add(new List<int>());
		}
		double startTime = Time.realtimeSinceStartupAsDouble;
		indexToState = new int[simulation.width * simulation.height];

		//Find which state each and every cell belongs to
		//Quick little optimization, cells next to each other are likely to be in the same state
		//Which turns out to be incredibly useful, almost a 10x speed improvement!
		List<int> cellsSansState = new List<int>();
		int prevStateIdx = -1;
		for (int q = 0; q < simulation.width * simulation.height; q++) {
			if (!simulation.cellIsValid(q)) continue;

			Vector2 pixelCoords = simulation.indexToCoord(q);
			Vector2 latLongOfPixel = (Vector2)Projection.renderSpaceToLatLongs((Vector2Double)pixelCoords);
			//Not greater than or equal to 0, because if this is state 0, we won't really save any time anyway

			int stateIdx = -1;
			if (prevStateIdx > 0) {
				if (Simulation.IsPointInPolygon(stateShapes[prevStateIdx], latLongOfPixel)) {
					stateIdx = prevStateIdx;
				} else { //Double else statement because I wasn't sure if C#'s && were short-circuited
					stateIdx = getStateIdxFromLatLong(latLongOfPixel);
				}
			} else {
				stateIdx = getStateIdxFromLatLong(latLongOfPixel);
			}

			prevStateIdx = stateIdx;
			//Logger.Log(stateIdx + " " + stateIndices.Count);
			if (stateIdx >= 0) {
				stateIndices[stateIdx].Add(q);
				indexToState[q] = stateIdx;
			}
			else {
				//Couldn't find the cell in the state shapefile
				//Put it in a list that we'll deal with later
				indexToState[q] = -1;
				cellsSansState.Add(q);
			}
		}

		int prevCellsSansStateSize = cellsSansState.Count;

		while (cellsSansState.Count != 0 && true) {
			//For all the cells without an attached state
			for (int q = 0; q < cellsSansState.Count; q++) {
				int currentCellIdx = cellsSansState[q];
				int[] neighbors = simulation.getNeighborIndices(q);
				int stateIdx = -1;
				//For all the neighbors
				for (int neighIdx = 0; neighIdx < neighbors.Length; neighIdx++) {
					//The idx of the simulation neighbor cell
					int neighborCellIdx = neighbors[neighIdx];
					if (!simulation.cellIsValid(neighborCellIdx)) continue;
					//If the neighbor hits
					if (indexToState[neighborCellIdx] >= 0) {
						stateIdx = indexToState[neighborCellIdx];
						break;
					}
				}
				//If we found a state
				if (stateIdx >= 0) {
					stateIndices[stateIdx].Add(currentCellIdx);
					indexToState[currentCellIdx] = stateIdx;
					//Removing it from the list while iterating through the list
					//Should be fine though
					cellsSansState.RemoveAt(q);
					//Go ahead and break anyway because I'm paranoid
					break;
				}
			}
			//If the last pass did nothing, just quit out and give up
			if (prevCellsSansStateSize == cellsSansState.Count) {
				break;
			}
		}


		Logger.Log("State thingy took " + (Time.realtimeSinceStartupAsDouble - startTime));


		//Initialize the chart datas
		charts = new List<GameObject>();
		GameObject chartModel;
		chartModel = GameObject.Find("UI_Main_CameraSpace/StateGraph/Chart");
		usaChartObj = GameObject.Find("UI_Main_CameraSpace/USAGraph/Chart");
		Chart usaChart = usaChartObj.GetComponent<Chart>();

		for (int q = 0; q < stateNames.Count; q++) {
			charts.Add(GameObject.Instantiate(chartModel, chartModel.transform.parent));
			Chart chart = charts[q].GetComponent<Chart>();
			//Set up the chart to have things
			for (int i = 0; i < simulation.model.compartmentInfoArray.Length; i++) {
				//Initialize the chart series
				chart.chartData.series = new List<Series>();
				usaChart.chartData.series = new List<Series>();
				for (int j = 0; j < simulation.model.compartmentCount; j++) {
					Series newSeries = new Series();
					newSeries.show = true;
					newSeries.colorIndex = j;
					newSeries.name = simulation.model.compartmentInfoArray[j].longName;
					newSeries.data = new List<Data>();
					chart.chartData.series.Add(newSeries);
					//Do the same for the usa
					Series usaSeries = new Series();
					usaSeries.show = true;
					usaSeries.colorIndex = j;
					usaSeries.name = simulation.model.compartmentInfoArray[j].longName;
					usaSeries.data = new List<Data>();
					usaChart.chartData.series.Add(usaSeries);
				}
				chart.chartOptions.plotOptions.dataColor[i] = simulation.model.compartmentInfoArray[i].mapDisplayColor;
				usaChart.chartOptions.plotOptions.dataColor[i] = simulation.model.compartmentInfoArray[i].mapDisplayColor;
			}
			charts[q].SetActive(false);
		}
		globalTotals = new DiseaseState(simulation.model.compartmentCount);
	}

	public void updateStats() {
		if (GlobalSettings.writeOutputFiles && !beganFileWrite)
			beginFileWrite();
		Simulation simulation = SimulationManager.simulation;
		if (lastStatsUpdate == simulation.runCount) {
			return;
		}
		lastStatsUpdate = simulation.runCount;
		globalTotals.setToZero();
		//For every state
		for (int q = 0; q < charts.Count; q++) {
			ChartData chartData = charts[q].GetComponent<ChartData>();
			ChartOptions chartOptions = charts[q].GetComponent<ChartOptions>();

			//Count the totals in this state
			DiseaseState totals = new DiseaseState(simulation.model.compartmentCount);
			totals.setToZero();

			//For cell in this state
			for (int i = 0; i < stateIndices[q].Count; i++) {
				Simulation.Cell cell = simulation.readCells[stateIndices[q][i]];
				for (int j = 0; j < cell.state.stateCount; j++) {
					totals.state[j] += cell.state.state[j];
					//Could get away with just taking the state totals and adding them to globalTotals, might be faster
					globalTotals.state[j] += cell.state.state[j];
				}
			}

			//Check if we have different data before adding data
			for (int i = 0; i < chartData.series.Count; i++) {
				//If we have a different value
				if (chartData.series[i].data.Count == 0 || chartData.series[i].data[chartData.series[i].data.Count-1].value != totals.state[i]) {

					//Little event guy here
					if (
						//There needs to be an older data point for this to work
						chartData.series[i].data.Count > 0 &&
						//If the previous tick had 0 infected
						chartData.series[simulation.model.droppingStateIdx].data[chartData.series[i].data.Count - 1].value == 0
						//And this tick has some infected
						&& totals.state[simulation.model.droppingStateIdx] > 0
						) {
						//Then fire the infection event
						//cjnote: This fires when anything in the state changes, so vaccinations cause this as well
						infectionReachesState.Invoke(q);
					}

					//Then add the data
					for (int j = 0; j < simulation.model.compartmentCount; j++) {
						chartData.series[j].data.Add(new Data(totals.state[j]));
					}
					chartData.categories.Add(simulation.dtSimulated.ToString());
					
					//Only do this once
					break;
				}
			}

			chartOptions.title.mainTitle = stateNames[q];
		}

		//Update the USA graph now
		ChartData usaChartData = usaChartObj.GetComponent<ChartData>();
		//for (int i = 0; i < usaChartData.series.Count; i++) {
			//If we have a different value
			if (true) {
				//Add the data
				for (int j = 0; j < simulation.model.compartmentCount; j++) {
					usaChartData.series[j].data.Add(new Data(globalTotals.state[j]));
				}
				usaChartData.categories.Add(simulation.dtSimulated.ToString());
				//Only do this once
			//	break;
			}
		//}
		
		if (usaChartData.categories.Count > graphTimeRangeMax) {
			int countDiff = usaChartData.categories.Count - graphTimeRangeMin;
			for (int q = countDiff-1; q >= 0; q--) {
				usaChartData.categories.RemoveAt(q);
				for (int i = 0; i < usaChartData.series.Count; i++) {
					usaChartData.series[i].data.RemoveAt(q);
				}
			}
		}
		

		if (GlobalSettings.writeOutputFiles)
			updateFileWrite();
		//Fire event if the infection has died out
		if (globalTotals.state[SimulationManager.simulation.model.droppingStateIdx] == 0 && SimulationManager.simulation.dtSimulated >= 10 && !invokedDeathOut) {
			infectionDiesOut.Invoke();
			invokedDeathOut = true;
		}
	}

	//Event handlers for events we subscribe to

	//Set the invoked death var to false,
	//Basically meaning we've started a new pandemic, so it can die out again, later, maybe
	void onZombieDropped() {
		invokedDeathOut = false;
	}


	StreamWriter outputFile;
	bool beganFileWrite = false;
	private void beginFileWrite() {
		if (beganFileWrite)
			return;
		Simulation simulation = SimulationManager.simulation;
		beganFileWrite = true;
		//Set up on destroy
		SimulationManager.main.onMainDestroy.AddListener(endFileWrite);
		
		System.DateTime time = System.DateTime.Now;

		string dateString = $"{time.Year}_{time.Month}_{time.Day}_{time.Hour}{time.Minute}{time.Second}{time.Millisecond}";
		string dataFileRootName = dateString + "-Data" + GlobalSettings.outputFilePostfix;
		string outFolder = GlobalSettings.outputPath;

		System.IO.Directory.CreateDirectory(outFolder);

		SimulationModel model = SimulationManager.simulation.model;

		//Write the parameter file
		List<KeyValuePair<string,string>> data = new List<KeyValuePair<string, string>>();

		System.Action<string, string> makeEntry = (key,val)=> {
			data.Add(new KeyValuePair<string, string>(key,val));
		};

		//Disease params
		for (int q = 0; q < model.parameterCount; q++) {
			makeEntry(model.parameterInfoArray[q].longName, model.parameters[q].ToString());
		}

		//Misc things
		makeEntry("Deterministic", (!SimulationManager.simulation.useTauLeaping).ToString());
		makeEntry("Airports", (SimulationManager.simulation.enableAirplanes).ToString());

		bool isGravity = SimulationManager.simulation.movementModel.GetType() == typeof(LocalizedGravityMovementModel);
		makeEntry("Gravity", (isGravity).ToString());
		//Gravity model or no? Would be better if this would be automagic, but whatever
		if (isGravity) {
			LocalizedGravityMovementModel gravModel = simulation.movementModel as LocalizedGravityMovementModel;
			makeEntry("Alpha", gravModel.parameters[0].ToString());
			makeEntry("Beta", gravModel.parameters[1].ToString());
			makeEntry("SpreadRate", (0.0f).ToString());
		} else {
			CJsMovementModel cjModel = simulation.movementModel as CJsMovementModel;
			makeEntry("Alpha", (0.0f).ToString());
			makeEntry("Beta", (0.0f).ToString());
			makeEntry("SpreadRate", cjModel.spreadRate.ToString());
		}
		makeEntry("RoadFactor", simulation.movementModel.roadFactor.ToString());
		makeEntry("WaterFactor", simulation.movementModel.waterFactor.ToString());
		makeEntry("HeightFactor", simulation.movementModel.heightFactor.ToString());
		makeEntry("StartingAirport(IfApplicable)", GlobalSettings.airportStartAt);

		StreamWriter paramFile = new StreamWriter(outFolder + dataFileRootName + "-Params.csv");
		//Write keys
		for (int q = 0; q < data.Count; q++) {
			if (q > 0) {
				paramFile.Write(",");
			}
			paramFile.Write(data[q].Key);
		}

		//Write values
		paramFile.Write("\n");
		for (int q = 0; q < data.Count; q++) {
			if (q > 0) {
				paramFile.Write(",");
			}
			paramFile.Write(data[q].Value);
		}

		paramFile.Flush();
		paramFile.Close();

		//Write the first line
		outputFile = new StreamWriter(outFolder + dataFileRootName + ".csv");
		outputFile.Write("Time");
		for (int stateId = 0; stateId < stateNames.Count; stateId++) {
			for (int q = 0; q < model.compartmentCount; q++) {
				string shortName = model.compartmentInfoArray[q].shortName;
				outputFile.Write(",");
				outputFile.Write(stateNames[stateId] + "_" + shortName);
			}
		}
		for (int q = 0; q < model.compartmentCount; q++) {
			string shortName = model.compartmentInfoArray[q].shortName;
			outputFile.Write(",");
			outputFile.Write("Totals" + "_" + shortName);
		}
		outputFile.Write("\n");
	}

	//Update the output file, call on stats update or whenever you feel
	private void updateFileWrite() {
		SimulationModel model = SimulationManager.simulation.model;
		
		outputFile.Write(SimulationManager.simulation.dtSimulated.ToString());
		for (int stateId = 0; stateId < charts.Count; stateId++) {
			ChartData chartData = charts[stateId].GetComponent<ChartData>();
			for (int q = 0; q < model.compartmentCount; q++) {
				outputFile.Write(",");
				outputFile.Write(chartData.series[q].data[chartData.series[q].data.Count-1].value);
			}
		}
		for (int q = 0; q < model.compartmentCount; q++) {
			outputFile.Write(",");
			outputFile.Write(globalTotals.state[q].ToString());
		}
		outputFile.Write("\n");
	}

	private void endFileWrite() {
		outputFile.Flush();
		outputFile.Close();
	}

	//Get the state that the given lat long coord is in
	public int getStateIdxFromLatLong(Vector2 coord) {
		for (int q = 0; q < stateShapes.Count; q++) {
			if (Simulation.IsPointInPolygon(stateShapes[q], coord)) {
				return q;
			}
		}
		return -1;
	}
}
