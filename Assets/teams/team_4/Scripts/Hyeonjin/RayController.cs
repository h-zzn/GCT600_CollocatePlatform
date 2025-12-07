using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RayController : MonoBehaviour
{
    public float maxRayDistance = 10f;
    [SerializeField]private LineRenderer lineRenderer;
    private Material lineMaterial;

    public GameObject HitObject { get; private set; }

    void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;

        lineMaterial = new Material(Shader.Find("Unlit/Color"));
        lineMaterial.color = Color.red;
        lineRenderer.material = lineMaterial;
    }

    void Update()
    {
        // 실시간으로 레이를 발사하여 HitObject를 업데이트
        Ray ray = GetRay();
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance))
        {
            HitObject = hit.collider.gameObject;
            ShowLine(ray.origin, hit.point);
            //Debug.Log("[RayController] hit object: " + HitObject.name);
        }
        else
        {
            HitObject = null;
            ShowLine(ray.origin, ray.origin + ray.direction * maxRayDistance);
        }

        // 라인 렌더러는 항상 활성화된 상태로 둠 (Update 루프가 돌 때)
        lineRenderer.enabled = true;
    }

    private Ray GetRay()
    {
        if (OVRManager.instance != null && OVRManager.instance.enabled)
        {
            return new Ray(transform.position, transform.forward * 2.1f);
        }
        else
        {
            return Camera.main.ScreenPointToRay(Input.mousePosition);
        }
    }

    void ShowLine(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void SetActive(bool isActive)
    {
        this.enabled = isActive;
        lineRenderer.enabled = isActive;
    }
}
