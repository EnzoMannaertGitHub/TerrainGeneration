using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    private Terrain _landscape = new Terrain();
    private NoiseGenerator _noiseGenerator;
    private int _currentIndex = 0;
    private float[,] _demoHeights;
    private float[,] _allHeights;

    private float _waitSec = .1f;
    private float _elapsedWaitSec = 0f;
    void Start()
    {
        _noiseGenerator = GetComponent<NoiseGenerator>();
        _landscape = GetComponent<Terrain>();
        float[,] zeroHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _demoHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _allHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _allHeights = _noiseGenerator.GetHeightValues();
        _landscape.terrainData.SetHeights(0, 0, zeroHeights);
    }

    void Update()
    {
        if (_currentIndex == 256)
            return;

        if (_elapsedWaitSec >= _waitSec)
        {
            _elapsedWaitSec = 0;

            int current = _currentIndex;
            for (int x = _currentIndex; x < current + 2; x++)
            {
                for (int y = 0; y < _noiseGenerator._height; y++)
                {
                    _demoHeights[x, y] = _allHeights[x, y];
                }
                _currentIndex++;
            }
            _landscape.terrainData.SetHeights(0, 0, _demoHeights);
        }
        else
            _elapsedWaitSec += Time.deltaTime;

    }

    void OnApplicationQuit()
    {
        _landscape.terrainData.SetHeights(0, 0, _allHeights);
    }
}
