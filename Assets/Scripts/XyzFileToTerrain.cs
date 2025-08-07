using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

public class XyzFileToTerrain : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Description("Path to XYZ File")]
    public string filePath = "D:\\Research\\hello hello\\Assets\\Scripts\\LasToXYZ\\XYZFiles\\output.xyz";

    [Description("Scale for the Terrain")]
    public float pointScale = 0.1f;

    [Description("Point Material")]
    public Material pointMaterial;

    Dictionary<int, Vector3> map = new();

    Dictionary<int, float> heights = new();

    //513, 1025, or 2049
    public int terrainResolution = 2049;
    //public float terrainHeight = 100f;

    void Start()
    {
        StartCoroutine(LoadPoints());

        //LoadHeights();
    }

    IEnumerator LoadPoints()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found");
            yield break;
        }

        List<Vector3> points = new();

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        using (StreamReader reader = new(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var tokens = line.Split(' ', '\t');
                if (tokens.Length < 3) continue;

                if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    points.Add(new Vector3(x, z, y));

                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    if (z < minZ) minZ = z;
                    if (z > maxZ) maxZ = z;
                }
            }
        }

        ///
        /// Finding the width and length
        /// In fact we are just finding deltaX,
        /// deltaY, deltaZ
        /// Which will be our sizes of the terrain
        ///
        float width = maxX - minX;
        float length = maxY - minY;
        //float height = maxZ - minZ;

        //number of points by height
        float[,] heights = new float[terrainResolution, terrainResolution];
        int[,] counts = new int[terrainResolution, terrainResolution];

        foreach (var p in points)
        {
            int xIndex = Mathf.FloorToInt(((p.x - minX) / width) * (terrainResolution - 1));
            int yIndex = Mathf.FloorToInt(((p.z - minY) / length) * (terrainResolution - 1));

            xIndex = Mathf.Clamp(xIndex, 0, terrainResolution - 1);
            yIndex = Mathf.Clamp(yIndex, 0, terrainResolution - 1);

            heights[yIndex, xIndex] += (p.y - minZ);
            counts[yIndex, xIndex]++;
        }

        for (int y = 0; y < terrainResolution; y++)
        {
            for (int x = 0; x < terrainResolution; x++)
            {
                if (counts[y, x] > 0)
                    heights[y, x] /= counts[y, x];
                else
                    heights[y, x] = -1f; // gaps
            }
        }

        TerrainPostProcessor processor = new TerrainPostProcessor();
        processor.FillHoles(ref heights);
        processor.MedianSmooth(ref heights);
        processor.GaussianSmooth(ref heights);

        float finalHeight = maxZ - minZ;
        float[,] normalizedHeights = new float[terrainResolution, terrainResolution];
        for (int y = 0; y < terrainResolution; y++)
            for (int x = 0; x < terrainResolution; x++)
                normalizedHeights[y, x] = Mathf.Clamp01(heights[y, x] / finalHeight);

        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = terrainResolution;

        
        

        ///
        /// As in the Lidar file the scale is 1:2,500 it means that
        /// each point (the height) is ~ in 2 meters apart from each other;
        /// therefore, on the terrain, the points have spikes, those spikes
        /// are cause due to this matter
        ///
        //float scaleFactor = 2f;
        //terrainData.size = new Vector3(width * scaleFactor, finalHeight * scaleFactor, length * scaleFactor);
        terrainData.size = new Vector3(width , finalHeight , length );

        terrainData.SetHeights(0, 0, normalizedHeights);

        Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
        terrain.transform.position = new Vector3(minX, minZ, minY);

        AssetDatabase.CreateAsset(terrainData, "Assets/SavedTerrains/RealTerrainUNBCDam.asset");
        AssetDatabase.SaveAssets();

        Debug.Log("Terrain generated with exact real-world elevation.");
        yield return null;
    }

    //IEnumerator LoadPoints()
    //{
    //    if (!File.Exists(filePath))
    //    {
    //        Debug.LogError("File not found");
    //        yield break;
    //    }

    //    List<Vector3> points = new();

    //    float minX = float.MaxValue, maxX = float.MinValue;
    //    float minY = float.MaxValue, maxY = float.MinValue;
    //    float minZ = float.MaxValue, maxZ = float.MinValue;

    //    using (StreamReader reader = new(filePath))
    //    {
    //        string line;
    //        while ((line = reader.ReadLine()) != null)
    //        {
    //            var tokens = line.Split(' ', '\t');
    //            if (tokens.Length < 3) continue;

    //            if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
    //                float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
    //                float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
    //            {
    //                points.Add(new Vector3(x, z, y));

    //                if (x < minX) minX = x;
    //                if (x > maxX) maxX = x;
    //                if (y < minY) minY = y;
    //                if (y > maxY) maxY = y;
    //                if (z < minZ) minZ = z;
    //                if (z > maxZ) maxZ = z;
    //            }
    //        }
    //    }

    //    float width = maxX - minX;
    //    float length = maxY - minY;
    //    float height = maxZ - minZ;

    //    float[,] heights = new float[terrainResolution, terrainResolution];
    //    int[,] counts = new int[terrainResolution, terrainResolution];

    //    foreach (var p in points)
    //    {
    //        int xIndex = Mathf.FloorToInt(((p.x - minX) / width) * (terrainResolution - 1));
    //        int yIndex = Mathf.FloorToInt(((p.z - minY) / length) * (terrainResolution - 1));

    //        xIndex = Mathf.Clamp(xIndex, 0, terrainResolution - 1);
    //        yIndex = Mathf.Clamp(yIndex, 0, terrainResolution - 1);

    //        heights[yIndex, xIndex] += (p.y - minZ); // сохраняем "чистую" высоту в метрах
    //        counts[yIndex, xIndex]++;
    //    }

    //    // Среднее по точкам в ячейке + нормализация по высоте
    //    for (int y = 0; y < terrainResolution; y++)
    //    {
    //        for (int x = 0; x < terrainResolution; x++)
    //        {
    //            if (counts[y, x] > 0)
    //                heights[y, x] = heights[y, x] / counts[y, x] / height; // нормализуем после усреднения
    //            else
    //                heights[y, x] = 0;
    //        }
    //    }

    //    TerrainData terrainData = new TerrainData();
    //    terrainData.heightmapResolution = terrainResolution;
    //    terrainData.size = new Vector3(width, height, length); // <-- точно соответствует метрам!
    //    terrainData.SetHeights(0, 0, heights);

    //    Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
    //    terrain.transform.position = new Vector3(minX, minZ, minY); // <-- абсолютное позиционирование

    //    Debug.Log("Terrain generated with exact real-world elevation.");
    //    yield return null;


    //if (!File.Exists(filePath))
    //{
    //    Debug.LogError("File not found: " + filePath);
    //    yield break;
    //}

    //Debug.Log("Reading file...");
    //List<Vector3> points = new List<Vector3>();



    //float minX = float.MaxValue, maxX = float.MinValue;
    //float minY = float.MaxValue, maxY = float.MinValue;
    //float minZ = float.MaxValue, maxZ = float.MinValue;

    //using (StreamReader reader = new StreamReader(filePath))
    //{
    //    string line;
    //    while ((line = reader.ReadLine()) != null)
    //    {
    //        string[] tokens = line.Split(' ');
    //        if (tokens.Length < 3) continue;

    //        if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
    //            float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
    //            float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
    //        {
    //            points.Add(new Vector3(x, z, y)); // Z = высота, X/Z → позиция

    //            if (x < minX) minX = x;
    //            if (x > maxX) maxX = x;
    //            if (y < minY) minY = y;
    //            if (y > maxY) maxY = y;
    //            if (z < minZ) minZ = z;
    //            if (z > maxZ) maxZ = z;
    //        }
    //    }
    //}

    //Debug.Log($"Loaded {points.Count} points");

    //float width = maxX - minX;
    //float length = maxY - minY;
    //float heightRange = maxZ - minZ;

    //float[,] heights = new float[terrainResolution, terrainResolution];
    //int[,] count = new int[terrainResolution, terrainResolution]; // Для усреднения

    //foreach (var p in points)
    //{
    //    int xIndex = Mathf.FloorToInt(((p.x - minX) / width) * (terrainResolution - 1));
    //    int yIndex = Mathf.FloorToInt(((p.z - minY) / length) * (terrainResolution - 1));

    //    xIndex = Mathf.Clamp(xIndex, 0, terrainResolution - 1);
    //    yIndex = Mathf.Clamp(yIndex, 0, terrainResolution - 1);

    //    heights[yIndex, xIndex] += (p.y - minZ) / heightRange;
    //    count[yIndex, xIndex]++;
    //}

    //// Усреднение
    //for (int y = 0; y < terrainResolution; y++)
    //{
    //    for (int x = 0; x < terrainResolution; x++)
    //    {
    //        if (count[y, x] > 0)
    //            heights[y, x] /= count[y, x];
    //        else
    //            heights[y, x] = 0;
    //    }
    //}

    //TerrainData terrainData = new TerrainData();
    //terrainData.heightmapResolution = terrainResolution;
    //terrainData.size = new Vector3(width, terrainHeight, length);
    //terrainData.SetHeights(0, 0, heights);

    //Terrain terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
    //terrain.transform.position = new Vector3(minX, 0, minY);

    //Debug.Log("Terrain generated.");
    //yield return null;

    //GameObject parent = new GameObject("PointCloud");

    //int counter = 0;
    //using (StreamReader reader = new StreamReader(filePath))
    //{
    //    string line;
    //    while ((line = reader.ReadLine()) != null)
    //    {
    //        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

    //        string[] tokens = line.Split(' ');
    //        if (tokens.Length < 3) continue;

    //        if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
    //            float.TryParse(tokens[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
    //            float.TryParse(tokens[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
    //        {
    //            //GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //            //point.transform.position = new Vector3(x, z, y); // поменяли местами оси под Unity
    //            //point.transform.localScale = Vector3.one * pointScale;
    //            //point.transform.parent = parent.transform;

    //            //if (pointMaterial != null)
    //            //{
    //            //    point.GetComponent<Renderer>().material = pointMaterial;
    //            //}

    //            map.Add(counter,new Vector3(x, z, y));
    //        }

    //        counter++;
    //        if (counter % 1000 == 0) yield return null; // разгрузка на каждый 1000-й объект

    //        if (counter > 10000) // ограничение на количество точек (можно убрать)
    //        {
    //            Debug.Log("Too many points. Stopping after 10k for performance.");
    //            break;
    //        }
    //    }
    //}
    //    Debug.Log("Loaded point cloud with " + parent.transform.childCount + " points."); 
}

