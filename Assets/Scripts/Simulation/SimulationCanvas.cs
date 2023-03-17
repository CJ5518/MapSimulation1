//#define GRAPHS_ON

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChartUtil;

//Click on the color thingies

public class SimulationCanvas : MonoBehaviour
{
	#region Refernces Vars
	public Main main;
	const float statisticsUpdatesPerSecond = 100.0f;
	float lastStatsTime = -100.0f;
	
	public ParameterPanel parameterPanel;
	[Header("The dt Slider")]
	public ParameterSlider dtSlider;

	[Header("Misc Thingies")]
	public TMP_Text topCanvasText;
	public TMP_Text simulatedDtText;
	public GameObject helpPanel;
	public Chart stateChart;
	public Chart totalChart;
	public PopulationDisplayBar populationDisplayBar;
	public TMP_Text stateGraphTextTitle;

	int activeChartIdx = 0;

	#endregion
	//Juicy function
	public void UpdateCanvas() {
		updateStatisticsLabel();
	}

	//Go from state idx to the idx that can be used to index the list of state on-hover masks
	Dictionary<int,int> stateIdxToHoverStateIdx;

	void Start() {
		//Load in stateIdxToHoverStateIdx
		stateIdxToHoverStateIdx = new Dictionary<int, int>();
		List<string> alphabeticalList = new List<string>(SimulationManager.stats.stateNames);
		alphabeticalList.Sort();
		for (int q = 0; q < alphabeticalList.Count; q++) {
			int actualStateIdx = -1;

			//Find what this state idx is
			for (int i = 0; i < SimulationManager.stats.stateNames.Count; i++) {
				if (SimulationManager.stats.stateNames[i] == alphabeticalList[q]) {
					actualStateIdx = i;
					break;
				}
			}
			stateIdxToHoverStateIdx.Add(actualStateIdx, q);
		}
		populationDisplayBar.GenerateBars(SimulationManager.simulation.model);
		populationDisplayBar.UpdateBars();
	}

	int prevHoverStateIdx = -1;
	void Update() {

		//Check which state we're hovering over
		Vector2? hoverLatLongMaybe = getLatLongFromScreenCoord(Input.mousePosition);
		bool onDifferentState = false;
		if (hoverLatLongMaybe != null) {
			Vector2 hoverLatLong = (Vector2)hoverLatLongMaybe;
			//Check if we're in the exact same state as we previously were
			//To improve performance
			if (prevHoverStateIdx >= 0) {
				//If this isn't true, we should search all the other states
				if (!Simulation.IsPointInPolygon(SimulationManager.stats.stateShapes[prevHoverStateIdx], hoverLatLong)) {
					prevHoverStateIdx = SimulationManager.stats.getStateIdxFromLatLong(hoverLatLong);
					onDifferentState = true;
				}
			} else {
				prevHoverStateIdx = SimulationManager.stats.getStateIdxFromLatLong(hoverLatLong);
			}
		}


		//Set the mask to the correct state
		if (prevHoverStateIdx >= 0 && onDifferentState) {
			string stateName = SimulationManager.stats.stateNames[prevHoverStateIdx];
			stateName = stateName.Replace(" ", System.String.Empty);
			SerializableListOfStateMasks stateMasks = SimulationManager.objectWithMeshRenderer.GetComponent<SerializableListOfStateMasks>();
			Material material = SimulationManager.objectWithMeshRenderer.GetComponent<MeshRenderer>().material;
			material.SetTexture("_State", stateMasks.stateTextures[stateIdxToHoverStateIdx[prevHoverStateIdx]]);

			//And set the chart data
			//stateChart.chartData = SimulationManager.stats.charts[prevHoverStateIdx].GetComponent<ChartData>();
			stateGraphTextTitle.text = SimulationManager.stats.charts[prevHoverStateIdx].GetComponent<ChartOptions>().title.mainTitle;
			SimulationManager.stats.charts[activeChartIdx].SetActive(false);
			activeChartIdx = prevHoverStateIdx;
			//Only set this active if the panel is open
			SimulationManager.stats.charts[activeChartIdx].SetActive(stateChart.gameObject.GetComponentInParent<WallPanelOpenClose>().open);
		}
	}

	#region Toggles
	/*
	 * Connect Toggle to this; default state of toggle should match whatever default state the game loads with or it will be flopped
	 */
	public void ToggleMove() {
		SimulationManager.simulation.moveZombies = !SimulationManager.simulation.moveZombies;
	}
	#endregion
	#region Sliders
	/*
	 * Connect Sliders here; must be in same order
	 */
	public void UpdateSliderValues() {
		parameterPanel.updateParameters(ref SimulationManager.simulation.model, ref SimulationManager.simulation.movementModel);
	}
	#endregion
	

	int lastIndex = -1;
	public unsafe void updateStatisticsLabel() {
		if (SimulationManager.simulation.dtSimulated < 10000.0f) {
			bool panelIsOpen = totalChart.gameObject.GetComponentInParent<WallPanelOpenClose>().open;
			if (panelIsOpen) {
				totalChart.UpdateChart();
				totalChart.gameObject.SetActive(true);
			} else {
				totalChart.gameObject.SetActive(false);
			}
		}

		if (prevHoverStateIdx >= 0) {
			bool panelIsOpen = stateChart.gameObject.GetComponentInParent<WallPanelOpenClose>().open;
			if (panelIsOpen) {
				SimulationManager.stats.charts[activeChartIdx].GetComponent<Chart>().UpdateChart();
				SimulationManager.stats.charts[activeChartIdx].SetActive(true);
			} else {
				SimulationManager.stats.charts[activeChartIdx].SetActive(false);
			}
		}

		populationDisplayBar.UpdateBars();


		//Count up the airports for a super accurate reading of total sus
		//Which isn't totally correct any more big sad.
		//for (int q = 0; q < main.simulation.airplanes.Count; q++) {
		//	totalSus += main.simulation.airplanes[q].susceptible;
		//}
		//Set top thingy text
		topCanvasText.text = SimulationManager.stats.globalTotals.ToString();

		//Set the simulated dt text
		//simulatedDtText.text = main.simulation.dtSimulated.ToString("F1") + " Hours";
	}

	//Get a pixel on the texture from a screen (commonly mouse) coordinate
	//Technically this function gets a texture coord an *any* quad clicked on
	public static Vector2 getPixelFromScreenCoord(Vector2 coord) {
		RaycastHit hit;
		//Didn't hit anything
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
			return new Vector2(-1,-1);

		
		Renderer rend = hit.transform.GetComponent<Renderer>();
		MeshCollider meshCollider = hit.collider as MeshCollider;

		Texture tex = rend.material.mainTexture;

		//Prevents errors if we hit an untextured quad
		if (tex == null)
			return new Vector2(-1,-1);

		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= tex.width;
		pixelUV.y *= tex.height;
		return pixelUV;
	}

	public Vector3 getRealCoordFromSimCoord(Vector2Int coord) {
		return getRealCoordFromSimCoord((Vector2)coord);
	}
	
	public Vector3 getRealCoordFromSimCoord(Vector2 coord) {
		
		//Fetch the Collider from the GameObject
		Collider m_Collider = SimulationManager.objectWithMeshRenderer.GetComponent<Collider>();
		//Fetch the center of the Collider volume
		Vector3 m_Center = m_Collider.bounds.center;
		//Fetch the size of the Collider volume
		Vector3 m_size = m_Collider.bounds.size;
		//Fetch the minimum and maximum bounds of the Collider volume
		Vector3 m_Min = m_Collider.bounds.min;
		Vector3 m_Max = m_Collider.bounds.max;

		//21 pixels up top
		//5 pixels on the left (washington)
		//27 on the right
		//2 on the bottom


		//Vector2 texCoord = SimulationCanvas.getPixelFromScreenCoord(Input.mousePosition);
		float xPercent = (coord.x - main.xCoordSub)/ ((float)main.simulation.width - main.widthSub);
		float yPercent = (coord.y-main.yCoordSub) / ((float)main.simulation.height-main.heightSub);
		float xPos = Mathf.Lerp(m_Max.x, m_Min.x, xPercent);
		float yPos = Mathf.Lerp(m_Min.y, m_Max.y, yPercent);

		Vector3 top = new Vector3(xPos, yPos, m_Max.z);
		Vector3 bottom = new Vector3(xPos, yPos, m_Min.z);
		RaycastHit hit;
		bool didHit = Physics.Raycast(top, new Vector3(0,0,-1), out hit, 20.0f);



		return didHit ? hit.point : bottom;
	}
	

	//Gets lat longs using the above function
	public static Vector2? getLatLongFromScreenCoord(Vector2 coord) {
		Vector2 renderSpaceCoord = getPixelFromScreenCoord(coord);
		if (renderSpaceCoord.x < 0) {
			return null;
		}
		return Projection.renderSpaceToLatLongs(renderSpaceCoord);
	}

}
