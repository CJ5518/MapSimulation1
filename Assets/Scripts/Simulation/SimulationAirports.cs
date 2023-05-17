using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class SimulationAirports {
	private string airportDataPath = Application.streamingAssetsPath + "/Data/AirportData/T_T100D_SEGMENT_US_CARRIER_ONLY.csv";
	private string airportLocationsPath = Application.streamingAssetsPath + "/Data/AirportData/cj-airports-locations.csv";

	// Run time settings \\
	public bool drawPlaneTrails = true;

	// Compile time settings \\

	//Minimum number of passengers a year for us to care about the movement
	//In persons per year, currently set to one plane every ten ticks minimum
	const int passengerThreshold = 365 * 24 * (planeSize / 10);
	//How big is a plane?
	const int planeSize = 150;

	LineRenderPlaneTrail planeTrailRenderer;


	//How many airports do we have?
	//This is set in loadData
	int airportCount = 0;

	//Contains persons per year and distance between airports
	AirportData airportData;

	Simulation simulation;

	//Ahem, this is a big one
	//This structure holds the data for how many planes each airport fires to every other airport
	//airportFireFrequency[a][n].key -> same.value
	//a = FREQUENCY
	//a = 1 means that one plane fires every time value of 1
	//a = 2, two hours to fire a plane
	//a = -2 = two planes fire every tick of time 1
	//a = -5 = 5 planes fire every tick of one, if negative the effective time is abs(1 / a)
	//obj[a] is a list of keyValue pairs, the airport paths that have this requency
	
	Dictionary<int, List<KeyValuePair<string, string>>> airportFireFrequency;
	Dictionary<string, int> airportCodeToSimCellIdx;

	

	//The list of cell idx's in the simulation that have airports


	public SimulationAirports(int airportCount, Simulation simulation) {
		this.simulation = simulation;
		loadData(airportCount);
		planeTrailRenderer = GameObject.Find("PlaneTrailLineRenderer").GetComponent<LineRenderPlaneTrail>();
		planeTrailRenderer.gameObject.SetActive(false);
	}

	//Load data from the csv file
	private void loadData(int airportCount) {
		airportData = new AirportData();

		//Open the airport lat/long data file
		TextReader textReader = File.OpenText(airportLocationsPath);

		airportFireFrequency = new Dictionary<int, List<KeyValuePair<string, string>>>();
		airportCodeToSimCellIdx = new Dictionary<string, int>();

		//For every airport
		while (true) {
			string line = textReader.ReadLine();
			//No more lines then break
			if (line == null) break;
			//The data is a csv file, split it into fields
			//LAT, LONG, NAME
			string[] fields = line.Split(',');
			//The lat loing of this airport
			Vector2Double latLongs = new Vector2Double(double.Parse(fields[1]), double.Parse(fields[0]));
			
			//If this airport is on the map (Removes Alaska and some other invalids)
			Vector2Int renderCoords = (Vector2Int)Projection.latLongsToRenderSpace(latLongs);
			Vector2Int? renderCoordsMaybe = wiggleRenderCoords(renderCoords);
			
			if (renderCoordsMaybe == null) {
				continue;
			} else {
				renderCoords = renderCoordsMaybe.Value;
			}
			
			if (!(renderCoords.x < 0 || renderCoords.y < 0)) {
				//Check if this idx is bien in the map
				int simIdx = simulation.coordToIndex(renderCoords);
				if (simulation.cellIsValid(simIdx)) {
					//Airport is valid, let's loop over every single other airport
					//Also add it to the dictionary real quick
					if (!airportCodeToSimCellIdx.ContainsKey(fields[2]))
						airportCodeToSimCellIdx.Add(fields[2], simIdx);
					foreach(KeyValuePair<string, int> entry in airportData.lookupTable) {
						//Flow rate from this one to every other one
						int personsPerYear = airportData.getInfo(fields[2], entry.Key).personsPerYear;
						if (personsPerYear >= passengerThreshold) {
							
							//Calulate the integer planes per tick
							float personsPerTick = personsPerYear / 365.0f / 24.0f;
							float planesPerTick = personsPerTick / planeSize;
							int intPlanesPerTick = 0;
							if (planesPerTick < 1.0f) {
								intPlanesPerTick = -Mathf.RoundToInt(1 / planesPerTick);
							} else {
								intPlanesPerTick = Mathf.RoundToInt(planesPerTick);
							}

							//If this is the first airport flow with this specific fire rate
							if (!airportFireFrequency.ContainsKey(intPlanesPerTick)) {
								airportFireFrequency.Add(intPlanesPerTick, new List<KeyValuePair<string, string>>());
							}
							airportFireFrequency[intPlanesPerTick].Add(new KeyValuePair<string, string>(fields[2], entry.Key));


						}
					}
				}
			}
		}

		//Now that we've loaded the data, let's weed it!
		weedAirportPairsRecursive();
	}

	private void weedAirportPairsRecursive() {
		foreach (KeyValuePair<int, List<KeyValuePair<string, string>>> upperDictPair in airportFireFrequency) {
			int fireFrequency = upperDictPair.Key;
			List<KeyValuePair<string,string>> airportPairs = weedAirportPairListRecursiveInner(upperDictPair.Value);
			//airportFireFrequency[fireFrequency] = airportPairs;
			//weedAirportPairsRecursive();
			//break;
		}
	}

	private List<KeyValuePair<string,string>> weedAirportPairListRecursiveInner(List<KeyValuePair<string,string>> airportPairs) {
		for (int q = 0; q < airportPairs.Count; q++) {
			if (!airportCodeToSimCellIdx.ContainsKey(airportPairs[q].Value)) {
				airportPairs.RemoveAt(q);
				return weedAirportPairListRecursiveInner(airportPairs);
			}
		}
		return airportPairs;
	}

	public int getIdFromCode(string code) {
		return airportData.getIdFromCode(code);
	}

	//The pun again
	//Ticks the airports, to be called AFTER tick simulation
	//Not sure if the order is that important but hey, consistency is key
	public void tickAirports(float dt) {
		int intDt = Mathf.RoundToInt(dt);
		foreach (KeyValuePair<int, List<KeyValuePair<string, string>>> upperDictPair in airportFireFrequency) {
			//If we good to fire a 'port
			if ((upperDictPair.Key < 0 || intDt % Mathf.Abs(upperDictPair.Key) == 0) || upperDictPair.Key > 0) {
				//Iterate over the firings
				for (int q = 0; q < upperDictPair.Value.Count; q++) {
					string originCode = upperDictPair.Value[q].Key;
					string destCode = upperDictPair.Value[q].Value;

					DiseaseState originState = simulation.readCells[airportCodeToSimCellIdx[originCode]].state;
					//Debug.Log(originState.state[simulation.model.droppingStateIdx] / (float)originState.numberOfPeople +" "+ 1.0f / planeSize + " " + originCode + " " + airportCodeToSimCellIdx[originCode]);
					if (originState.state[simulation.model.droppingStateIdx] / (float)originState.numberOfPeople > 1.0f / planeSize) {
						Simulation.Cell destCell = simulation.readCells[airportCodeToSimCellIdx[destCode]];
						if (destCell.state.state[simulation.model.startingStateIdx] > 2) {
							destCell.state.state[simulation.model.startingStateIdx]--;
							destCell.state.state[simulation.model.droppingStateIdx]++;
							doPlaneTrail(originCode, destCode, dt);
						}
						simulation.readCells[airportCodeToSimCellIdx[destCode]] = destCell;
					}
				}
			}
		}
	}


	Dictionary<string, float> planeTrailsLast = new Dictionary<string, float>();
	const float planeTrailLifetime = 1.0f;

	private void doPlaneTrail(string originCode, string destCode, float dt) {
		if (!drawPlaneTrails) return;

		//This one has not been idk even TODO fix this
		if (!planeTrailsLast.ContainsKey(originCode + destCode)) {
			planeTrailsLast[originCode + destCode] = dt;
		} else {
			if (dt - planeTrailsLast[originCode + destCode] < planeTrailLifetime) {
				return;
			}
		}
		Vector3 realEnd = SimulationManager.simulationCanvas.getRealCoordFromSimCoord(SimulationManager.simulation.indexToCoord(airportCodeToSimCellIdx[destCode]));
		Vector3 realStart = SimulationManager.simulationCanvas.getRealCoordFromSimCoord(SimulationManager.simulation.indexToCoord(airportCodeToSimCellIdx[originCode]));
		LineRenderPlaneTrail planeTrail = GameObject.Instantiate(planeTrailRenderer.gameObject).GetComponent<LineRenderPlaneTrail>();
		planeTrail.start = realStart;
		planeTrail.end = realEnd;
		planeTrail.lifetime = planeTrailLifetime;
		planeTrail.gameObject.SetActive(true);
		GameObject.Destroy(planeTrail.gameObject, planeTrailLifetime);
	}

	//"Wiggles" the render coords to get them onto the map if they happen to be off the map, which happens to be the case for LAX
	//Which is one of the single largest airports in the world
	private Vector2Int? wiggleRenderCoords(Vector2Int coords) {
		//Firstly, are we on the map?
		if (simulationCoordIsValid(coords)) {
			return coords;
		}
		//Otherwise, wiggle it around a bit
		for (int x = coords.x - 1; x <= coords.x + 1; x++) {
			for (int y = coords.y - 1; y <= coords.y + 1; y++) {
				Vector2Int newCoords = new Vector2Int(x,y);
				if (simulationCoordIsValid(newCoords)) {
					return newCoords;
				}
			}
		}
		return null;
	}

	private bool simulationCoordIsValid(Vector2Int coords) {
		return simulation.cellIsValid(simulation.coordToIndex(coords));
	}

	private void drawPlaneTrail() {
		
	}
}