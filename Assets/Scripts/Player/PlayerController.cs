using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public bool canMove { get; private set; } = true;

	[Header("Functional Options:")]
	[SerializeField] private bool moveInputSmooth = false;
	[SerializeField] private bool canSprint = true;
	[SerializeField] private bool canJump = true;
	[SerializeField] private bool canCrouch = true;
	[SerializeField] private bool canUseHeadbob = true;
	[SerializeField] private bool willSlideOnSlopes = true;
	[SerializeField] private bool canInteract = true;
	[SerializeField] private bool canVaultLedges = true;
	[SerializeField] private bool canClimb = true;

	[Header("Controls:")]
	[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
	[SerializeField] private KeyCode jumpKey = KeyCode.Space;
	[SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
	[SerializeField] private KeyCode interactKey = KeyCode.E;
	[SerializeField] private KeyCode vaultLedgeKey = KeyCode.Space;

	[Header("Movement Parameters:")]
	[SerializeField] private bool isSprintToggle = false;
	[SerializeField] private bool useOwnGroundCheck = true;
	[SerializeField] private float walkSpeed = 3f;
	[SerializeField] private float sprintSpeed = 6f;
	[SerializeField] private float crouchSpeed = 1.5f;
	[SerializeField] private float slopeSpeed = 8f;
	private bool isSprinting => canSprint && isSprintingPressed;
	private bool isSprintingPressed;

	[Header("Look Parameters:")]
	[SerializeField, Range(1, 10)] private float lookSpeedX = 2f;
	[SerializeField, Range(1, 10)] private float lookSpeedY = 2f;
	[SerializeField, Range(1, 180)] private float lowerLookLimit = 80f;
	[SerializeField, Range(1, 180)] private float upperLookLimit = 80f;

	[Header("Jumping Parameters:")]
	[SerializeField] private float jumpFoce = 8f;
	[SerializeField] private float gravity = 30f;
	private bool shouldJump => IsGrounded() && Input.GetKey(jumpKey);

	[Header("Crouch Parameters:")]
	[SerializeField] private float crouchingHeight = 0.9f;
	[SerializeField] private float standingHeight = 1.6f;
	[SerializeField] private float timeToCrouch = 0.5f;
	[SerializeField] private Vector3 crouchingCenterPoint = new Vector3(0, 0.9f, 0);
	[SerializeField] private Vector3 standingCenterPoint = new Vector3(0, 0, 0);
	private bool isCrouching;
	private bool duringCrouchAnimation;
	private bool shouldCrouch => IsGrounded() && Input.GetKeyDown(crouchKey) && !duringCrouchAnimation;

	[Header("Headbob Parameters:")]
	[SerializeField] private float walkBobSpeed = 14f;
	[SerializeField] private float walkBobAmount = 0.05f;
	[SerializeField] private float sprintBobSpeed = 18f;
	[SerializeField] private float sprintBobAmount = 0.1f;
	[SerializeField] private float crouchBobSpeed = 8f;
	[SerializeField] private float crouchBobAmount = 0.025f;
	private float defaultYPos;
	private float defaultXPos;
	private float headbobTimer = 0;

	// SLIDING PARAMETERS:
	private Vector3 hitPointNormal;
	private bool isSliding
	{
		get
		{
			if (IsGrounded() && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 1.25f))
			{
				hitPointNormal = slopeHit.normal;
				return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
			}
			else
			{
				return false;
			}
		}
	}

	[Header("Interaction Parameters:")]
	[SerializeField] private float interactionDistance = default;
	[SerializeField] private Vector3 interactionRayPoint = default;
	[SerializeField] private LayerMask interactionLayers = default;
	private IInteractable currentInteractable;

	[Header("Vault Ledges Parameters:")]
	[SerializeField] private TEST_VaultCollider greenZone;
	[SerializeField] private TEST_VaultCollider redZone;
	[SerializeField] private float climbDuration;
	private bool isVaulting;
	private bool checkIfCanVault => greenZone.touchingValidObject && !redZone.touchingValidObject;

	[Header("Climbing Parameters:")]
	[SerializeField] private float climbSpeed;
	[SerializeField] private bool buttonToggle;
	private Vector3 ClimbDirection;
	private bool isClimbing;

	private Camera playerCamera;
	private CharacterController characterController;

	private Vector3 moveDirection;
	private Vector2 currentMoveInput;

	private float rotationX = 0;

	private void Awake()
	{
		playerCamera = GetComponentInChildren<Camera>();
		characterController = GetComponent<CharacterController>();
		defaultYPos = playerCamera.transform.localPosition.y;
		defaultXPos = playerCamera.transform.localPosition.x;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		if (canMove)
		{
			HandleMovementInput();
			HandleMouseInput();

			if (canSprint)
				HandleSprinting();
			if (canJump && !isSliding)
				HandleJump();
			if (canCrouch)
				HandleCrouch();
			if (canUseHeadbob)
				HandleHeadbob();
			if (canInteract)
			{
				HandeInteractionCheck();
				HandeInteractionInput();
			}
			if (canVaultLedges)
				Vault();
			if (canClimb)
				ClimbLadder();

			ApplyFinalMovement();
		}
	}

	private void HandleMovementInput()
	{
		if (moveInputSmooth)
			currentMoveInput = new Vector2(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
		else
			currentMoveInput = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));

		currentMoveInput = currentMoveInput.normalized;

		if (isCrouching)
			currentMoveInput *= crouchSpeed;
		else
			currentMoveInput *= isSprinting ? sprintSpeed : walkSpeed;

		float moveDirectionY = moveDirection.y;

		moveDirection = transform.TransformDirection(Vector3.forward) * currentMoveInput.x + transform.TransformDirection(Vector3.right) * currentMoveInput.y;
		moveDirection.y = moveDirectionY;
	}

	private void HandleMouseInput()
	{
		rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
		rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
		playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

		transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
	}

	private void HandleJump()
	{
		if (shouldJump)
		{
			moveDirection.y = jumpFoce;
		}
	}

	private void HandleSprinting()
	{
		if (isSprintToggle)
		{
			if (Input.GetKeyDown(sprintKey))
				isSprintingPressed = !isSprintingPressed;
			if (currentMoveInput.x == 0)
				isSprintingPressed = false;
		}
		else
		{
			if (Input.GetKey(sprintKey))
				isSprintingPressed = true;
			else
				isSprintingPressed = false;
		}

	}

	private void HandleCrouch()
	{
		if (shouldCrouch)
		{
			StartCoroutine(CrouchStand());
		}
	}

	private void HandleHeadbob()
	{
		if (!IsGrounded()) return;

		if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
		{
			headbobTimer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
			playerCamera.transform.localPosition = new Vector3(
				defaultXPos + Mathf.Sin(headbobTimer) * (isCrouching ? crouchBobAmount / 3 : isSprinting ? sprintBobAmount / 3 : walkBobAmount / 3),
				defaultYPos + Mathf.Sin(headbobTimer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
				playerCamera.transform.localPosition.z);
		}
		else
		{
			playerCamera.transform.localPosition = new Vector3(defaultXPos, defaultYPos, playerCamera.transform.localPosition.z);
		}
	}

	private void HandeInteractionCheck()
	{
		if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
		{
			if (hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()))
			{
				hit.collider.gameObject.TryGetComponent(out currentInteractable);
				if (currentInteractable != null)
				{
					currentInteractable.OnFocused();
				}
			}
		}
		else if (currentInteractable != null)
		{
			currentInteractable.OnLoseFocused();
			currentInteractable = null;
		}
	}
	private void HandeInteractionInput()
	{
		if (Input.GetKeyDown(interactKey) && currentInteractable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayers))
		{
			if (!currentInteractable.gameObject.TryGetComponent<ClimbableSurface>(out ClimbableSurface ladder))
			{

			}
			else
				currentInteractable.OnInteract();
		}
	}

	private void Vault()
	{
		if (!checkIfCanVault) return;
		if (isVaulting) return;

		if (Input.GetKeyDown(vaultLedgeKey))
		{
			if (Physics.Raycast(greenZone.transform.position + (transform.TransformDirection(Vector3.forward) * characterController.radius) + (Vector3.up * characterController.height / 2), Vector3.down, out RaycastHit firstHit, 1f))
			{
				StartCoroutine(LerpVault(firstHit.point, climbDuration));
			}
		}
	}

	private void ClimbLadder()
	{
		if (!isClimbing)
		{
			if (Physics.Raycast(transform.position + Vector3.up * characterController.height / 4, transform.TransformDirection(Vector3.forward), out RaycastHit climbableCheck, characterController.radius + 0.1f))
			{
				if (buttonToggle ? Input.GetKeyDown(interactKey) : true)
				{
					if (climbableCheck.transform.TryGetComponent(out ClimbableSurface climbableSurface))
					{
						isClimbing = true;
						ClimbDirection = transform.TransformDirection(Vector3.forward);
					}
				}
			}
		}
		else
		{
			if (Physics.Raycast(transform.position + Vector3.up * characterController.height / 4, ClimbDirection, out RaycastHit climbableCheck, characterController.radius + 0.1f))
			{
				if (!climbableCheck.transform.TryGetComponent(out ClimbableSurface climbableSurface))
				{
					isClimbing = false;
					moveDirection.y = 4;
					moveDirection.x = 2;
				}
			}
			else
			{
				isClimbing = false;
				moveDirection.y = 4;
				moveDirection.x = 2;
			}
		}



		if (isClimbing)
		{
			moveDirection = transform.TransformDirection(Vector3.up) * currentMoveInput.x;
			moveDirection = moveDirection.normalized * climbSpeed;
			moveDirection.x = 0;
			moveDirection.z = 0;

			if (moveDirection.y < -0.1f && IsGrounded())
			{
				isClimbing = false;
			}
		}
	}

	private void ApplyFinalMovement()
	{
		if (!IsGrounded() && !isClimbing)
			moveDirection.y -= gravity * Time.deltaTime;

		if (characterController.velocity.y < -1 && IsGrounded())
			moveDirection.y = 0; // PROBLEM? WITH CLIMB_LADDER <<<--------------------

		if (willSlideOnSlopes && isSliding)
			moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

		characterController.Move(moveDirection * Time.deltaTime);
	}

	private bool IsGrounded()
	{
		if (useOwnGroundCheck)
			return Physics.Raycast(transform.position + Vector3.down * 0.1f, Vector3.down, 0.1f);
		else
			return characterController.isGrounded;
	}

	private IEnumerator CrouchStand()
	{
		if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, standingHeight - crouchingHeight + 0.1f))
			yield break;

		duringCrouchAnimation = true;

		float timeElapsed = 0;
		float targetHeight = isCrouching ? standingHeight : crouchingHeight;
		float currentHeight = characterController.height;
		Vector3 targetCenterPoint = isCrouching ? standingCenterPoint : crouchingCenterPoint;
		Vector3 currentCenterPoint = characterController.center;

		while (timeElapsed < timeToCrouch)
		{
			characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
			characterController.center = Vector3.Lerp(currentCenterPoint, targetCenterPoint, timeElapsed / timeToCrouch);
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		characterController.height = targetHeight;
		characterController.center = targetCenterPoint;

		isCrouching = !isCrouching;

		duringCrouchAnimation = false;
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
