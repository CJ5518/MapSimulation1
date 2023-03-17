using UnityEngine;
using UnityEngine.UI;

public class TextColor : MonoBehaviour{

	TMPro.TextMeshProUGUI text;
	Image image;

	private void Start() {
		text = GetComponent<TMPro.TextMeshProUGUI>()? GetComponent<TMPro.TextMeshProUGUI>() : null;
		image = GetComponent<Image>() ? GetComponent<Image>() : null;
	}

	public void SetTextWhite() {
		text.color = Color.white;
	}
	public void SetTextGrey() {
		text.color = new Color(0.67f,0.69f,0.75f,1.0f);
	}
	public void SetImageWhite() {
		image.color = Color.white;
	}
	public void SetImageGrey() {
		image.color = new Color(0.67f, 0.69f, 0.75f, 1.0f);
	}
	public void SetImageRed() {
		image.color = Color.red;
	}
}