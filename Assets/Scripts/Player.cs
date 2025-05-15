
using UnityEngine;
using Platformer2DCore;

class Player : MonoBehaviour {
	[SerializeField] private CharacterMovement.HorizontalMovementParameters horizontalMovementParams;
	[SerializeField] private CharacterMovement.VerticalMovementParameters verticalMovementParams;
	[SerializeField] private float groundCheckRadius = 0.2f;
	[SerializeField] private float attackCheckRadius = 1.4f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask enemyLayer;

	[SerializeField] [Range(0.0f, 1.0f)] private float attackCoolDown = 0.2f;
	[SerializeField] [Range(0.0f, 1.0f)] private float attackBuffer = 0.3f;
	[SerializeField] [Range(0.0f, 1.0f)] private float ivBuffer = 0.2f; // invenurable

	private Rigidbody2D rigidBody;
	private CharacterMovement movement;
	private CharacterCollision collision;
	private bool isGrounded = false;
	private float jumpTime = 0.0f;
	private Animator animator;
	private float attackCoolDownCounter = 0.0f;
	private float attackBufferCounter = 0.0f;
	private float ivBufferCounter = 0.0f;
	private bool isLeft = false;
	private GameManager gameManager;

	// init all references here
	void Awake() {
		this.rigidBody = GetComponent<Rigidbody2D>();
		this.collision = new CharacterCollision(rigidBody, GetComponent<CapsuleCollider2D>(), groundLayer, groundCheckRadius);
		this.movement = new CharacterMovement(horizontalMovementParams, verticalMovementParams);
		this.animator = GetComponentInChildren<Animator>();
	}

	void Start() {
		this.gameManager = GameManager.Instance;
	}

	void FixedUpdate() {
		collision.Update();
		// don't set grounded if recently jumped (under hang time), to prevent double jump due to fixed update ground check
		if (Time.fixedTime - jumpTime > verticalMovementParams.jumpHangTime)
			isGrounded = collision.CheckGrounded();
	}


	void Update() {
		if (gameManager.gameOver || gameManager.gameState != GameManager.GameState.GAMEPLAY) return;
		Vector2 velocity = rigidBody.velocity;

		// input
		float horizontalAxis = gameManager.controls.Gameplay.Move.ReadValue<float>();
		bool isJumpDown = gameManager.controls.Gameplay.Jump.ReadValue<float>() > 0.0f; // isPressed() will be added by unity in future
		bool isAttackDown = gameManager.controls.Gameplay.Attack.triggered;

		isLeft = (horizontalAxis < -0.5f) ? true : (horizontalAxis > 0.5f) ? false : isLeft;
		bool isAttack = isAttackDown && attackCoolDownCounter <= 0;

		// buffer counters
		attackCoolDownCounter = (isAttack) ? attackCoolDown : attackCoolDownCounter - Time.deltaTime;
		attackBufferCounter = (isAttack) ? attackBuffer : attackBufferCounter - Time.deltaTime;
		ivBufferCounter -= Time.deltaTime;

		// movement
		CharacterMovement.MoveResult moveResult = movement.Move(velocity, isGrounded, horizontalAxis, isJumpDown);
		rigidBody.velocity = moveResult.velocity;
		if (moveResult.jumpTriggered) {
			isGrounded = false;
			jumpTime = Time.deltaTime;
			gameManager.noJumps++;
		}

		// animation
		var localScale = transform.localScale;
		localScale.x = (isLeft) ? -1.0f : 1.0f;
		transform.localScale = localScale;
		animator.SetBool("running", Mathf.Abs(moveResult.velocity.x) > 0.5f);

		if (isAttack) {
			animator.SetTrigger("attack");
			SoundManager.Instance.playSound(SoundManager.SoundType.PlayerAttack);
			gameManager.noAttacks++;
		}
		if (attackBufferCounter >= 0.0f) {
			CheckAttackCollision();
		}
	}

	private void CheckAttackCollision() {
		Collider2D[] colliders = Physics2D.OverlapCircleAll((isLeft) ? collision.attackColliderPosLeft : collision.attackColliderPosRight, groundCheckRadius, enemyLayer);
		foreach (var collider in colliders) {
			SoundManager.Instance.playSound(SoundManager.SoundType.EnemyKill);
			gameManager.enemiesKilled++;
			Destroy(collider.gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D other) {
		if (ivBufferCounter > 0.0f || gameManager.godMode) return;
		if (other.gameObject.tag == "Enemy") {
			if (gameManager.playerDamaged() == 0) {
				animator.SetTrigger("death");
				this.rigidBody.simulated = false;
				SoundManager.Instance.playSound(SoundManager.SoundType.GameOver);
				Destroy(this.gameObject, 1.0f);
			} else {
				ivBufferCounter = ivBuffer;
				animator.SetTrigger("hit");
				Vector3 dir = (this.transform.position - other.gameObject.transform.position).normalized;
				rigidBody.velocity = dir * 8.0f;
				SoundManager.Instance.playSound(SoundManager.SoundType.PlayerHit);
			}
		}
	}

	// Dev-Only debug / editor related methods
	void OnDrawGizmos() {
		Gizmos.color = (isGrounded) ? Color.red : Color.green;
		if (collision != null) {
			Gizmos.DrawWireSphere(collision.groundCheckColliderPos, groundCheckRadius);
			Gizmos.DrawWireSphere(collision.attackColliderPosRight, attackCheckRadius);
			Gizmos.DrawWireSphere(collision.attackColliderPosLeft, attackCheckRadius);
		}
	}

	void OnGUI() {
	}

}