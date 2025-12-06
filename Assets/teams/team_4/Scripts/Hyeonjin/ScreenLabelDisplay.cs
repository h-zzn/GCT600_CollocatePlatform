using UnityEngine;
using TMPro;
using Meta.XR.MRUtilityKit;
using System.Collections;
using System.Collections.Generic;

public class ScreenLabelDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI labelText;

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private float checkInterval = 0.2f;

    // ★ ScreenID → Label 매핑
    private Dictionary<string, string> screenLabelMap = new Dictionary<string, string>()
    {
        { "SCREEN_Tiger", "Hojak-do" },
        { "SCREEN_Person", "Pyungan-do" },
        { "SCREEN_Flower", "Chochung-do" }
    };

    private MRUKAnchor assignedScreen;

    public void Initialize(MRUKAnchor screenAnchor)
    {
        assignedScreen = screenAnchor;
        StartCoroutine(WaitForScreenID());
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

        // ★ 매핑된 라벨 찾기
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
}