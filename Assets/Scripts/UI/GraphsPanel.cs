using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ChartUtil;

public class GraphsPanel : MonoBehaviour {
	List<Chart> charts = new List<Chart>();
	public Chart chartTemplate;
	public GraphPanelChartButton buttonTemplate;
	public GameObject chartParent;
	public GameObject buttonParent;

	Chart activeChart = null;

	SortedDictionary<string, Transform> stateButtons = new SortedDictionary<string, Transform>();

	public Chart makeEntry(string text, bool noSort = false) {
		Chart newChart = Instantiate(chartTemplate, chartParent.transform);
		charts.Add(newChart);

		//Make the button
		GraphPanelChartButton newButton = Instantiate(buttonTemplate, buttonParent.transform);
		newButton.text.text = text;
		newButton.associatedChart = newChart;
		newButton.mothership = this;
		newButton.stateName = text;
		if (!noSort)
			stateButtons.Add(text, newButton.transform);
		

		newChart.gameObject.SetActive(false);
		newButton.gameObject.SetActive(true);

		return newChart;
	}

	public Chart getEntry(int idx) {
		return charts[idx];
	}

	public void sortEntries() {
		int q = 2;
		foreach(KeyValuePair<string, Transform> entry in stateButtons) {
			entry.Value.SetSiblingIndex(q);
			q++;
		}
	}

	public void onButtonClick(Chart chart) {

		if (activeChart != null) {
			activeChart.Clear();
			activeChart.gameObject.SetActive(false);
		}
		activeChart = chart;
		activeChart.gameObject.SetActive(true);
		activeChart.UpdateChart();
	}
}