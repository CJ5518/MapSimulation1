using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CompartmentEnum : MonoBehaviour
{
	/// <summary>
	/// Enum for the questions in the disease model scene.
	/// Add to end if modifying. Adding in middle or beginning causes cascading index errors applied by Unity caring about
	/// the index rather than the name.
	/// </summary>
	[Serializable]
	public enum Questions
	{
		None,
		Presets,
		ContactRate,
		InfectionProbability,
		EILatency,
		RecoveryRate,
		MortalityRate,
		VaccinationDistribution,
		VaccinationUptake,
		BreakthroughRate,
		WaningImmunityRate,
		MovementModel,
		DiseaseImmobilization,
		Introduction,
		AirplaneSecurityAgainstExposed,
		Elevation,
		Highways,
		Water,
		Income,
		VaccinationHesitancy,
		LayerIntroduction,
		PopMovementIntroduction,
		AirplaneSecurityAgainstInfected,
		Income2,
		IncomeInfectionRate,
		IncomeVaccineEffectiveness,
		IncomeDiseaseOnsetTime,
		IncomeMortalityRate,
		IncomeWaningImmunity,
		Predictions,
		PredictionInfectionLength,
		PredictionInfectionCount,
		PredictionDeadCount,
		PredictionHomeState,
		PredictionAffectHomeState,
		InfectionDuration,
		GillespieQuestion,
		Finalize
	}

	/// <summary>
	/// Gives bounds to sliders
	/// </summary>
	public struct QuestionData
	{
		public float min;
		public float max;
	}

	/// <summary>
	/// Holds the bounds for the sliders
	/// </summary>
	public static QuestionData[] QuestionsMinMax;
	
	/// <summary>
	/// List that stores the current order of questions. Each question self enrolls at start.
	/// </summary>
	public static List<Questions> QuestionIndex;

	private void Awake()
	{
		CompartmentEvents.init();
		//Create slider bounds objects
		QuestionsMinMax = new QuestionData[Enum.GetNames(typeof(Questions)).Length];
		for(int i = 0; i < QuestionsMinMax.Length; i++)
		{
			QuestionsMinMax[i] = new QuestionData();
		}

		QuestionIndex = new List<Questions>(Enum.GetNames(typeof(Questions)).Length);
		
		SetMinMax(Questions.ContactRate, 0f, 250f);
		SetMinMax(Questions.InfectionProbability, 0f, 1.0f);
		SetMinMax(Questions.EILatency, 1f, 150f);
		SetMinMax(Questions.RecoveryRate, 0f, 1f);
		SetMinMax(Questions.VaccinationDistribution, 0f, 20000f);
		SetMinMax(Questions.VaccinationUptake, 0f, 1f);
		SetMinMax(Questions.BreakthroughRate, 0f, 1f);
		SetMinMax(Questions.MortalityRate, 0f, 1f);
		SetMinMax(Questions.InfectionDuration, 1f, 250f);

		SetMinMax(Questions.WaningImmunityRate, 0f, 50000f);
		SetMinMax(Questions.MovementModel, 0f, 1f);
		SetMinMax(Questions.DiseaseImmobilization, 0f, 4f);
		SetMinMax(Questions.AirplaneSecurityAgainstExposed, 0f, 4f);
		SetMinMax(Questions.AirplaneSecurityAgainstInfected, 0f, 4f);
		SetMinMax(Questions.Elevation, 0f, 4f);
		SetMinMax(Questions.Highways, 0f, 4f);
		SetMinMax(Questions.Water, 0f, 4f);


		SetMinMax(Questions.IncomeInfectionRate, 0f, 4f);
		SetMinMax(Questions.IncomeVaccineEffectiveness, 0f, 4f);
		SetMinMax(Questions.IncomeDiseaseOnsetTime, 0f, 4f);
		SetMinMax(Questions.IncomeMortalityRate, 0f, 4f);
		SetMinMax(Questions.IncomeWaningImmunity, 0f, 4f);

		SetMinMax(Questions.VaccinationHesitancy, 0f, 4f);
	}

	/// <summary>
	/// Helper function to set bounds
	/// </summary>
	/// <param name="question">CompartmentEnum.Question.X</param>
	/// <param name="min">Min val for slider</param>
	/// <param name="max">Max val for slider</param>
	private static void SetMinMax(Questions question, float min, float max)
	{
		QuestionsMinMax[(int)question].min = min;
		QuestionsMinMax[(int)question].max = max;
	}

	/// <summary>
	/// Helper function to help display the correct text on slider handles and buttons
	/// </summary>
	/// <param name="question">Question to fetch format of</param>
	/// <returns>String format of how to display slider or button value</returns>
	public static string FetchFormatting(Questions question)
	{
		Questions[] wholeNums = { Questions.ContactRate, Questions.EILatency, Questions.VaccinationDistribution, Questions.DiseaseImmobilization, Questions.WaningImmunityRate };
		if (Array.Exists(wholeNums, element => element == question))
			return "0";
		else
			return "0.000";
	}
}
