using UnityEngine;
using TMPro;
 
public class FPSDisplay : MonoBehaviour {
	public TextMeshProUGUI FpsText;
	
	public float pollingTime = 0.5f;

	private float accumulator;
	private int frameCount;
 
	void Update() {
		accumulator += Time.unscaledDeltaTime;
		frameCount++;
		if (accumulator >= pollingTime) {
			int frameRate = Mathf.RoundToInt((float)frameCount / accumulator);
			accumulator = 0;
			frameCount = 0;

			FpsText.text = "FPS: " + frameRate.ToString();
		}
	}
}
