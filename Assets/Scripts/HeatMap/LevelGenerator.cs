using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary>
///creating multiple level tiles.
///</summary>
public class LevelGenerator : MonoBehaviour
{
    [SerializeField]
    private int mapWidthIntiles, mapDepthIntiles;

    [SerializeField]
    private GameObject tilePrefab;
  
    void Start()
    {
        GenerateMap();
    }

    ///<summary>
    ///Creates a level tiles by iterating through all the tile coordinates. 
    /// Calculates each tiles postions based on the tiles coordiante and the instantiate a copy of it from the level tile prefab.
    ///</summary>
    void GenerateMap()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int)tileSize.x;
        int tileDepth = (int)tileSize.z;

        for (int xTileIndex = 0 ; xTileIndex < mapWidthIntiles ; xTileIndex++)
        {
            for(int zTileIndex = 0; zTileIndex < mapWidthIntiles ; zTileIndex++)
            {
                Vector3 tilePosition = new Vector3(this.gameObject.transform.position.x + xTileIndex * tileWidth, this.gameObject.transform.position.y, this.gameObject.transform.position.z + zTileIndex * tileDepth);

                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
            } 
        }
    }
}
