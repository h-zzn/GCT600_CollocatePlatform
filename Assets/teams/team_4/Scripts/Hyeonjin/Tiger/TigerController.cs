using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TigerController : MonoBehaviour
{        
    [Header("Tiger Settings")]
    [SerializeField] private GameObject tigerObject;
    [SerializeField] private float distanceFromScreen = 2f;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager decalManager;


    private Animator animator;
    private GameObject currentFusedBaekja; // fusedBaekja 참조 저장


    private void Awake()
    {
        if (tigerObject == null)
            tigerObject = this.gameObject;

        animator = GetComponent<Animator>();

    }

    private void Start()
    {
        var mover = tigerObject.GetComponent<TigerMover>();
        decalManager = FindFirstObjectByType<DecalManager>();
        mover.OnLastJumpFinished += TryStartDecalProjection;

        // 초기에는 비활성 상태
        tigerObject.SetActive(false);
        
        // BaekjaHandler의 OnBaekjaCreated 이벤트 구독
        if (BaekjaHandler.Instance != null)
        {
            BaekjaHandler.Instance.OnBaekjaCreated += OnFusedBaekjaCreated;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (BaekjaHandler.Instance != null)
        {
            BaekjaHandler.Instance.OnBaekjaCreated -= OnFusedBaekjaCreated;
        }
    }

    // fusedBaekja가 생성되면 참조 저장
    private void OnFusedBaekjaCreated(GameObject fusedBaekja)
    {
        currentFusedBaekja = fusedBaekja;
        Debug.Log($"[TigerController] FusedBaekja 참조 저장: {fusedBaekja.name}");
    }

    public void AppearTiger()
    {
        PlaceTigerBehindScreen();
        tigerObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(tigerObject, fadeDuration, 1f);
        
        //데칼 적용
        //TryStartDecalProjection();
    }

    private void TryStartDecalProjection()
    {
        Debug.Log($"[TigerController] 데칼 시작 대상: {currentFusedBaekja.name}");
        decalManager.StartDecal(currentFusedBaekja);
    }

    private void PlaceTigerBehindScreen()
    {
        Debug.Log("[TigerController] Placing Tiger behind SCREEN anchor.");

        var screenAnchor = MRUKManager.Instance.ScreenAnchor;

        if (screenAnchor == null)
        {
            Debug.LogWarning("[TigerController] SCREEN anchor missing!");
            return;
        }

        Vector3 pos = screenAnchor.transform.position;
        pos.x -= distanceFromScreen;
        pos.y = 0f;

        // tigerObject에 위치/회전 적용
        tigerObject.transform.position = pos;

        // // 타이거가 스크린을 바라보게 회전
        // Quaternion rot = Quaternion.LookRotation(-screenAnchor.transform.forward);
        // tigerObject.transform.rotation = rot;

        Debug.Log("[TigerController] Tiger placed exactly at SCREEN anchor position.");
    }
}