using UnityEngine;
using System.Collections;
using System.Linq;
using Meta.XR.MRUtilityKit;

public class ScreenIDAssigner : MonoBehaviour
{
    [SerializeField]
    private string[] screenNames = { "Tiger", "Flower", "Person" };

    private AnchorPrefabSpawner spawner;

    private void Start()
    {
        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // MRUK 준비될 때까지 대기
        while (MRUKManager.Instance == null || !MRUKManager.Instance.IsReady)
            yield return null;

        // PrefabSpawner 찾기
        spawner = FindObjectOfType<AnchorPrefabSpawner>();

        if (spawner == null)
        {
            Debug.LogError("[ScreenIDAssigner] AnchorPrefabSpawner not found!");
            yield break;
        }

        // PrefabSpawner의 spawn 이벤트 구독
        spawner.onPrefabSpawned.AddListener(OnPrefabsSpawned);

        // 이미 스폰된 Prefab이 있으면 즉시 Assign
        yield return new WaitForSeconds(0.2f);
        OnPrefabsSpawned();
    }

    private void OnDestroy()
    {
        if (spawner != null)
            spawner.onPrefabSpawned.RemoveListener(OnPrefabsSpawned);
    }

    private void OnPrefabsSpawned()
    {
        StartCoroutine(AssignAfterDelay());
    }

    private IEnumerator AssignAfterDelay()
    {
        // MRUK가 prefab 배치를 모두 마칠 때까지 한 프레임 대기
        yield return null;

        AssignIDs();
    }

    private void AssignIDs()
    {
        // PrefabSpawner가 실제로 생성한 SCREEN prefab만 접근
        var spawnerObjects = FindObjectOfType<AnchorPrefabSpawner>().AnchorPrefabSpawnerObjects;

        var screens = spawnerObjects
            .Select(kv => kv.Value.GetComponentInChildren<ScreenIdentifier>())
            .Where(s => s != null)
            .ToList();

        if (screens.Count == 0)
        {
            Debug.Log("[ScreenIDAssigner] No screens found.");
            return;
        }

        // 좌표 기반 중복 제거 + 정렬
        var uniqueScreens = screens
            .GroupBy(s => RoundVec3(s.transform.position))
            .Select(g => g.First())
            .OrderBy(s => s.transform.position.x)
            .ThenBy(s => s.transform.position.z)
            .ToList();

        Debug.Log($"[ScreenIDAssigner] Assigning IDs to {uniqueScreens.Count} screen(s)");

        for (int i = 0; i < uniqueScreens.Count; i++)
        {
            string contentName = (i < screenNames.Length)
                ? screenNames[i]
                : $"Extra_{i + 1}";

            string finalID = $"SCREEN_{contentName}";
            uniqueScreens[i].screenID = finalID;

            Debug.Log($"[ScreenIDAssigner] {uniqueScreens[i].name} → {finalID}");
        }
    }

    private Vector3 RoundVec3(Vector3 v)
    {
        return new Vector3(
            Mathf.Round(v.x * 100f) / 100f,
            Mathf.Round(v.y * 100f) / 100f,
            Mathf.Round(v.z * 100f) / 100f
        );
    }
}
