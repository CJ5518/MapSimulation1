using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChartUtil;

public class GraphPanelChartButton : MonoBehaviour {
	public TMP_Text text;
	public Chart associatedChart;
	public GraphsPanel mothership;
	public string stateName;

	public void onClick() {
		BehaviourLogger.logItem(stateName + "GraphButtonClicked");
		mothership.onButtonClick(associatedChart);
	}
}
