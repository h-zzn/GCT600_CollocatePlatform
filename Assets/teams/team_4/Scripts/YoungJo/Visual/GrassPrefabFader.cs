using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// grass group 프리팹의 자식(각 low grass prefab)을
/// 꺼진 상태에서 순서대로 켜지면서 페이드 인시키는 스크립트.
/// 프리팹 루트에 붙여서 사용.
/// </summary>
public class GrassPrefabFader : MonoBehaviour
{
    [Header("Target Group (생략하면 이 오브젝트 기준)")]
    public Transform grassGroup;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f;     // 한 덩어리 풀 페이드인 시간
    public float delayBetween = 0.3f;     // 덩어리들 사이 딜레이

    private readonly List<Renderer[]> _grassRendererGroups = new List<Renderer[]>();
    private readonly List<Transform> _grassRoots = new List<Transform>();

    private void Awake()
    {
        if (grassGroup == null)
            grassGroup = transform;

        _grassRendererGroups.Clear();
        _grassRoots.Clear();

        // grassGroup의 "직접 자식"만 순서대로 사용
        for (int i = 0; i < grassGroup.childCount; i++)
        {
            Transform child = grassGroup.GetChild(i);
            Renderer[] renderers = child.GetComponentsInChildren<Renderer>(includeInactive: true);

            if (renderers.Length > 0)
            {
                _grassRendererGroups.Add(renderers);
                _grassRoots.Add(child);
            }
        }

        // 시작 시: 자식들만 끄고, 알파 0으로 세팅
        for (int i = 0; i < _grassRendererGroups.Count; i++)
        {
            Transform root = _grassRoots[i];
            root.gameObject.SetActive(false);           // 각 low grass 루트 비활성
            SetGroupAlpha(_grassRendererGroups[i], 0f); // 알파 0
        }
    }

    private void OnEnable()
    {
        // 프리팹이 Instantiate될 때마다 자동 재생
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        if (_grassRendererGroups.Count == 0)
            yield break;

        for (int i = 0; i < _grassRendererGroups.Count; i++)
        {
            Renderer[] group = _grassRendererGroups[i];
            Transform root = _grassRoots[i];

            // 1) 이번 그룹 켜기
            root.gameObject.SetActive(true);

            // 2) 알파 0 → 1 페이드
            yield return StartCoroutine(FadeInGroup(group));

            // 3) 다음 그룹까지 딜레이
            if (i < _grassRendererGroups.Count - 1 && delayBetween > 0f)
                yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator FadeInGroup(Renderer[] renderers)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            SetGroupAlpha(renderers, normalized);
            yield return null;
        }

        SetGroupAlpha(renderers, 1f);
    }

    /// <summary>
    /// 하나의 low grass prefab(그 안의 모든 Renderer)에 대해 알파 설정
    /// </summary>
    private void SetGroupAlpha(Renderer[] renderers, float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;

            Material mat = r.material;
            if (mat == null) continue;

            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color"))
            {
                Color c = mat.GetColor("_Color");
                c.a = alpha;
                mat.SetColor("_Color", c);
            }
        }
    }
}
