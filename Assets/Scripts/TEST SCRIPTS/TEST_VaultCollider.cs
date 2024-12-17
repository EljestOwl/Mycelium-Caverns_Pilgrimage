using UnityEngine;

public class TEST_VaultCollider : MonoBehaviour
{
	[SerializeField] private LayerMask validLayers;
	[SerializeField] private bool OnlyAcceptsLayermask;
	public bool touchingValidObject { get; private set; }

	private void OnTriggerStay(Collider other)
	{
		if (!other.CompareTag("Player") && !other.isTrigger && OnlyAcceptsLayermask ? (validLayers == (validLayers | (1 << other.gameObject.layer))) : true)
			touchingValidObject = true;
	}

	private void OnTriggerExit(Collider other)
	{
		touchingValidObject = false;
	}
}
