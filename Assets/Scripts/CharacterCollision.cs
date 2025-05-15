using UnityEngine;

namespace Platformer2DCore {
	public class CharacterCollision {

		// supplied
		private readonly Rigidbody2D rigidBody;
		private readonly CapsuleCollider2D capsuleCollider;
		private readonly LayerMask groundLayer;
		private readonly float groundCheckRadius = 0.2f;

		// calculation
		public Vector2 groundCheckColliderPos { private set; get; }
		public Vector2 attackColliderPosRight { private set; get; }
		public Vector2 attackColliderPosLeft { private set; get; }



		public CharacterCollision(Rigidbody2D rigidBody, CapsuleCollider2D capsuleCollider, LayerMask groundLayer, float groundCheckRadius) {
			this.rigidBody = rigidBody;
			this.capsuleCollider = capsuleCollider;
			this.groundCheckRadius = groundCheckRadius;
			this.groundLayer = groundLayer;
			Update();
		}

		public void Update() {
			// update collider pos;
			groundCheckColliderPos = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.center.y - capsuleCollider.size.y * 0.8f);
			attackColliderPosRight = new Vector2(capsuleCollider.bounds.center.x + capsuleCollider.size.x * 0.5f, capsuleCollider.bounds.center.y);
			attackColliderPosLeft = new Vector2(capsuleCollider.bounds.center.x - capsuleCollider.size.x * 0.5f, capsuleCollider.bounds.center.y);
		}

		public bool CheckGrounded() {
			// to avoid multipe jumps due to buffer and hang
			return Physics2D.OverlapCircleAll(groundCheckColliderPos, groundCheckRadius, groundLayer).Length != 0;
		}
	}
}

