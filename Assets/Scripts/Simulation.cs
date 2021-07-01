//By Carson Rueber

//TODO: Things should be based on chance, include some randomness

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using System;

//Handles a cell based simulation of stuff
public class Simulation {
	//Array of textures needed in the simulation itself
	private Texture2D[] simulationTextures;

	private Texture2D[] populationTextures;

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
		public fixed float numberOfPeople[(int)PopulationRasterType.PopulationTypeCount];
		public fixed float susceptible[(int)PopulationRasterType.PopulationTypeCount];
		public fixed float infected[(int)PopulationRasterType.PopulationTypeCount];
		public fixed float recovered[(int)PopulationRasterType.PopulationTypeCount];
		public fixed float exposed[(int)PopulationRasterType.PopulationTypeCount];
		public fixed float dead[(int)PopulationRasterType.PopulationTypeCount];

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
		//Draw proportionally to the cell or not, i don't really know what this means/does/why
		//It is but the boss requested it so here we are
		public bool drawProportion;
		
		//Max number of people in a cell per demographic
		public fixed float maxNumberOfPeople[(int)PopulationRasterType.PopulationTypeCount];

		//Parameters
		public float beta, alpha, gamma;

		//The number of times the tick function has been run
		public uint runCount;

		//The demographic to influence the output pixel color
		public int drawDemographic;
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
		beta = 1.0f, alpha = 1.0f, gamma = 1.0f,

		runCount = 0,

		drawDemographic = 0
	};

	//Cell buffers
	//Internally: you always read from readCells, and write to writeCells
	//Externally: writeCells is fairly useless, read/write to readCells if you want to read/change state
	public NativeArray<Cell> writeCells;
	public NativeArray<Cell> readCells;


	//Random seeds for the simulation, because UnityEngine.Random doesn't work in jobs
	//And we need seeds for the other random generators
	private NativeArray<uint> randomSeeds;


	//Constructor
	//All textures must be of the same width and height, different formats are allowed
	public Simulation(Texture2D[] populationTextures, Texture2D[] simulationTextures) {
		this.populationTextures = populationTextures;
		this.simulationTextures = simulationTextures;
		Init();
	}

	//The pun
	//Ticks the simulation once
	public unsafe void tickSimulation() {
		data.runCount++;

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
			textureMetadataArray = textureMetadataArray,
			randomSeeds = randomSeeds
		};

		JobHandle jobHandle = job.Schedule(data.width * data.height, 13755);
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

	//Deletes the native arrays used by the simulation
	//A required step that you can do in OnDestroy or whenever you're done with the simulation object
	public void deleteNativeArrays() {
		readCells.Dispose();
		writeCells.Dispose();
		textureDataPointers.Dispose();
		textureMetadataArray.Dispose();
		randomSeeds.Dispose();
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
		data.width = simulationTextures[0].width;
		data.height = simulationTextures[0].height;

		convertTextureArrayFormats(simulationTextures);
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

		//Init the random seeds
		randomSeeds = new NativeArray<uint>(data.width * data.height, Allocator.Persistent);

		for (int q = 0; q < randomSeeds.Length; q++) {
			int val = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			randomSeeds[q] = *(uint*)&val;
		}

		//Fire off a different init function
		InitCells();
	}

	//Initialize the cell arrays
	private unsafe void InitCells() {
		//Native arrays
		readCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);
		writeCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);

		//Set the maximum per demographic to 0
		for (int q = 0; q < (int)PopulationRasterType.PopulationTypeCount; q++) {
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

				//Population
				for (int q = 0; q < (int)PopulationRasterType.PopulationTypeCount; q++) {
					Texture2D texture = populationTextures[q];
					Color32 color = texture.GetPixel(x, y);
					float numberOfPeople = colorToFloat(color);

					readCell.numberOfPeople[q] = numberOfPeople;
					readCell.susceptible[q] = numberOfPeople;
					readCell.infected[q] = 0;
					readCell.recovered[q] = 0;
					readCell.dead[q] = 0;

					//Keep track of the maximum
					if (numberOfPeople > data.maxNumberOfPeople[q]) {
						data.maxNumberOfPeople[q] = numberOfPeople;
					}
				}

				//If there's nobody here
				if (readCell.numberOfPeople[(int)PopulationRasterType.FullPopulation] == 0) {
					readCell.inMask = false;
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
		[ReadOnly]
		public NativeArray<uint> randomSeeds;

		const int FullPop = (int)PopulationRasterType.FullPopulation;

		//The function that gets called for every index
		public unsafe void Execute(int index) {
			if (!cellIsValid(index))
				return;
			Cell readCell = readCells[index];
			Cell writeCell = readCell;

			//Color32* textureDataPointer = (Color32*)textureDataPointers[texIdx];
			//Then just use it as an array

			//Random numbers
			Unity.Mathematics.Random random = new Unity.Mathematics.Random(randomSeeds[index] * data.runCount);

			//Spread in this cell, because of this cell
			float newExposed = data.beta * 
				((readCell.susceptible[FullPop] * readCell.infected[FullPop]) / readCell.numberOfPeople[FullPop]);
			float newInfected = data.alpha * readCell.exposed[FullPop];
			float newRecovered = data.gamma * readCell.infected[FullPop];

			writeCell.susceptible[FullPop] -= newExposed;
			writeCell.exposed[FullPop] += newExposed - newInfected;
			writeCell.infected[FullPop] += newInfected - newRecovered;
			writeCell.recovered[FullPop] += newRecovered;

			//Spread in this cell, because of other cells

			float exposedToBeTaken = 0;

			int[] neighborIndices = getNeighborIndices(index);
			for (int q = 0; q < neighborIndices.Length; q++) {
				if (cellIsValid(neighborIndices[q])) {
					Cell neighborCell = readCells[neighborIndices[q]];
					float newExposedNeighbor = data.beta *
						((neighborCell.susceptible[FullPop] * neighborCell.infected[FullPop])
						/ (float)neighborCell.numberOfPeople[FullPop]);
					exposedToBeTaken += newExposedNeighbor / 4.0f;
				}
			}

			if (exposedToBeTaken > writeCell.susceptible[FullPop])
				exposedToBeTaken = writeCell.susceptible[FullPop];
			writeCell.susceptible[FullPop] -= exposedToBeTaken;
			writeCell.exposed[FullPop] += exposedToBeTaken;

			//Clamp the numbers
			writeCell.susceptible[FullPop] = Mathf.Clamp(writeCell.susceptible[FullPop], 0, float.MaxValue);
			writeCell.exposed[FullPop] = Mathf.Clamp(writeCell.exposed[FullPop], 0, float.MaxValue);
			writeCell.infected[FullPop] = Mathf.Clamp(writeCell.infected[FullPop], 0, float.MaxValue);
			writeCell.recovered[FullPop] = Mathf.Clamp(writeCell.recovered[FullPop], 0, float.MaxValue);

			//Percentages
			float infectedPercentage = writeCell.infected[FullPop] / writeCell.numberOfPeople[FullPop];
			float deadPercentage = writeCell.dead[FullPop] / writeCell.numberOfPeople[FullPop];
			float recoveredPercentage = writeCell.recovered[FullPop] / writeCell.numberOfPeople[FullPop];

			//Compute the color
			Color color;

			//Need to do more with this
			if (data.drawProportion) {
				float max = Mathf.Log10(data.maxNumberOfPeople[FullPop]);
				color = new Color(
					Mathf.Log10(writeCell.infected[FullPop] / max),
					Mathf.Log10(writeCell.recovered[FullPop] / max),
					Mathf.Log10(writeCell.dead[FullPop] / max)
				);
			}
			else {
				color = new Color(infectedPercentage, recoveredPercentage, deadPercentage);
			}

			if (!data.drawInfected)
				color.r = 0.0f;
			if (!data.drawRecovered)
				color.g = 0.0f;
			if (!data.drawDead)
				color.b = 0.0f;

			float v = Mathf.Pow(
				1.0f - (readCell.numberOfPeople[data.drawDemographic] / data.maxNumberOfPeople[data.drawDemographic]),
				3.0f
			);
			color = Color.Lerp(color, new Color(v,v,v, 1.0f), 0.3f);
			color.a = 1f;
			
			//Write back the data
			drawTextureData[index] = color;
			writeCells[index] = writeCell;
		}

		//Returns the indices of the neighbors to an index
		//Indices may or may not be valid, or even in the array at all
		public int[] getNeighborIndices(int index) {
			int[] ret = new int[4];
			ret[0] = index - 1;
			ret[1] = index + 1;
			ret[2] = index + data.width;
			ret[3] = index - data.width;
			return ret;
		}

		//Verify if a cell is valid
		//Cell is in bounds and also in the mask
		public bool cellIsValid(int index) {
			return index >= 0 && index < (data.width * data.height) && readCells[index].inMask;
		}

		//Rounds a float
		public float round(float n) {
			return Mathf.Floor(n + 0.5f);
		}
		//Gets the fractional part of a float, [0,1)
		public float getFractionalPart(float n) {
			return n - Mathf.FloorToInt(n);
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
			//Verify that it's the same size
			if (array[q].width != data.width || array[q].height != data.height)
				throw new Exception("Texture #" + q.ToString() + " is not of the same height and width as the other textures");

			//Create dummy texture to copy the data in a new format
			Texture2D dummy = new Texture2D(data.width, data.height, TextureFormat.RGBA32, false);
			dummy.SetPixels(array[q].GetPixels());
			dummy.Apply();
			array[q] = dummy;
		}
	}
}
