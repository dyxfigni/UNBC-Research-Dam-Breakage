using UnityEngine;

public class LayersToTerrain : MonoBehaviour
{
    public Terrain targetTerrain;
    public int terrainResolution = 2049;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(targetTerrain == null)
        {
            GameObject terrainObj = GameObject.Find("TerrainUNBC");
            if(terrainObj!=null)
                targetTerrain = terrainObj.GetComponent<Terrain>();              
        }

        if

        TerrainLayer[] layers = new TerrainLayer[3];

        
        layers[0] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Grass.terrainlayer");
        layers[1] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Dirt.terrainlayer");
        layers[2] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Cliffs.terrainlayer");

        
    }


}
