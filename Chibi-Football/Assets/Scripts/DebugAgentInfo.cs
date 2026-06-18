using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

public class DebugAgentInfo : MonoBehaviour
{
    void Start()
    {
        var behaviorParams = GetComponent<BehaviorParameters>();
        if (behaviorParams != null)
        {
            Debug.Log($"[DEBUG] Agent '{gameObject.name}' has Behavior Name: '{behaviorParams.BehaviorName}'");

            var model = behaviorParams.Model;
            if (model != null)
            {
                Debug.Log($"[DEBUG] Agent '{gameObject.name}' has a model assigned in Unity Editor: '{model.name}'");
            }
            else
            {
                Debug.Log($"[DEBUG] Agent '{gameObject.name}' has no model assigned in Unity (expects CLI model).");
            }
        }
        else
        {
            Debug.LogWarning($"[DEBUG] Agent '{gameObject.name}' is missing BehaviorParameters.");
        }
    }
}