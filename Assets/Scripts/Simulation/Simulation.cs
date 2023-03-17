//By Carson Rueber

//#define _DEBUG

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.SceneManagement;
using Unity.Jobs;
using UnityEngine;
using System;
using System.Collections.Generic;
using ShapeImporter;
using System.Threading;
using System.IO;
using UnityEngine.Events;

//Small issue with simulated dt:
//The dt in the simulation is the dt at the start of the sim, and so if we could get that it would be perfect
//But if dt is changed during the simulation, simulatedDt will become inaccurate.

//Official debug cell: 45908
//Somehwere in the atlanta blob
//I think?

//It hath been decreed that dt is in units of hours
//Thus it shall forever be
//Doesn't really matter all that much in the context of the simulation itself

//Handles a cell based simulation of stuff
public class Simulation {

	//Some events
	public UnityEvent onTickEnd;


	#region MemberVariables


	private Texture2D populationTexture;
	private Texture2D elevationTexture;
	private Texture2D vaccRateTexture;
	private Texture2D waterTexture;
	private Texture2D roadsTexture;
	//Array of textures needed in the simulation itself
	private Texture2D[] simulationTextures;
	//The model the simulation uses
	public SimulationModel model;
	//The movement model the simulation uses
	public SimulationMovementModel movementModel;

	public SimulationAirports simulationAirports;

	//This array is assumed to be sorted with high commerical_ops airports first
	//At least, the data that this array comes from is assumed to be sorted like that
	/////public Airport[] airports;
	//Our own local copy of the passenger data

	//List of the airplanes currently flying around

	//////public List<Airplane> airplanes;

	//Cell buffers
	//Internally (Meaning the Execute function): you always read from readCells, and write to writeCells
	//Externally: writeCells is fairly useless, read/write to readCells if you want to read/change state
	//Obviously try not to do this while the simulation is updating or it will crash and burn
	public Cell[] writeCells;
	public Cell[] readCells;

	//Worker threads for the simulation
	private Thread[] simulationThreads;
	private int threadCount;

	//Metadata for the textures
	public NativeArray<TextureMetadata> textureMetadataArray;
	//Pointers to the texutreArray that are only ever valid in the simulation job
	private NativeArray<IntPtr> textureDataPointers;
	//The background color of the draw texture
	public Color backgroundColor = new Color(0f, 0f, 0f);
	//The draw texture
	public Texture2D drawTexture;

	public bool enableAirplanes = true;

	#endregion

	#region StructDefinitions

	//Cell struct, contains all the individual information for a cell
	public struct Cell {
		//buffers for the different counts of people in this cell
		public DiseaseState state;
		//Int, I think meters? Idk check the data
		public int elevation;
		//0-1 percentage of people vaccinated in this cell, should be removed when we get the timeseries up
		public float vaccRate;
		//0-1 the amount of this cell that has water in it
		public float waterLevel;
		//True if this cell has a major road in it
		public bool hasRoad;

		public bool inMask; //Are we in the mask

		public Cell(Cell otherCell) {
			elevation = otherCell.elevation;
			hasRoad = otherCell.hasRoad;
			inMask = otherCell.inMask;
			vaccRate = otherCell.vaccRate;
			waterLevel = otherCell.waterLevel;
			state = new DiseaseState(otherCell.state.stateCount);
			state.dt = otherCell.state.dt;
			for (int q = 0; q < otherCell.state.stateCount; q++) {
				state.state[q] = otherCell.state.state[q];
			}
		}
	}



	//Texture metadata struct
	//Mostly just here in case we need it
	//And it took some effort to get it in that I don't want to have to redo if this ever becomes important
	public struct TextureMetadata {
		public float weight;
	}

	public int width; //Dimensions of input/output textures
	public int height;
	
	//Drawing information
	//Which states do we draw? (indices)
	public List<int> statesToDraw;
	//The output colors, it was just easier to put this here than generate it
	//Took less brainpower
	Color[] colorMasks = {
		new Color(1,0,0,0),
		new Color(0,1,0,0),
		new Color(0,0,1,0),
		new Color(0,0,0,1)
	};


	//Draw the populations as relative to the cell if true, if false then draw log transformed
	public bool drawRelative = false;
	//Landon's log constants
	private const float logDrawHshift = 6.172f;
	private const float logDrawYshift = -260000f;
	private const float logDrawLogScale = 1.000007f;
	private const float logDrawSHshift = 8000f;
	private const float logDrawSYshift = -3000000f;
	private const float logDrawSLogScale = 1.000003f;
	public float logDrawMax;
	public float logDrawSMax;
	//Do we discretize like a chode or stochastize like a chad?
	public bool useTauLeaping = false;

	//Gravity model parameters
	float gravityMovementAlpha = 0.6f;
	float gravityMovementBeta = 0.3f;

	//Should be something like zombie climbing multiplier
	public bool zombiesCanClimbMountains = true;
	//In meters I think? idfk check the data
	public int maxClimbDistance = 500;
	public bool waterAffectsZombies = true;
	//Factor applied to zombie movement in regards to roads
	public float roadMultiplier = 3.0f;
		
	//Max number of people in a cell
	public float maxNumberOfPeople;
	
	//The number of times the tick function has been run
	public int runCount = 0;

	//Every time we run the simulation, add whatever dt is to this
	//So it would be total dt simulated
	public float dtSimulated = 0.0f;
	//What's the difference between this and the above?
	//The number of simulated hours
	public float hoursPassed = 0.0f;


	public bool moveZombies = true;

	public int maxElevation = -9999;
	public int minElevation = 9999;

	//Default 1, in units of hours
	public float dt = 1.0f;

	public int lowestValidIndex = int.MaxValue;
	public int highestValidIndex = int.MinValue;
	#endregion

	#region AirplanesAndPorts


	//Constructor
	//All textures must be of the same width and height, different formats are allowed,
	//but will be turned into RGBA32 anyway
	public Simulation(
		Texture2D populationTexture,
		Texture2D elevationTexture,
		Texture2D vaccRateTexture,
		Texture2D waterTexture,
		Texture2D roadsTexture,
		Texture2D[] simulationTextures,
		SimulationModel model,
		SimulationMovementModel movementModel
		) 
	{
		this.populationTexture = populationTexture;
		this.elevationTexture = elevationTexture;
		this.vaccRateTexture = vaccRateTexture;
		this.waterTexture = waterTexture;
		this.roadsTexture = roadsTexture;
		this.simulationTextures = simulationTextures;
		this.model = model;
		this.movementModel = movementModel;

		onTickEnd = new UnityEvent();


		//We make the first one the example
		width = populationTexture.width;
		height = populationTexture.height;
		
		//Create drawTexture
		drawTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
		drawTexture.filterMode = FilterMode.Trilinear;

		//Set all the pixels to be the background color
		//When it gets updated, teh background ones won't be, so the background will remain constant
		for (int x = 0; x < width; ++x) {
			for (int y = 0; y < height; ++y) {
				drawTexture.SetPixel(x, y, backgroundColor);
			}
		}
		Init();
	}

	#if _DEBUG
	public StreamWriter debugStreamWriter;
	public Main main;
	#endif


	//Init everything
	public unsafe void Init() {
		//Native arrays
		textureMetadataArray = new NativeArray<TextureMetadata>(simulationTextures.Length, Allocator.Persistent);
		textureDataPointers = new NativeArray<IntPtr>(simulationTextures.Length, Allocator.Persistent);
		convertTextureArrayFormats(simulationTextures);
		convertTextureFormat(ref elevationTexture);
		convertTextureFormat(ref populationTexture);

		//Default initialization for textureMetadata
		for (int q = 0; q < textureMetadataArray.Length; q++) {
			TextureMetadata textureMetadata = textureMetadataArray[q];
			textureMetadata.weight = 1.0f / textureMetadataArray.Length;
			textureMetadataArray[q] = textureMetadata;
		}
		//Fire off some other init functions
		InitCells();
		InitAirports();

		//Things for drawing non-relative
		logDrawSMax = Mathf.Log(
			logDrawSHshift + maxNumberOfPeople, logDrawSLogScale
			) + logDrawSYshift;

		logDrawMax = Mathf.Log(
			logDrawHshift + maxNumberOfPeople, logDrawLogScale
			) + logDrawYshift;

		//Other drawing stuff
		statesToDraw = new List<int>();
		
		//Thread stuff
		threadCount = SystemInfo.processorCount;
		threadCount = 1;
		simulationThreads = new Thread[threadCount];

		#if _DEBUG
		debugStreamWriter = new StreamWriter("C:/Users/carso/mapSimLog.txt");
		main = GameObject.Find("GameManager").GetComponent<Main>();
		#endif
	}

	//Initialize the cell arrays
	private unsafe void InitCells() {
		//Native arrays - not native no more
		readCells = new Cell[width * height];
		writeCells = new Cell[width * height];

		//Set the maximum to 0
		maxNumberOfPeople = 0;

		//Init every cell
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				int index = coordToIndex(x, y);

				//Set some default cell values
				Cell readCell = new Cell();
				readCell.state = new DiseaseState(model.compartmentCount);

				//Innocent until proven guilty
				readCell.inMask = true;

				Color32 color;

				//Population
				color = populationTexture.GetPixel(x, y);
				float floatPeople = (int)colorToFloat(color);

				//colorToFloat seems to return something that's not NaN when floatToColor should be making NaN
				//Something might be wrong with one of the two functions
				Color32 nanColor = floatToColor(float.NaN);
				bool isNaN = color.a == nanColor.a;
				isNaN = isNaN && color.r == nanColor.r;
				isNaN = isNaN && color.g == nanColor.g;
				isNaN = isNaN && color.b == nanColor.b;

				int numberOfPeople = isNaN ? -1 : (int)floatPeople;

				//If this is a no data type area
				if (numberOfPeople < 0) {
					readCell.inMask = false;
				}

				readCell.state.setToZero();
				readCell.state.state[model.startingStateIdx] = numberOfPeople;

				//Keep track of the maximum
				if (numberOfPeople > maxNumberOfPeople) {
					maxNumberOfPeople = numberOfPeople;
				}

				//Set elevation
				color = elevationTexture.GetPixel(x, y);
				readCell.elevation = colorToInt(color);

				if (readCell.elevation < minElevation) minElevation = readCell.elevation;
				if (readCell.elevation > maxElevation) maxElevation = readCell.elevation;

				//Set vaccRate
				color = vaccRateTexture.GetPixel(x, y);
				readCell.vaccRate = colorToFloat(color);
				//Goofy little fix to this issue
				//Where goofy means bad
				if (readCell.vaccRate <= 0.0f)
					readCell.vaccRate = 0.5f;
				
				//Water level
				color = waterTexture.GetPixel(x,y);
				readCell.waterLevel = colorToFloat(color);

				//Presence of road
				color = roadsTexture.GetPixel(x,y);
				readCell.hasRoad = colorToInt(color) == 1;

				if (readCell.inMask) {
					if (index < lowestValidIndex) {
						lowestValidIndex = index;
					}
					if (index > highestValidIndex) {
						highestValidIndex = index;
					}
				}
				
				//Write back the cell
				readCells[index] = readCell;
				writeCells[index] = readCell;
				//Debug.Log("Max nuber of people in one cell: " + data.maxNumberOfPeople);
			}
		}
	}

	//simple function, create the simulation airports object
	private void InitAirports() {
		simulationAirports = new SimulationAirports(10, this);
		Debug.Log("-------BEGIN-------");
		Vector2Int OG = new Vector2Int(20,20);
		Vector2Double latLons = Projection.renderSpaceToLatLongs(OG);
		
		Vector2Double renderAgain = Projection.latLongsToRenderSpace(latLons);
		Debug.Log($"OG: {OG} LatLons: {latLons}");
		Debug.Log($"LatLons: {latLons} RenderAgain: {renderAgain}");
		//46423
		Vector2Int coord = indexToCoord(46423);

		Debug.Log($"Coord: {coord} Latlon = {Projection.renderSpaceToLatLongs((Vector2Double)coord)}");
		Debug.Log("-------END-------");
	}

	#region UpdateFunctions

	//Has start tick been called and end tick has yet to be called?
	private bool startedTick;

	//Is the simulation *actually* running?
	public bool SimulationIsRunning {
		get {
			if (startedTick) {
				for (int q = 0; q < threadCount; q++) {
					if (!simulationThreads[q].Join(TimeSpan.Zero)) {
						return true;
					}
				}
				return false;
			}
			return false;
		}
	}

	//Starts a tick of the simulation, MUST call endTick before calling this again
	//You also mustn't write to readCells while simulationIsRunning is true
	NativeArray<Color32> drawTextureData;
	public unsafe void beginTick() {
		if (startedTick) return;
		startedTick = true;

		//The unsafe part
		//Get pointers to the raw texture data
		for (int q = 0; q < simulationTextures.Length; q++) {
			textureDataPointers[q] = new IntPtr(
				NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(simulationTextures[q].GetRawTextureData<Color32>())
			);
		}
		drawTextureData = drawTexture.GetRawTextureData<Color32>();

		int sizeOfEach = highestValidIndex - lowestValidIndex;
		sizeOfEach = ((threadCount - (sizeOfEach % threadCount)) + sizeOfEach) / threadCount;
		
		for (int q = 0; q < threadCount; q++) {
			int startIdx = lowestValidIndex + (q * sizeOfEach);
			int endIdx = lowestValidIndex + ((q+1) * sizeOfEach);
			endIdx = endIdx <= highestValidIndex+1 ? endIdx : highestValidIndex+1;
			simulationThreads[q] = new Thread(() => updateSimulation(startIdx, endIdx));
			simulationThreads[q].Priority = System.Threading.ThreadPriority.Lowest;
			simulationThreads[q].Start();
		}
	}

	//Ends a tick started by beginTick
	//Call this whenever you want to edit readcells, just to be safe
	//Basically you can edit anything that isn't a NativeCollection because those are shared memory,
	//whereas most everything else is copied to the simulation
	public void endTick() {
		if (!startedTick) return;
		startedTick = false;
		for (int q = 0; q < threadCount; q++) {
			simulationThreads[q].Join();
		}

		drawTexture.Apply();

		//Swap the cell buffers
		Cell[] tmp = readCells;
		readCells = writeCells;
		writeCells = tmp;
		
		dtSimulated += dt;
		if (enableAirplanes)
			simulationAirports.tickAirports(dt); //Board some planes afterward
		runCount++;

		onTickEnd.Invoke();
	}

	//The pun
	//Ticks the simulation once
	public unsafe void tickSimulation() {
		beginTick();
		endTick();
	}
	
	#endregion

	//Deletes the native arrays used by the simulation
	//A required step that you can do in OnDestroy or whenever you're done with the simulation object
	public void deleteNativeArrays() {
		textureDataPointers.Dispose();
		textureMetadataArray.Dispose();
		#if _DEBUG
		debugStreamWriter.Flush();
		debugStreamWriter.Close();
		#endif
	}

	//Resets the simulation back to the way it was at the start
	public void reset() {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	#region InitFunctions

	#endregion
	

	//The function that gets called for every index [start, endIdx)
	public void updateSimulation(int startIdx, int endIdx) {
		for (int index = startIdx; index < endIdx; index++) {
			if (!cellIsValid(index)) {
				drawTextureData[index] = new Color32(0,0,0,0);
				continue;
			}
				
			
			Cell readCell = readCells[index];
			//For whatever reason, we need to explicitely copy the cell
			Cell writeCell = new Cell(readCells[index]);

			//Color32* textureDataPointer = (Color32*)textureDataPointers[texIdx];
			//Then just use it as an array

			//if (writeCell.state.vaccinated > 0.0f) Debug.Log("Index #" + index + " has been naughty");
			//Spread in this cell, because of other cells

			int[] neighborIndices = getNeighborIndices(index);
			int ourContribution = 0;
			
			for (int q = 0; q < neighborIndices.Length; q++) {
				if (cellIsValid(neighborIndices[q])) {
					Cell neighborCell = readCells[neighborIndices[q]];
					 
					//The zombies moving to this cell
					int neighborMoveZombies = (int)movementModel.getCellSpreadValue(neighborIndices[q], index, this);
					
					writeCell.state.state[model.droppingStateIdx] += neighborMoveZombies;

					//We give an amount to each neighbor based on readCell, since we're here going
					//through all valid neighbors, might as well count what we owe
					int ours = (int)movementModel.getCellSpreadValue(index, neighborIndices[q], this);
					ourContribution += ours;
				}
			}

			writeCell.state.state[model.droppingStateIdx] -= ourContribution;

			//Run disease simulation
			if (readCell.state.numberOfPeople > 0) {
				if (useTauLeaping)
					writeCell.state = SimulationAlgorithms.tauLeaping(writeCell.state, ref model, 0.03f, true, dt);
				else
					writeCell.state = SimulationAlgorithms.basicTick(writeCell.state, ref model, dt);
			}

			//Clamp the states because my code is bad and this probably fixes things
			for (int q = 0; q < writeCell.state.stateCount; q++) {
				writeCell.state.state[q] = Mathf.Clamp(writeCell.state.state[q], 0, int.MaxValue);
			}

			//Compute the color


			Color color;
			float max;
			//Can only draw 4 things at the same time
			float[] colVals = new float[4];

			if (drawRelative) {
				max = readCell.state.numberOfPeople;

				for (int q = 0; q < statesToDraw.Count; q++) {
					colVals[q] = Mathf.Clamp01((writeCell.state.state[statesToDraw[q]]) / max);
				}
			}
			else {
				/*
				max = data.maxNumberOfPeople;

				infectedColVal = Mathf.Clamp01((writeCell.state.infected) / max);
				deadColVal = Mathf.Clamp01((writeCell.state.dead) / max);
				vaccinatedColVal = Mathf.Clamp01((1 + writeCell.state.vaccinated) / max);
				exposedColVal = Mathf.Clamp01((1 + writeCell.state.exposed) / max);
				*/

				//Log transform!
				const float hshift = logDrawHshift;
				const float yshift = logDrawYshift;
				const float logScale = logDrawLogScale;

				for (int q = 0; q < statesToDraw.Count; q++) {
					colVals[q] = Mathf.Clamp01((Mathf.Log(hshift + writeCell.state.state[statesToDraw[q]], logScale) + yshift) / logDrawMax);
				}
				//using susceptible!  it gets it's own log transform.
				//It does not
				//const float sHshift = logDrawSHshift;
				//const float sYshift = logDrawSYshift;
				//const float sLogScale = logDrawSLogScale;

				//vaccinatedColVal = Mathf.Clamp01((Mathf.Log(sHshift + writeCell.state.state[0], sLogScale) + sYshift) / logDrawMax);*/
			}



			//Color for the states
			color = new Color(0,0,0,0);
			for (int q = 0; q < statesToDraw.Count; q++) {
				color += colVals[q] * colorMasks[q];
			}

			//Write back the data
			drawTextureData[index] = color;
			writeCells[index] = writeCell;
		}
	}

	//Returns the indices of the neighbors to an index
	//Indices may or may not be valid, or even in the array at all
	public int[] getNeighborIndices(int index) {
		int[] ret = new int[8];
		ret[0] = index - 1; //left
		ret[1] = index + 1; //right
		ret[2] = index + width; //top
		ret[3] = index - width; //bottom
		ret[4] = ret[0] + width;
		ret[5] = ret[0] - width;
		ret[6] = ret[1] + width;
		ret[7] = ret[1] - width;
		return ret;
	}

	/*

	#region MovementModels

	public float getCellSpreadContributionGravity(int giverIdx, int receiverIdx) {
		Cell giverCell = readCells[giverIdx];
		Cell receiverCell = readCells[receiverIdx];
		float amount = 0.0f;
		amount = (
			Mathf.Pow(giverCell.state.state[model.droppingStateIdx], gravityMovementAlpha) *
			Mathf.Pow(receiverCell.state.state[model.startingStateIdx], gravityMovementBeta)
		) / 121.0f;

		//Lame fix for the bug where a cell would give too many things
		if (amount > giverCell.state.state[model.droppingStateIdx] / 8.0f) amount = giverCell.state.state[model.droppingStateIdx] / 8.0f;
		//Make sure it's greater than one * dt on our way out
		
		amount = amount >= 10.0f * dt ? amount : 0.0f;
		return amount;
	}

	#endregion

*/

	//Verify if a cell is valid
	//Cell is in bounds and also in the mask
	public bool cellIsValid(int index) {
		return (index >= 0) && (index < (width * height)) && readCells[index].inMask;
	}

	//Functions for encoding data into colors

	//Encodes an integer into a color
	public static Color32 intToColor(int n) {
		Color32 color = new Color32();

		color.r = (byte)n;
		color.g = (byte)(n >> 8);
		color.b = (byte)(n >> 16);
		color.a = (byte)(n >> 24);

		return color;
	}
	
	//Decodes a color into an integer, for use with intToColor
	public static int colorToInt(Color32 color) {
		int ret = 0;
		ret |= color.a;
		ret = ret << 8;
		ret |= color.b;
		ret = ret << 8;
		ret |= color.g;
		ret = ret << 8;
		ret |= color.r;
		return ret;
	}

	//Same as intToColor, but does the unsafe pointer magic to
	//cast the int to the float without changing the binary data
	public unsafe static Color32 floatToColor(float n) {
		return intToColor(*(int*)&n);
	}

	//Same as colorToInt, but does the unsafe pointer magic to
	//cast the int to the float without changing the binary data
	public unsafe static float colorToFloat(Color32 color) {
		int n = colorToInt(color);
		return *((float*)&n);
	}

	//Calculate r0
	//R0 calculation, calcs as if the cell was only sus
	/*public unsafe float calculateR0(int cellIdx) {
		Cell cell = readCells[cellIdx];
		float r0 = 0.0f;
		float beta = data.diseaseParameters.beta;
		float N = cell.numberOfPeople;
		float S = N - 1.0f;
		float Z = 1.0f;
		float SZN = (S * Z) / N;
		float gamma = data.diseaseParameters.gamma;
		float delta = data.diseaseParameters.delta;
		r0 = beta * SZN * (-1.0f / ((-gamma * Z) - (delta * SZN)));
		return r0;
	}*/

	//Coordinate system functions

	//Takes an x-y coordinate in render space and returns an index in the array
	public int coordToIndex(int x, int y) {
		return (y * width) + x;
	}

	//Takes an x-y coordinate in render space and returns an index in the array
	public int coordToIndex(Vector2 vec) {
		return ((int)vec.y * width) + (int)vec.x;
	}

	//Takes an x-y coordinate in render space and returns an index in the array 
	public int coordToIndex(Vector2Int vec) {
		return (vec.y * width) + vec.x;
	}

	//Takes an array index and converts it into an x-y coordinate
	public Vector2Int indexToCoord(int index) {
		Vector2Int ret = new Vector2Int();
		ret.x = index % width;
		ret.y = (index - ret.x) / width;
		return ret;
	}

	//Converts an array of textures to RGBA32
	//Uses data.width/height so make sure those are set properly
	private void convertTextureArrayFormats(Texture2D[] array) {
		for (int q = 0; q < array.Length; q++) {
			convertTextureFormat(ref array[q]);
		}
	}

	//Converts the format of the given texture to RGBA32 for consistency
	private void convertTextureFormat(ref Texture2D tex) {
			//Verify that it's the same size
			if (tex.width != width || tex.height != height)
				throw new Exception("Texture " + tex.ToString() + " is not of the same height and width as the other textures");
			//Only convert if we need to
			if (tex.format == TextureFormat.RGBA32) return;
			//Create dummy texture to copy the data in a new format
			Texture2D dummy = new Texture2D(width, height, TextureFormat.RGBA32, false);
			dummy.SetPixels(tex.GetPixels());
			dummy.Apply();
			tex = dummy;
	}

	//https://stackoverflow.com/a/14998816
	//Checks if a point is in a polygon defined by it's vertices.
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

#endregion