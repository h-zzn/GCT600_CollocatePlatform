using UnityEngine;
using Meta.XR.MRUtilityKit;
using System;

public class TigerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint;
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

    // AnimatedCharacterController에서 호출하는 함수
    public void StartMoving(MRUKAnchor anchor)
    {
        if (anchor == null)
        {
            Debug.LogError("[TigerMover] StartMoving: anchor is NULL");
            return;
        }

        // 이동 목표지점 = TABLE 앵커 transform
        targetPoint = anchor.transform;

        Debug.Log($"[TigerMover] StartMoving(): TargetPoint set to → {anchor.name}");

        // 이동 시작 → Walk 애니메이션 켜기
        //animator.SetBool("isWalking", true);

        isLastJumping = false;
    }

    private void Update()
    {
        if (targetPoint == null) return;

        // Jump 상태 감지
        if (IsInJumpingState())
        {
            if (isLastJumping)
            {
                OnLastJumpFinished?.Invoke();
                FadeUtility.Instance?.FadeOutOpaque(gameObject, fadeDuration, 0f);
                Destroy(gameObject, fadeDuration);
            }
            return;
        }

        // Walk 중일 때만 이동
        if (IsInWalkState())
            MoveToTarget();
    }

    private bool IsInWalkState()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("Walk");
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
            // 도착 → 마지막 Jump 시작 조건
            animator.SetBool("isWalking", false);
            isLastJumping = true;
        }
    }
}
