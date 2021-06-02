// Decompiled with JetBrains decompiler
// Type: Simulation
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class Simulation {
	//Array of input textures
	public Texture2D[] textureArray;
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
	public struct Cell {
		public float propensityForVirus; //How much we want virus
		public float propensityToHeal; //how much we want to heal
		public float health;
		public bool inMask; //Are we in the mask
	}

	//Texture metadata struct
	public struct TextureMetadata {
		public float weight;
	}

	//Struct that holds all the data needed by the simulation
	public struct SimulationDataStruct {
		public int width; //Dimensions of input/output textures
		public int height;
		public float season; //Unused
		public float seasonAdder;
		public Color virusColor; //Colors for unhealthy/healthy cells
		public Color healthyColor;
		public float propensityForVirusModifier; //Added to propensity for virus
	}
	//Set the default values here
	public SimulationDataStruct data = new SimulationDataStruct() {
		season = 0.0f,
		seasonAdder = 0.0005f,
		virusColor = Color.red,
		healthyColor = Color.green,
		propensityForVirusModifier = 0.0f
	};

	//Cell buffers
	//Internally: you always read from readCells, and write to writeCells
	//Externally: writeCells is fairly useless, read/write to readCells if you want to read/change state
	public NativeArray<Cell> writeCells;
	public NativeArray<Cell> readCells;


	//Constructor
	//All textures must be of the same width and height, different formats are allowed
	public Simulation(Texture2D[] textures) {
		textureArray = textures;
		Init();
	}

	//The pun
	//Ticks the simulation once
	public unsafe void tickSimulation() {
		//The unsafe part
		//Get pointers to the raw texture data
		for (int q = 0; q < textureArray.Length; q++) {
			textureDataPointers[q] = new IntPtr(
				NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(textureArray[q].GetRawTextureData<Color32>())
			);
		}
		//Create and start the simulation
		var job = new SimulationJob() {
			readCells = readCells,
			writeCells = writeCells,
			drawTextureData = this.drawTexture.GetRawTextureData<Color32>(),
			data = this.data,
			textureDataPointers = this.textureDataPointers,
			textureMetadataArray = this.textureMetadataArray
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
	}

	//Init everything
	public void Init() {
		//Native arrays
		textureMetadataArray = new NativeArray<TextureMetadata>(textureArray.Length, Allocator.Persistent);
		textureDataPointers = new NativeArray<IntPtr>(textureArray.Length, Allocator.Persistent);

		//Default initialization for textureMetadata
		for (int q = 0; q < textureMetadataArray.Length; q++) {
			TextureMetadata textureMetadata = textureMetadataArray[q];
			textureMetadata.weight = 1.0f / textureMetadataArray.Length;
			textureMetadataArray[q] = textureMetadata;
		}
		//We make the first one the example
		data.width = textureArray[0].width;
		data.height = textureArray[0].height;

		//Converting the texture formats
		for (int q = 0; q < textureArray.Length; q++) {
			//Verify that it's the same size
			if (textureArray[q].width != data.width || textureArray[q].height != data.height)
				throw new Exception("Texture #" + q.ToString() + " is not of the same height and width as the other textures");

			//Create dummy texture to copy the data in a new format
			Texture2D dummy = new Texture2D(data.width, data.height, TextureFormat.RGBA32, false);
			dummy.SetPixels(textureArray[q].GetPixels());
			dummy.Apply();
			textureArray[q] = dummy;
		}
		
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

	private void InitCells() {
		//Native arrays
		readCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);
		writeCells = new NativeArray<Cell>(data.width * data.height, Allocator.Persistent);

		//For every cell
		for (int x = 0; x < data.width; x++) {
			for (int y = 0; y < data.height; y++) {
				int index = coordToIndex(x, y);

				//Set some default cell values
				Cell readCell = readCells[index];
				readCell.health = 1f;

				//If it's in the mask, and therefore should be updated
				readCell.inMask = true;
				for (int q = 0; q < textureArray.Length; q++) {
					if (textureArray[q].GetPixel(x, y).a < 0.2f) {
						readCell.inMask = false;
						break;
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

		//The function that gets called for every index
		public unsafe void Execute(int index) {
			if (!cellIsValid(index))
				return;
			Cell readCell = readCells[index];
			Cell writeCell = readCell;

			//Color32* textureDataPointer = (Color32*)textureDataPointers[texIdx];
			//Then just use it as an array


			//Get a healthyness percentage
			float sumHealth = 0.0f;
			int numNeighbors = 0;
			int[] neighborIndices = getNeighborIndices(index);
			for (int q = 0; q < neighborIndices.Length; q++) {
				if (cellIsValid(neighborIndices[q])) {
					numNeighbors++;
					sumHealth += readCells[neighborIndices[q]].health;
				}
			}
			float healthyPercentage = sumHealth / (float)numNeighbors;

			//Damage yourself
			if (healthyPercentage < 1.0f) {
				float factor = Mathf.Clamp01(Mathf.Pow(readCell.propensityForVirus, 3f));
				writeCell.health -= (1.5f * (1.0f - healthyPercentage)) * factor;
				writeCell.health = Mathf.Clamp01(writeCell.health);
			}

			//Calculate the propensity for virus
			float propensityForVirus = 0.0f;
			for (int q = 0; q < textureDataPointers.Length; q++) {
				Color32* textureDataPointer = (Color32*)textureDataPointers[q];
				propensityForVirus += (textureDataPointer[index].r / (float)byte.MaxValue) * textureMetadataArray[q].weight;
			}
			//Set the values
			writeCell.propensityForVirus = Mathf.Clamp01(propensityForVirus + this.data.propensityForVirusModifier);
			writeCell.propensityToHeal = 1f - writeCell.propensityForVirus;

			//Compute the color
			Color color = Color.Lerp(Color.white, data.virusColor, 1.0f - writeCell.health);
			color.a = 1f;

			//Write back the data
			drawTextureData[index] = color;
			writeCells[index] = writeCell;
		}
		//Cell is in bounds and also in the mask
		private bool cellIsValid(int index) {
			return index >= 0 && index < (data.width * data.height) && readCells[index].inMask;
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
}
