using UnityEngine;

public class TEST_Interactable : MonoBehaviour, IInteractable
{
	public void OnInteract()
	{
		Debug.Log("Interactet with: " + gameObject.name); ;
	}

	public void OnFocused()
	{
		Debug.Log("Focused on: " + gameObject.name);
	}

	public void OnLoseFocused()
	{
		Debug.Log("Lost focused on: " + gameObject.name); ;
	}

	public string GetInteractionDescription()
	{
		return null;
	}
}
