using UnityEngine;
using Meta.XR.MRUtilityKit;
using System;

public class MongryongMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform targetPoint;
    public float firstWalkDistance = 3.0f;
    public float moveSpeed = 1.5f;
    public float stopDistance = 0.3f;
    public float fadeDuration = 2f;
    public float rotationSpeed = 5f;
    public float finalWalkDuration = 3f;

    private Animator animator;
    public event Action OnLastActionFinished;

    private Vector3 firstTargetPosition;
    private MRUKAnchor screenAnchor;
    
    private Transform mainCamera;
    private Quaternion savedRotation;

    private bool lookingCompleted = false;
    private float finalWalkStartTime = 0f;
    private bool rotationCallbackExecuted = false;

    private enum State
    {
        StandingIdle,
        FirstWalking,
        StandingStill1,
        RotatingToCamera1,
        FirstLooking,
        RotatingToTable,   
        SecondWalking,
        StandingStill2,
        RotatingToCamera2,
        FinalLooking,
        RotatingBack2,      
        FinalWalking
    }

    private State currentState = State.StandingIdle;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        mainCamera = Camera.main.transform;
    }

    public void StartMoving(MRUKAnchor tableAnchor, MRUKAnchor screenAnchor)
    {
        if (tableAnchor == null)
        {
            Debug.LogError("[MongryongMover] StartMoving: tableAnchor is NULL");
            return;
        }

        if (screenAnchor == null)
        {
            Debug.LogError("[MongryongMover] StartMoving: screenAnchor is NULL");
            return;
        }

        targetPoint = tableAnchor.transform;
        this.screenAnchor = screenAnchor;

        // 기존 screen center 기준에서 앞쪽으로 firstWalkDistance 만큼 떨어진 지점 계산
        firstTargetPosition = transform.position + 
                             (screenAnchor.transform.up * firstWalkDistance);

        firstTargetPosition.y = 0f;

        Debug.Log($"[MongryongMover] First target: {firstTargetPosition}");
        Debug.Log($"[MongryongMover] Final target (Table): {tableAnchor.name}");

        currentState = State.StandingIdle;
    }

    private void Update()
    {
        if (targetPoint == null) return;

        switch (currentState)
        {
            case State.StandingIdle:
                CheckStandingIdleToWalking();
                break;

            case State.FirstWalking:
                MoveToFirstTarget();
                break;
            
            case State.StandingStill1:
                CheckStandingStill("Standing Idle 0", () => {
                    rotationCallbackExecuted = false;
                    currentState = State.RotatingToCamera1;
                    Debug.Log("[MongryongMover] StandingStill1 (Standing Idle 0) → RotatingToCamera1");
                });
                break;

            case State.RotatingToCamera1:
                RotateToCamera(() => {
                    currentState = State.FirstLooking;
                    lookingCompleted = false;
                    animator.SetTrigger("look");
                });
                break;

            case State.FirstLooking:
                CheckFirstLookingEnd();
                break;

            // Table 방향으로 회전
            case State.RotatingToTable:
                RotateToTable(() => {
                    animator.SetBool("isWalking", true);
                    currentState = State.SecondWalking;
                    Debug.Log("[MongryongMover] Rotation to table complete → SecondWalking");
                });
                break;

            case State.SecondWalking:
                MoveToFinalTarget();
                break;
            
            case State.StandingStill2:
                CheckStandingStill("Standing Idle 1", () => {
                    rotationCallbackExecuted = false;
                    currentState = State.RotatingToCamera2;
                    Debug.Log("[MongryongMover] StandingStill2 (Standing Idle 1) → RotatingToCamera2");
                });
                break;

            case State.RotatingToCamera2:
                RotateToCamera(() => {
                    currentState = State.FinalLooking;
                    lookingCompleted = false;
                    animator.SetTrigger("look");
                });
                break;

            case State.FinalLooking:
                CheckFinalLookingEnd();
                break;

            // 원래 방향으로 회전
            case State.RotatingBack2:
                RotateBack(() => {
                    animator.SetBool("isWalking", true);
                    finalWalkStartTime = Time.time;
                    currentState = State.FinalWalking;
                    Debug.Log("[MongryongMover] Rotation back complete → FinalWalking (Fading out)");
                });
                break;

            case State.FinalWalking:
                ContinueFinalWalking();
                break;
        }
    }

    private void CheckStandingIdleToWalking()
    {
        if (IsInState("Standing Idle"))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 0.95f)
            {
                animator.SetBool("isWalking", true);
                currentState = State.FirstWalking;
                Debug.Log("[MongryongMover] State: StandingIdle → FirstWalking");
            }
        }
    }

    private void CheckStandingStill(string idleStateName, System.Action onComplete)
    {
        if (IsInState(idleStateName))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 0.3f && !animator.IsInTransition(0))
            {
                onComplete?.Invoke();
            }
        }
    }

    private void MoveToFirstTarget()
    {
        if (!IsInState("Walking"))
            return;

        Vector3 dir = firstTargetPosition - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
            savedRotation = transform.rotation;  // 현재 방향 저장
            rotationCallbackExecuted = false;
            currentState = State.StandingStill1;
            Debug.Log("[MongryongMover] Reached first target → StandingStill1");
        }
    }

    private void CheckFirstLookingEnd()
    {
        if (IsInState("Looking"))
        {
            if (!lookingCompleted)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0))
                {
                    lookingCompleted = true;
                    rotationCallbackExecuted = false;
                    // Table 방향으로 회전하도록 변경
                    currentState = State.RotatingToTable;
                    Debug.Log("[MongryongMover] FirstLooking end → RotatingToTable");
                }
            }
        }
    }

    private void MoveToFinalTarget()
    {
        if (!IsInState("Walking 0"))
            return;

        Vector3 dir = targetPoint.position - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;
        }
        else
        {
            animator.SetBool("isWalking", false);
            savedRotation = transform.rotation;  // Table 도착 시 방향 저장
            rotationCallbackExecuted = false;
            currentState = State.StandingStill2;
            Debug.Log("[MongryongMover] Reached Table → StandingStill2");
        }
    }

    private void CheckFinalLookingEnd()
    {
        if (IsInState("Looking 0"))
        {
            if (!lookingCompleted)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 0.95f && !animator.IsInTransition(0))
                {
                    lookingCompleted = true;
                    rotationCallbackExecuted = false;
                    // 원래 방향으로 회전하도록 변경
                    currentState = State.RotatingBack2;
                    Debug.Log("[MongryongMover] FinalLooking end → RotatingBack2");
                }
            }
        }
    }

    private void ContinueFinalWalking()
    {
        if (!IsInState("Walking 1"))
            return;

        // 저장된 방향(savedRotation)으로 걷기
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (Time.time - finalWalkStartTime >= finalWalkDuration)
        {
            Debug.Log("[MongryongMover] Final walking complete!");
            OnLastActionFinished?.Invoke();
            FadeUtility.Instance?.FadeOutOpaque(gameObject, fadeDuration, 0f);
            Destroy(gameObject, fadeDuration);
        }
    }

    // Table 방향으로 회전하는 새 함수
    private void RotateToTable(System.Action onComplete)
    {
        Vector3 directionToTable = targetPoint.position - transform.position;
        directionToTable.y = 0;

        if (directionToTable == Vector3.zero)
        {
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToTable);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = targetRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[MongryongMover] Rotation to table complete");
        }
    }

    private void RotateToCamera(System.Action onComplete)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[MongryongMover] Main Camera not found!");
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Vector3 directionToCamera = mainCamera.position - transform.position;
        directionToCamera.y = 0;

        if (directionToCamera == Vector3.zero)
        {
            if (!rotationCallbackExecuted)
            {
                rotationCallbackExecuted = true;
                onComplete?.Invoke();
            }
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = targetRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[MongryongMover] Rotation to camera complete");
        }
    }

    private void RotateBack(System.Action onComplete)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, savedRotation, Time.deltaTime * rotationSpeed);

        if (Quaternion.Angle(transform.rotation, savedRotation) < 1f && !rotationCallbackExecuted)
        {
            transform.rotation = savedRotation;
            rotationCallbackExecuted = true;
            onComplete?.Invoke();
            Debug.Log("[MongryongMover] Rotation back complete");
        }
    }

    private bool IsInState(string stateName)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
}