using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    private Terrain _landscape = new Terrain();
    private NoiseGenerator _noiseGenerator;
    ErosionGenerator _erosionGenerator;
    private int _currentNoiseIndex = 0;
    private float[,] _demoHeights;
    private float[,] _allHeights;
    [SerializeField] private bool _isdemo = true;
    [SerializeField] private float _waitSec = .1f;
    private float _elapsedWaitSec = 0f;
    [SerializeField] private int _thermalIterations = 100;
    private int _currentThermalIterations = 0;
    [SerializeField] private int _hydraulicIterations = 10;
    private int _currentHydraulicIterations = 0;

    void Start()
    {
        _noiseGenerator = GetComponent<NoiseGenerator>();
        _erosionGenerator = GetComponent<ErosionGenerator>();
        _erosionGenerator._isDemo = _isdemo;
        _landscape = GetComponent<Terrain>();
        float[,] zeroHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _demoHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _allHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _allHeights = _noiseGenerator.GetHeightValues();
        _landscape.terrainData.SetHeights(0, 0, zeroHeights);
        _erosionGenerator._iterationsThermal = 1;

    }

    void Update()
    {
        if (_currentNoiseIndex == 256)
        {
            if (_elapsedWaitSec >= _waitSec)
            {
                _elapsedWaitSec = 0;
                if (_currentThermalIterations <= _thermalIterations)
                {
                    _erosionGenerator.ExecuteThermalErosion();
                    _currentThermalIterations++;
                }
                else
                {

                }
            }
            else
                _elapsedWaitSec += Time.deltaTime;

            return;
        }

        if (_elapsedWaitSec >= _waitSec)
        {
            _elapsedWaitSec = 0;

            int current = _currentNoiseIndex;
            for (int x = _currentNoiseIndex; x < current + 2; x++)
            {
                for (int y = 0; y < _noiseGenerator._height; y++)
                {
                    _demoHeights[x, y] = _allHeights[x, y];
                }
                _currentNoiseIndex++;
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
