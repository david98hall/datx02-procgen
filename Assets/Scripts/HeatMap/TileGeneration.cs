using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///Assigned TerrainTypes. 
/// Has a name, height and color
///</summary>
[System.Serializable]
public class TerrainType 
{
    public string name;
    public float height;
    public Color color;
} 
///<summary>
/// Generationg a noise map for the tile and then assigning a texture to the noisemap
///</summary>
public class TileGeneration : MonoBehaviour
{
   
    [SerializeField]
    private TerrainType[] heatTerrainTypes;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    private float heightMultiplier;

    [SerializeField]
    private AnimationCurve heightCurve;

    //Generating the heightmap 
    [SerializeField]
    NoiseMapGenerator noiseMapGenerator; 

    // used to show the heightmap
    [SerializeField]
    private MeshRenderer tileRenderer;

    //access the mesh vertices
    [SerializeField]
    private MeshFilter meshFilter;

    //handeles collisions with the tile
    [SerializeField]
    private MeshCollider meshCollider;

    // the scale og the heightmap
    [SerializeField]
    private float mapScale;

    private float centerVertexZ, maxDistanceZ;



    void Start()
    {
        GenerateTile(centerVertexZ, maxDistanceZ);
    }

    ///<summary>
    /// Calculates the depth and width og the heightmap. 
    /// Then it calls GenerateNoiseMap() with depth, width and mapscale
    ///</summary>
    void GenerateTile(float centerVertexZ, float maxDistanceZ)
    {
        // calculate tile depth and width based on the mesh vertices
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int tileDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int tileWidth = tileDepth;

        //calculate the offset based on the tile position
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;
         // calculate the offsets based on the tile position
        float [,] heightMap = this.noiseMapGenerator.GenerateNoiseMap(tileDepth, tileWidth, this.mapScale, offsetX, offsetZ, waves);

        Vector3 tileDimensions = this.meshFilter.mesh.bounds.size;
        float distanceBetweenVertices = tileDimensions.z / (float)tileDepth;
        float vertexOffestZ = this.gameObject.transform.position.z / distanceBetweenVertices;

        float[,] heatMap = this.noiseMapGenerator.GenerateUniformNoiseMap(tileDepth, tileWidth, centerVertexZ, maxDistanceZ, vertexOffestZ);

        // generate a heightMap using noise
        Texture2D heightTexture = BuildTexture(heightMap, this.heatTerrainTypes);
        this.tileRenderer.material.mainTexture = heightTexture;

        UpdateMeshVertices(heightMap);
    }

    ///<summary>
    /// Generates a texture2D for the tile and then assign this texture to the tile material
    ///</summary> 
    private Texture2D BuildTexture(float [,] heightMap, TerrainType[] terrainTypes)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);


        Color[] colorMap = new Color[tileDepth * tileWidth];
        for(int zIndex = 0; zIndex< tileDepth; zIndex++)
        {
            for(int xIndex = 0; xIndex<tileWidth;xIndex++)
            {
                // transform the 2D map indes to an array index
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = heightMap[zIndex, xIndex];
                // choose a terrain type according to the height vaules
                TerrainType terrainType = ChooseTerrainType (height, terrainTypes);
                // assign the color accoriding to the terrain type
                colorMap[colorIndex] = terrainType.color;
            }
        }
        //create a new texture and set its pixel colors
        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();
        return tileTexture;
    }
    ///<summary>
    /// Iterate through terrainTypes array and return the first types whoee height is greater 
    /// than the height value.
    ///</summary>
    TerrainType ChooseTerrainType (float height, TerrainType[] terrainTypes)
    {
        foreach(TerrainType terrainType in terrainTypes)
        {
            if (height < terrainType.height)
            {
                return terrainType;
            } 
        }
        return terrainTypes[terrainTypes.Length-1];
    }

    ///<summary>
    ///  This method will be responsible for changing the Plane Mesh vertices according to the height map, 
    ///and its called in the end of the GenerateTile method.
    ///</summary>
    void UpdateMeshVertices (float [,] heightMap)
    {
        int tileDepth = heightMap.GetLength(0);
        int tileWidth = heightMap.GetLength(1);

        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        //iterate through all the heightMap coordinates and updating the vertex index
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth ; zIndex++)
        {
            for (int xIndex = 0 ; xIndex < tileWidth ; xIndex++)
            {
                float height = heightMap [zIndex, xIndex];

                Vector3 vertex = meshVertices[vertexIndex];
                // Changes the vertex Y coordinate, proportional to the height vaule
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);
                vertexIndex++;
            }
        }
        //update the verticies in the mesh and update its properties
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        // update the mesh collider
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }
}
