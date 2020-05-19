using UnityEngine;
using System.Collections;

// ... This class is used to store all the data that needs updating in the editor ... //

public class UpdatableData : ScriptableObject
{

	public event System.Action OnValuesUpdated;
	public bool autoUpdate;

	// Only compile the code if it is containing the reference to the unity editor namespace
	// This will allow for the code to compile when using the unity editor
	// Once the project is built the code will be ignored in the build version
#if UNITY_EDITOR

	protected virtual void OnValidate()
	{
		if (autoUpdate)
		{
			// Subscribe to callback
			UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
		}
	}

	public void NotifyOfUpdatedValues()
	{
		// Unsubscribe from callback
		UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
		OnValuesUpdated?.Invoke();
	}

#endif

}