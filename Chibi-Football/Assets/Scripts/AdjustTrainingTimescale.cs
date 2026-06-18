//This script lets you change time scale during training. It is not a required script for this demo to function

using UnityEngine;
using UnityEngine.InputSystem;

namespace MLAgentsExamples
{
    public class AdjustTrainingTimescale : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            var kb = Keyboard.current;
            if (kb.digit1Key.wasPressedThisFrame) { Time.timeScale = 1f; }
            if (kb.digit2Key.wasPressedThisFrame) { Time.timeScale = 2f; }
            if (kb.digit3Key.wasPressedThisFrame) { Time.timeScale = 3f; }
            if (kb.digit4Key.wasPressedThisFrame) { Time.timeScale = 4f; }
            if (kb.digit5Key.wasPressedThisFrame) { Time.timeScale = 5f; }
            if (kb.digit6Key.wasPressedThisFrame) { Time.timeScale = 6f; }
            if (kb.digit7Key.wasPressedThisFrame) { Time.timeScale = 7f; }
            if (kb.digit8Key.wasPressedThisFrame) { Time.timeScale = 8f; }
            if (kb.digit9Key.wasPressedThisFrame) { Time.timeScale = 9f; }
            if (kb.digit0Key.wasPressedThisFrame) { Time.timeScale *= 2f; }
        }
    }
}
