using UnityEngine;
using System.Linq;
using System.Collections;
using Meta.XR.MRUtilityKit;
using TMPro;

public class ScreenIDAssigner : MonoBehaviour
{
    [SerializeField]
    private string[] screenNames = { "Tiger", "Flower", "Person" };

    private void Start()
    {
        StartCoroutine(WaitForMRUKManager());
    }

    private IEnumerator WaitForMRUKManager()
    {
        while (MRUKManager.Instance == null)
            yield return null;

        MRUKManager.Instance.OnRoomReady += HandleRoomReady;

        if (MRUKManager.Instance.IsReady)
            HandleRoomReady(MRUKManager.Instance.CurrentRoom);
    }

    private void OnDisable()
    {
        if (MRUKManager.Instance != null)
            MRUKManager.Instance.OnRoomReady -= HandleRoomReady;
    }

    private void HandleRoomReady(MRUKRoom room)
    {
        AssignIDs();
    }

    private void AssignIDs()
    {
        // 1. MRUKAnchor에서 SCREEN 레이블을 가진 앵커 찾기
        var screenAnchors = FindObjectsOfType<MRUKAnchor>()
            .Where(a => a.Label == MRUKAnchor.SceneLabels.SCREEN)
            .ToList();
        
        if (screenAnchors.Count == 0)
        {
            Debug.LogWarning("[ScreenIDAssigner] No SCREEN anchors found.");
            return;
        }

        // 2. anchor를 위치 순서대로 정렬 
        screenAnchors = screenAnchors
            .OrderBy(s => s.transform.position.x)
            .ThenBy(s => s.transform.position.z)
            .ToList();

        // 3. 각 anchor의 child에서 ScreenIdentifier 찾기 / 없으면 생성
        for (int i = 0; i < screenAnchors.Count; i++)
        {
            var screenAnchor = screenAnchors[i];

            // anchor의 child 중 screenIDentigier 있는지 체크
            var IdComp = screenAnchor.GetComponentInChildren<ScreenIdentifier>();

            // 없으면 자동으로 붙이기
            if (IdComp == null)
            {
                Transform childScreen = screenAnchor.transform.GetChild(0);

                if (childScreen != null)
                {
                    IdComp = childScreen.gameObject.AddComponent<ScreenIdentifier>();
                    Debug.Log($"[ScreenIDAssigner] Added ScreenIdentifier to '{childScreen.name}' under SCREEN anchor '{screenAnchor.name}'.");
                }
                else
                {
                    Debug.LogWarning($"[ScreenIDAssigner] SCREEN anchor '{screenAnchor.name}' has no child to attach ScreenIdentifier.");
                    continue;
                }
            }

            // 4. ID 할당
            string contentName = (i < screenNames.Length)
                ? screenNames[i]
                : $"Extra_{i + 1}";

            IdComp.screenID = $"SCREEN_{contentName}";

            Debug.Log($"[ScreenIDAssigner] {screenAnchor.name} → {IdComp.screenID}");
        }
    }
}
