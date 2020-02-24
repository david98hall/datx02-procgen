using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Whittaker = Whittaker.Whittaker;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WhittakerTest : MonoBehaviour
{
    
    public int length = 1;
    public int width = 1;
    void Start()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        Vector3[] vertices = new Vector3[(length + 1) * (width + 1)];

        for (int z = 0, i = 0; z <= length; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                // todo compute y
                vertices[i] = new Vector3(x, 0, z);
            }
        }
        
        Debug.Log("");
        int[] triangles = new int[6 * length * width];
        for (int z = 0, offset = 0, vertex = 0; z < length; z++, vertex++)
        {
            for (int x = 0; x < width; x++, vertex++)
            {
                triangles[offset++] = vertex;
                triangles[offset++] = vertex + 1;
                triangles[offset++] = vertex + width + 1;
                triangles[offset++] = vertex + width + 2;
                triangles[offset++] = vertex + width + 1;
                triangles[offset++] = vertex + 1;
                Debug.Log(vertex);
            }
        }
        
        Color[] testColors = {Color.red, Color.yellow, Color.blue};
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = testColors[i % testColors.Length];
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        /*
        Vector3[] vertices = new Vector3[6];
        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, 1);
        vertices[2] = new Vector3(1, 0, 0);
        vertices[3] = new Vector3(1, 0, 1);
        vertices[4] = new Vector3(1, 0, 0);
        vertices[5] = new Vector3(0, 0, 1);

        int[] triangles = new[] {0, 1, 2, 3, 4, 5};
        
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.red;
        }
        
        _mesh.Clear();
        _mesh.vertices = vertices;
        //_mesh.triangles = triangles;
        //_mesh.colors = colors;
        */
        //old();
    }

    /*
    private void OnDrawGizmos()
    {
        if (_mesh.vertices == null) return;

        foreach (Vector3 vertex in _mesh.vertices)
        {
            Gizmos.DrawSphere(vertex, .1f);
        }
    }
    */

    private void old()
    {
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.AddRange(new []
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
        });
        
        triangles.AddRange(new [] {0, 1, 2, 3, 4, 5});

        Color[] colors = new Color[vertices.Count];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new global::Whittaker.Whittaker().GetColor(vertices[i]);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors;
    }
}
