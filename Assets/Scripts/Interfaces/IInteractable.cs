using UnityEngine;

public interface IInteractable
{
    GameObject gameObject { get; }
    public void OnInteract();
	public void OnFocused();
	public void OnLoseFocused();
}
