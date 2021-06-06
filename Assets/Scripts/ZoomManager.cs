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
        public bool CanControlPlayer;
        public bool AlwaysVisible;
        public bool IsMainLevel;
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
    [Range(0, 0.1f)]
    public float CenteringSpeed = 0.02f;
    public Vector2 PanBoundsWidthHeight = new Vector2(23.5f, 15.33f);
    [Range(0, 0.5f)]
    public float AlphaSpeed = 0.2f;

    Vector2? desiredPos;
    [Range(0, 0.5f)]
    public float ZoomPanSpeed = 0.1f;

    bool canPan;
    Vector2 prevMouse;

    public AnimationCurve SizeToPitch;

    void Awake() {
        ZoomCamera = ZoomCamera == null ? Camera.main : ZoomCamera;
        desiredSize = ZoomCamera.orthographicSize;
    }

    public void CanPanEnable() => canPan = true;
    public void CanPanDisable() => canPan = false;

    void Update() {
        var scrolled = Input.mouseScrollDelta.y != 0;
        // Zoom camera
        {
            if (scrolled) {
                var scrollAmount = ScrollAmount.Evaluate(desiredSize);
                desiredSize -= Input.mouseScrollDelta.y * scrollAmount;

                var mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var vecToMouse = ZoomCamera.transform.position - mousePoint;
                var dirToMouse = -Mathf.Sign(Input.mouseScrollDelta.y) * vecToMouse * 0.15f;
                desiredPos = desiredPos ?? ZoomCamera.transform.position;
                desiredPos += dirToMouse;
            }
            var newSize = Mathf.Lerp(ZoomCamera.orthographicSize, desiredSize, 1 - ScrollSmoothing);
            AudioManager.Inst.VolumeScroll = Mathf.Clamp01(200 * Mathf.Abs(ZoomCamera.orthographicSize - newSize));
            AudioManager.Inst.PitchScroll = SizeToPitch.Evaluate(newSize);
            ZoomCamera.orthographicSize = newSize;
        }
        // Figure out current zoom level
        ZoomCamera.orthographicSize = Mathf.Clamp(ZoomCamera.orthographicSize, MinCameraSize, ZoomConfigs.Last().MaxCameraSize);
        var currentZoomIndex = ZoomConfigs.IndexOf(ZoomCamera, (config, cam) => config.MaxCameraSize >= cam.orthographicSize);
        var currentZoom = ZoomConfigs[currentZoomIndex];
        GameManager.Inst.CanControlPlayer = currentZoom.CanControlPlayer;
        // Move camera
        {
            if (currentZoom.CanMove) {
                if (canPan) {
                    if (Input.GetMouseButton(0)) {
                        var prevPoint = ZoomCamera.ScreenToWorldPoint(prevMouse);
                        var newPoint = ZoomCamera.ScreenToWorldPoint(Input.mousePosition);
                        ZoomCamera.transform.Translate(prevPoint - newPoint);
                        desiredPos = null;
                    }
                }
            } else {
                ZoomCamera.transform.position = Vector2.Lerp(ZoomCamera.transform.position, Vector2.zero, CenteringSpeed).to3(ZoomCamera.transform.position.z);
            }
            var isMainZoomingIn = currentZoom.IsMainLevel && ZoomCamera.orthographicSize < currentZoom.TargetCameraSize;
            if (!currentZoom.CanMove && !isMainZoomingIn) {
                desiredPos = null;
            }
            if (desiredPos.HasValue) {
                ZoomCamera.transform.position = Vector2.Lerp(ZoomCamera.transform.position, desiredPos.Value, ZoomPanSpeed).to3(ZoomCamera.transform.position.z);
            }
            if (currentZoom.CanMove || isMainZoomingIn) {
                var aspect = (float)Screen.width / (float)Screen.height;
                var camHeight = ZoomCamera.orthographicSize * 2;
                var camWidth = camHeight * aspect;
                var minX = ZoomCamera.transform.position.x - camWidth/2;
                var maxX = ZoomCamera.transform.position.x + camWidth/2;
                var minY = ZoomCamera.transform.position.y - camHeight/2;
                var maxY = ZoomCamera.transform.position.y + camHeight/2;
                if (minX < -PanBoundsWidthHeight.x/2) {
                    ZoomCamera.transform.Translate(new Vector2(-PanBoundsWidthHeight.x/2 - minX, 0));
                }
                if (maxX > PanBoundsWidthHeight.x/2) {
                    ZoomCamera.transform.Translate(new Vector2(PanBoundsWidthHeight.x/2 - maxX, 0));
                }
                if (minY < -PanBoundsWidthHeight.y/2) {
                    ZoomCamera.transform.Translate(new Vector2(0, -PanBoundsWidthHeight.y/2 - minY));
                }
                if (maxY > PanBoundsWidthHeight.y/2) {
                    ZoomCamera.transform.Translate(new Vector2(0, PanBoundsWidthHeight.y/2 - maxY));
                }
            }
        }
        // Limit camera
        {
            desiredSize = Mathf.Clamp(desiredSize, MinCameraSize, ZoomConfigs.Last().MaxCameraSize);
            ZoomCamera.orthographicSize = Mathf.Clamp(ZoomCamera.orthographicSize, MinCameraSize, ZoomConfigs.Last().MaxCameraSize);
        }
        // Enable current zoom group
        {
            for (var i = 0; i < ZoomConfigs.Length; i++) {
                var enabled = i == currentZoomIndex;
                var zoom = ZoomConfigs[i];
                var over = i >= currentZoomIndex || zoom.AlwaysVisible;
                zoom.Group.alpha = Mathf.Lerp(zoom.Group.alpha, enabled ? 1 : over ? 0.35f : 0, AlphaSpeed);
                zoom.Group.interactable = enabled;
                zoom.Group.blocksRaycasts = enabled;
            }
        }
        // Target size when resting
        {
            if (scrolled) {
                targetSizeDelay = TargetSizeDelay;
            }
            if (targetSizeDelay > 0) {
                targetSizeDelay -= Time.deltaTime;
                if (targetSizeDelay <= 0) {
                    desiredSize = currentZoom.TargetCameraSize;
                    desiredPos = null;
                }
            }
        }
        // Shortcuts to zoom
        {
            var nextIndex = currentZoomIndex + (Input.GetKeyDown(KeyCode.UpArrow) ? -1 : Input.GetKeyDown(KeyCode.DownArrow) ? 1 : 0);
            if (nextIndex != currentZoomIndex && nextIndex >= 0 && nextIndex < ZoomConfigs.Length) {
                desiredSize = ZoomConfigs[nextIndex].TargetCameraSize;
            }
        }
        // Stuff
        prevMouse = Input.mousePosition;
    }
}
