using System.Collections;
using UnityEngine;

public class TEST_Vaulting_02 : MonoBehaviour
{
	[Header("Vaulting Parameters:")]
	[SerializeField] private KeyCode vaultKey = KeyCode.Space;
	[SerializeField] private TEST_VaultCollider greenZone;
	[SerializeField] private TEST_VaultCollider redZone;
	[SerializeField] private float climbingMaxHeight;
	[SerializeField] private float playerHeight;
	[SerializeField] private float playerRadius;
	[SerializeField] private float climbDuration;

	private bool checkIfCanVault => greenZone.touchingValidObject && !redZone.touchingValidObject;

	private Camera playerCamera;
	[SerializeField] private bool canVault;
	private bool isVaulting;

	private void Awake()
	{
		playerCamera = GetComponentInChildren<Camera>();
	}

	private void Update()
	{
		canVault = checkIfCanVault;

		if (Input.GetKey(vaultKey))
		{
			Vault();
		}
	}

	private void Vault()
	{
		if (isVaulting) return;
		if (!checkIfCanVault) return;

		if (Physics.Raycast(greenZone.transform.position + (transform.TransformDirection(Vector3.forward) * playerRadius) + (Vector3.up * playerHeight / 2), Vector3.down, out RaycastHit firstHit, 1f))
		{
			StartCoroutine(LerpVault(firstHit.point, climbDuration));
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
