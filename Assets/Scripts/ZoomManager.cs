using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class ZoomManager : Manager<ZoomManager> {

    [System.Serializable]
    public class ZoomConfig {
        public float MaxCameraSize;
        public float TargetCameraSize;
        public CanvasGroup Group;
        public bool CanMove;
    }

    public ZoomConfig[] ZoomConfigs = Enumerable.Range(0, 7).Select(x => new ZoomConfig()).ToArray();

    public Camera ZoomCamera;
    public float MinCameraSize;
    public AnimationCurve ScrollAmount = AnimationCurve.Linear(0, 1, 0, 1);
    [Range(0.75f, 0.99f)]
    public float ScrollSmoothing = 0.95f;
    float desiredSize;
    [Range(0, 0.5f)]
    public float TargetSizeDelay = 0.15f;
    float targetSizeDelay;
    [Range(0, 0.5f)]
    public float AlphaSpeed = 0.2f;

    void Awake() {
        ZoomCamera = ZoomCamera == null ? Camera.main : ZoomCamera;
        desiredSize = ZoomCamera.orthographicSize;
    }

    void Update() {
        // Zoom camera
        {
            var scrollAmount = ScrollAmount.Evaluate(desiredSize);
            desiredSize -= Input.mouseScrollDelta.y * scrollAmount;
            ZoomCamera.orthographicSize = Mathf.Lerp(ZoomCamera.orthographicSize, desiredSize, 1 - ScrollSmoothing);
        }
        // Limit camera
        {
            desiredSize = Mathf.Clamp(desiredSize, MinCameraSize, ZoomConfigs.Last().MaxCameraSize);
            ZoomCamera.orthographicSize = Mathf.Clamp(ZoomCamera.orthographicSize, MinCameraSize, ZoomConfigs.Last().MaxCameraSize);
        }
        // Figure out current zoom level
        ZoomConfig currentZoom = ZoomConfigs.Find(config => config.MaxCameraSize >= ZoomCamera.orthographicSize);
        // Enable current zoom group
        {
            foreach (var zoom in ZoomConfigs) {
                var enabled = zoom == currentZoom;
                zoom.Group.alpha = Mathf.Lerp(zoom.Group.alpha, enabled ? 1 : 0, AlphaSpeed);
                zoom.Group.interactable = enabled;
                zoom.Group.blocksRaycasts = enabled;
            }
        }
        // Target size when resting
        {
            if (Input.mouseScrollDelta.y != 0) {
                targetSizeDelay = TargetSizeDelay;
            }
            if (targetSizeDelay > 0) {
                targetSizeDelay -= Time.deltaTime;
                if (targetSizeDelay <= 0) {
                    desiredSize = currentZoom.TargetCameraSize;
                }
            }
        }
    }
}
