using UnityEngine;

namespace Platformer2DCore {
	public class CharacterMovement {
		[System.Serializable]
		public class HorizontalMovementParameters {
			[Range(0.0f, 20.0f)] public float maxVelocity;
			[Range(0.0f, 1.0f)] public float dampingMovement;
			[Range(0.0f, 1.0f)] public float dampingStop;
			[Range(0.0f, 1.0f)] public float dampingJump;
			[Range(0.0f, 1.0f)] public float dampingFall;

			[Range(0.0f, 20.0f)] public float dampingConstant;
			[Range(0.0f, 1.0f)] public float turnRate;
			[Range(0.0f, 0.1f)] public float turnRecover;

			public HorizontalMovementParameters(
				float maxVelocity,
				float dampingMovement,
				float dapmingStop,
				float dampingJump,
				float dampingFall,
				float dampingConstant,
				float turnRate,
				float turnRecover
			) {
				this.maxVelocity = maxVelocity;
				this.dampingMovement = dampingMovement;
				this.dampingStop = dapmingStop;
				this.dampingJump = dampingJump;
				this.dampingFall = dampingFall;
				this.dampingConstant = dampingConstant;
				this.turnRate = turnRate;
				this.turnRecover = turnRecover;
			}
		};

		[System.Serializable]
		public class VerticalMovementParameters {
			[Range(0.0f, 20.0f)] public float maxVelocity;
			[Range(1.0f, 5.0f)] public float fallMultiplier;
			[Range(1.0f, 5.0f)] public float controllableJumpMultiplier;
			[Range(0.0f, 1.0f)] public float jumpHangTime;
			[Range(0.0f, 1.0f)] public float jumpBufferLength;

			public VerticalMovementParameters(float maxVelocity, float fallMultiplier, float controllableJumpMultiplier, float jumpHangTime, float jumpBufferLength) {
				this.maxVelocity = maxVelocity;
				this.fallMultiplier = fallMultiplier;
				this.controllableJumpMultiplier = controllableJumpMultiplier;
				this.jumpHangTime = jumpHangTime;
				this.jumpBufferLength = jumpBufferLength;
			}
		}

		public readonly struct MoveResult {
			public Vector2 velocity { get; }
			public bool jumpTriggered { get; }

			public MoveResult(Vector2 velocity, bool jumpTriggered) {
				this.velocity = velocity;
				this.jumpTriggered = jumpTriggered;
			}
		}

		// Supplied fields
		HorizontalMovementParameters horizontalParams;
		VerticalMovementParameters verticalParams;

		// Calculation / Calculated fields
		private bool isJumpDownLast = false;
		private float jumpHangCounter = 0.0f;
		private float jumpBufferCount = 0.0f;
		private float fakeHorizontalAxis = 1.0f;

		public CharacterMovement(
			HorizontalMovementParameters horizontalParams,
			VerticalMovementParameters verticalParams) {
			this.horizontalParams = horizontalParams;
			this.verticalParams = verticalParams;
		}

		private float calculateDamping(float num, float dampingConstant) {
			return Mathf.Pow(1.0f - num, Time.deltaTime * dampingConstant);
		}

		private bool calculateVerticalMovement(ref Vector2 velocity, bool isGrounded, bool isJump, bool jumpDownEvent) {
			bool jumpTriggered = false;
			// hang time
			jumpHangCounter = (isGrounded) ? verticalParams.jumpHangTime : jumpHangCounter - Time.deltaTime;
			// jump buffer
			jumpBufferCount = (jumpDownEvent) ? verticalParams.jumpBufferLength : jumpBufferCount - Time.deltaTime;

			// players can register jump just before they hit the ground (jumpBuffer) or just after they start falling (jumpHang)
			// This is to create the illusion of perfectly timed jumps.
			if (jumpBufferCount >= 0.0f && jumpHangCounter > 0.0f) {
				velocity.y = verticalParams.maxVelocity;
				// avoid multipe jump due to buffer and hang
				jumpHangCounter = 0;
				jumpBufferCount = 0;
				jumpTriggered = true;
			}
			// controllable jump, add gravity if isJump is false and the character is in air
			if (!isJump && velocity.y > 0) {
				velocity += Vector2.up * Physics2D.gravity.y * (verticalParams.controllableJumpMultiplier - 1.0f) * Time.deltaTime;
			}
			// fall faster
			else if (velocity.y < 0.0f) {
				// -1.0f too account for physics systems gravity;
				velocity += Vector2.up * Physics2D.gravity.y * (verticalParams.fallMultiplier - 1.0f) * Time.deltaTime;
			}

			return jumpTriggered;
		}

		private void calculateHorizontalMovment(ref Vector2 velocity, float horizontalAxis, bool isGrounded) {
			// when the player turns wait till they slow down by (turnRate)% of their maximum movement speed
			if (Mathf.Sign(horizontalAxis) != Mathf.Sign(velocity.x) && Mathf.Abs(velocity.x) > horizontalParams.maxVelocity * (1.0f - horizontalParams.turnRate)) {
				horizontalAxis = 0.0f;
				this.fakeHorizontalAxis = 0.0f; // set fakeHorizontalAxis for turn recover
			} else {
				// pick absolute minumum
				horizontalAxis = Mathf.Sign(horizontalAxis) * Mathf.Min(Mathf.Abs(horizontalAxis), Mathf.Abs(this.fakeHorizontalAxis));
				velocity.x += horizontalAxis;
				this.fakeHorizontalAxis += horizontalParams.turnRecover;
			}
			// movement damping
			if (Mathf.Abs(velocity.x) > 0.01f) {
				if (Mathf.Abs(horizontalAxis) < 0.01f) // stop
					velocity.x *= calculateDamping(horizontalParams.dampingStop, horizontalParams.dampingConstant);
				else if (!isGrounded && velocity.y > 0.01f)
					velocity.x *= calculateDamping(horizontalParams.dampingJump, horizontalParams.dampingConstant);
				else if (!isGrounded && velocity.y < -0.01f)
					velocity.x *= calculateDamping(horizontalParams.dampingFall, horizontalParams.dampingConstant);
				else
					velocity.x *= calculateDamping(horizontalParams.dampingMovement, horizontalParams.dampingConstant);
				velocity.x = Mathf.Sign(velocity.x) * Mathf.Clamp(Mathf.Abs(velocity.x), 0, horizontalParams.maxVelocity);
			} else velocity.x = 0;
		}

		/// <summary>
		/// This functions calculates player movement based on the given paramters.
		/// </summary>
		/// <param name="velocity">Last velocity of the character</param>
		/// <param name="isGrounded">Is the character groundend</param>
		/// <param name="horizontalAxis">Horizontal Axis movement</param>
		/// <param name="isJump">Is the jump button pressed</param>
		/// <returns>MoveRuslt struct containing new velocity and jumpTriggered flag</returns>
		public MoveResult Move(Vector2 velocity, bool isGrounded, float horizontalAxis, bool isJumpDown) {
			bool jumpDownEvent = (isJumpDownLast == false) && (isJumpDown == true);
			isJumpDownLast = isJumpDown;

			bool jumpTriggered = calculateVerticalMovement(ref velocity, isGrounded, isJumpDown, jumpDownEvent);
			calculateHorizontalMovment(ref velocity, horizontalAxis, isGrounded);
			return new MoveResult(velocity, jumpTriggered);
		}
	}
}