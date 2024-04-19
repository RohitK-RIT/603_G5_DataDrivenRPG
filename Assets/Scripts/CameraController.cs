using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;
    [SerializeField]
    private float borderSize = 5f;
    [SerializeField]
    private float groundHeight = 50f;
    [SerializeField]
    private float maxZoom = 0.8f;
    private float targetZoom;
    [SerializeField]
    private float zoomSpeed = 30f;
    [SerializeField]
    private float zoomEase = 2f;
    [SerializeField]
    private float cameraPitch = 55f;

    
    Vector3 initPt, zoomPt;
    Transform camTM;
    bool rotating = false;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    // Start is called before the first frame update
    void Start()
    {
        targetZoom = 0f;
        camTM = transform.GetChild(0).transform;

        initPt = camTM.localPosition;
        zoomPt = new(0, -groundHeight, 0);

        camTM.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        Vector3 pos = transform.position;
        pos.y = groundHeight;
        transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDir = new();
        Vector3 pos = transform.position;

        // handle input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || Input.mousePosition.x <= borderSize)
            inputDir.x = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - borderSize)
            inputDir.x = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || Input.mousePosition.y <= borderSize)
            inputDir.z = -1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - borderSize)
            inputDir.z = 1;

        if (!rotating)
        {
            if (Input.GetKey(KeyCode.Q))
                StartCoroutine(RotateCamera(90f, 3f));
            else if (Input.GetKey(KeyCode.E))
                StartCoroutine(RotateCamera(-90f, 3f));
        }

        // Move the camera
        if (inputDir != Vector3.zero)
            pos += Time.deltaTime * cameraSpeed * (Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * inputDir.normalized);

        transform.position = pos;

        // Camera zooming
        targetZoom = Mathf.Clamp(targetZoom + 0.1f * Input.mouseScrollDelta.y, 0, maxZoom);
        Vector3 dir = Vector3.Lerp(initPt, zoomPt, targetZoom) - camTM.localPosition;
        camTM.localPosition += Time.deltaTime * dir.normalized * Mathf.Lerp(0f, zoomSpeed, Mathf.Min(zoomEase, dir.magnitude) / zoomEase);
    }

    IEnumerator RotateCamera(float newYaw, float speed)
    {
        rotating = true;

        float start = transform.rotation.eulerAngles.y;
        float end = start + newYaw;
        Vector3 newRot = transform.rotation.eulerAngles;
        for (float t = 0f; t <= 1f; t += Time.deltaTime * speed)
        {
            // ease in out
            newRot.y = Mathf.Lerp(start, end, -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
            transform.rotation = Quaternion.Euler(newRot);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        newRot.y = end;
        transform.rotation = Quaternion.Euler(newRot);

        rotating = false;
    }
}
