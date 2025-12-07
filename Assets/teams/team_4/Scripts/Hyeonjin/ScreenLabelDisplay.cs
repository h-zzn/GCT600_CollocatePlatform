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
    [SerializeField] private Button actionButton; // 버튼 참조

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Content Canvas Prefabs")]
    [SerializeField] private GameObject tigerCanvasPrefab;   // Tiger용 Canvas
    [SerializeField] private GameObject personCanvasPrefab;  // Person용 Canvas
    [SerializeField] private GameObject flowerCanvasPrefab;  // Flower용 Canvas

    [Header("Canvas Spawn Settings")]
    [SerializeField] private float canvasDistance = 0.8f; // 스크린으로부터의 거리
    [SerializeField] private Vector3 canvasOffset = Vector3.zero;

    [Header("Canvas Spawn Settings")]
    [SerializeField] private float canvasDistanceRight = 0.5f; // 오른쪽으로 얼마나 떨어뜨릴지
    [SerializeField] private float canvasDistanceFront = 0.3f; // 앞으로 얼마나 띄울지
    [SerializeField] private Vector3 canvasScale = new Vector3(0.0008f, 0.0008f, 0.0008f); // Canvas 크기

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
    private GameObject currentActiveCanvas; // 현재 활성화된 Canvas 인스턴스

    private void Awake()
    {
        // Canvas 매핑 초기화
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
        SetupButton();
        StartCoroutine(WaitForScreenID());
    }

    private void SetupButton()
    {
        // Button을 자동으로 찾거나 Inspector에서 할당
        if (actionButton == null)
        {
            actionButton = GetComponentInChildren<Button>();
        }

        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnButtonClick);
            Debug.Log("[ScreenLabelDisplay] Button listener added");
        }
        else
        {
            Debug.LogWarning("[ScreenLabelDisplay] Button not found!");
        }
    }

    private void OnButtonClick()
    {
        var identifier = assignedScreen.GetComponentInChildren<ScreenIdentifier>(true);
        if (identifier != null && !string.IsNullOrEmpty(identifier.screenID))
        {
            ToggleContentCanvas(identifier.screenID);
        }
        else
        {
            Debug.LogWarning("[ScreenLabelDisplay] ScreenID not available on button click");
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
                
                // ★ Screen의 오른쪽 위치 계산
                Vector3 rightOffset = assignedScreen.transform.right * canvasDistanceRight;
                Vector3 frontOffset = assignedScreen.transform.up * canvasDistanceFront;
                
                Vector3 spawnPosition = assignedScreen.transform.position + rightOffset + frontOffset;
                
                currentActiveCanvas.transform.position = spawnPosition;
                
                // Canvas가 Screen을 향하도록 회전 (Screen과 같은 방향)
                // currentActiveCanvas.transform.rotation = assignedScreen.transform.rotation;
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

        // 매핑된 라벨 찾기
        if (screenLabelMap.TryGetValue(screenID, out string mappedLabel))
        {
            labelText.text = mappedLabel;
            Debug.Log($"[ScreenLabelDisplay] ScreenID '{screenID}' → Label '{mappedLabel}'");
        }
        else
        {
            // 매핑이 없으면 원본 ID 그대로 표시
            labelText.text = screenID;
            Debug.LogWarning($"[ScreenLabelDisplay] No mapping found for '{screenID}', using original ID");
        }
    }

    private void OnDestroy()
    {
        // 정리: 활성화된 Canvas가 있으면 제거
        if (currentActiveCanvas != null)
        {
            Destroy(currentActiveCanvas);
        }

        // Button listener 제거
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnButtonClick);
        }
    }
}