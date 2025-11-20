using UnityEngine;
using Meta.XR.MRUtilityKit;
using System;

public class TigerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public Transform targetPoint;
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.3f;
    public float fadeDuration = 2f;    

    private Animator animator;
    public event Action OnLastJumpFinished;

    private bool isLastJumping = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        isLastJumping = false;
        
        if (targetPoint == null)
        {
            // Debug.LogError("[TigerMover] Target Point is not assigned.");
        }
        
        if (MRUKManager.Instance != null)
        {
            if (MRUKManager.Instance.IsReady)
                AssignTableAnchor(MRUKManager.Instance.CurrentRoom);
            else
                MRUKManager.Instance.OnRoomReady += AssignTableAnchor;
        }
    }

    private void Update()
    {
        if (targetPoint == null) return;

        if (IsInJumpingState())
        {
            Debug.Log("[TigerMover] Jumping 상태 감지.");

            if (isLastJumping)
            {
                OnLastJumpFinished?.Invoke();
                Debug.Log("[TigerMover] 마지막 점프 완료, 사라짐 처리 시작.");
                FadeUtility.Instance?.FadeOut(gameObject, fadeDuration, 0f);

                Destroy(gameObject, fadeDuration);
            }
            return;
        }

        if (!IsInWalkState()) return;

        MoveToTarget();
    }

    private void AssignTableAnchor(MRUKRoom room)
    {
        if (room == null)
        {
            Debug.LogWarning("[TigerMover] No MRUKRoom provided.");
            return;
        }

        foreach (var anchor in room.Anchors)
        {
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE)
            {
                targetPoint = anchor.transform;
                Debug.Log($"[TigerMover] TABLE Anchor set as target → {anchor.name}");
                return;
            }
        }

        Debug.LogWarning("[TigerMover] TABLE Anchor not found.");
    }

    private bool IsInWalkState()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return info.IsName("Walk");  
    }

    private bool IsInJumpingState()
    {
        AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);

        if (clips.Length == 0) return false;

        return clips[0].clip.name.Contains("Jumping");
    }

    private void MoveToTarget()
    {
        Vector3 dir = targetPoint.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            // 회전
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);

            // 이동
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            // 도착 → Walk 끄기
            animator.SetBool("isWalking", false);
            isLastJumping = true;
        }
    }
}
