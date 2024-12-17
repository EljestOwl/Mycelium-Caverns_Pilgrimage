using UnityEngine;

public class ClimbableSurface : MonoBehaviour, IInteractable
{
	[SerializeField] private string interactionDescription;

	public string GetInteractionDescription()
	{
		return interactionDescription;
	}

	public void OnFocused()
	{
	}

	public void OnInteract()
	{
	}

	public void OnLoseFocused()
	{
	}


}
