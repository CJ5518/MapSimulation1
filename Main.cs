// Decompiled with JetBrains decompiler
// Type: Main
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DADC71AF-6ED1-41B5-9B7D-530B78799929
// Assembly location: C:\Users\carso\Desktop\Build\MapSimulation0_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour {
	public Texture2D populationData;
	public Texture2D temperaureData;
	private static Simulation simulation;
	private static Texture2D airplaneImage;
	private static Canvas canvas;
	private static GameObject imageGameObject;
	private static MovableRawImage drawImage;
	private static Button colorButton;
	private static Button resetButton;
	private static Button airportButton;
	private bool needEndButton;
	private GameObject panelGameObject;
	private GameObject panelDisplayImageGameObject;
	private static Slider weightSlider;
	private static Button plusButton;
	private static Button minusButton;
	private static Text indexText;
	private int currentEditingIndex;
	public List<Main.Airport> airports;

	private void Start() {
		Main.simulation = new Simulation(new Texture2D[1]
		{
	  this.populationData
		});
		int num = 0;
		while (num < this.populationData.width)
			++num;
		Main.airplaneImage = UnityEngine.Resources.Load<Texture2D>("AirplaneImage");
		Main.canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		Main.imageGameObject = GameObject.Find("Canvas/MapImage");
		Main.drawImage = Main.imageGameObject.GetComponent<MovableRawImage>();
		Main.drawImage.texture = (Texture)Main.simulation.drawTexture;
		Main.colorButton = GameObject.Find("Canvas/ColorButton").GetComponent<Button>();
		Main.colorButton.onClick.AddListener(new UnityAction(this.OnColorButtonClick));
		Main.colorButton = GameObject.Find("Canvas/ResetButton").GetComponent<Button>();
		Main.colorButton.onClick.AddListener(new UnityAction(this.OnResetButtonClick));
		Main.airportButton = GameObject.Find("Canvas/AirportButton").GetComponent<Button>();
		Main.airportButton.onClick.AddListener(new UnityAction(this.OnAirportButtonClick));
		this.panelGameObject = GameObject.Find("Canvas/Panel");
		this.panelDisplayImageGameObject = GameObject.Find("Canvas/Panel/DisplayImage");
		this.panelDisplayImageGameObject.GetComponent<RawImage>().texture = (Texture)Main.simulation.textureArray[this.currentEditingIndex];
		Main.weightSlider = GameObject.Find("Canvas/Panel/WeightSlider").GetComponent<Slider>();
		Main.weightSlider.value = 1f / (float)Main.simulation.textureArray.Length;
		Main.plusButton = GameObject.Find("Canvas/Panel/PlusButton").GetComponent<Button>();
		Main.minusButton = GameObject.Find("Canvas/Panel/MinusButton").GetComponent<Button>();
		Main.plusButton.onClick.AddListener(new UnityAction(this.OnPlusButtonClick));
		Main.minusButton.onClick.AddListener(new UnityAction(this.OnMinusButtonClick));
		Main.indexText = GameObject.Find("Canvas/Panel/IndexText").GetComponent<Text>();
		this.currentEditingIndex = 0;
		this.airports = new List<Main.Airport>();
	}

	private void Update() {
		this.updateAirports();
		Main.simulation.tickSimulation();
		Simulation.TextureMetadata textureMetadata = Main.simulation.textureMetadataArray[this.currentEditingIndex];
		textureMetadata.weight = Main.weightSlider.value;
		Main.simulation.textureMetadataArray[this.currentEditingIndex] = textureMetadata;
		if (Input.GetMouseButton(0) && (UnityEngine.Object)EventSystem.current.currentSelectedGameObject == (UnityEngine.Object)null) {
			Vector2 pixelFromScreenCoord = Main.drawImage.getPixelFromScreenCoord((Vector2)Input.mousePosition);
			int index = Main.simulation.coordToIndex(pixelFromScreenCoord);
			Simulation.Cell readCell = Main.simulation.readCells[index];
			readCell.health = 0.0f;
			Main.simulation.readCells[index] = readCell;
		}
		if (!Input.GetKeyDown(KeyCode.Q))
			return;
		this.panelGameObject.SetActive(!this.panelGameObject.activeSelf);
	}

	private void OnGUI() {
		if (!Event.current.type.Equals((object)EventType.Repaint))
			return;
		for (int index = 0; index < this.airports.Count; ++index) {
			Main.Airport airport = this.airports[index];
			GameObject gameObject1 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "s");
			GameObject gameObject2 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "e");
			Rect rect = gameObject1.GetComponent<RectTransform>().rect;
			Vector2 center1 = rect.center;
			rect = gameObject2.GetComponent<RectTransform>().rect;
			Vector2 center2 = rect.center;
			double percentMoved = (double)airport.percentMoved;
			Graphics.DrawTexture(new Rect(Vector2.Lerp(center1, center2, (float)percentMoved), new Vector2(20f, 20f)), (Texture)Main.airplaneImage);
		}
	}

	private void OnColorButtonClick() {
		if (Main.simulation.data.healthyColor == Color.green) {
			Main.simulation.data.virusColor = new Color(1f, 0.5529412f, 0.0f);
			Main.simulation.data.healthyColor = Color.blue;
		}
		else {
			Main.simulation.data.virusColor = Color.red;
			Main.simulation.data.healthyColor = Color.green;
		}
	}

	private void OnResetButtonClick() {
		for (int index = 0; index < this.airports.Count + 1; ++index) {
			GameObject gameObject1 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "s");
			if ((bool)(UnityEngine.Object)gameObject1)
				UnityEngine.Object.Destroy((UnityEngine.Object)gameObject1);
			GameObject gameObject2 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "e");
			if ((bool)(UnityEngine.Object)gameObject2)
				UnityEngine.Object.Destroy((UnityEngine.Object)gameObject2);
		}
		this.airports.Clear();
		Main.simulation.deleteNativeArrays();
		Main.simulation.Init();
		Main.drawImage.texture = (Texture)Main.simulation.drawTexture;
	}

	private void OnAirportButtonClick() {
		GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(GameObject.Find("Canvas/AirportButtonTemplate"));
		gameObject1.transform.SetParent(Main.imageGameObject.transform);
		gameObject1.GetComponent<RectTransform>().position = Input.mousePosition;
		Text componentInChildren = gameObject1.GetComponentInChildren<Text>();
		if (!this.needEndButton) {
			componentInChildren.text = this.airports.Count.ToString() + "s";
			this.needEndButton = true;
		}
		else {
			componentInChildren.text = this.airports.Count.ToString() + "e";
			this.needEndButton = false;
			this.airports.Add(new Main.Airport() {
				percentMoved = 0.0f,
				timeSinceFired = 0.0f
			});
			GameObject gameObject2 = new GameObject(this.airports.Count.ToString(), new System.Type[1]
			{
		typeof (RawImage)
			});
			gameObject2.transform.SetParent(Main.imageGameObject.transform);
			gameObject2.GetComponent<RectTransform>().sizeDelta = new Vector2(10f, 10f);
		}
		gameObject1.name = componentInChildren.text;
	}

	private void updateAirports() {
		for (int index = 0; index < this.airports.Count; ++index) {
			Main.Airport airport = this.airports[index];
			GameObject gameObject1 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "s");
			GameObject gameObject2 = GameObject.Find("Canvas/MapImage/" + index.ToString() + "e");
			GameObject.Find("Canvas/MapImage/" + index.ToString());
			Vector2 pixelFromScreenCoord1 = Main.drawImage.getPixelFromScreenCoord((Vector2)gameObject1.GetComponent<RectTransform>().position);
			Vector2 pixelFromScreenCoord2 = Main.drawImage.getPixelFromScreenCoord((Vector2)gameObject2.GetComponent<RectTransform>().position);
			airport.indexStart = Main.simulation.coordToIndex(pixelFromScreenCoord1);
			airport.indexEnd = Main.simulation.coordToIndex(pixelFromScreenCoord2);
			airport.timeSinceFired += Time.deltaTime;
			if ((double)airport.percentMoved >= 1.0) {
				if ((double)airport.carryHealth < 0.899999976158142) {
					Simulation.Cell readCell = Main.simulation.readCells[airport.indexEnd];
					readCell.health = airport.carryHealth;
					Main.simulation.readCells[airport.indexEnd] = readCell;
				}
				airport.percentMoved = 0.0f;
				airport.timeSinceFired = 0.0f;
				Vector3 position1 = gameObject1.GetComponent<RectTransform>().position;
				RectTransform component1 = gameObject1.GetComponent<RectTransform>();
				RectTransform component2 = gameObject2.GetComponent<RectTransform>();
				Vector3 position2 = component1.position;
				component1.position = component2.position;
				component2.position = position2;
			}
			if ((double)airport.timeSinceFired >= 5.0 || (double)airport.percentMoved > 0.0) {
				if ((double)airport.percentMoved == 0.0)
					airport.carryHealth = Main.simulation.readCells[airport.indexStart].health;
				float magnitude = (pixelFromScreenCoord1 - pixelFromScreenCoord2).magnitude;
				airport.percentMoved += 1f / magnitude;
			}
			this.airports[index] = airport;
		}
	}

	private void OnPlusButtonClick() {
		++this.currentEditingIndex;
		this.updateEditingIndex();
	}

	private void OnMinusButtonClick() {
		--this.currentEditingIndex;
		this.updateEditingIndex();
	}

	private void updateEditingIndex() {
		if (this.currentEditingIndex < 0)
			this.currentEditingIndex = 0;
		if (this.currentEditingIndex >= Main.simulation.textureMetadataArray.Length)
			this.currentEditingIndex = Main.simulation.textureMetadataArray.Length - 1;
		Main.indexText.text = this.currentEditingIndex.ToString();
		Main.weightSlider.value = Main.simulation.textureMetadataArray[this.currentEditingIndex].weight;
		this.panelDisplayImageGameObject.GetComponent<RawImage>().texture = (Texture)Main.simulation.textureArray[this.currentEditingIndex];
	}

	private void OnDestroy() => Main.simulation.deleteNativeArrays();

	public struct Airport {
		public int indexStart;
		public int indexEnd;
		public float percentMoved;
		public float timeSinceFired;
		public float carryHealth;
	}
}
