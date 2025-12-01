using UnityEngine;
using Meta.XR.MRUtilityKit;

public class AnimatedCharacterController : MonoBehaviour
{
    [Header("Character Prefabs")] 
    [SerializeField] private GameObject tigerPrefab;
    [SerializeField] private GameObject flowerPrefab;
    [Header("Person Prefabs")]
    [SerializeField] private GameObject[] personPrefabs;   // ★ 기존 personPrefab 삭제


    [Header("Settings")]
    [SerializeField] private float distanceFromScreen = 2f;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager decalManager;

    private GameObject currentCharacter;      // 현재 생성된 캐릭터 Clone
    private MRUKAnchor currentScreenAnchor;
    private GameObject currentFusedBaekja;
    private DecalType currentDecalType;

    private TigerMover tigerMover;

    private void Start()
    {
        if (BaekjaHandler.Instance != null)
            BaekjaHandler.Instance.OnBaekjaCreated += OnFusedBaekjaCreated;
    }

    private void OnDestroy()
    {
        if (BaekjaHandler.Instance != null)
            BaekjaHandler.Instance.OnBaekjaCreated -= OnFusedBaekjaCreated;
    }

    private void OnFusedBaekjaCreated(GameObject fusedBaekja)
    {
        currentFusedBaekja = fusedBaekja;
    }

    // 캐릭터 스폰 공통 함수
    private void SpawnCharacter(GameObject prefab, MRUKAnchor anchor)
    {
        // 기존 캐릭터가 존재했다면 제거
        if (currentCharacter != null)
            Destroy(currentCharacter);

        currentScreenAnchor = anchor;

        // Clone 생성
        currentCharacter = Instantiate(prefab);

        // 배치
        PlaceCharacterBehindScreen(currentCharacter);

        // 등장 연출
        FadeUtility.Instance?.FadeIn(currentCharacter, fadeDuration, 1f);

        // TABLE 앵커 가져오기
        MRUKAnchor tableAnchor = null;
        if (MRUKManager.Instance != null && MRUKManager.Instance.TableAnchor != null)
        {
            tableAnchor = MRUKManager.Instance.TableAnchor;
        }
        else
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found in MRUKManager!");
            return;
        }

        // TigerMover 체크
        var tigerMover = currentCharacter.GetComponent<TigerMover>();
        if (tigerMover != null)
        {
            tigerMover.OnLastJumpFinished += TryStartDecalProjection;
            tigerMover.StartMoving(tableAnchor);
            Debug.Log("[AnimatedCharacterController] TigerMover started");
            return;
        }

        // ChunhyangMover 체크
        var chunhyangMover = currentCharacter.GetComponent<ChunhyangMover>();
        if (chunhyangMover != null)
        {
            // Chunhyang은 decal projection 없음
            chunhyangMover.OnLastActionFinished += TryStartDecalProjection;
            // Screen 앵커도 함께 전달
            chunhyangMover.StartMoving(tableAnchor, currentScreenAnchor);
            Debug.Log("[AnimatedCharacterController] ChunhyangMover started");
            return;
        }

        // butterflyMover 체크
        var butterflyMover = currentCharacter.GetComponent<ButterflyMover>();
        if (butterflyMover != null)
        {
            butterflyMover.OnLastActionFinished += TryStartDecalProjection;
            butterflyMover.StartMoving(tableAnchor, currentScreenAnchor);
            Debug.Log("[AnimatedCharacterController] ButterflyMover started");
            return;
        }
    }

    private void SpawnPersonGroup(MRUKAnchor anchor)
    {
        if (personPrefabs == null || personPrefabs.Length != 3)
        {
            Debug.LogError("[AnimatedCharacterController] personPrefabs array must contain exactly 3 prefabs!");
            return;
        }

        currentScreenAnchor = anchor;

        // TABLE 앵커 가져오기
        MRUKAnchor tableAnchor = MRUKManager.Instance.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor missing!");
            return;
        }

        // 3명 좌/중/우 offset
        Vector3[] offsets = new Vector3[]
        {
            new Vector3(-0.5f, 0, 0),
            Vector3.zero,
            new Vector3(0.5f, 0, 0)
        };

        for (int i = 0; i < 3; i++)
        {
            // 각각 서로 다른 prefab을 instantiate
            GameObject person = Instantiate(personPrefabs[i]);

            // 위치 배치 (offset 적용)
            PlaceCharacterBehindScreenWithOffset(person, offsets[i]);

            // Fade-in
            FadeUtility.Instance?.FadeIn(person, fadeDuration, 1f);

            // mover 실행
            var mover = person.GetComponent<ChunhyangMover>();
            if (mover != null)
            {
                mover.OnLastActionFinished += TryStartDecalProjection;
                mover.StartMoving(tableAnchor, currentScreenAnchor);
            }

            Debug.Log($"[AnimatedCharacterController] Spawned person #{i+1} ({personPrefabs[i].name})");
        }
    }


    private void PlaceCharacterBehindScreenWithOffset(GameObject obj, Vector3 extraOffset)
    {
        if (currentScreenAnchor == null)
            return;

        // 동일한 로직 적용
        MRUKAnchor tableAnchor = MRUKManager.Instance?.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        Vector3 toTable = tableAnchor.transform.position - currentScreenAnchor.transform.position;
        float dot = Vector3.Dot(currentScreenAnchor.transform.up.normalized, toTable.normalized);
        
        Vector3 behindDirection;
        if (dot > 0)
        {
            behindDirection = -currentScreenAnchor.transform.up;
        }
        else
        {
            behindDirection = currentScreenAnchor.transform.up;
        }

        Vector3 pos = currentScreenAnchor.transform.position + (behindDirection * distanceFromScreen);

        // offset 적용 (Screen의 로컬 좌표계 기준)
        pos += currentScreenAnchor.transform.right * extraOffset.x;  // 좌우
        pos += behindDirection * extraOffset.z;  // 앞뒤 (배치 방향 기준)
        
        pos.y = 0f;

        obj.transform.position = pos;

        // 캐릭터가 Screen을 바라보도록
        Vector3 lookDir = -behindDirection;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            obj.transform.rotation = Quaternion.LookRotation(lookDir);
    }


    // Tiger 호출
    public void AppearTiger(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Tiger;
        SpawnCharacter(tigerPrefab, anchor);
    }

    // Flower 호출
    public void AppearFlower(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Flower;
        SpawnCharacter(flowerPrefab, anchor);
    }

    // Person 호출
    public void AppearPerson(MRUKAnchor anchor)
    {
        currentDecalType = DecalType.Person;
        SpawnPersonGroup(anchor);
    }

    // Tiger에서만 실행됨
    private void TryStartDecalProjection()
    {
        if (decalManager != null && currentFusedBaekja != null)
        {
            decalManager.StartDecal(currentFusedBaekja, currentDecalType);
        }
    }

    private void PlaceCharacterBehindScreen(GameObject obj)
    {
        if (currentScreenAnchor == null)
        {
            Debug.LogWarning("currentScreenAnchor missing");
            return;
        }

        // 테이블 위치 가져오기
        MRUKAnchor tableAnchor = MRUKManager.Instance?.TableAnchor;
        if (tableAnchor == null)
        {
            Debug.LogError("[AnimatedCharacterController] TABLE anchor not found!");
            return;
        }

        // Screen에서 Table로의 방향 벡터
        Vector3 toTable = tableAnchor.transform.position - currentScreenAnchor.transform.position;
        
        // Screen의 up 벡터와 Table 방향의 내적 계산
        float dot = Vector3.Dot(currentScreenAnchor.transform.up.normalized, toTable.normalized);
        
        // up이 Table을 향하고 있으면 반대쪽(-up)에, 아니면 같은쪽(+up)에 배치
        Vector3 behindDirection;
        if (dot > 0)
        {
            // Screen의 up이 Table을 향함 → 반대쪽에 배치
            behindDirection = -currentScreenAnchor.transform.up;
            Debug.Log($"[AnimatedCharacterController] Screen faces table → placing on opposite side (dot={dot:F2})");
        }
        else
        {
            // Screen의 up이 Table 반대편을 향함 → Table 쪽에 배치
            behindDirection = currentScreenAnchor.transform.up;
            Debug.Log($"[AnimatedCharacterController] Screen faces away from table → placing on table side (dot={dot:F2})");
        }
        
        // Screen 위치에서 배치 방향으로 distanceFromScreen만큼 이동
        Vector3 pos = currentScreenAnchor.transform.position + (behindDirection * distanceFromScreen);
        
        // Y축은 바닥에 고정
        pos.y = 0f;
        
        obj.transform.position = pos;
        
        // 캐릭터가 Screen을 바라보도록 회전
        Vector3 lookDirection = -behindDirection; // 배치 방향의 반대 = Screen 방향
        lookDirection.y = 0;
        
        if (lookDirection != Vector3.zero)
        {
            obj.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        Debug.Log($"[AnimatedCharacterController] Screen pos: {currentScreenAnchor.transform.position}, " +
                $"Placement direction: {behindDirection}, " +
                $"Character placed at: {pos}");
    }
}
