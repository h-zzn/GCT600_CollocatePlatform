using UnityEngine;
using Meta.XR.MRUtilityKit;

public class AnimatedCharacterController : MonoBehaviour
{
    [Header("Character Prefabs")] 
    [SerializeField] private GameObject tigerPrefab;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private GameObject personPrefab;

    [Header("Settings")]
    [SerializeField] private float distanceFromScreen = 2f;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager decalManager;

    private GameObject currentCharacter;      // 현재 생성된 캐릭터 Clone
    private MRUKAnchor currentScreenAnchor;
    private GameObject currentFusedBaekja;

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

        // tiger mover 연결 (Tiger만 점프 이벤트 있음)
        tigerMover = currentCharacter.GetComponent<TigerMover>();
        if (tigerMover != null)
        {
            tigerMover.OnLastJumpFinished += TryStartDecalProjection;
            if (MRUKManager.Instance != null && MRUKManager.Instance.TableAnchor != null)
            {
                tigerMover.StartMoving(MRUKManager.Instance.TableAnchor);
            }
            else
            {
                Debug.LogError("[AnimatedCharacterController] TABLE anchor not found in MRUKManager!");
            }
        }
    }

    // Tiger 호출
    public void AppearTiger(MRUKAnchor anchor)
    {
        SpawnCharacter(tigerPrefab, anchor);
    }

    // Flower 호출
    public void AppearFlower(MRUKAnchor anchor)
    {
        SpawnCharacter(flowerPrefab, anchor);
    }

    // Person 호출
    public void AppearPerson(MRUKAnchor anchor)
    {
        SpawnCharacter(personPrefab, anchor);
    }

    // Tiger에서만 실행됨
    private void TryStartDecalProjection()
    {
        if (decalManager != null && currentFusedBaekja != null)
        {
            decalManager.StartDecal(currentFusedBaekja);
        }
    }

    private void PlaceCharacterBehindScreen(GameObject obj)
{
    if (currentScreenAnchor == null)
    {
        Debug.LogWarning("currentScreenAnchor missing");
        return;
    }

    // Screen의 UP 방향의 반대쪽 = 뒤쪽
    Vector3 behindDirection = -currentScreenAnchor.transform.up;
    
    // Screen 위치에서 뒤쪽으로 distanceFromScreen만큼 이동
    Vector3 pos = currentScreenAnchor.transform.position + (behindDirection * distanceFromScreen);
    
    // Y축은 바닥에 고정
    pos.y = 0f;
    
    obj.transform.position = pos;
    
    // Tiger가 Screen을 바라보도록 회전 (up 방향 기준)
    Vector3 lookDirection = currentScreenAnchor.transform.up;
    lookDirection.y = 0;  // Y축 회전만
    if (lookDirection != Vector3.zero)
    {
        obj.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    Debug.Log($"[PlaceCharacter] Screen pos: {currentScreenAnchor.transform.position}, " +
              $"Screen up: {currentScreenAnchor.transform.up}, " +
              $"Tiger placed at: {pos}");
    }
}
