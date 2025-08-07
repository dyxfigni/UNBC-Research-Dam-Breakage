using System.Collections.Generic;
using UnityEngine;

public class TerrainPostProcessor
{ 
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void FillHoles(ref float[,] heights)
    {
        int res = heights.GetLength(0);
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                if (heights[y, x] < 0)
                {
                    float sum = 0f;
                    int count = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int ny = y + dy, nx = x + dx;
                            if (ny >= 0 && ny < res && nx >= 0 && nx < res && heights[ny, nx] >= 0)
                            {
                                sum += heights[ny, nx];
                                count++;
                            }
                        }
                    }
                    heights[y, x] = count > 0 ? sum / count : 0f;
                }
            }
        }
    }

    public void MedianSmooth(ref float[,] heights)
    {
        int res = heights.GetLength(0);
        float[,] result = new float[res, res];
        for (int y = 1; y < res - 1; y++)
        {
            for (int x = 1; x < res - 1; x++)
            {
                List<float> values = new();
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        values.Add(heights[y + dy, x + dx]);
                values.Sort();
                result[y, x] = values[values.Count / 2];
            }
        }
        for (int y = 1; y < res - 1; y++)
            for (int x = 1; x < res - 1; x++)
                heights[y, x] = result[y, x];
    }

    public void GaussianSmooth(ref float[,] heights)
    {
        int[,] kernel = {
        {1, 2, 1},
        {2, 4, 2},
        {1, 2, 1}
    };
        int res = heights.GetLength(0);
        float[,] result = new float[res, res];
        for (int y = 1; y < res - 1; y++)
        {
            for (int x = 1; x < res - 1; x++)
            {
                float sum = 0;
                int weightSum = 0;
                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int val = kernel[ky + 1, kx + 1];
                        sum += heights[y + ky, x + kx] * val;
                        weightSum += val;
                    }
                }
                result[y, x] = sum / weightSum;
            }
        }
        for (int y = 1; y < res - 1; y++)
            for (int x = 1; x < res - 1; x++)
                heights[y, x] = result[y, x];
    }
    

}
