using System.Collections;
using UnityEngine;

public class TEST_Vaulting_01 : MonoBehaviour
{
	[SerializeField] private LayerMask vaultLayer;
	[SerializeField] private KeyCode vaultKey = KeyCode.Space;
	private Camera playerCamera;
	[SerializeField] private float playerHeight;
	[SerializeField] private float playerRadius;
	[SerializeField] private float climbDuration;
	private bool isVaulting;

	private void Awake()
	{
		playerCamera = GetComponentInChildren<Camera>();
	}

	private void Update()
	{
		Vault();
	}

	private void Vault()
	{
		if (Input.GetKey(vaultKey) && !isVaulting)
		{
			if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit firstHit, 1f, vaultLayer))
			{
				if (Physics.Raycast(firstHit.point + (playerCamera.transform.forward * playerRadius) + (Vector3.up * 0.6f * playerHeight), Vector3.down, out RaycastHit secondHit, playerHeight))
				{
					StartCoroutine(LerpVault(secondHit.point, climbDuration));
				}
			}
		}

	}

	IEnumerator LerpVault(Vector3 targetPos, float duration)
	{
		isVaulting = true;

		float timeElapsed = 0;
		Vector3 startPos = transform.position;

		while (timeElapsed < duration)
		{
			transform.position = Vector3.Lerp(startPos, targetPos, timeElapsed / duration);
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		transform.position = targetPos;

		isVaulting = false;
	}
}
