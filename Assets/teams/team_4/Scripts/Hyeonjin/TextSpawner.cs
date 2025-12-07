using UnityEngine;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class TextSpawner : MonoBehaviour
{
    [Header("Prefab Sequence")]
    [SerializeField] private GameObject firstPrefab;   // 10초 동안 표시
    [SerializeField] private GameObject secondPrefab;  // 10초 동안 표시
    [SerializeField] private GameObject thirdPrefab;   // 계속 표시

    [Header("Timing")]
    [SerializeField] private float firstDuration = 10f;
    [SerializeField] private float secondDuration = 10f;

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0.5f, 0); // TABLE 위 얼마나 띄울지
    [SerializeField] private Vector3 spawnScale = Vector3.one;

    private GameObject currentInstance;

    private void Start()
    {
        // MRUKManager의 OnRoomReady 이벤트 구독
        if (MRUKManager.Instance != null)
        {
            MRUKManager.Instance.OnRoomReady += OnRoomReady;
            Debug.Log("[TableSequenceSpawner] Subscribed to OnRoomReady");
        }
        else
        {
            Debug.LogError("[TableSequenceSpawner] MRUKManager.Instance is null!");
        }
    }

    private void OnRoomReady(MRUKRoom room)
    {
        Debug.Log("[TableSequenceSpawner] Room is ready, starting sequence");
        StartCoroutine(SpawnSequence());
    }

    private IEnumerator SpawnSequence()
    {
        var tableAnchor = MRUKManager.Instance.TableAnchor;

        if (tableAnchor == null)
        {
            Debug.LogError("[TableSequenceSpawner] TABLE anchor not found!");
            yield break;
        }

        Vector3 spawnPosition = tableAnchor.transform.position + spawnOffset;

        // ========== 1단계: 첫 번째 프리팹 (10초) ==========
        if (firstPrefab != null)
        {
            Debug.Log("[TableSequenceSpawner] Spawning first prefab");
            currentInstance = Instantiate(firstPrefab); // rotation 파라미터 제거
            currentInstance.transform.position = spawnPosition;
            currentInstance.transform.localScale = spawnScale;

            yield return new WaitForSeconds(firstDuration);

            Destroy(currentInstance);
            Debug.Log("[TableSequenceSpawner] First prefab destroyed");
        }
        else
        {
            Debug.LogWarning("[TableSequenceSpawner] First prefab is not assigned");
        }

        // ========== 2단계: 두 번째 프리팹 (10초) ==========
        if (secondPrefab != null)
        {
            Debug.Log("[TableSequenceSpawner] Spawning second prefab");
            currentInstance = Instantiate(secondPrefab); // rotation 파라미터 제거
            currentInstance.transform.position = spawnPosition;
            currentInstance.transform.localScale = spawnScale;

            yield return new WaitForSeconds(secondDuration);

            Destroy(currentInstance);
            Debug.Log("[TableSequenceSpawner] Second prefab destroyed");
        }
        else
        {
            Debug.LogWarning("[TableSequenceSpawner] Second prefab is not assigned");
        }

        // ========== 3단계: 세 번째 프리팹 (계속 유지) ==========
        if (thirdPrefab != null)
        {
            Debug.Log("[TableSequenceSpawner] Spawning third prefab (permanent)");
            currentInstance = Instantiate(thirdPrefab); // rotation 파라미터 제거
            currentInstance.transform.position = spawnPosition;
            currentInstance.transform.localScale = spawnScale;
        }
        else
        {
            Debug.LogWarning("[TableSequenceSpawner] Third prefab is not assigned");
        }

        Debug.Log("[TableSequenceSpawner] Sequence complete");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (MRUKManager.Instance != null)
        {
            MRUKManager.Instance.OnRoomReady -= OnRoomReady;
        }
    }
}
