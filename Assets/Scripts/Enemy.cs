
using UnityEngine;
using Platformer2DCore;

class Enemy : MonoBehaviour {
	[SerializeField] private CharacterMovement.HorizontalMovementParameters horizontalMovementParams;
	[SerializeField] private CharacterMovement.VerticalMovementParameters verticalMovementParams;
	[SerializeField] private float groundCheckRadius = 0.2f;
	[SerializeField] private LayerMask groundLayer;

	[SerializeField] [Range(0.0f, 20.0f)] private float dirChangeTime = 1.0f;

	private Rigidbody2D rigidBody;
	private CharacterMovement movement;
	private CharacterCollision collision;
	private bool isGrounded = false;
	private float jumpTime = 0.0f;
	private bool isLeft = true;
	private PlayerControls controls;
	private Animator animator;
	private float dirChangeCounter = 0.0f;

	// init all references here
	void Awake() {
		this.rigidBody = GetComponent<Rigidbody2D>();
		this.collision = new CharacterCollision(rigidBody, GetComponent<CapsuleCollider2D>(), groundLayer, groundCheckRadius);
		this.movement = new CharacterMovement(horizontalMovementParams, verticalMovementParams);
		this.controls = new PlayerControls();
		this.animator = GetComponentInChildren<Animator>();
	}

	void Start() {
	}

	void OnEnable() {
		controls.Enable();
	}

	void OnDisable() {
		controls.Disable();
	}

	void FixedUpdate() {
		collision.Update();
		// don't set grounded if recently jumped (under hang time), to prevent double jump due to fixed update ground check
		if (Time.fixedTime - jumpTime > verticalMovementParams.jumpHangTime)
			isGrounded = collision.CheckGrounded();
	}

	void Update() {
		if (GameManager.Instance.gameState == GameManager.GameState.GAMEPLAY) {
			UpdateMovement();
		}
	}

	float enemyAI() {
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right, 4f, groundLayer);
		isLeft = (hit.collider) ? true : isLeft;
		if (hit.collider) dirChangeCounter = dirChangeTime;
		if (dirChangeCounter <= 0.0f) {
			isLeft = !isLeft;
			dirChangeCounter = dirChangeTime;
		} else {
			dirChangeCounter -= Time.deltaTime;
		}
		return (isLeft) ? -1.0f : 1.0f;
	}

	void UpdateMovement() {
		Vector2 velocity = rigidBody.velocity;

		// input
		float horizontalAxis = enemyAI();
		bool isJumpDown = false;

		// movement
		CharacterMovement.MoveResult moveResult = movement.Move(velocity, isGrounded, horizontalAxis, isJumpDown);
		rigidBody.velocity = moveResult.velocity;
		if (moveResult.jumpTriggered) {
			isGrounded = false;
			jumpTime = Time.deltaTime;
		}

		// animation
		var localScale = transform.localScale;
		localScale.x = (horizontalAxis < 0) ? -1.0f : 1.0f;
		transform.localScale = localScale;
	}

	// Dev-Only debug / editor related methods
	void OnDrawGizmos() {
		Gizmos.color = (isGrounded) ? Color.red : Color.green;
		if (collision != null) {
			Gizmos.DrawWireSphere(collision.groundCheckColliderPos, groundCheckRadius);
			Debug.DrawRay(collision.groundCheckColliderPos, Vector2.left * 2f, Color.white);
		}
	}

	void OnGUI() {
	}

}