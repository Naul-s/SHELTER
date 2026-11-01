using UnityEngine;

namespace Shelter.Environment
{
    /// <summary>
    /// 가짜 볼류메트릭 광선 메시(Shelter/LightShaft 셰이더용). 로컬 +Z 방향으로 뻗는 뿔대를 만든다 —
    /// 원형(전등 빛 원뿔) 또는 사각형(창문으로 들어오는 달빛 기둥). 시작 단면(z=0)에서 끝 단면(z=length)으로 넓어진다.
    /// 메시는 실행·편집 시 코드로 만들고 저장하지 않는다(씬에는 치수만 직렬화). 그림자·조명과 무관한 순수 연출.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LightShaft : MonoBehaviour
    {
        private const int CircleSegments = 24;
        private const int LengthSegments = 6;

        public enum Shape { Circle, Rectangle }

        [SerializeField] private Shape _shape = Shape.Circle;
        [Tooltip("시작 단면 크기(m). 원형은 x 가 반지름, 사각형은 가로·세로.")]
        [SerializeField] private Vector2 _startSize = new Vector2(0.15f, 0.15f);
        [Tooltip("끝 단면 크기(m).")]
        [SerializeField] private Vector2 _endSize = new Vector2(1.5f, 1.5f);
        [SerializeField, Min(0.1f)] private float _length = 3f;

        private Mesh _mesh;

        public void Configure(Shape shape, Vector2 startSize, Vector2 endSize, float length)
        {
            _shape = shape;
            _startSize = startSize;
            _endSize = endSize;
            _length = length;
            Rebuild();
        }

        private void OnEnable()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled) Rebuild();
        }

        private void OnDestroy()
        {
            if (_mesh == null) return;
            if (Application.isPlaying) Destroy(_mesh);
            else DestroyImmediate(_mesh);
        }

        private void Rebuild()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "LightShaft", hideFlags = HideFlags.DontSave };
                GetComponent<MeshFilter>().sharedMesh = _mesh;
            }

            var renderer = GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            if (_shape == Shape.Circle) BuildCircle();
            else BuildRectangle();
        }

        /// <summary>원형 단면 뿔대. 법선은 옆면 기울기를 반영한 바깥 방향.</summary>
        private void BuildCircle()
        {
            int ring = CircleSegments + 1;
            var vertices = new Vector3[ring * (LengthSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            float slope = (_endSize.x - _startSize.x) / _length;

            for (int l = 0; l <= LengthSegments; l++)
            {
                float t = (float)l / LengthSegments;
                float radius = Mathf.Lerp(_startSize.x, _endSize.x, t);
                for (int s = 0; s < ring; s++)
                {
                    float a = s * Mathf.PI * 2f / CircleSegments;
                    var dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                    int v = l * ring + s;
                    vertices[v] = dir * radius + Vector3.forward * (t * _length);
                    normals[v] = (dir - Vector3.forward * slope).normalized;
                    uvs[v] = new Vector2((float)s / CircleSegments, t);
                }
            }

            Apply(vertices, normals, uvs, ring);
        }

        private void BuildRectangle()
        {
            // 네 면을 따로(모서리에서 법선이 갈라지게) — 면당 2개 정점 x 4 = 둘레 8
            var corners = new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) };
            const int ring = 8;
            var vertices = new Vector3[ring * (LengthSegments + 1)];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];

            for (int l = 0; l <= LengthSegments; l++)
            {
                float t = (float)l / LengthSegments;
                Vector2 half = Vector2.Lerp(_startSize, _endSize, t) * 0.5f;
                for (int side = 0; side < 4; side++)
                {
                    Vector2 c0 = corners[side];
                    Vector2 c1 = corners[(side + 1) % 4];
                    Vector2 mid = (c0 + c1) * 0.5f;
                    var outward = new Vector3(mid.x, mid.y, 0f);
                    for (int k = 0; k < 2; k++)
                    {
                        Vector2 c = k == 0 ? c0 : c1;
                        int v = l * ring + side * 2 + k;
                        vertices[v] = new Vector3(c.x * half.x, c.y * half.y, t * _length);
                        normals[v] = outward;
                        uvs[v] = new Vector2((side + k) / 4f, t);
                    }
                }
            }

            Apply(vertices, normals, uvs, ring, pairs: true);
        }

        private void Apply(Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int ring, bool pairs = false)
        {
            int quadsPerRing = pairs ? ring / 2 : ring - 1;
            var triangles = new int[LengthSegments * quadsPerRing * 6];
            int t = 0;
            for (int l = 0; l < LengthSegments; l++)
            {
                for (int q = 0; q < quadsPerRing; q++)
                {
                    int a = l * ring + (pairs ? q * 2 : q);
                    int b = a + 1;
                    int c = a + ring;
                    int d = b + ring;
                    triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
                    triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
                }
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
        }
    }
}
