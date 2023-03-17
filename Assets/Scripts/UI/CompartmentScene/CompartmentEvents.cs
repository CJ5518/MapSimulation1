using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class CompartmentEvents
{
	public delegate void CompartmentIntegerEventHandler(int newValue);
	public delegate void CompartmentQuestionIntEventHandler(CompartmentEnum.Questions question, int newValue);
	public delegate void CompartmentQuestionFloatEventHandler(CompartmentEnum.Questions question, float newValue);


	//THESE VALUES MEAN NOTHING, CHECK INIT
	public static int currVal = 0;
	public static int oldVal = 0;
	public static int maxVal = 14;
	public static int minVal = 0;

	//Integer
	public static event CompartmentIntegerEventHandler OnValueChange;
	public static event CompartmentIntegerEventHandler OnLoadPreset;
	public static event CompartmentQuestionIntEventHandler OnConfidenceChange;

	//Float
	public static event CompartmentQuestionFloatEventHandler OnQuestionValueChange;

	public static void init() {
		currVal = 0;
		oldVal = 0;
		maxVal = 14;
		minVal = 0;

		//Clear the old event handlers because shit gets fucked if we don't when we reload the scene
		if (OnValueChange != null) {
			foreach(Delegate d in OnValueChange?.GetInvocationList()) {
				OnValueChange -= (CompartmentIntegerEventHandler)d;
			}
		}
		if (OnLoadPreset != null) {
			foreach(Delegate d in OnLoadPreset?.GetInvocationList()) {
				OnLoadPreset -= (CompartmentIntegerEventHandler)d;
			}
		}
		if (OnConfidenceChange != null) {
			foreach(Delegate d in OnConfidenceChange?.GetInvocationList()) {
				OnConfidenceChange -= (CompartmentQuestionIntEventHandler)d;
			}
		}
		if (OnQuestionValueChange != null) {
			foreach(Delegate d in OnQuestionValueChange?.GetInvocationList()) {
				OnQuestionValueChange -= (CompartmentQuestionFloatEventHandler)d;
			}
		}
	}

	/// <summary>
	/// Sets current question value
	/// </summary>
	/// <param name="newValue"></param>
	public static void SetNewValue(int newValue)
	{
		//Debug.Log($"SetNewValue {newValue}");
		if (newValue == currVal)
		{
			return;
		}
		if(!(minVal <= newValue && newValue <= maxVal))
		{
			//Debug.Log($"SetNewVal OOB: {newValue} Truth values {minVal <= newValue} && {newValue <= maxVal} : {minVal <= newValue && newValue <= maxVal} ! {!(minVal <= newValue && newValue <= maxVal)}");
			return;
		}
		oldVal = currVal;
		currVal = newValue;
		//Debug.Log($"Old: {oldVal} New: {newValue} Curr: {currVal} ");
		OnValueChange?.Invoke(newValue);
	}

	public static void IncValue()
	{
		if (currVal + 1 > maxVal) return;
		oldVal = currVal;
		OnValueChange?.Invoke(++currVal);
	}

	public static void DecValue()
	{
		if (currVal - 1 < minVal) return;
		oldVal = currVal;
		OnValueChange?.Invoke(--currVal);
	}

	/// <summary>
	/// Sets question's value
	/// </summary>
	/// <param name="question"></param>
	/// <param name="value"></param>
	public static void SetNewQuestionValue(CompartmentEnum.Questions question, float value)
	{
		OnQuestionValueChange?.Invoke(question, value);
	}

	/// <summary>
	/// Sets question's confidence
	/// </summary>
	/// <param name="question"></param>
	/// <param name="value"></param>
	public static void SetNewConfidence(CompartmentEnum.Questions question, int value)
	{
		OnConfidenceChange?.Invoke(question, value);
	}

	public static void LoadPreset(int value)
	{
		Debug.Log("Loading preset");
		SimulationSetupData.useTheseNumbers = true;
		OnLoadPreset?.Invoke(value);
	}
}
