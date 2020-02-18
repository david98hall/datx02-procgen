using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Whittaker : MonoBehaviour
{
    private void mesh()
    {
        Mesh mesh  = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0)
        };

        int[] triangles = new int[]
        {
            0, 1, 2
        };
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh();
    }
}
