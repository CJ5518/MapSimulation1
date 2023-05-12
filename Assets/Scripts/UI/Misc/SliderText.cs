using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderText : MonoBehaviour
{

	Slider slider; // this script should be put on the slider. this is found in start.
	public TextMeshProUGUI decimalText = null;
	public TextMeshProUGUI InverseText = null;
	Vector2 fractionValue;

	void Start()
	{
		slider = GetComponent<Slider>();
	}

	/// <summary>
	/// Default slider text
	/// </summary>
	/// <param name="value"></param>
	public void SliderUpdateTexts(float value)
	{
		if (decimalText)
			decimalText.text = value.ToString("0.000");
		if (InverseText)
		{
			if (value <= 0)
				InverseText.text = "No chance";
			else if (value >= 1)
				InverseText.text = "100%";
			else
				InverseText.text = ToFraction(value, 30);

		}
	}

	#region Question-Specific Texts

	public void MovementModel(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "Gravity Model";
					transform.parent.Find("MovementModelFlavourText").GetComponent<TextMeshProUGUI>().text = "The gravity model of population movement is based on gravity.\n\nPopulationMovement = Pop1 * Pop2 / dist ^ 2";
					return;
				case 1:
					InverseText.text = "Our custom model";
					transform.parent.Find("MovementModelFlavourText").GetComponent<TextMeshProUGUI>().text = "A potentially less accurate, but certainly more interesting model for disease spread.";
					return;
				case 2:
					InverseText.text = "";
					return;
				case 3:
					InverseText.text = "";
					return;
				case 4:
					InverseText.text = "";
					return;
				default:
					Debug.Log("SliderText.MovementModel value default");
					return;
			}
		}
	}

	public void DiseaseImmobilizationTexts(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "Not at all";
					return;
				case 1:
					InverseText.text = "Little immobilization";
					return;
				case 2:
					InverseText.text = "Moderate immobilization";
					return;
				case 3:
					InverseText.text = "Significant immobilization";
					return;
				case 4:
					InverseText.text = "Complete immobilization";
					return;
				default:
					Debug.Log("SliderText.DiseaseImmobilizationTexts value default");
					return;
			}
		}
	}

	public void AirportText(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "No restrictions";
					return;
				case 1:
					InverseText.text = "Encouraged not to fly";
					return;
				case 2:
					InverseText.text = "Symptomatic turned away at security";
					return;
				case 3:
					InverseText.text = "Negative test required";
					return;
				case 4:
					InverseText.text = "Mandatory at-airport testing";
					return;
				default:
					Debug.Log("SliderText.AirportText value default");
					return;
			}
		}
	}

	public void VaccinationHesitancy(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "No, for this simulation let's assume that everyone will want to be vaccinated as soon as possible.";
					return;
				case 1:
					InverseText.text = "Yes, but let's reduce the hesitancy shown here by about half.";
					return;
				case 2:
					InverseText.text = "Yes, let's use these data to directly adjust vaccination rates.";
					return;
				case 3:
					InverseText.text = "Yes, but I think vaccination hesitancy will be more pronounced than shown here.  Increase these values by about half.";
					return;
				case 4:
					InverseText.text = "No.  For this simulation vaccines should not be used.";
					return;
				default:
					Debug.Log("SliderText.VaccinationHesitancy value default");
					return;
			}
		}
	}


	public void ElevationText(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "None at all";
					return;
				case 1:
					InverseText.text = "Mountains affect disease transmission a small amount";
					return;
				case 2:
					InverseText.text = "Large hills affect disease transmission. Mountains significantly reduce transmission.";
					return;
				case 3:
					InverseText.text = "Any large or medium changes in elevation significantly reduce disease transmission.";
					return;
				case 4:
					InverseText.text = "Small changes in elevation significantly affect disease transmission";
					return;
				default:
					Debug.Log("SliderText.ElevationText value default");
					return;
			}
		}
	}

	public void HighwayText(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "None at all";
					return;
				case 1:
					InverseText.text = "Highways give a minor boost to transmission to neighboring highway cells.";
					return;
				case 2:
					InverseText.text = "Highways moderately increase transmission to neighboring highway cells.";
					return;
				case 3:
					InverseText.text = "Highways significantly increase transmission to neighboring highway cells.";
					return;
				case 4:
					InverseText.text = "Highways guarentee transmission to neighboring highway cells.";
					return;
				default:
					Debug.Log("SliderText.HighwayText value default");
					return;
			}
		}
	}

	public void WaterText(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "None at all";
					return;
				case 1:
					InverseText.text = "Water gives a minor reduction to transmission to neighboring cells.";
					return;
				case 2:
					InverseText.text = "Water moderately reduces transmission to neighboring cells.";
					return;
				case 3:
					InverseText.text = "Water significantly reduces transmission to neighboring cells.";
					return;
				case 4:
					InverseText.text = "Water prevents transmission to neighboring cells.";
					return;
				default:
					Debug.Log("SliderText.WaterText value default");
					return;
			}
		}
	}

	public void IncomeText(float value)
	{
		if (decimalText)
			decimalText.text = (value + 1).ToString("0");
		if (InverseText)
		{
			switch (value)
			{
				case 0:
					InverseText.text = "Major decrease";
					return;
				case 1:
					InverseText.text = "Minor decrease";
					return;
				case 2:
					InverseText.text = "None at all";
					return;
				case 3:
					InverseText.text = "Minor increase";
					return;
				case 4:
					InverseText.text = "Major increase";
					return;
				default:
					Debug.Log("SliderText.IncomeText value default");
					return;
			}
		}
	}

	#endregion Question-Specific Texts



	#region Lando Functions

	string ToFraction(float number, int precision = 4)
	{
		int w, n, d;
		RoundToMixedFraction(number, precision, out w, out n, out d);
		var stringFraction = $"{w}";
		if (n > 0) stringFraction = (w > 0) ? $"{w} {n}/{d}" : $"{n} out of {d}";
		return stringFraction;
	}

	static void RoundToMixedFraction(float input, int accuracy, out int whole, out int numerator, out int denominator)
	{
		float decimalAccuracy = (float)accuracy;
		whole = (int)input;
		var fraction = Mathf.Abs(input - whole);
		if (fraction == 0)
		{
			numerator = 0;
			denominator = 1;
			return;
		}


		int n = Enumerable.Range(0, accuracy + 1).SkipWhile(e => (e / decimalAccuracy) < fraction).First();
		float hi = n / decimalAccuracy;
		float lo = (n - 1) / decimalAccuracy;
		if ((fraction - lo) < (hi - fraction)) n--;
		if (n == accuracy)
		{
			whole++;
			numerator = 0;
			denominator = 1;
			return;
		}
		var gcd = GreatestCommonDivisor(n, accuracy);
		numerator = n / gcd;
		denominator = accuracy / gcd;
	}

	static int GreatestCommonDivisor(int a, int b)
	{
		if (b == 0) return a;
		else return GreatestCommonDivisor(b, a % b);
	}

	#endregion Lando Functions
}