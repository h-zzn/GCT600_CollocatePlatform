using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TigerController : MonoBehaviour
{        
    [Header("Tiger Settings")]
    [SerializeField] private GameObject tigerObject;
    [SerializeField] private float fadeDuration = 2f;

    [SerializeField] private DecalManager DecalManager;

    [SerializeField] private BaekjaManager baekjaManager;

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
        tigerObject.SetActive(true);
        FadeUtility.Instance?.FadeIn(tigerObject, fadeDuration, 0f);
    }
}