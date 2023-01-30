using UnityEngine;
using TMPro;
 
public class FPSDisplay : MonoBehaviour {
	public TextMeshProUGUI FpsText;
	
	public float pollingTime = 0.5f;
	private float time;
	private int frameCount;
 
	void Update() {
		// Update time.
		time += Time.deltaTime;

		// Count this frame.
		frameCount++;

		if (time >= pollingTime) {
			// Update frame rate.
			int frameRate = Mathf.RoundToInt((float)frameCount / time);
			FpsText.text = frameRate.ToString();

			// Reset time and frame count.
			time -= pollingTime;
			frameCount = 0;
		}
	}
}
