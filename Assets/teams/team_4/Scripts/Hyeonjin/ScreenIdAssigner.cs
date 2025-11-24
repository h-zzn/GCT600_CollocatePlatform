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
        if (screens.Length == 0)
        {
            Debug.LogWarning("[ScreenIDAssigner] No SCREEN prefabs found.");
            return;
        }

        // 위치 기반 정렬
        var ordered = screens
            .OrderBy(s => s.transform.position.x)
            .ThenBy(s => s.transform.position.z)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            string contentName;

            if (i < screenNames.Length)
                contentName = screenNames[i];     // 입력한 이름 사용
            else
                contentName = $"Extra_{i + 1}";   // 초과할 경우 예비 이름

            string finalID = $"SCREEN_{contentName}";

            ordered[i].screenID = finalID;

            Debug.Log($"[ScreenIDAssigner] {ordered[i].name} → {ordered[i].screenID}");
        }
    }
}
