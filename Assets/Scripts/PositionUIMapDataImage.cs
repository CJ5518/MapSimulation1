using UnityEngine;
using UnityEngine.UI;

public class PositionUIMapDataImage : MonoBehaviour {

	public Slider slider;
	int sliderValue;

	[System.Serializable]
	public struct Item {
		public Transform image;
		public Vector2Int activeSliderValueRange;
		[HideInInspector] public Vector3 positionGoal;
		[HideInInspector] public Vector3 rotationGoal;
	}
	public Item[] items;
	float damping = 5f;
	
    void Start() {
		slider.onValueChanged.AddListener(UpdateValue);
	}

	void Update() {
		for (int i = 0; i < items.Length; i++) {
			items[i].image.position = Vector3.Lerp(items[i].image.position,
				items[i].positionGoal, Time.deltaTime * damping);
			items[i].image.rotation = Quaternion.Lerp(items[i].image.rotation,
				Quaternion.Euler(items[i].rotationGoal), Time.deltaTime * damping);
		}
	}


	void UpdateValue(float value) {
		sliderValue = Mathf.RoundToInt(value);
		//Logger.Log("slider value: " + sliderValue);
		UpdatePositionGoals();
	}
	void UpdatePositionGoals() {
		for(int i = 0; i < items.Length; i++) {
			bool lessThanMin = sliderValue < items[i].activeSliderValueRange.x;
			bool greaterThanMax = sliderValue > items[i].activeSliderValueRange.y;

			if (lessThanMin) {
				int difference = items[i].activeSliderValueRange.x - sliderValue;
				items[i].positionGoal = Vector3.zero - Vector3.up * (3 + difference / 2f);
				items[i].rotationGoal = new Vector3(90, 0, 0);
			}
			else if (greaterThanMax) {
				int difference = sliderValue - items[i].activeSliderValueRange.y;
				items[i].positionGoal = Vector3.zero + Vector3.up * (3 + difference / 2f);
				items[i].rotationGoal = new Vector3(90, 0, 0);
			}
			else {
				items[i].positionGoal = Vector3.zero;
				items[i].rotationGoal = Vector3.zero;
			}
		}
	}
}