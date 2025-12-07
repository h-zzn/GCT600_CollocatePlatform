using UnityEngine;

public class DecalManager : MonoBehaviour
{
    [Header("Decal Textures")]
    [SerializeField] private Texture2D tigerDecal;
    [SerializeField] private Texture2D flowerDecal;
    [SerializeField] private Texture2D personDecal;

    // 런타임에 자동으로 찾을 테이블용 Grass 페이더
    private GrassPrefabFader _cachedGrassFader;

    // 기본값: Tiger
    public void StartDecal(GameObject baekja)
    {
        StartDecal(baekja, DecalType.Tiger);
    }

    public void StartDecal(GameObject baekja, DecalType type)
    {
        if (baekja == null)
        {
            Debug.LogWarning("[DecalManager] StartDecal: baekja == null");
            return;
        }

        // 1) 백자 안에서 ProjectionController 찾기
        ProjectionController proj =
            baekja.GetComponentInChildren<ProjectionController>(true);

        if (proj == null)
        {
            Debug.LogWarning("[DecalManager] ProjectionController not found on baekja.");
            return;
        }

        // 2) 텍스처 세팅 + 프로젝션
        Texture2D decalTex = GetTextureByType(type);
        if (decalTex == null)
        {
            Debug.LogWarning($"[DecalManager] No decal texture assigned for type: {type}");
        }

        proj.autoRun = false;

        if (decalTex != null)
            proj.SetDecalTexture(decalTex);

        proj.ProjectOnce();
        proj.PlayFadeIn();
        ActivateGrassEffect();

        Debug.Log($"[DecalManager] StartDecal 완료 (type: {type}, tex: {(decalTex != null ? decalTex.name : "null")})");
    }

    /// <summary>
    /// 씬 안에서 테이블에 붙어 있는 GrassAutoFaderOnDecal을 1회 탐색 후 캐싱.
    /// (테이블이 하나라고 가정)
    /// </summary>
    private void ActivateGrassEffect()
    {
        // 0) 테이블 앵커 확인
        var tableAnchor = MRUKManager.Instance != null ? MRUKManager.Instance.TableAnchor : null;
        if (tableAnchor == null)
        {
            Debug.LogWarning("[DecalManager] ActivateGrassEffect: TableAnchor is null.");
            return;
        }

        // 1) 캐시가 없으면, 테이블 밑에서만 GrassPrefabFader 검색
        if (_cachedGrassFader == null)
        {
            _cachedGrassFader = tableAnchor.GetComponentInChildren<GrassPrefabFader>(true);

            if (_cachedGrassFader == null)
            {
                Debug.LogWarning("[DecalManager] GrassPrefabFader not found under TableAnchor.");
                return;
            }
        }

        var grassGroup = _cachedGrassFader.gameObject;

        // 2) 항상 OnEnable을 다시 태우고 싶다면, 한 번 껐다 켜는 것이 가장 확실
        if (grassGroup.activeSelf)
        {
            grassGroup.SetActive(false);
        }

        // SetActive(true) → GrassPrefabFader.OnEnable() → FadeSequence() 실행
        grassGroup.SetActive(true);

        Debug.Log("[DecalManager] Grass grassGroup activated & fade started.");
    }
    private Texture2D GetTextureByType(DecalType type)
    {
        switch (type)
        {
            case DecalType.Tiger:  return tigerDecal;
            case DecalType.Flower: return flowerDecal;
            case DecalType.Person: return personDecal;
            default:               return null;
        }
    }
}
