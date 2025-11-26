using UnityEngine;
using System.Linq;
using System.Collections;
using Meta.XR.MRUtilityKit;

public class ScreenIDAssigner : MonoBehaviour
{
    [SerializeField]
    private string[] screenNames = { "Tiger", "Flower", "Person" };

    private void Start()
    {
        StartCoroutine(WaitForMRUKManager());
    }

    private IEnumerator WaitForMRUKManager()
    {
        while (MRUKManager.Instance == null)
            yield return null;

        MRUKManager.Instance.OnRoomReady += HandleRoomReady;

        if (MRUKManager.Instance.IsReady)
            HandleRoomReady(MRUKManager.Instance.CurrentRoom);
    }

    private void OnDisable()
    {
        if (MRUKManager.Instance != null)
            MRUKManager.Instance.OnRoomReady -= HandleRoomReady;
    }

    private void HandleRoomReady(MRUKRoom room)
    {
        AssignIDs();
    }

    private void AssignIDs()
    {
        var screens = FindObjectsOfType<ScreenIdentifier>();

        // 활성화된 오브젝트만 필터링
        var activeScreens = screens
            .Where(s => s.gameObject.activeSelf)
            .ToList();

        // 위치 기반으로 중복 제거 (같은 위치 = 중복)
        var uniqueScreens = activeScreens
            .GroupBy(s => new Vector3(
                Mathf.Round(s.transform.position.x * 100f) / 100f,
                Mathf.Round(s.transform.position.y * 100f) / 100f,
                Mathf.Round(s.transform.position.z * 100f) / 100f
            ))
            .Select(g => g.First())
            .OrderBy(s => s.transform.position.x)
            .ThenBy(s => s.transform.position.z)
            .ToList();

        Debug.Log($"[ScreenIDAssigner] Unique screens after deduplication: {uniqueScreens.Count}");

        for (int i = 0; i < uniqueScreens.Count; i++)
        {
            string contentName = i < screenNames.Length 
                ? screenNames[i] 
                : $"Extra_{i + 1}";

            string finalID = $"SCREEN_{contentName}";
            uniqueScreens[i].screenID = finalID;
            //uniqueScreens[i].gameObject.name = $"SCREEN_OBJ_{contentName}";
            
            Debug.Log($"[ScreenIDAssigner] {uniqueScreens[i].name} → {finalID}");
        }
    }
}
