// taken from https://www.youtube.com/watch?v=xOCScMQIxrU

using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour {
    public TextMeshProUGUI FpsText;

    private float pollingTime = 1f;
    private float time;
    private int frameCount;
    private int minFrameRate = int.MaxValue;
    private int maxFrameRate = int.MinValue;

	private void Start() {
		// Make sure only 1 FPSDisplay is attached to a GameObject.
		Debug.Log($"FPSDisplay is attached to {gameObject.name}");
	}

    void Update() {
        // Updates the currrent time and increments the frame count.
		// unscalledDeltaTime is used so that the time is not affected by the time scale.
        time += Time.unscaledDeltaTime;
        frameCount++;

        // Current frame rate, min framerate and max framrate.
        int currentFrameRate = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        minFrameRate = Mathf.Min(minFrameRate, currentFrameRate);
        maxFrameRate = Mathf.Max(maxFrameRate, currentFrameRate);

		/*
		if (FpsText == null) {
			Debug.LogWarning("FpsText is null");
		} else {
			Debug.Log("Updating FPS");
			FpsText.text = $"{currentFrameRate} FPS [↑{maxFrameRate} - ↓{minFrameRate}]";
		}
		*/
        FpsText.text = $"FPS: {currentFrameRate} [↑{maxFrameRate} ↓{minFrameRate}]";
		// Debug.Log($"FPS: {currentFrameRate} [↑{maxFrameRate} ↓{minFrameRate}]");

        if (time >= pollingTime) {
            // Reset time and frame count for the next polling.
            time -= pollingTime;
            frameCount = 0;
            minFrameRate = int.MaxValue;
            maxFrameRate = int.MinValue;
        }
    }
}