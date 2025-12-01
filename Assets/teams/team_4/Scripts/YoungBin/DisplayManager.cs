using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    [Header("Canvas References")]
    public CanvasDisplayManager canvas1; // Canvas1 (호작도 - Tiger)
    public CanvasDisplayManager canvas2; // Canvas2 (평안감사도 - Person)
    public CanvasDisplayManager canvas3; // Canvas3 (초충도 - Flower)

    private void Update()
    {
        // 데모용 키 입력
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeImage("Tiger");
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeImage("Person");
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeImage("Flower");
    }

    public void ChangeImage(string message)
    {
        Debug.Log($"[DisplayManager] ChangeImage received: {message}");
        
        switch (message)
        {
            case "Tiger": // 호작도
                if (canvas1 != null) canvas1.Toggle();
                break;

            case "Person": // 평안감사도
                if (canvas2 != null) canvas2.Toggle();
                break;

            case "Flower": // 초충도
                if (canvas3 != null) canvas3.Toggle();
                break;

            default:
                Debug.LogWarning($"[DisplayManager] Unknown message: {message}");
                break;
        }
    }
}