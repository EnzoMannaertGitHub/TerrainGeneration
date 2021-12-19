using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NoiseGenerator : MonoBehaviour
{
    private Terrain _landscape = new Terrain();
    private List<Vector2> _grid = new List<Vector2>();
    
    private List<Vector2> _gradientTable = new List<Vector2>();
    private int _width = 256;
    private int _height = 256;

    public float _scale = 20f;

    public float _offsetX = 100f;
    public float _offsetY = 100f;

    float[,] pixmap = new float[257, 257];
    float roughness = 84;
    public int r = 84;
    int step;
    int w = 257, h = 257;
    float scale;
    private void Start()
    {
        _offsetX = Random.Range(0f, 9999f);
        _offsetY = Random.Range(0f, 9999f);

    }

    private void Update()
    {
        plasmaFractal(r);
        _landscape = GetComponent<Terrain>();
        _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = _width + 1;

        terrainData.size = new Vector3(_width, 20, _height);

        terrainData.SetHeights(0, 0, Generateheights());

        return terrainData;
    }

    float[,] Generateheights()
    {
        float[,] heights = new float[_width, _height];
        for(int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                heights[x, y] = Math.Abs(pixmap[x, y]);
                //heights[x, y] = CalculateHeight(x, y);
            }
        }

        return heights;
    }
    float CalculateHeight(int x, int y)
    {
        float xCoord = (float)x / _width * _scale + _offsetX;
        float yCoord = (float)y / _height * _scale + _offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    void plasmaFractal(int r)
    {
        int p, x, y, gray;
        // Initialize the corner points to random values
        pixmap[0, 0] = Random.Range(0, 1);
        pixmap[0,h - 1] = Random.Range(0, 1);
        pixmap[w - 1,0] = Random.Range(0, 1);
        pixmap[w - 1,h - 1] = Random.Range(0, 1);

        // Set the roughness factor, higher values = rougher surface
        roughness = r;
        step = _width;

        // Iterate through the matrix. When step becomes 1, all the pixels
        // have been calculated. Apply the Square algorithm, followed by the
        // Diamond algorithm.
        while (step > 0)
        {
            squareStep(pixmap, step);
            diamondStep(pixmap, step);
            step = step / 2;
            roughness = roughness / 2;
        }
    }

    void squareStep(float[,] R, int stepsize)
    {
        int x, y, hs = stepsize / 2;
        float mid, a, b, c, d, n;
        for (y = hs; y < h; y = y + stepsize)
            for (x = hs; x < w; x = x + stepsize)
            {
                n = 4;
                // Extract values from the corners of the square
                a = R[x - hs,y - hs]; // top-left
                b = R[x + hs,y - hs]; // top-right
                c = R[x - hs,y + hs]; // bottom-left
                d = R[x + hs,y + hs]; // bottom-right
                                       // Calculate the index of the central value of the square
                mid = (a + b + c + d) / n;
                // Add the offset
                mid = mid + Random.Range(-roughness, roughness);
                R[x,y] = mid;
            }
    }

    void diamondStep(float[,] R, int stepsize)
    {
        int x, y, hs = stepsize / 2;
        float p, a, b, c, d, n;
        // x-offset   
        for (y = 0; y < h; y = y + stepsize)
            for (x = hs; x < w; x = x + stepsize)
            {
                n = 4;
                // Extract values from the corners of the diamond
                // Make sure the values are within bounds
                if (x - hs >= 0) a = R[x - hs,y]; else { a = 0; n = n - 1; }
                if (x + hs < w) b = R[x + hs,y]; else { b = 0; n = n - 1; }
                if (y - hs >= 0) c = R[x,y - hs]; else { c = 0; n = n - 1; }
                if (y + hs < h) d = R[x,y + hs]; else { d = 0; n = n - 1; }
                // Calculate the index of the central value of the diamond
                p = (a + b + c + d) / n;
                // Add the offset
                p = p + Random.Range(-roughness, roughness);
                R[x, y] = p;
            }
        // y-offset   
        for (y = hs; y < h; y = y + stepsize)
            for (x = 0; x < w; x = x + stepsize)
            {
                n = 4;
                // Extract values from the corners of the diamond
                // Make sure the values are within bounds
                if (x - hs >= 0) a = R[x - hs, y]; else { a = 0; n = n - 1; }
                if (x + hs < w) b = R[x + hs,y]; else { b = 0; n = n - 1; }
                if (y - hs >= 0) c = R[x,y - hs]; else { c = 0; n = n - 1; }
                if (y + hs < h) d = R[x,y + hs]; else { d = 0; n = n - 1; }
                // Calculate the index of the central value of the diamond
                p = (a + b + c + d) / n;
                // Add the offset
                p = p + Random.Range(-roughness, roughness);
                R[x,y] = p;
            }
    }
}