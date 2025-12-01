using UnityEngine;
using Meta.XR.MRUtilityKit;
using System;

public class ButterflyMover : MonoBehaviour
{
    [Header("Target Anchors")]
    private Transform tableAnchor;
    private MRUKAnchor screenAnchor;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float distanceFromScreen = 3f;
    [SerializeField] private float hoveringDuration = 3f;
    
    [Header("Circling Settings")]
    [SerializeField] private float circleRadius = 1.5f;
    [SerializeField] private float circleSpeed = 90f;
    [SerializeField] private float circleHeight = 1.5f;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;

    private Transform mainCamera;
    private Vector3 targetPosition;
    private float stateTimer = 0f;
    private float circleAngle = 0f;
    private float startAngle = 0f;  // 회전 시작 각도
    private Vector3 circleCenter;
    
    public event Action OnLastActionFinished;

    private enum State
    {
        MovingToScreenFront,
        Hovering,
        MovingToCamera,
        CirclingCamera,
        MovingToTable,
        CirclingTable,
        Complete
    }

    private State currentState = State.MovingToScreenFront;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    public void StartMoving(MRUKAnchor tableAnchor, MRUKAnchor screenAnchor)
    {
        if (tableAnchor == null)
        {
            Debug.LogError("[ButterflyMover] StartMoving: tableAnchor is NULL");
            return;
        }

        if (screenAnchor == null)
        {
            Debug.LogError("[ButterflyMover] StartMoving: screenAnchor is NULL");
            return;
        }

        this.tableAnchor = tableAnchor.transform;
        this.screenAnchor = screenAnchor;

        targetPosition = screenAnchor.transform.position + 
                        (screenAnchor.transform.up * distanceFromScreen);
        targetPosition.y = circleHeight;

        Debug.Log($"[ButterflyMover] Started! Target position: {targetPosition}");
        currentState = State.MovingToScreenFront;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.MovingToScreenFront:
                MoveToScreenFront();
                break;

            case State.Hovering:
                Hover();
                break;

            case State.MovingToCamera:
                MoveToCamera();
                break;

            case State.CirclingCamera:
                CircleAround(mainCamera.position);
                break;

            case State.MovingToTable:
                MoveToTable();
                break;

            case State.CirclingTable:
                CircleAround(tableAnchor.position);
                break;
        }
    }

    private void MoveToScreenFront()
    {
        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;

        if (distance > 0.3f)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                // ★ Y축만 회전 (기울기 없이)
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
        else
        {
            transform.position = targetPosition;
            stateTimer = 0f;
            currentState = State.Hovering;
            Debug.Log("[ButterflyMover] Reached screen front → Hovering");
        }
    }

    private void Hover()
    {
        stateTimer += Time.deltaTime;

        float bob = Mathf.Sin(Time.time * 2f) * 0.1f;
        Vector3 hoverPos = targetPosition;
        hoverPos.y += bob;
        transform.position = hoverPos;

        if (stateTimer >= hoveringDuration)
        {
            currentState = State.MovingToCamera;
            Debug.Log("[ButterflyMover] Hovering done → Moving to camera");
        }
    }

    private void MoveToCamera()
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("[ButterflyMover] Main camera is null!");
            currentState = State.Complete;
            return;
        }

        Vector3 cameraTarget = mainCamera.position;
        cameraTarget.y = circleHeight;

        Vector3 direction = cameraTarget - transform.position;
        float distance = direction.magnitude;

        if (distance > circleRadius)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            // ★ Y축만 회전 (기울기 없이)
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            // 현재 위치 기준으로 시작 각도 계산
            circleCenter = mainCamera.position;
            circleCenter.y = circleHeight;
            
            Vector3 offset = transform.position - circleCenter;
            offset.y = 0;
            circleAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            startAngle = circleAngle;
            
            currentState = State.CirclingCamera;
            Debug.Log($"[ButterflyMover] Circling camera (start: {circleAngle:F1}°)");
        }
    }

    private void CircleAround(Vector3 center)
    {
        // 회전 각도 증가
        circleAngle += circleSpeed * Time.deltaTime;

        // 원 궤도 위치 계산
        float radians = circleAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * circleRadius,
            0,
            Mathf.Sin(radians) * circleRadius
        );

        Vector3 circlePosition = center;
        circlePosition.y = circleHeight;
        circlePosition += offset;

        transform.position = circlePosition;

        // 회전 중심을 바라보도록 (Y축만)
        Vector3 lookDirection = center - transform.position;
        lookDirection.y = 0; // Y축 제거
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 시작 각도로부터 360도 돌았는지 체크
        float angleDiff = circleAngle - startAngle;
        if (angleDiff >= 360f)
        {
            if (currentState == State.CirclingCamera)
            {
                currentState = State.MovingToTable;
                Debug.Log("[ButterflyMover] Camera circle complete → Moving to table");
            }
            else if (currentState == State.CirclingTable)
            {
                currentState = State.Complete;
                Debug.Log("[ButterflyMover] Table circle complete!");
                OnLastActionFinished?.Invoke();
                
                FadeUtility.Instance?.FadeOutOpaque(gameObject, fadeDuration, 0f);
                Destroy(gameObject, fadeDuration);
            }
        }
    }

    private void MoveToTable()
    {
        if (tableAnchor == null)
        {
            Debug.LogWarning("[ButterflyMover] Table anchor is null!");
            currentState = State.Complete;
            return;
        }

        Vector3 tableTarget = tableAnchor.position;
        tableTarget.y = circleHeight;

        Vector3 direction = tableTarget - transform.position;
        float distance = direction.magnitude;

        if (distance > circleRadius)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            // ★ Y축만 회전 (기울기 없이)
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
            // 현재 위치 기준으로 시작 각도 계산
            circleCenter = tableAnchor.position;
            circleCenter.y = circleHeight;
            
            Vector3 offset = transform.position - circleCenter;
            offset.y = 0;
            circleAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            startAngle = circleAngle;
            
            currentState = State.CirclingTable;
            Debug.Log($"[ButterflyMover] Circling table (start: {circleAngle:F1}°)");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentState == State.CirclingCamera || currentState == State.CirclingTable)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(circleCenter, circleRadius);
        }
    }
}