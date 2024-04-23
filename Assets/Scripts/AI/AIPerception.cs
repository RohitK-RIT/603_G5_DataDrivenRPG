using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class AIPerception : MonoBehaviour
    {
        [SerializeField] private float fovAngle = 30f;
        [SerializeField] private float sightDistance = 10f;
        [SerializeField] private float height = 1f;
        [SerializeField] private Color meshColor = Color.red;
        [SerializeField] private int scanFrequency = 30;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask occlusionLayer;
        [SerializeField] private Vector3 eyeOffset = new(0, 1, 0);

        public bool HasTarget => VisibleUnits?.Count > 0;
        public List<Unit> VisibleUnits { get; private set; }

        private Vector3 EyePosition => transform.position + eyeOffset;
        public float SightDistance => sightDistance;

        private Mesh _mesh;
        private int _count;
        private float _scanInterval;
        private float _scanTimer;
        private Collider[] _colliders = new Collider[10];

        // Start is called before the first frame update
        private void Start()
        {
            _scanInterval = 1f / scanFrequency;
            VisibleUnits = new List<Unit>();
        }

        // Update is called once per frame
        private void Update()
        {
            _scanTimer -= Time.deltaTime;
            if (_scanTimer < 0)
            {
                _scanTimer += _scanInterval;
                Scan();
            }
        }

        private void Scan()
        {
            _count = Physics.OverlapSphereNonAlloc(transform.position, sightDistance, _colliders, targetLayer, QueryTriggerInteraction.Collide);

            VisibleUnits.Clear();
            for (int i = 0; i < _count; i++)
            {
                if (IsInSight(_colliders[i].gameObject))
                {
                    var unit = _colliders[i].GetComponent<Unit>();
                    if (unit)
                        VisibleUnits.Add(unit);
                }
            }
        }

        private bool IsInSight(GameObject obj)
        {
            var origin = transform.position;
            var destination = obj.transform.position;
            var direction = destination - origin;
            // if (direction.y < 0 || direction.y > height)
            //     return false;

            direction.y = transform.position.y;
            var deltaAngle = Vector3.Angle(direction, transform.forward);
            if (deltaAngle > fovAngle / 2f)
                return false;

            origin.y += height / 2;
            destination.y += origin.y;
            if (Physics.Linecast(origin, destination, occlusionLayer))
                return false;

            return true;
        }

        private Mesh CreateWedgeMesh()
        {
            var mesh = new Mesh();

            const int segments = 10;
            const int numTriangles = segments * 4 + 4;
            const int numVertices = numTriangles * 3;

            var vertices = new Vector3[numVertices];
            var triangles = new int[numVertices];

            var bottomCenter = Vector3.zero;
            var bottomLeft = Quaternion.Euler(0, -fovAngle / 2f, 0) * Vector3.forward * sightDistance;
            var bottomRight = Quaternion.Euler(0, fovAngle / 2f, 0) * Vector3.forward * sightDistance;

            var topCenter = bottomCenter + Vector3.up * height;
            var topRight = bottomRight + Vector3.up * height;
            var topLeft = bottomLeft + Vector3.up * height;

            var vertexIndex = 0;

            // Left Side
            vertices[vertexIndex++] = bottomCenter;
            vertices[vertexIndex++] = bottomLeft;
            vertices[vertexIndex++] = topLeft;

            vertices[vertexIndex++] = topLeft;
            vertices[vertexIndex++] = topCenter;
            vertices[vertexIndex++] = bottomCenter;

            // Right Side
            vertices[vertexIndex++] = bottomCenter;
            vertices[vertexIndex++] = topCenter;
            vertices[vertexIndex++] = topRight;

            vertices[vertexIndex++] = topRight;
            vertices[vertexIndex++] = bottomRight;
            vertices[vertexIndex++] = bottomCenter;

            var angle = -fovAngle / 2f;
            var deltaAngle = fovAngle / segments;
            for (int i = 0; i < segments; i++)
            {
                bottomLeft = Quaternion.Euler(0, angle, 0) * Vector3.forward * sightDistance;
                bottomRight = Quaternion.Euler(0, angle + deltaAngle, 0) * Vector3.forward * sightDistance;

                topRight = bottomRight + Vector3.up * height;
                topLeft = bottomLeft + Vector3.up * height;

                // Far Side
                vertices[vertexIndex++] = bottomLeft;
                vertices[vertexIndex++] = bottomRight;
                vertices[vertexIndex++] = topRight;

                vertices[vertexIndex++] = topRight;
                vertices[vertexIndex++] = topLeft;
                vertices[vertexIndex++] = bottomLeft;

                // Top Side
                vertices[vertexIndex++] = topCenter;
                vertices[vertexIndex++] = topLeft;
                vertices[vertexIndex++] = topRight;

                // Bottom Side
                vertices[vertexIndex++] = bottomCenter;
                vertices[vertexIndex++] = bottomRight;
                vertices[vertexIndex++] = bottomLeft;

                angle += deltaAngle;
            }

            for (int i = 0; i < numVertices; i++)
            {
                triangles[i] = i;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        private void OnValidate()
        {
            _mesh = CreateWedgeMesh();
            _scanInterval = 1f / scanFrequency;
        }

        private void OnDrawGizmos()
        {
            if (_mesh)
            {
                Gizmos.color = HasTarget ? Color.green : meshColor;
                Gizmos.DrawMesh(_mesh, transform.position, transform.rotation);
            }
        }
    }
}