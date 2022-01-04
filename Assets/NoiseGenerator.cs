using System;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Diagnostics;
using System.Collections.Generic;

public class NoiseGenerator : MonoBehaviour
{
    Erosionenerator _ErosionGenerator = null;
    private Stopwatch _s = new Stopwatch();
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
            PrintTime();
            _ErosionGenerator.ResetMaps();
            }
    }

    private Terrain _landscape = new Terrain(); 
    public Terrain Landscape { set { _landscape = value; } get { return _landscape; } }

    private int _width = 256;
    private int _height = 256;

    #region EDITORPARAMETERS
    [Header("PERLIN NOISE PARAMETERS")]
    [SerializeField] public float _scale = 20f;
    [SerializeField] public float _offsetX = 100f;
    [SerializeField] public float _offsetY = 100f;

    [Header("DIAMOND SQUARE NOISE PARAMETERS")]
    [SerializeField] public float _roughness = 84;
    [SerializeField] public float _topRightValue = 0.147603f;
    [SerializeField] public float _topLeftValue = 0.3303762f;
    [SerializeField] public float _bottomRightValue = 0.006591558f;
    [SerializeField] public float _bottomLeftValue = 0.3981243f;
    #endregion
    #region DIAMONDSQUAREVARIABLES
    private float[,] _noiseMap = new float[257, 257];
    private float _r = 84;
    private int _step;
    private int _w = 257, _h = 257;
    #endregion
    private void Start()
    {
        _ErosionGenerator = GetComponent<Erosionenerator>();
        _landscape = GetComponent<Terrain>();
        _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
        PrintTime();
    }
    #region TERRAINFUNCTIONS
    public void RegenerateTerrain()
    {
        if (_landscape == null)
            _landscape = GetComponent<Terrain>();
        if(_ErosionGenerator == null)
            _ErosionGenerator = GetComponent<Erosionenerator>();

        _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
        PrintTime();
        _ErosionGenerator.ResetMaps();
    }
    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        _s.Start();
        terrainData.heightmapResolution = _width + 1;

        terrainData.size = new Vector3(_width, 20, _height);

        terrainData.SetHeights(0, 0, Generateheights());
        _s.Stop();

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
    public float[,] GetHeightValues()
    {
        return _landscape.terrainData.GetHeights(0, 0, _width, _height);
    }
    #endregion
    #region PERLIN
    float PerlinNoise(int x, int y)
    {
        float xCoord = (float)x / _width * _scale + _offsetX;
        float yCoord = (float)y / _height * _scale + _offsetY;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
    #endregion
    #region DIAMONDSQUARE
    void DiamondSquare(float r)
    {
        //Fixed values => I got good results with these
        _noiseMap[0, 0] = _bottomLeftValue;
        _noiseMap[0, _h - 1] = _topLeftValue;
        _noiseMap[_w - 1, 0] = _bottomRightValue;
        _noiseMap[_w - 1,_h - 1] = _topRightValue;

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
        float mid;
        float topLeftValue;
        float topRightValue;
        float bottomLeftValue;
        float bottomRightValue;
        float nrOfCorners;

        int halfStep = stepsize / 2;

        for (int y  = halfStep; y < _h; y += stepsize)
        {
            for (int x = halfStep; x < _w; x += stepsize)
            {
                nrOfCorners = 4;

                // Get values from the corners(SQUARE)
                topLeftValue = noiseMap[x - halfStep, y - halfStep];
                topRightValue = noiseMap[x + halfStep, y - halfStep];
                bottomLeftValue = noiseMap[x - halfStep, y + halfStep];
                bottomRightValue = noiseMap[x + halfStep, y + halfStep];

                // Calculate the central value
                mid = (topLeftValue + topRightValue + bottomLeftValue + bottomRightValue) / nrOfCorners;

                // Add the offset
                mid += Random.Range(-_r, _r);
                noiseMap[x, y] = mid;
            }
        }
    }
    void diamondStep(float[,] noiseMap, int stepsize)
    {
        float mid;
        float leftValue;
        float rightValue;
        float bottomValue;
        float topValue;
        float nrOfCorners;

        int halfStep = stepsize / 2;

        // x-offset   
        for (int y = 0; y < _h; y += stepsize)
        {
            for (int x = halfStep; x < _w; x += stepsize)
            {
                nrOfCorners = 4;

                // Get values from the corners(DIAMOND)
                //if out of bounds remove one corner and set value to 0
                if (x - halfStep >= 0) leftValue = noiseMap[x - halfStep, y]; else { leftValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (x + halfStep < _w) rightValue = noiseMap[x + halfStep, y]; else { rightValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (y - halfStep >= 0) bottomValue = noiseMap[x, y - halfStep]; else { bottomValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (y + halfStep < _h) topValue = noiseMap[x, y + halfStep]; else { topValue = 0; nrOfCorners = nrOfCorners - 1; }

                // Calculate the index of the central value
                mid = (leftValue + rightValue + bottomValue + topValue) / nrOfCorners;

                // Add the offset
                mid += Random.Range(-_r, _r);
                noiseMap[x, y] = mid;
            }
        }
        // y-offset   
        for (int y = halfStep; y < _h; y += stepsize)
        {
            for (int x = 0; x < _w; x += stepsize)
            {
                nrOfCorners = 4;

                // Get values from the corners
                //if out of bounds remove one corner and set value to 0
                if (x - halfStep >= 0) leftValue = noiseMap[x - halfStep, y]; else { leftValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (x + halfStep < _w) rightValue = noiseMap[x + halfStep, y]; else { rightValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (y - halfStep >= 0) bottomValue = noiseMap[x, y - halfStep]; else { bottomValue = 0; nrOfCorners = nrOfCorners - 1; }
                if (y + halfStep < _h) topValue = noiseMap[x, y + halfStep]; else { topValue = 0; nrOfCorners = nrOfCorners - 1; }

                // Calculate the index of the central value
                mid = (leftValue + rightValue + bottomValue + topValue) / nrOfCorners;

                // Add the offset
                mid += Random.Range(-_r, _r);
                noiseMap[x, y] = mid;
            }
        }
    }
    #endregion

    void PrintTime()
    {
        TimeSpan ts = _s.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}",
            ts.Seconds,
            ts.Milliseconds);
        UnityEngine.Debug.Log($"It took " + elapsedTime);
        _s.Reset();
    }
}