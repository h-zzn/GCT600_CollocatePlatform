using UnityEngine;

public class BowlController : MonoBehaviour
{
    [Header("Bowl Asset")]
    [Tooltip("실제 Bowl 모델 (Bowl01)")]
    public GameObject bowlModelPrefab;
    
    
    [Header("Bowl Dimensions (in meters)")]
    [Tooltip("실제 그릇 윗면 반지름")]
    public float actualBowlRadius = 0.085f;
    
    [Tooltip("실제 그릇 깊이")]
    public float actualBowlDepth = 0.08f;
    
    [Tooltip("그릇 테두리 높이")]
    public float bowlWallHeight = 0.03f;
    
    [Header("Grabbable Settings")]
    public bool makeGrabbable = true;
    public bool useGravity = false;

    private GameObject bowlInstance;

    void Start()
    {
        CreateBowl();
        SetupMarbleClipping();
    }

    void CreateBowl()
    {
        if (bowlModelPrefab != null)
        {
            bowlInstance = Instantiate(bowlModelPrefab, transform);
            bowlInstance.name = "Bowl01";
            //bowlInstance.transform.localPosition = Vector3.zero;
            bowlInstance.transform.localPosition = new Vector3(0, 0, 0);  // 필요시 조정
            bowlInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("Bowl model prefab (Bowl01) not assigned!");
            return;
        }

        if (makeGrabbable)
        {
            SetupGrabbable();
        }
    }

    void SetupGrabbable()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = true;
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Collider existingCollider = GetComponentInChildren<Collider>();
        
        if (existingCollider == null)
        {
            MeshFilter[] meshFilters = bowlInstance.GetComponentsInChildren<MeshFilter>();
            
            if (meshFilters.Length > 0)
            {
                MeshCollider collider = meshFilters[0].gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                Debug.Log("Added MeshCollider to Bowl01");
            }
            else
            {
                SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = actualBowlRadius;
                sphereCollider.center = new Vector3(0, -actualBowlDepth * 0.5f, 0);
                Debug.Log("Added SphereCollider to Bowl");
            }
        }
        
        Debug.Log("Bowl Grabbable setup complete");
    }

    void SetupMarbleClipping()
    {
        MarbleViewClipping clipping = GetComponentInChildren<MarbleViewClipping>();
        
        if (clipping == null)
        {
            Debug.LogWarning("MarbleViewClipping not found in Bowl children!");
            return;
        }

        Transform marbleTransform = transform.Find("Marble");
        if (marbleTransform == null)
        {
            Debug.LogWarning("Marble not found in Bowl!");
            return;
        }

        clipping.bowl = this.transform;
        clipping.marble = marbleTransform.gameObject;
        clipping.SetBowlDimensions(actualBowlRadius, actualBowlDepth, bowlWallHeight);

        Debug.Log($"MarbleViewClipping configured: radius={actualBowlRadius}m, depth={actualBowlDepth}m");
    }

    void OnValidate()
    {
        if (Application.isPlaying && bowlInstance != null)
        {
            
            MarbleViewClipping clipping = GetComponentInChildren<MarbleViewClipping>();
            if (clipping != null)
            {
                clipping.SetBowlDimensions(actualBowlRadius, actualBowlDepth, bowlWallHeight);
            }
        }
    }
}