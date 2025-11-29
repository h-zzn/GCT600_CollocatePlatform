using UnityEngine;

public class BowlDummy : MonoBehaviour
{
    [Header("Bowl Asset")]
    [Tooltip("실제 Bowl 모델 (Bowl01 등)")]
    public GameObject bowlModelPrefab;
    
    [Header("Bowl Dimensions (in meters)")]
    [Tooltip("윗면 반지름")]
    public float radius = 0.085f;
    
    [Tooltip("그릇 깊이")]
    public float depth = 0.08f;
    
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
            // Bowl01 에셋 인스턴스화
            bowlInstance = Instantiate(bowlModelPrefab, transform);
            bowlInstance.name = "BowlModel";
            bowlInstance.transform.localPosition = Vector3.zero;
            bowlInstance.transform.localRotation = Quaternion.identity;
            
            Debug.Log("Bowl created from prefab: " + bowlModelPrefab.name);
        }
        else
        {
            Debug.LogWarning("Bowl model prefab not assigned! Creating dummy mesh instead.");
            CreateDummyBowl();
            return;
        }

        // Grabbable 설정
        if (makeGrabbable)
        {
            SetupGrabbable();
        }
    }

    void SetupGrabbable()
    {
        // Bowl 에셋에 이미 Collider가 있는지 확인
        Collider existingCollider = bowlInstance.GetComponentInChildren<Collider>();
        
        if (existingCollider == null)
        {
            // Collider가 없으면 추가
            MeshFilter meshFilter = bowlInstance.GetComponentInChildren<MeshFilter>();
            if (meshFilter != null)
            {
                MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                collider.convex = true;
                Debug.Log("Added MeshCollider to Bowl");
            }
            else
            {
                // MeshFilter가 없으면 간단한 Collider 추가
                SphereCollider sphereCollider = bowlInstance.AddComponent<SphereCollider>();
                sphereCollider.radius = radius;
                sphereCollider.center = new Vector3(0, -depth * 0.5f, 0);
                Debug.Log("Added SphereCollider to Bowl");
            }
        }

        // Rigidbody 추가 (Grab을 위해 필요)
        Rigidbody rb = bowlInstance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bowlInstance.AddComponent<Rigidbody>();
        }
        
        rb.isKinematic = true;
        rb.useGravity = useGravity;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        Debug.Log("Bowl Grabbable setup complete");
    }

    void CreateDummyBowl()
    {
        // 에셋이 없을 때 기존 dummy mesh 사용
        bowlInstance = new GameObject("BowlMesh");
        bowlInstance.transform.parent = transform;
        bowlInstance.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = bowlInstance.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = bowlInstance.AddComponent<MeshRenderer>();

        meshFilter.mesh = GenerateDummyBowlMesh();

        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        meshRenderer.material = mat;

        if (makeGrabbable)
        {
            MeshCollider collider = bowlInstance.AddComponent<MeshCollider>();
            collider.convex = true;
            
            Rigidbody rb = bowlInstance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = useGravity;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    void SetupMarbleClipping()
    {
        MarbleViewClipping2 clipping = GetComponentInChildren<MarbleViewClipping2>();
        
        if (clipping == null)
        {
            Debug.LogWarning("MarbleViewClipping2 not found in Bowl children!");
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
        clipping.bowlRadius = this.radius;
        clipping.bowlDepth = this.depth;
        clipping.bowlWallHeight = 0.03f;

        Debug.Log($"MarbleViewClipping2 configured: radius={radius}m, depth={depth}m");
    }

    Mesh GenerateDummyBowlMesh()
    {
        Mesh mesh = new Mesh();
        int segments = 32;
        float bottomRadiusRatio = 0.47f;

        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        float bottomRadius = radius * bottomRadiusRatio;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2;
            float cosAngle = Mathf.Cos(angle);
            float sinAngle = Mathf.Sin(angle);

            vertices[i] = new Vector3(cosAngle * radius, 0, sinAngle * radius);
            vertices[i + segments + 1] = new Vector3(cosAngle * bottomRadius, -depth, sinAngle * bottomRadius);
        }

        for (int i = 0; i < segments; i++)
        {
            int ti = i * 6;
            triangles[ti] = i;
            triangles[ti + 1] = i + segments + 1;
            triangles[ti + 2] = i + 1;
            triangles[ti + 3] = i + 1;
            triangles[ti + 4] = i + segments + 1;
            triangles[ti + 5] = i + segments + 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}