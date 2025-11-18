using UnityEngine;
using Meta.XR.MRUtilityKit;

public class TigerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint;
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.3f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (targetPoint == null)
        {
            Debug.LogError("[TigerMover] Target Point is not assigned.");
        }
        
        if (MRUKManager.Instance != null)
        {
            if (MRUKManager.Instance.IsReady)
                AssignTableAnchor(MRUKManager.Instance.CurrentRoom);
            else
                MRUKManager.Instance.OnRoomReady += AssignTableAnchor;
        }
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

    private void Update()
    {
        if (targetPoint == null) return;

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector3 dir = targetPoint.position - transform.position;
        dir.y = 0;
        float dist = dir.magnitude;

        // 이동 중 애니메이션 트리거
        animator.SetBool("isWalking", dist > stopDistance);

        if (dist > stopDistance)
        {
            // 바라보기
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);

            // 실제 이동
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
