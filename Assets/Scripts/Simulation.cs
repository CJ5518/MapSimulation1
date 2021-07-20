//By Carson Rueber

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using System;
using System.Collections.Generic;
using ShapeImporter;

//Handles a cell based simulation of stuff
public class Simulation {
	private Texture2D[] populationTextures;
	private Texture2D elevationTexture;
	//Array of textures needed in the simulation itself
	private Texture2D[] simulationTextures;

	//Metadata for the textures
	public NativeArray<TextureMetadata> textureMetadataArray;
	//Pointers to the texutreArray that are almost never valid
	private NativeArray<IntPtr> textureDataPointers;
	//The background color of the draw texture
	public Color backgroundColor = new Color(0.2392157f, 0.6f, 0.7686275f);
	//The draw texture
	public Texture2D drawTexture;
	//Batch count, messing with this will impact performance
	//See unity parallel for jobs
	private const int batchCount = 13755;

	//Cell struct, contains all the individual information for a cell
	public unsafe struct Cell {
		//buffers for the different counts of people in this cell
		public fixed float numberOfPeople[(int)Population.PopulationCount];
		public fixed float susceptible[(int)Population.PopulationCount];
		public fixed float infected[(int)Population.PopulationCount];
		public fixed float recovered[(int)Population.PopulationCount];
		public fixed float exposed[(int)Population.PopulationCount];
		public fixed float vaccinated[(int)Population.PopulationCount];
		public int elevation;

		public bool inMask; //Are we in the mask
	}

	//Texture metadata struct
	public struct TextureMetadata {
		public float weight;
	}

	//Struct that holds all the data needed by the simulation
	public unsafe struct SimulationDataStruct {
		public int width; //Dimensions of input/output textures
		public int height;
		public float season; //Unused
		public float seasonAdder;
		public Color infectedColor; //Colors
		public Color deadColor;
		public Color recoveredColor;

		//Should we allow these factors to be involved in the color of the pixels
		public bool drawInfected;
		public bool drawDead;
		public bool drawRecovered;
		//Draw elevation or population
		public bool drawElevation;
		
		//Max number of people in a cell per demographic
		public fixed float maxNumberOfPeople[(int)Population.PopulationCount];

		//Parameters
		public float beta, alpha, gamma, sigma, delta;

		//The number of times the tick function has been run
		public uint runCount;

		//The demographic to influence the output pixel color
		public int drawDemographic;

		public float spreadRate;
		public bool moveZombies;
	}
	//Set the default values here
	public SimulationDataStruct data = new SimulationDataStruct() {
		season = 0.0f,
		seasonAdder = 0.0005f,
		infectedColor = Color.red,
		deadColor = Color.blue,
		recoveredColor = Color.green,

		drawInfected = true,
		drawDead = true,
		drawRecovered = false,
		drawElevation = false,
		beta = 1.0f, alpha = 1.0f, gamma = 1.0f, sigma = 1.0f, delta = 1.0f,

		runCount = 0,

		drawDemographic = 0,
		spreadRate = 1.0f,
		moveZombies = true
	};

	//Cell buffers
	//Internally: you always read from readCells, and write to writeCells
	//Externally: writeCells is fairly useless, read/write to readCells if you want to read/change state
	public NativeArray<Cell> writeCells;
	public NativeArray<Cell> readCells;

	//Constructor
	//All textures must be of the same width and height, different formats are allowed
	public Simulation(Texture2D[] populationTextures, Texture2D elevationTexture, Texture2D[] simulationTextures) {
		this.populationTextures = populationTextures;
		this.elevationTexture = elevationTexture;
		this.simulationTextures = simulationTextures;
		Init();
	}

	JobHandle jobHandle;
	public bool simulationIsRunning = false;
	//Starts a tick of the simulation, MUST call endTick before calling this again
	//You also mustn't write to readCells while simulationIsRunning
	public unsafe void beginTick() {
		data.runCount++;
		simulationIsRunning = true;

		//The unsafe part
		//Get pointers to the raw texture data
		for (int q = 0; q < simulationTextures.Length; q++) {
			textureDataPointers[q] = new IntPtr(
				NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(simulationTextures[q].GetRawTextureData<Color32>())
			);
		}
		//Create and start the simulation
		var job = new SimulationJob() {
			readCells = readCells,
			writeCells = writeCells,
			drawTextureData = drawTexture.GetRawTextureData<Color32>(),
			data = data,
			textureDataPointers = textureDataPointers,
			textureMetadataArray = textureMetadataArray
		};

		jobHandle = job.Schedule(data.width * data.height, batchCount);
	}

	//Ends a tick started by beginTick
	//Call this whenever you want to edit readcells, just to be safe
	//Basically you can edit anything that isn't a NativeCollection because those are shared memory,
	//whereas most everything else is copied to the simulation
	public void endTick() {
		if (!simulationIsRunning) return;
		simulationIsRunning = false;
		jobHandle.Complete();

		drawTexture.Apply();

		//Swap the cell buffers
		NativeArray<Cell> tmp = readCells;
		readCells = writeCells;
		writeCells = tmp;

		//Updating the season variable
		//Not currently used but it's here if we need it
		data.season += data.seasonAdder;
		if (data.season >= 1.0 || data.season <= 0.0)
			data.seasonAdder *= -1f;
	}

	//The pun
	//Ticks the simulation once
	public unsafe void tickSimulation() {
		beginTick();
		endTick();
	}

	//Deletes the native arrays used by the simulation
	//A required step that you can do in OnDestroy or whenever you're done with the simulation object
	public void deleteNativeArrays() {
		readCells.Dispose();
		writeCells.Dispose();
		textureDataPointers.Dispose();
		textureMetadataArray.Dispose();
	}

	//Init everything
	public unsafe void Init() {
		//Native arrays
		textureMetadataArray = new NativeArray<TextureMetadata>(simulationTextures.Length, Allocator.Persistent);
		textureDataPointers = new NativeArray<IntPtr>(simulationTextures.Length, Allocator.Persistent);

		//Default initialization for textureMetadata
		for (int q = 0; q < textureMetadataArray.Length; q++) {
			TextureMetadata textureMetadata = textureMetadataArray[q];
			textureMetadata.weight = 1.0f / textureMetadataArray.Length;
			textureMetadataArray[q] = textureMetadata;
		}
		//We make the first one the example
		data.width = populationTextures[0].width;
		data.height = populationTextures[0].height;

		convertTextureArrayFormats(simulationTextures);
		convertTextureFormat(ref elevationTexture);
		convertTextureArrayFormats(populationTextures);
		
		//Create drawTexture
		drawTexture = new Texture2D(data.width, data.height, TextureFormat.RGBA32, false);
		drawTexture.filterMode = FilterMode.Point;

		//Set all the pixels to be the background color
		//When it gets updated, teh background ones won't be, so the background will remain constant
		for (int x = 0; x < data.width; ++x) {
			for (int y = 0; y < data.height; ++y) {
				drawTexture.SetPixel(x, y, backgroundColor);
			}
		}

		//Fire off a different init function
		InitCells();
	}

	//Initialize the cell arrays
	private unsafe void InitCells() {
		//Native arrays
		readCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);
		writeCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);

		//Load the shapeFile data
		ShapeFile shapeFile = new ShapeFile();
		shapeFile.ReadShapes(Application.streamingAssetsPath + "/USA_Reprojected.shp");

		ShapeFileRecord usaRecord = shapeFile.MyRecords[0];
		Vector2[] renderSpaceUSA = new Vector2[usaRecord.Points.Count];

		//Put the lat longs into render space
		for (int q = 0; q < usaRecord.Points.Count; q++) {
			renderSpaceUSA[q] = (Vector2)Projection.projectionToRenderSpace(Projection.projectVector(
				usaRecord.Points[q]
			));
		}

		//Set the maximum per demographic to 0
		for (int q = 0; q < (int)Population.PopulationCount; q++) {
			data.maxNumberOfPeople[q] = 0;
		}

		//Init every cell
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				int index = coordToIndex(x, y);

				//Set some default cell values
				Cell readCell = readCells[index];

				//Innocent until proven guilty
				readCell.inMask = true;

				Color32 color;

				//Population
				for (int q = 0; q < (int)Population.PopulationCount; q++) {
					Texture2D texture = populationTextures[q];
					color = texture.GetPixel(x, y);
					float numberOfPeople = colorToFloat(color);

					if (float.IsNaN(numberOfPeople)) Debug.Log("Is NaN");

					readCell.numberOfPeople[q] = numberOfPeople;
					readCell.susceptible[q] = numberOfPeople;
					readCell.infected[q] = 0;
					readCell.recovered[q] = 0;
					readCell.vaccinated[q] = 0;

					//Keep track of the maximum
					if (numberOfPeople > data.maxNumberOfPeople[q]) {
						data.maxNumberOfPeople[q] = numberOfPeople;
					}
				}
				//Set elevation
				color = elevationTexture.GetPixel(x, y);
				readCell.elevation = colorToInt(color);

				//If there's nobody here
				if (readCell.numberOfPeople[(int)Population.FullPopulation] == 0) {
					//And we're not in the USA
					if (!IsPointInPolygon(renderSpaceUSA, new Vector2(x,y) * Projection.pixelSize)) {
						readCell.inMask = false;
					}
				}
				
				//Write back the cell
				readCells[index] = readCell;
				writeCells[index] = readCell;
			}
		}
	}


	//The real meat and potatoes
	//Handles the simulation
	public struct SimulationJob : IJobParallelFor {
		//Inputs and outputs
		[ReadOnly]
		public NativeArray<Cell> readCells;
		[WriteOnly]
		public NativeArray<Cell> writeCells;
		[WriteOnly]
		public NativeArray<Color32> drawTextureData;
		[ReadOnly]
		public SimulationDataStruct data;
		[ReadOnly]
		public NativeArray<IntPtr> textureDataPointers;
		[ReadOnly]
		public NativeArray<TextureMetadata> textureMetadataArray;

		const int FullPop = (int)Population.FullPopulation;

		//The function that gets called for every index
		public unsafe void Execute(int index) {
			if (!cellIsValid(index))
				return;
			Cell readCell = readCells[index];
			Cell writeCell = readCell;

			//Color32* textureDataPointer = (Color32*)textureDataPointers[texIdx];
			//Then just use it as an array

			//Spread in this cell, because of this cell
			float SZN = (readCell.susceptible[FullPop] * readCell.infected[FullPop]) / readCell.numberOfPeople[FullPop];
			SZN = float.IsNaN(SZN) ? 0.0f : SZN;
			float newExposed = data.beta * SZN;
			float newVaccinated = data.sigma * readCell.susceptible[FullPop];
			float newInfected = data.alpha * readCell.exposed[FullPop];
			float newRecovered = data.gamma * readCell.infected[FullPop];
			float newKilledZombies = data.delta * SZN;

			writeCell.susceptible[FullPop] += -newExposed - newVaccinated;
			writeCell.vaccinated[FullPop] += newVaccinated;
			writeCell.exposed[FullPop] += newExposed - newInfected;
			writeCell.infected[FullPop] += newInfected - newRecovered - newKilledZombies;
			writeCell.recovered[FullPop] += newRecovered + newKilledZombies;

			//Spread in this cell, because of other cells

			int[] neighborIndices = getNeighborIndices(index);
			float ourContribution = 0;

			for (int q = 0; q < neighborIndices.Length; q++) {
				if (cellIsValid(neighborIndices[q])) {
					Cell neighborCell = readCells[neighborIndices[q]];

					if (neighborCell.infected[FullPop] >= 1.0f) {
						float neighborMoveZombies = getCellSpreadContribution(neighborIndices[q]);

						//Can't have the new number of infected be too large if we don't also subtract later
						if (!data.moveZombies) {
							neighborMoveZombies = Mathf.Clamp(neighborMoveZombies, 0.0f, writeCell.susceptible[FullPop]);
						}
						writeCell.infected[FullPop] += neighborMoveZombies;
						writeCell.numberOfPeople[FullPop] += neighborMoveZombies;
					}

					//We give an amount to each neighbor based on readCell, since we're here going
					//through all valid neighbors, might as well count what we owe
					if (data.moveZombies)
						ourContribution += getCellSpreadContribution(index);
				}
			}

			writeCell.numberOfPeople[FullPop] -= ourContribution;
			writeCell.infected[FullPop] -= ourContribution;


			//Clamp the numbers
			writeCell.susceptible[FullPop] = Mathf.Clamp(writeCell.susceptible[FullPop], 0, float.MaxValue);
			writeCell.exposed[FullPop] = Mathf.Clamp(writeCell.exposed[FullPop], 0, float.MaxValue);
			writeCell.infected[FullPop] = Mathf.Clamp(writeCell.infected[FullPop], 0, float.MaxValue);
			writeCell.recovered[FullPop] = Mathf.Clamp(writeCell.recovered[FullPop], 0, float.MaxValue);

			//Compute the color


			Color color;

			//Really just toggles between log transform or not
			
			float max = Mathf.Log10(data.maxNumberOfPeople[FullPop]);
			color = new Color(
				Mathf.Log10(writeCell.infected[FullPop]) / max,
				Mathf.Log10(writeCell.recovered[FullPop]) / max,
				Mathf.Log10(writeCell.vaccinated[FullPop]) / max
			);

			if (!data.drawInfected)
				color.r = 0.0f;
			if (!data.drawRecovered)
				color.g = 0.0f;
			if (!data.drawDead)
				color.b = 0.0f;

			//If this cell has not been touched by the virus
			if (writeCell.susceptible[FullPop] == writeCell.numberOfPeople[FullPop]) {
				float v;
				float lerpAmount = 0.3f;
				//This will likely be changed to an enum once more than two data layers exist
				if (!data.drawElevation)
					v = Mathf.Log10(writeCell.numberOfPeople[FullPop]) / Mathf.Log10(data.maxNumberOfPeople[FullPop]);
				else {
					v = Mathf.Clamp01(readCell.elevation / 7000.0f);
					lerpAmount = 0.5f;
				}
				//Override the color
				color = Color.Lerp(Color.black, new Color(v,v,v,1.0f), lerpAmount);
			}

			color.a = 1f;
			
			//Write back the data
			drawTextureData[index] = color;
			writeCells[index] = writeCell;
		}

		//Returns the indices of the neighbors to an index
		//Indices may or may not be valid, or even in the array at all
		public int[] getNeighborIndices(int index) {
			int[] ret = new int[8];
			ret[0] = index - 1; //left
			ret[1] = index + 1; //right
			ret[2] = index + data.width; //top
			ret[3] = index - data.width; //bottom
			ret[4] = ret[0] + data.width;
			ret[5] = ret[0] - data.width;
			ret[6] = ret[1] + data.width;
			ret[7] = ret[1] - data.width;
			return ret;
		}

		public unsafe float getCellSpreadContribution(int index) {
			Cell cell = readCells[index];
			float amount = cell.infected[FullPop] / 80.0f;
			//Fudge factor
			float factor = Mathf.Clamp(Mathf.Sqrt(cell.numberOfPeople[FullPop]), float.Epsilon, float.MaxValue)
				/ Mathf.Sqrt(data.maxNumberOfPeople[FullPop]);

			amount *= factor * data.spreadRate;
			//Make sure it's greater than one on our way out
			return amount >= 1.0f ? amount : 0.0f;
		}

		//Verify if a cell is valid
		//Cell is in bounds and also in the mask
		public bool cellIsValid(int index) {
			return index >= 0 && index < (data.width * data.height) && readCells[index].inMask;
		}
	}

	//Functions for encoding data into colors
	public static Color32 intToColor(int n) {
		Color32 color = new Color32();

		color.r = (byte)n;
		color.g = (byte)(n >> 8);
		color.b = (byte)(n >> 16);
		color.a = (byte)(n >> 24);

		return color;
	}
	
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

	public unsafe static Color32 floatToColor(float n) {
		return intToColor(*(int*)&n);
	}

	public unsafe static float colorToFloat(Color32 color) {
		int n = colorToInt(color);
		return *(float*)&n;
	}

	//Coordinate system functions
	public int coordToIndex(int x, int y) {
		return (y * data.width) + x;
	}

	public int coordToIndex(Vector2 vec) {
		return ((int)vec.y * data.width) + (int)vec.x;
	}

	public Vector2 indexToCoord(int index) {
		Vector2 ret;
		ret.x = index % data.width;
		ret.y = (index - ret.x) / data.width;
		return ret;
	}

	//Identical to the function in SimulationJob
	//Verify if a cell is valid
	//Cell is in bounds and also in the mask
	public bool cellIsValid(int index) {
		return index >= 0 && index < (data.width * data.height) && readCells[index].inMask;
	}

	//Converts an array of textures to RGBA32
	//Uses data.width/height so make sure those are set properly
	private void convertTextureArrayFormats(Texture2D[] array) {
		for (int q = 0; q < array.Length; q++) {
			convertTextureFormat(ref array[q]);
		}
	}

	private void convertTextureFormat(ref Texture2D tex) {
			//Verify that it's the same size
			if (tex.width != data.width || tex.height != data.height)
				throw new Exception("Texture " + tex.ToString() + " is not of the same height and width as the other textures");
			//Only convert if we need to
			if (tex.format == TextureFormat.RGBA32) return;
			//Create dummy texture to copy the data in a new format
			Texture2D dummy = new Texture2D(data.width, data.height, TextureFormat.RGBA32, false);
			dummy.SetPixels(tex.GetPixels());
			dummy.Apply();
			tex = dummy;
	}

	//https://stackoverflow.com/a/14998816
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
