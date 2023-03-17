using TMPro;
using UnityEngine;
using UnityEngine.UI;

//The topmost bar that shows the 
public class PopulationDisplayBar : MonoBehaviour {
	//The total population number
	float totalPopulationCount;
	const float fullBarWidth = 1160;

	public GameObject templateImage;

	public struct Bar {
		public float count;
		public float barWidth;
		public Image barObject;
		public TextMeshProUGUI barText;
	}
	Bar[] bars;


	void Start() {

	}

	public void GenerateBars(SimulationModel model) {
		bars = new Bar[model.compartmentCount];
		for (int i = 0; i < bars.Length; i++) {
			bars[i].barObject = GenerateBarImageObject(model.compartmentInfoArray[i].mapDisplayColor).GetComponent<Image>();
			bars[i].barText = bars[i].barObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
		}
	}

	private GameObject GenerateBarImageObject(Color32 color) {
		GameObject obj = GameObject.Instantiate(templateImage, templateImage.transform.parent);
		obj.GetComponent<Image>().color = color;

		obj.SetActive(true);
		return obj;
	}

	public void UpdateBars() {
		if (bars == null) {
			GenerateBars(SimulationManager.simulation.model);
		}
		totalPopulationCount = SimulationManager.stats.globalTotals.numberOfPeople;
		for (int q = 0; q < SimulationManager.stats.globalTotals.stateCount; q++) {
			UpdateBarGivenCount(q, SimulationManager.stats.globalTotals.state[q]);
		}
	}


	void UpdateBarGivenCount(int index, float count) {
		float percent = count / totalPopulationCount;

		bars[index].count = count;
		bars[index].barWidth = fullBarWidth * percent;
		bars[index].barObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bars[index].barWidth);
		
		if (bars[index].barWidth < 62f) bars[index].barText.text = " ";
		else if (bars[index].barWidth < 233f) bars[index].barText.text = (percent).ToString("#0%");
		else bars[index].barText.text = count.ToString("#,##0,,M") + (percent).ToString(" (#0%)");
	}
}