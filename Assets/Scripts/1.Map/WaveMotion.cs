using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaveMotion : MonoBehaviour
{
    [Header("Wave Settings")]
    public float waveHeight = 0.3f;
    public float waveSpeed = 1f;
    public float waveFrequency = 1f;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] displacedVertices;
    private Vector3 objectScale;

    void Start()
    {
        // Mesh'in kopyasını al, değiştirilebilir hale getir
        mesh = GetComponent<MeshFilter>().mesh = Instantiate(GetComponent<MeshFilter>().mesh);
        baseVertices = mesh.vertices;
        displacedVertices = new Vector3[baseVertices.Length];

        objectScale = transform.localScale;
    }

    void Update()
    {
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 vertex = baseVertices[i];

            // Objeye göre normalize dalga
            float scaledX = vertex.x / objectScale.x;
            float scaledZ = vertex.z / objectScale.z;

            // Yüksekliği sinüsle veriyoruz
            vertex.y += Mathf.Sin(Time.time * waveSpeed + (scaledX + scaledZ) * waveFrequency) * waveHeight;

            displacedVertices[i] = vertex;
        }

        mesh.vertices = displacedVertices;
        mesh.RecalculateNormals();
    }
}
