using System.Collections;
using UnityEngine;
using MicahW.PointGrass;   // PointGrassRenderer 네임스페이스

public class GrassSequentialFader : MonoBehaviour
{
    [Header("Target Grass Renderers (순서대로 페이드인)")]
    public PointGrassRenderer[] grassRenderers;

    [Header("Fade Parameters")]
    public float fadeDuration = 2f;      // 한 패치가 0→1까지 올라가는 시간
    public float delayBetween = 0.3f;    // 패치들 사이 간격


    private void OnEnable()
    {
        StartFadeSequence();
    }

    /// <summary>
    /// DecalManager 등 외부에서 호출하는 진입 함수
    /// </summary>
    public void StartFadeSequence()
    {
        // 시작할 때 모두 0으로 초기화
        if (grassRenderers != null)
        {
            foreach (var r in grassRenderers)
            {
                if (r == null) continue;
                r.SetPointLODFactor(0f);    // 내부에서 Clamp01
            }
        }

        StopAllCoroutines();
        StartCoroutine(FadeSequence());
    }

    private IEnumerator FadeSequence()
    {
        if (grassRenderers == null) yield break;

        for (int i = 0; i < grassRenderers.Length; i++)
        {
            var r = grassRenderers[i];
            if (r == null) continue;

            yield return StartCoroutine(FadeInRenderer(r));

            // 다음 패치로 넘어가기 전에 잠깐 쉬어 줌
            if (i < grassRenderers.Length - 1 && delayBetween > 0f)
                yield return new WaitForSeconds(delayBetween);
        }
    }

    private IEnumerator FadeInRenderer(PointGrassRenderer renderer)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / fadeDuration);
            renderer.SetPointLODFactor(normalized);   // 0 → 1 로 점점 증가
            yield return null;
        }

        renderer.SetPointLODFactor(1f);   // 마지막에 확실하게 1로
    }
}
