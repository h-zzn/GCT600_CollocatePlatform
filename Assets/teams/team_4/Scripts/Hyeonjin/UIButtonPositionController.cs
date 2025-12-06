using UnityEngine;

public class UIPositionManager : MonoBehaviour
{
    [Header("UI Prefab (World Space Canvas)")]
    public GameObject uiPrefab;

    [Header("Distance from Screen")]
    public float distanceFromScreen = 1.5f;

    [Header("Extra Offset (Optional)")]
    public Vector3 extraOffset = Vector3.zero;

    private void Start()
    {
        MRUKManager.Instance.OnRoomReady += OnRoomReadyHandler;
    }

    private void OnRoomReadyHandler(Meta.XR.MRUtilityKit.MRUKRoom room)
    {
        AttachToAllScreens();
    }

    private void AttachToAllScreens()
    {
        var screens = MRUKManager.Instance.ScreenAnchors;

        if (screens == null || screens.Count == 0)
        {
            Debug.LogWarning("[UIPositionManager] No SCREEN anchors found.");
            return;
        }

        var tableAnchor = MRUKManager.Instance.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[UIPositionManager] TABLE anchor not found!");
            return;
        }

        foreach (var screen in screens)
        {
            Debug.Log($"[UIPositionManager] Placing UI near SCREEN: {screen.name}");

            GameObject ui = Instantiate(uiPrefab);

            // 위치 배치
            PlaceUIBehindScreen(ui, screen, tableAnchor);

            // ★ Label 업데이트
            var labelDisplay = ui.GetComponent<ScreenLabelDisplay>();
            if (labelDisplay != null)
            {
                labelDisplay.Initialize(screen);
            }
            else
            {
                Debug.LogWarning("[UIPositionManager] ScreenLabelDisplay not found on UI prefab!");
            }
        }
    }

    private void PlaceUIBehindScreen(GameObject ui, Meta.XR.MRUtilityKit.MRUKAnchor screenAnchor, 
                                      Meta.XR.MRUtilityKit.MRUKAnchor tableAnchor)
    {
        Vector3 toTable = tableAnchor.transform.position - screenAnchor.transform.position;
        float dot = Vector3.Dot(screenAnchor.transform.up.normalized, toTable.normalized);
        
        Vector3 behindDirection;
        if (dot > 0)
        {
            behindDirection = -screenAnchor.transform.up;
            Debug.Log($"[UIPositionManager] Screen faces table → placing on opposite side (dot={dot:F2})");
        }
        else
        {
            behindDirection = screenAnchor.transform.up;
            Debug.Log($"[UIPositionManager] Screen faces away from table → placing on table side (dot={dot:F2})");
        }
        
        Vector3 pos = screenAnchor.transform.position + (behindDirection * distanceFromScreen);
        
        pos += screenAnchor.transform.right * extraOffset.x;
        pos += behindDirection * extraOffset.z;
        pos += Vector3.up * extraOffset.y;
        
        if (extraOffset.y == 0)
        {
            pos.y = 0f;
        }
        
        ui.transform.position = pos;
        
        Vector3 lookDirection = behindDirection;
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            ui.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        Debug.Log($"[UIPositionManager] Screen pos: {screenAnchor.transform.position}, " +
                  $"Placement direction: {behindDirection}, " +
                  $"UI placed at: {pos}");
    }
}