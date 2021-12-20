using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NoiseGenerator : MonoBehaviour
{
    public enum NoiseFunction
    {
        PERLIN = 0,
        DIAMONDSQUARE = 1
    }
    private NoiseFunction _usedNoise = NoiseFunction.PERLIN;
    public NoiseFunction UsedNoise
    {
        set {
            _usedNoise = value;
            _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
            }
    }

    private Terrain _landscape = new Terrain(); 
    private int _width = 256;
    private int _height = 256;

    [Header("PERLIN NOISE PARAMETERS")]
    [SerializeField] private float _scale = 20f;
    [SerializeField] private float _offsetX = 100f;
    [SerializeField] private float _offsetY = 100f;

    [Header("DIAMOND SQUARE NOISE PARAMETERS")]
    [SerializeField] private int _roughness = 84;

    private float[,] _noiseMap = new float[257, 257];
    private float _r = 84;
    private int _step;
    private int _w = 257, _h = 257;

    private void Start()
    {
        _offsetX = Random.Range(0f, 9999f);
        _offsetY = Random.Range(0f, 9999f);

        _landscape = GetComponent<Terrain>();
        _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
    }

    public void RegenerateTerrain()
    {
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
        if (_usedNoise == NoiseFunction.DIAMONDSQUARE)
            DiamondSquare(_roughness);

        float[,] heights = new float[_width, _height];
        for(int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                switch (_usedNoise)
                {
                    case NoiseFunction.PERLIN:
                        heights[x, y] = PerlinNoise(x, y);
                        break;
                    case NoiseFunction.DIAMONDSQUARE:
                        heights[x, y] = Math.Abs(_noiseMap[x, y]);
                        break;
                }
            }
        }

        return heights;
    }
    float PerlinNoise(int x, int y)
    {
        float xCoord = (float)x / _width * _scale + _offsetX;
        float yCoord = (float)y / _height * _scale + _offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }

    void DiamondSquare(int r)
    {
        //Fixed values => I got good results with these
        _noiseMap[0, 0] = 0.3981243f;
        _noiseMap[0, _h - 1] = 0.3303762f;
        _noiseMap[_w - 1,0] = 0.006591558f;
        _noiseMap[_w - 1,_h - 1] = 0.147603f;

        _r = r;
        _step = _width;

        while (_step > 0)
        {
            squareStep(_noiseMap, _step);
            diamondStep(_noiseMap, _step);

            _step /= 2;
            _r /= 2;
        }
    }
    void squareStep(float[,] noiseMap, int stepsize)
    {
        int x, y, hs = stepsize / 2;
        float mid, a, b, c, d, n;
        for (y = hs; y < _h; y = y + stepsize)
            for (x = hs; x < _w; x = x + stepsize)
            {
                n = 4;
                // Get values from the corners
                a = noiseMap[x - hs,y - hs]; // top-left
                b = noiseMap[x + hs,y - hs]; // top-right
                c = noiseMap[x - hs,y + hs]; // bottom-left
                d = noiseMap[x + hs,y + hs]; // bottom-right

                // Calculate the index of the central value
                mid = (a + b + c + d) / n;

                // Add the offset
                mid = mid + Random.Range(-_r, _r);
                noiseMap[x,y] = mid;
            }
    }

    void diamondStep(float[,] noiseMap, int stepsize)
    {
        int x, y, hs = stepsize / 2;
        float centralPoint, a, b, c, d, n;

        // x-offset   
        for (y = 0; y < _h; y = y + stepsize)
            for (x = hs; x < _w; x = x + stepsize)
            {
                n = 4;
                // Get values from the corners
                if (x - hs >= 0) a = noiseMap[x - hs,y]; else { a = 0; n = n - 1; }
                if (x + hs < _w) b = noiseMap[x + hs,y]; else { b = 0; n = n - 1; }
                if (y - hs >= 0) c = noiseMap[x,y - hs]; else { c = 0; n = n - 1; }
                if (y + hs < _h) d = noiseMap[x,y + hs]; else { d = 0; n = n - 1; }

                // Calculate the index of the central value
                centralPoint = (a + b + c + d) / n;

                // Add the offset
                centralPoint = centralPoint + Random.Range(-_r, _r);
                noiseMap[x, y] = centralPoint;
            }

        // y-offset   
        for (y = hs; y < _h; y = y + stepsize)
            for (x = 0; x < _w; x = x + stepsize)
            {
                n = 4;
                // Get values from the corners
                if (x - hs >= 0) a = noiseMap[x - hs, y]; else { a = 0; n = n - 1; }
                if (x + hs < _w) b = noiseMap[x + hs,y]; else { b = 0; n = n - 1; }
                if (y - hs >= 0) c = noiseMap[x,y - hs]; else { c = 0; n = n - 1; }
                if (y + hs < _h) d = noiseMap[x,y + hs]; else { d = 0; n = n - 1; }

                // Calculate the index of the central value
                centralPoint = (a + b + c + d) / n;

                // Add the offset
                centralPoint = centralPoint + Random.Range(-_r, _r);
                noiseMap[x,y] = centralPoint;
            }
    }
}