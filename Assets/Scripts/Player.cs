using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public enum MoveModes {
        Random, Left, Right
    }

    public enum JumpModes {
        Die, Jump
    }


    public float MoveSpeed = 5;
    public MoveModes LeftMoveMode;
    public KeyCode LeftKey = KeyCode.None;
    public MoveModes RightMoveMode;
    public KeyCode RightKey = KeyCode.None;

    public float JumpForce = 40;
    public float JumpMultiplier = 1;
    public JumpModes JumpMode = JumpModes.Die;
    public KeyCode JumpKey = KeyCode.None;

    bool isOnGround;
    public float OnGroundTime = 0.25f;
    float onGroundTime;

    Animator animator;
    new Rigidbody2D rigidbody2D;

    void Awake() {
        animator = GetComponentInChildren<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate() {
        // Decay OnGroundTime
        {
            onGroundTime -= Time.deltaTime;
            if (onGroundTime <= 0) {
                isOnGround = false;
            }
        }
        // Move
        {
            Vector2 force = Input.GetKey(LeftKey)
                ? GetForce(LeftMoveMode)
                : Input.GetKey(RightKey)
                    ? GetForce(RightMoveMode)
                    : Vector2.zero
                ;
            rigidbody2D.AddForce(force, ForceMode2D.Force);
            if (isOnGround) {
                animator.Play(force.x == 0
                    ? "Idle"
                    : "Run"
                );
            } else {
                animator.Play("Jump");
            }
            if (force.x > 0) {
                animator.transform.localScale = animator.transform.localScale.withX(1);
            } else if (force.x < 0) {
                animator.transform.localScale = animator.transform.localScale.withX(-1);
            }
        }
        // Jump
        {
            if (isOnGround && Input.GetKey(JumpKey)) {
                DoJump(JumpMode);
                isOnGround = false;
            }
        }
    }

    void OnCollisionStay2D(Collision2D other) {
        if (other.gameObject.transform.position.y < transform.position.y) {
            isOnGround = true;
            onGroundTime = OnGroundTime;
        }
    }

    public Vector2 GetForce(MoveModes mode) {
        switch (mode) {
            case MoveModes.Left:
                return Vector2.left * MoveSpeed;
            case MoveModes.Right:
                return Vector2.right * MoveSpeed;
            case MoveModes.Random:
                var toggler = (int)(Time.timeSinceLevelLoad * 3) % 2 == 0;
                return new Vector2(toggler ? 1 : -1, 0) * MoveSpeed;
        }
        return Vector2.zero;
    }

    public void DoJump(JumpModes mode) {
        switch (mode) {
            case JumpModes.Die:
                Debug.LogError("TODO: DIE");
                break;
            case JumpModes.Jump:
                rigidbody2D.velocity = rigidbody2D.velocity.withY(JumpForce);
                break;
        }
    }
}
