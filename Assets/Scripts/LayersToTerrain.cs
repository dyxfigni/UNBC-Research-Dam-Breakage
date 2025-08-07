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

        if (targetTerrain == null)
        {
            Debug.LogError("Terrain not found!");
            return;
        }

        ApplyTextureLayers();     
        
    }

    void ApplyTextureLayers()
    {
        TerrainData terrainData = targetTerrain.terrainData;

        TerrainLayer[] layers = new TerrainLayer[3];


        layers[0] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Grass.terrainlayer");
        layers[1] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Dirt.terrainlayer");
        layers[2] = Resources.Load<TerrainLayer>("D:\\Research\\hello hello\\Assets\\SavedTerrains\\TerrainLayers\\Cliffs.terrainlayer");


        terrainData.terrainLayers = layers;

        int alphaRes = terrainData.alphamapResolution;
        float[,,] alphaMap = new float[alphaRes, alphaRes, 3];

        float[,] heights = terrainData.GetHeights(
            0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        for(int y = 0; y < alphaRes; y++)
        {
            for(int x = 0; x< alphaRes; x++)
            {
                float normX = (float)x / (alphaRes - 1);
                float normY = (float)y / (alphaRes - 1);

                int heightX = Mathf.RoundToInt(normX * (terrainData.heightmapResolution - 1));
                int heightY = Mathf.RoundToInt(normY * (terrainData.heightmapResolution - 1));

                float height = heights[heightY, heightX];

                float[] weights = new float[3];

                if (height < 0.4f) weights[0] = 1f;
                else if (height < 0.75f) weights[1] = 1f;
                else weights[2] = 1f;

                float total = weights[0] + weights[1] + weights[2];
                for (int i = 0; i < 3; i++)
                    alphaMap[y, x, i] = weights[i] / total;
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
        Debug.Log("Terrain textures applied by height");
    }


}
