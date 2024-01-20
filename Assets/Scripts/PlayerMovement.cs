using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField] private float moveSpeed = 6f;
	[SerializeField] private float airMultiplier = 0.4f;
	private float movementMultiplier = 10f;

	[Header("Sprinting")]
	[SerializeField] private float walkSpeed = 4f;
	[SerializeField] private float sprintSpeed = 6f;
	[SerializeField] private float acceleration = 10f;

	[Header("Jumping")]
	public float jumpForce = 5f;

	[Header("Keybinds")]
	[SerializeField] private KeyCode jumpKey = KeyCode.Space;
	[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

	[Header("Drag")]
	[SerializeField] private float groundDrag = 6f;
	[SerializeField] private float airDrag = 2f;
	
	[SerializeField] private Transform orientation;

	private float horizontalMovement;
	private float verticalMovement;

	[Header("Ground Detection")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private LayerMask groundMask;
	[SerializeField] private float groundDistance = 0.2f;
	public bool isGrounded { get; private set; }

	private Vector3 moveDirection;
	private Vector3 slopeMoveDirection;

	private Rigidbody rb;

	private RaycastHit slopeHit;
	
	private float playerHeight = 2f;

	private bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
		{
			if (slopeHit.normal != Vector3.up)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
	}

	private void Update()
	{
		isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

		MyInput();
		ControlDrag();
		ControlSpeed();

		if (Input.GetKeyDown(jumpKey) && isGrounded)
		{
			Jump();
		}

		slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
	}

	private void MyInput()
	{
		horizontalMovement = Input.GetAxisRaw("Horizontal");
		verticalMovement = Input.GetAxisRaw("Vertical");

		moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
	}

	private void Jump()
	{
		if (isGrounded)
		{
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
	}

	private void ControlSpeed()
	{
		if (Input.GetKey(sprintKey) && isGrounded)
		{
			moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
		}
		else
		{
			moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
		}
	}

	private void ControlDrag()
	{
		if (isGrounded)
		{
			rb.drag = groundDrag;
		}
		else
		{
			rb.drag = airDrag;
		}
	}

	private void FixedUpdate()
	{
		MovePlayer();
	}

	private void MovePlayer()
	{
		if (isGrounded && !OnSlope())
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
		}
		else if (isGrounded && OnSlope())
		{
			rb.AddForce(slopeMoveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
		}
		else if (!isGrounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
		}
	}
}