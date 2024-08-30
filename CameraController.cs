using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float zoomSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 50f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Pan
        Vector3 pos = transform.position;

        if (Input.GetKey("w"))
            pos.y += panSpeed * Time.deltaTime;
        if (Input.GetKey("s"))
            pos.y -= panSpeed * Time.deltaTime;
        if (Input.GetKey("d"))
            pos.x += panSpeed * Time.deltaTime;
        if (Input.GetKey("a"))
            pos.x -= panSpeed * Time.deltaTime;

        transform.position = pos;

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}