using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class NoiseGenerator : MonoBehaviour
{
    private Terrain _landscape = new Terrain();

    private int _width = 256;
    private int _height = 256;

    public float _scale = 20f;

    public float _offsetX = 100f;
    public float _offsetY = 100f;

    private void Start()
    {
        _offsetX = Random.Range(0f, 9999f);
        _offsetY = Random.Range(0f, 9999f);
    }
    private void Update()
    {
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
                heights[x, y] = CalculateHeight(x, y);
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
}