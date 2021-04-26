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

    public Vector2 DieBounds = new Vector2(22.5f, 14.0f);

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
            var force = Input.GetKey(LeftKey)
                ? GetForce(LeftMoveMode)
                : Input.GetKey(RightKey)
                    ? GetForce(RightMoveMode)
                    : Vector2.zero
                ;
            if (!GameManager.Inst.CanControlPlayer || !GameManager.Inst.CanControlPlayer2) {
                force = Vector2.zero;
            }
            rigidbody2D.AddForce(force, ForceMode2D.Force);
            if (isOnGround) {
                animator.Play(force.x == 0
                    ? "Idle"
                    : "Run"
                );
            } else {
                animator.Play("Jump");
            }
            AudioManager.Inst.PlaySteps = isOnGround && force.x != 0;
            if (force.x > 0) {
                animator.transform.localScale = animator.transform.localScale.withX(1);
            } else if (force.x < 0) {
                animator.transform.localScale = animator.transform.localScale.withX(-1);
            }
        }
        // Jump
        {
            if (GameManager.Inst.CanControlPlayer && GameManager.Inst.CanControlPlayer2) {
                if (isOnGround && Input.GetKey(JumpKey)) {
                    DoJump(JumpMode);
                    isOnGround = false;
                }
            }
        }
        // Die
        {
            if (
                transform.position.x >= DieBounds.x/2 ||
                transform.position.x <= -DieBounds.x/2 ||
                transform.position.y >= DieBounds.y/2 ||
                transform.position.y <= -DieBounds.y/2
            ) {
                GameManager.Inst.Die();
            }
        }
    }

    public void SetJumpMode(int mode) {
        JumpMode = (JumpModes) mode;
    }

    public void SetLeftMode(int mode) {
        LeftMoveMode = (MoveModes) mode;
    }

    public void SetRightMode(int mode) {
        RightMoveMode = (MoveModes) mode;
    }

    void setKey(ref KeyCode key, int index) {
        if (index == 1) {
            key = KeyCode.A;
        } else if (index == 2) {
            key = KeyCode.Space;
        } else if (index == 4) {
            key = KeyCode.LeftArrow;
        } else if (index == 5) {
            key = KeyCode.D;
        } else if (index == 8) {
            key = KeyCode.RightArrow;
        } else if (index == 9) {
            key = KeyCode.R;
        } else {
            key = KeyCode.None;
        }
    }

    public void SetJumpKey(int index) {
        setKey(ref JumpKey, index);
    }

    public void SetLeftKey(int index) {
        setKey(ref LeftKey, index);
    }

    public void SetRightKey(int index) {
        setKey(ref RightKey, index);
    }

    void OnCollisionStay2D(Collision2D other) {
        if (other.gameObject.transform.position.y < transform.position.y) {
            isOnGround = true;
            onGroundTime = OnGroundTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Flag") {
            GameManager.Inst.TouchedFlag();
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
                GameManager.Inst.Die();
                break;
            case JumpModes.Jump:
                rigidbody2D.velocity = rigidbody2D.velocity.withY(JumpForce);
                AudioManager.Inst.PlaySound("jump");
                break;
        }
    }
}
