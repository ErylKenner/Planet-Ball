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
			accumulator = 0;
			frameCount = 0;
			int frameRate = Mathf.RoundToInt((float)frameCount / accumulator);
			FpsText.text = "FPS: " + frameRate.ToString();
		}
	}
}
