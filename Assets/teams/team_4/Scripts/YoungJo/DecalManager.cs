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
        // 이미 찾은 적 있으면 그대로 사용
        if (_cachedGrassFader == null)
        {
            // 씬 전체에서 GrassPrefabFader 한 번 검색 (비활성 자식 포함)
            _cachedGrassFader = FindObjectOfType<GrassPrefabFader>(true);

            if (_cachedGrassFader == null)
            {
                Debug.LogWarning("DecalManager: GrassPrefabFader not found in scene.");
                return;
            }
        }

        GameObject grassGroup = _cachedGrassFader.gameObject;

        if (!grassGroup.activeSelf)
        {
            // prefab에서 inactive였던 GrassGroup을 활성화
            // → OnEnable() → FadeSequence() 자동 실행
            grassGroup.SetActive(true);
        }
        else
        {
            // 이미 한 번 켜진 상태에서 다시 효과 재생하고 싶으면,
            // _cachedGrassFader.RestartFade(); 같은 메서드를 추가해서 호출
        }
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
