using UnityEngine;
using System;

public class StraightMover : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("이동할 목표 오브젝트")]
    [SerializeField] private Transform targetObject;
    
    [Header("Movement Settings")]
    [Tooltip("이동 속도")]
    [SerializeField] private float moveSpeed = 2f;
    
    [Tooltip("목표 지점 도착 거리")]
    [SerializeField] private float stopDistance = 0.5f;
    
    [Tooltip("이동 시작 딜레이 (초)")]
    [SerializeField] private float startDelay = 0f;
    
    [Header("Fade Settings")]
    [Tooltip("Fade 지속 시간")]
    [SerializeField] private float fadeDuration = 2f;
    
    [Tooltip("도착 후 자동으로 페이드아웃 및 삭제")]
    [SerializeField] private bool destroyOnArrival = true;

    private bool isMoving = false;
    private float delayTimer = 0f;
    
    public event Action OnArrival;

    private void Start()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("[StraightMover] Target object is not assigned!");
        }
        
        delayTimer = startDelay;
        
        if (startDelay <= 0)
        {
            StartMoving();
        }
    }

    private void Update()
    {
        // 딜레이 처리
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0)
            {
                StartMoving();
            }
            return;
        }

        if (!isMoving || targetObject == null) return;

        // 목표 지점까지의 방향과 거리 계산
        Vector3 direction = targetObject.position - transform.position;
        float distance = direction.magnitude;

        // 도착 체크
        if (distance <= stopDistance)
        {
            OnReachTarget();
            return;
        }

        // 목표를 향해 이동
        direction.Normalize();
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // // 선택사항: 이동 방향을 바라보도록 회전
        // if (direction != Vector3.zero)
        // {
        //     Quaternion targetRotation = Quaternion.LookRotation(direction);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        // }
    }

    public void StartMoving()
    {
        isMoving = true;
        Debug.Log($"[StraightMover] Started moving towards {(targetObject != null ? targetObject.name : "NULL")}");
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private void OnReachTarget()
    {
        isMoving = false;
        Debug.Log("[StraightMover] Reached target!");
        
        OnArrival?.Invoke();

        if (destroyOnArrival)
        {
            // Fade out 후 삭제
            if (FadeUtility.Instance != null)
            {
                FadeUtility.Instance.FadeOutOpaque(gameObject, fadeDuration, 0f);
                Destroy(gameObject, fadeDuration);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    // Target 설정 함수 (외부에서 호출 가능)
    public void SetTarget(Transform target)
    {
        targetObject = target;
        Debug.Log($"[StraightMover] Target set to: {(target != null ? target.name : "NULL")}");
    }

    // Gizmo로 목표 지점 연결선 표시 (에디터에서만 보임)
    private void OnDrawGizmos()
    {
        if (targetObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetObject.position);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetObject.position, stopDistance);
        }
    }
}