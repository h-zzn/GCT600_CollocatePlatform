using UnityEngine;
using System;
using System.Collections;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;

public class BaekjaHandler : MonoBehaviour
{
    public static BaekjaHandler Instance;
    public event Action<GameObject> OnBaekjaCreated;

    // [Header("Initial Baekja Settings")]
    // [SerializeField] private GameObject baekjaAPrefab;
    // [SerializeField] private GameObject baekjaBPrefab;
    // [SerializeField] private float spacing = 0.25f;    // 두 백자 간 간격

    [Header("Fused Baekja Settings")]
    [SerializeField] private GameObject fusedBaekjaPrefab;
    [SerializeField] private GameObject mergeEffectPrefab;
    
    //[SerializeField] private float spawnYThreshold = 1.5f;
    [SerializeField] private float fadeDuration = 2f;    // 페이드 인/아웃 지속 시간
    [SerializeField] private GameObject tableObject;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        StartCoroutine(WaitForTableAnchor());
    }

    private IEnumerator WaitForTableAnchor()
    {
        // MRUKManager 준비될 때까지 대기
        while (MRUKManager.Instance == null || !MRUKManager.Instance.IsReady)
            yield return null;

        // TableAnchor 준비될 때까지 대기
        while (MRUKManager.Instance.TableAnchor == null)
            yield return null;

        tableObject = MRUKManager.Instance.TableAnchor.gameObject;
        Debug.Log($"[BaekjaHandler] Assigned TABLE anchor: {tableObject.name}");
    }

    private void AssignTableAnchor(MRUKRoom room)
    {
        if (room == null)
        {
            Debug.LogWarning("[BaekjaHandler] No MRUKRoom provided.");
            return;
        }

        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE)
            {
                tableObject = anchor.gameObject;
                Debug.Log($"[BaekjaHandler] Found TABLE anchor → {tableObject.name}");
                break;
            }
        }

        if (tableObject == null)
        {
            Debug.LogWarning("[BaekjaHandler] TABLE anchor not found in current MRUK room.");
        }

        //StartCoroutine(WaitAndSpawnInitialBaekjas());
    }

    // IEnumerator WaitAndSpawnInitialBaekjas()
    // {
    //     // 최소 2프레임 기다리기
    //     yield return null;
    //     yield return null;

    //     // Renderer나 Collider 로드될 때까지 최대 1초(60프레임) 대기
    //     Renderer rend = null;
    //     Collider col = null;
    //     int frameCount = 0;
    //     while (rend == null && col == null && frameCount < 60)
    //     {
    //         rend = spawnPoint.GetComponentInChildren<Renderer>(true);
    //         col = spawnPoint.GetComponentInChildren<Collider>(true);
    //         frameCount++;
    //         yield return null;
    //     }

    //     if (rend == null && col == null)
    //     {
    //         Debug.LogWarning("[BaekjaHandler] TABLE prefab still has no Renderer or Collider after waiting.");
    //         yield break;
    //     }

    //     Debug.Log($"[BaekjaHandler] TABLE mesh ready after {frameCount} frames.");
    //     // SpawnInitialBaekjas();
    // }

    
    // private void SpawnInitialBaekjas()
    // {
    //     if (spawnPoint == null)
    //     {
    //         Debug.LogWarning("[BaekjaHandler] TABLE anchor not found.");
    //         return;
    //     }

    //     Renderer rend = spawnPoint.GetComponentInChildren<Renderer>();
    //     if (rend == null)
    //     {
    //         Debug.LogWarning("[BaekjaHandler] TABLE has no Renderer.");
    //         return;
    //     }

    //     // 테이블 표면과 중심 계산
    //     Bounds bounds = rend.bounds;
    //     float topY = bounds.center.y + bounds.extents.y;
    //     Vector3 center = bounds.center;
    //     Vector3 right = spawnPoint.transform.right.normalized;

    //     // 중앙 기준 좌우로 배치
    //     Vector3 posA = center - right * (spacing * 0.5f);
    //     Vector3 posB = center + right * (spacing * 0.5f);
    //     posA.y = posB.y = topY + spawnYThreshold;

    //     // 인스펙터에서 지정된 프리팹으로 스폰
    //     if (baekjaAPrefab == null || baekjaBPrefab == null)
    //     {
    //         Debug.LogError("[BaekjaHandler] Please assign both Baekja prefabs in the Inspector!");
    //         return;
    //     }

    //     GameObject baekjaA = Instantiate(baekjaAPrefab, posA, Quaternion.identity);
    //     GameObject baekjaB = Instantiate(baekjaBPrefab, posB, Quaternion.identity);

    //     Debug.Log($"[BaekjaHandler] Spawned Baekja A at {posA}, Baekja B at {posB}");
    // }

    public void SpawnFusedBaekja(GameObject baekja1, GameObject baekja2, GameObject perfectBaekja)
    {
        if (tableObject == null)
        {
            Debug.LogWarning("[BaekjaHandler] tablePosition not assigned (TABLE anchor missing).");
            return;
        }

        // 중복 호출 방지
        if (baekja1 == null || baekja2 == null) return;


        // Renderer를 기준으로 높이 계산
        Renderer rend = tableObject.GetComponentInChildren<Renderer>();      // child에서 렌더러 찾기
        if (rend == null)
        {
            Debug.LogWarning("[BaekjaHandler] No renderer in Table's child.");
            return;
        }

        float topY = rend.bounds.center.y + rend.bounds.extents.y;
        Vector3 spawnPos = new Vector3(
            tableObject.transform.position.x,
            topY,
            tableObject.transform.position.z
        );


        // 정해진 위치(테이블 위)에 가상 백자 초기화
        GameObject fusedBaekja = Instantiate(
            fusedBaekjaPrefab,
            spawnPos,
            fusedBaekjaPrefab.transform.rotation
        );

        if (mergeEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                mergeEffectPrefab,
                spawnPos,
                Quaternion.identity
            );
            // 영빈 추가
            if (Camera.main != null)
            {
                // 카메라의 위치를 가져오되, 높이(y)는 이펙트의 높이로 고정
                Vector3 targetPostition = new Vector3(
                    Camera.main.transform.position.x, 
                    effect.transform.position.y, 
                    Camera.main.transform.position.z
                );
                
                effect.transform.LookAt(targetPostition);
                
                // 만약 이펙트가 거꾸로(등지고) 나온다면 아래 주석을 해제하세요.
                effect.transform.Rotate(0, 90, 0);
            }
            // 영빈 추가 끝

            // 파티클이면 수명 이후 자동 삭제 (선택)
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                // 일반 프리팹이면 적당한 시간 뒤 제거
                Destroy(effect, 3f);
            }
        }
        else
        {
            Debug.LogWarning("[BaekjaHandler] mergeEffectPrefab is not assigned.");
        }

        if (SoundManager.Instance == null)
        {
            Debug.LogError("[BaekjaHandler] SoundManager.Instance is NULL. Cannot play BaekjaMerge SFX.");
        }
        else
        {
            Debug.Log("[BaekjaHandler] Trying to play SFX3D: BaekjaMerge");

            SoundManager.Instance.PlaySFX(SoundID.BaekjaAppear);
        }
        // 서서히 나타나게 하기
        FadeUtility.Instance.FadeIn(fusedBaekja, fadeDuration, 0f);

        OnBaekjaCreated?.Invoke(fusedBaekja);       // 가상 백자가 생성되었다는 것을 알림
        
        if (perfectBaekja != null)
        {
            perfectBaekja.transform.position = spawnPos;
            perfectBaekja.transform.rotation = fusedBaekjaPrefab.transform.rotation;
            //perfectBaekja.SetActive(true);
            Debug.Log($"[BaekjaHandler] PerfectBaekja 활성화 및 위치 설정: {spawnPos}");
        }
        else
        {
            Debug.LogWarning("[BaekjaHandler] PerfectBaekja가 할당되지 않았습니다.");
        }

        // 기존 두 백자는 서서히 사라지고 파괴
        FadeUtility.Instance.FadeOut(baekja1, fadeDuration);
        FadeUtility.Instance.FadeOut(baekja2, fadeDuration);
        Destroy(baekja1, fadeDuration); // 페이드 다 끝난 후 제거
        Destroy(baekja2, fadeDuration); ;
    }
}
