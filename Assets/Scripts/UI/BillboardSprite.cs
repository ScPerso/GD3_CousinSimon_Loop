using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
