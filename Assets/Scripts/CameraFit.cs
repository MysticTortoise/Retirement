
using System;
using UnityEngine;

public class CameraFit : MonoBehaviour
{
    [SerializeField] private Vector2 CameraFitBox;
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCameraBounds();
    }

    private void Update()
    {
        UpdateCameraBounds();
    }

    private void UpdateCameraBounds()
    {
        float minOrthoHeight = CameraFitBox.y / 2;
        float minWidth = CameraFitBox.x / 2 / cam.aspect;
        cam.orthographicSize = Mathf.Max(minOrthoHeight, minWidth);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, CameraFitBox);
    }
}
