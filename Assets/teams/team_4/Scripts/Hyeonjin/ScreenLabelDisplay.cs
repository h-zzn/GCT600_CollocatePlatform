using UnityEngine;
using TMPro;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScreenLabelDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject buttonObject; // Button GameObject 참조 (onClick 대신 Collider용)

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Content Canvas Prefabs")]
    [SerializeField] private GameObject tigerCanvasPrefab;
    [SerializeField] private GameObject personCanvasPrefab;
    [SerializeField] private GameObject flowerCanvasPrefab;

    [Header("Canvas Spawn Settings")]
    [SerializeField] private float canvasDistanceRight = 0.5f;
    [SerializeField] private float canvasDistanceFront = 0.3f;

    // ScreenID → Label 매핑
    private Dictionary<string, string> screenLabelMap = new Dictionary<string, string>()
    {
        { "SCREEN_Tiger", "Hojak-do" },
        { "SCREEN_Person", "Pyungan-do" },
        { "SCREEN_Flower", "Chochung-do" }
    };

    // ScreenID → Canvas Prefab 매핑
    private Dictionary<string, GameObject> screenCanvasMap;

    private MRUKAnchor assignedScreen;
    private GameObject currentActiveCanvas;

    private void Awake()
    {
        screenCanvasMap = new Dictionary<string, GameObject>()
        {
            { "SCREEN_Tiger", tigerCanvasPrefab },
            { "SCREEN_Person", personCanvasPrefab },
            { "SCREEN_Flower", flowerCanvasPrefab }
        };
    }

    public void Initialize(MRUKAnchor screenAnchor)
    {
        assignedScreen = screenAnchor;
        SetupButtonTrigger();
        StartCoroutine(WaitForScreenID());
    }

    private void SetupButtonTrigger()
    {
        // Button GameObject 자동으로 찾기
        if (buttonObject == null)
        {
            var button = GetComponentInChildren<Button>();
            if (button != null)
            {
                buttonObject = button.gameObject;
            }
        }

        if (buttonObject != null)
        {
            // ButtonHandTrigger 컴포넌트 추가 (없으면)
            if (buttonObject.GetComponent<ButtonHandTrigger>() == null)
            {
                buttonObject.AddComponent<ButtonHandTrigger>();
            }

            // BoxCollider 추가 (없으면)
            if (buttonObject.GetComponent<BoxCollider>() == null)
            {
                BoxCollider collider = buttonObject.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                
                // RectTransform 크기에 맞춰 Collider 크기 조정
                RectTransform rect = buttonObject.GetComponent<RectTransform>();
                if (rect != null)
                {
                    collider.size = new Vector3(rect.rect.width, rect.rect.height, 10f);
                }
                
                Debug.Log("[ScreenLabelDisplay] BoxCollider added to button");
            }

            Debug.Log("[ScreenLabelDisplay] Button trigger setup complete");
        }
        else
        {
            Debug.LogWarning("[ScreenLabelDisplay] Button object not found!");
        }
    }

    // ★ ButtonHandTrigger에서 호출할 Public 메서드
    public void OnButtonTriggered()
    {
        var identifier = assignedScreen.GetComponentInChildren<ScreenIdentifier>(true);
        if (identifier != null && !string.IsNullOrEmpty(identifier.screenID))
        {
            ToggleContentCanvas(identifier.screenID);
        }
        else
        {
            Debug.LogWarning("[ScreenLabelDisplay] ScreenID not available on button trigger");
        }
    }

    private void ToggleContentCanvas(string screenID)
    {
        // 이미 Canvas가 활성화되어 있으면 토글(닫기)
        if (currentActiveCanvas != null)
        {
            Destroy(currentActiveCanvas);
            currentActiveCanvas = null;
            Debug.Log($"[ScreenLabelDisplay] Closed content canvas for {screenID}");
            return;
        }

        // 해당 screenID에 맞는 Canvas Prefab 찾기
        if (screenCanvasMap.TryGetValue(screenID, out GameObject canvasPrefab))
        {
            if (canvasPrefab != null)
            {
                // Canvas 인스턴스 생성
                currentActiveCanvas = Instantiate(canvasPrefab);
                
                // Screen의 오른쪽 위치 계산
                Vector3 rightOffset = assignedScreen.transform.right * canvasDistanceRight;
                Vector3 frontOffset = assignedScreen.transform.up * canvasDistanceFront;
                
                Vector3 spawnPosition = assignedScreen.transform.position + rightOffset + frontOffset;
                
                currentActiveCanvas.transform.position = spawnPosition;
                
                // Screen의 up 방향을 바라보도록 회전
                currentActiveCanvas.transform.rotation = Quaternion.LookRotation(assignedScreen.transform.up);
                
                Debug.Log($"[ScreenLabelDisplay] Opened content canvas for {screenID} at {spawnPosition}");
            }
            else
            {
                Debug.LogWarning($"[ScreenLabelDisplay] Canvas prefab is null for {screenID}");
            }
        }
        else
        {
            Debug.LogWarning($"[ScreenLabelDisplay] No canvas mapping found for {screenID}");
        }
    }

    private IEnumerator WaitForScreenID()
    {
        float elapsedTime = 0f;
        ScreenIdentifier identifier = null;

        Debug.Log($"[ScreenLabelDisplay] Waiting for ScreenID assignment...");

        while (elapsedTime < maxWaitTime)
        {
            identifier = assignedScreen.GetComponentInChildren<ScreenIdentifier>(true);

            if (identifier != null && !string.IsNullOrEmpty(identifier.screenID))
            {
                Debug.Log($"[ScreenLabelDisplay] Found ScreenID: {identifier.screenID} after {elapsedTime:F2}s");
                break;
            }

            yield return new WaitForSeconds(checkInterval);
            elapsedTime += checkInterval;
        }

        // 최종 업데이트
        if (identifier != null && !string.IsNullOrEmpty(identifier.screenID))
        {
            UpdateLabel(identifier.screenID);
        }
        else
        {
            Debug.LogWarning($"[ScreenLabelDisplay] ScreenID not assigned after {maxWaitTime}s!");
        }
    }

    private void UpdateLabel(string screenID)
    {
        if (labelText == null)
        {
            Debug.LogWarning("[ScreenLabelDisplay] labelText is not assigned!");
            return;
        }

        if (screenLabelMap.TryGetValue(screenID, out string mappedLabel))
        {
            labelText.text = mappedLabel;
            Debug.Log($"[ScreenLabelDisplay] ScreenID '{screenID}' → Label '{mappedLabel}'");
        }
        else
        {
            labelText.text = screenID;
            Debug.LogWarning($"[ScreenLabelDisplay] No mapping found for '{screenID}', using original ID");
        }
    }

    private void OnDestroy()
    {
        if (currentActiveCanvas != null)
        {
            Destroy(currentActiveCanvas);
        }
    }
}