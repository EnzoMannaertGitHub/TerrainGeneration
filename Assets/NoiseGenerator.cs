using System;
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

    #region EDITORPARAMETERS
    [Header("PERLIN NOISE PARAMETERS")]
    [SerializeField] private float _scale = 20f;
    [SerializeField] private float _offsetX = 100f;
    [SerializeField] private float _offsetY = 100f;

    [Header("DIAMOND SQUARE NOISE PARAMETERS")]
    [SerializeField] private float _roughness = 84;
    #endregion
    #region DIAMONDSQUAREVARIABLES
    private float[,] _noiseMap = new float[257, 257];
    private float _r = 84;
    private int _step;
    private int _w = 257, _h = 257;
    #endregion
    private void Start()
    {
        _offsetX = Random.Range(0f, 9999f);
        _offsetY = Random.Range(0f, 9999f);

        _landscape = GetComponent<Terrain>();
        _landscape.terrainData = GenerateTerrain(_landscape.terrainData);
    }

    #region TERRAINFUNCTIONS
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
}