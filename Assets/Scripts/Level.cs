using UnityEngine;

public class Level : MonoBehaviour {

    Player player;
    Vector3 originalPos;
    Quaternion originalRot;

    void Awake() {
        player = GetComponentInChildren<Player>();
        originalPos = player.transform.position;
        originalRot = player.transform.rotation;
    }

    public void Setup() {
        player.transform.SetPositionAndRotation(originalPos, originalRot);
        player.enabled = true;
        player.GetComponentInChildren<Rigidbody2D>().gravityScale = 1;
    }
}