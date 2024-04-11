using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 10f;
    [SerializeField]
    private float boarderSize = 5f;
    [SerializeField]
    private float groundHeight = 50f;
    [SerializeField]
    private Vector3 orientation = new(60, 45, 0);

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
        transform.rotation = Quaternion.Euler(orientation);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 inputDir = new();
        Vector3 pos = transform.position;
        pos.y = groundHeight;

        // handle input
        if (Input.GetKey(KeyCode.LeftArrow) || Input.mousePosition.x <= boarderSize)
            inputDir.x = -1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.mousePosition.x >= Screen.width - boarderSize)
            inputDir.x = 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.mousePosition.y <= boarderSize)
            inputDir.z = -1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.mousePosition.y >= Screen.height - boarderSize)
            inputDir.z = 1;

        // Handle camera height from ground
        // We are spherecasting AND raycasting. This allows for smooth interpolation
        // as the camera traverses a potentially bumpy ground.
        // Layer 0 should be the environment layer (Default)
        if (Physics.SphereCast(new(transform.position.x, 100f, transform.position.z), 8f, Vector3.down, out RaycastHit hit, 500f, 1 << 0))
        {
            RaycastHit bottomHit = new()
            {
                point = new(transform.position.x, groundHeight, transform.position.z)
            };
            Physics.Raycast(transform.position, Vector3.down, out bottomHit);
            float dot = Mathf.Min(Vector3.Dot(hit.normal, Vector3.up), 1);
            pos.y += Mathf.Lerp(hit.point.y, bottomHit.point.y, Mathf.Acos(dot));
        }

        // Move the camera
        if (inputDir != Vector3.zero)
            pos += Time.deltaTime * cameraSpeed * (Quaternion.Euler(0, 45, 0) * inputDir.normalized);

        transform.position = pos;
    }
}
