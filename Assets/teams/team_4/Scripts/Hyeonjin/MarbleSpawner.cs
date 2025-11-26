using UnityEngine;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class MarbleSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject marblePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnHeightOffset = 0.5f; // OTHER 앵커 위로 얼마나 띄울지
    [SerializeField] private bool spawnOnStart = true;

    private void Start()
    {
        if (spawnOnStart)
        {
            StartCoroutine(WaitAndSpawn());
        }
    }

    private IEnumerator WaitAndSpawn()
    {
        // MRUKManager가 준비될 때까지 대기
        while (MRUKManager.Instance == null || !MRUKManager.Instance.IsReady)
        {
            yield return null;
        }

        SpawnMarbles();
    }

    // Marble 스폰 함수 (외부에서도 호출 가능)
    public void SpawnMarbles()
    {
        if (marblePrefab == null)
        {
            Debug.LogError("[MarbleSpawner] Marble prefab is not assigned!");
            return;
        }

        if (MRUKManager.Instance == null || !MRUKManager.Instance.IsReady)
        {
            Debug.LogWarning("[MarbleSpawner] MRUKManager is not ready!");
            return;
        }

        var otherAnchors = MRUKManager.Instance.OtherAnchors;

        if (otherAnchors == null || otherAnchors.Count == 0)
        {
            Debug.LogWarning("[MarbleSpawner] No OTHER anchors found!");
            return;
        }

        Debug.Log($"[MarbleSpawner] Found {otherAnchors.Count} OTHER anchors. Spawning marbles...");

        // 각 OTHER 앵커마다 Marble 스폰
        for (int i = 0; i < otherAnchors.Count; i++)
        {
            var anchor = otherAnchors[i];
            if (anchor == null) continue;

            // Marble 생성
            GameObject marble = Instantiate(marblePrefab);

            // 위치 설정: OTHER 앵커 위치 + 높이 오프셋
            Vector3 spawnPosition = anchor.transform.position;
            spawnPosition.y += spawnHeightOffset;
            marble.transform.position = spawnPosition;

            // 이름 설정
            marble.name = $"Marble_{i}_{anchor.name}";

            Debug.Log($"[MarbleSpawner] Spawned {marble.name} at {spawnPosition}");
        }

        Debug.Log($"[MarbleSpawner] Successfully spawned {otherAnchors.Count} marbles!");
    }

    // // 모든 스폰된 Marble 제거 (필요시)
    // public void ClearMarbles()
    // {
    //     GameObject[] marbles = GameObject.FindGameObjectsWithTag("Marble"); // Tag 사용 시
    //     foreach (var marble in marbles)
    //     {
    //         Destroy(marble);
    //     }

    //     Debug.Log("[MarbleSpawner] Cleared all marbles");
    // }
}