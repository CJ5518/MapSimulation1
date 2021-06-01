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
    public Texture2D[] textureArray;
    public NativeArray<Simulation.TextureMetadata> textureMetadataArray;
    public NativeArray<IntPtr> textureDataPointers;
    public Color backgroundColor = new Color(0.2392157f, 0.6f, 0.7686275f);
    public Texture2D drawTexture;
    private const int batchCount = 13755;
    public Simulation.SimulationDataStruct data = new Simulation.SimulationDataStruct() {
        season = 0.0f,
        seasonAdder = 0.0005f,
        virusColor = Color.red,
        healthyColor = Color.green,
        propensityForVirusModifier = 0.0f
    };
    public NativeArray<Simulation.Cell> writeCells;
    public NativeArray<Simulation.Cell> readCells;

    public Simulation(Texture2D[] textures) {
        this.textureArray = textures;
        this.Init();
    }

    public unsafe void tickSimulation() {
        for (int index = 0; index < this.textureArray.Length; ++index)
            this.textureDataPointers[index] = new IntPtr(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<Color32>(this.textureArray[index].GetRawTextureData<Color32>()));
        new Simulation.SimulationJob() {
            readCells = this.readCells,
            writeCells = this.writeCells,
            drawTextureData = this.drawTexture.GetRawTextureData<Color32>(),
            data = this.data,
            textureDataPointers = this.textureDataPointers,
            textureMetadataArray = this.textureMetadataArray
        }.Schedule<Simulation.SimulationJob>(this.data.width * this.data.height, 13755).Complete();
        this.drawTexture.Apply();
        NativeArray<Simulation.Cell> readCells = this.readCells;
        this.readCells = this.writeCells;
        this.writeCells = readCells;
        this.data.season += this.data.seasonAdder;
        if ((double)this.data.season <= 1.0 && (double)this.data.season >= 0.0)
            return;
        this.data.seasonAdder *= -1f;
    }

    public void deleteNativeArrays() {
        this.readCells.Dispose();
        this.writeCells.Dispose();
        this.textureDataPointers.Dispose();
        this.textureMetadataArray.Dispose();
    }

    public void Init() {
        this.textureMetadataArray = new NativeArray<Simulation.TextureMetadata>(this.textureArray.Length, Allocator.Persistent);
        this.textureDataPointers = new NativeArray<IntPtr>(this.textureArray.Length, Allocator.Persistent);
        for (int index = 0; index < this.textureMetadataArray.Length; ++index) {
            Simulation.TextureMetadata textureMetadata = this.textureMetadataArray[index];
            textureMetadata.weight = 1f / (float)this.textureMetadataArray.Length;
            this.textureMetadataArray[index] = textureMetadata;
        }
        this.data.width = this.textureArray[0].width;
        this.data.height = this.textureArray[0].height;
        for (int index = 0; index < this.textureArray.Length; ++index) {
            if (this.textureArray[index].width != this.data.width || this.textureArray[index].height != this.data.height)
                throw new Exception("Texture #" + index.ToString() + " is not of the same height and width as the other textures");
            Texture2D texture2D = new Texture2D(this.data.width, this.data.height, TextureFormat.RGBA32, false);
            texture2D.SetPixels(this.textureArray[index].GetPixels());
            texture2D.Apply();
            this.textureArray[index] = texture2D;
        }
        this.drawTexture = new Texture2D(this.data.width, this.data.height, TextureFormat.RGBA32, false);
        this.drawTexture.filterMode = FilterMode.Point;
        for (int x = 0; x < this.data.width; ++x) {
            for (int y = 0; y < this.data.height; ++y)
                this.drawTexture.SetPixel(x, y, this.backgroundColor);
        }
        this.InitCells();
    }

    public void InitCells() {
        this.readCells = new NativeArray<Simulation.Cell>(this.data.width * this.data.height, Allocator.Persistent);
        this.writeCells = new NativeArray<Simulation.Cell>(this.data.width * this.data.height, Allocator.Persistent);
        for (int x = 0; x < this.data.width; ++x) {
            for (int y = 0; y < this.data.height; ++y) {
                int index1 = this.coordToIndex(x, y);
                Simulation.Cell readCell = this.readCells[index1];
                readCell.health = 1f;
                readCell.inMask = true;
                for (int index2 = 0; index2 < this.textureArray.Length; ++index2) {
                    if ((double)this.textureArray[index2].GetPixel(x, y).a < 0.200000002980232) {
                        readCell.inMask = false;
                        break;
                    }
                }
                this.readCells[index1] = readCell;
                this.writeCells[index1] = readCell;
            }
        }
    }

    public int coordToIndex(int x, int y) => y * this.data.width + x;

    public int coordToIndex(Vector2 vec) => (int)vec.y * this.data.width + (int)vec.x;

    public Vector2 indexToCoord(int index) {
        Vector2 vector2;
        vector2.x = (float)(index % this.data.width);
        vector2.y = ((float)index - vector2.x) / (float)this.data.width;
        return vector2;
    }

    public struct SimulationDataStruct {
        public int width;
        public int height;
        public float season;
        public float seasonAdder;
        public Color virusColor;
        public Color healthyColor;
        public float propensityForVirusModifier;
    }

    public struct TextureMetadata {
        public float weight;
    }

    public struct Cell {
        public float propensityForVirus;
        public float propensityToHeal;
        public float health;
        public bool inMask;
    }

    public struct SimulationJob : IJobParallelFor {
        [ReadOnly]
        public NativeArray<Simulation.Cell> readCells;
        [WriteOnly]
        public NativeArray<Simulation.Cell> writeCells;
        [WriteOnly]
        public NativeArray<Color32> drawTextureData;
        [ReadOnly]
        public Simulation.SimulationDataStruct data;
        [ReadOnly]
        public NativeArray<IntPtr> textureDataPointers;
        [ReadOnly]
        public NativeArray<Simulation.TextureMetadata> textureMetadataArray;

        public unsafe void Execute(int index) {
            if (!this.cellIsValid(index))
                return;
            Simulation.Cell readCell = this.readCells[index];
            Simulation.Cell cell = readCell;
            float num1 = 0.0f;
            int num2 = 0;
            int[] neighborIndices = this.getNeighborIndices(index);
            for (int index1 = 0; index1 < neighborIndices.Length; ++index1) {
                if (this.cellIsValid(neighborIndices[index1])) {
                    ++num2;
                    num1 += this.readCells[neighborIndices[index1]].health;
                }
            }
            float num3 = num1 / (float)num2;
            if ((double)num3 != 1.0) {
                float num4 = Mathf.Clamp01(Mathf.Pow(readCell.propensityForVirus, 3f));
                cell.health -= (float)(1.5 * (1.0 - (double)num3)) * num4;
                cell.health = Mathf.Clamp01(cell.health);
            }
            float num5 = 0.0f;
            for (int index1 = 0; index1 < this.textureDataPointers.Length; ++index1) {
                Color32* textureDataPointer = (Color32*)(void*)this.textureDataPointers[index1];
                num5 += (float)textureDataPointer[index].r / (float)byte.MaxValue * this.textureMetadataArray[index1].weight;
            }
            cell.propensityForVirus = Mathf.Clamp01(num5 + this.data.propensityForVirusModifier);
            cell.propensityToHeal = 1f - cell.propensityForVirus;
            Color color = Color.Lerp((double)readCell.health > 1.0 ? Color.Lerp(Color.white, this.data.healthyColor, readCell.health - 1f) : Color.Lerp(Color.white, this.data.virusColor, 1f - readCell.health), new Color(1f - readCell.propensityForVirus, 1f - readCell.propensityForVirus, 1f - readCell.propensityForVirus, 1f), 0.5f);
            color.a = 1f;
            this.drawTextureData[index] = (Color32)color;
            this.writeCells[index] = cell;
        }

        private bool cellIsValid(int index) => index >= 0 && index < this.data.width * this.data.height && this.readCells[index].inMask;

        public int[] getNeighborIndices(int index) => new int[4]
        {
      index - 1,
      index + 1,
      index + this.data.width,
      index - this.data.width
        };
    }
}
