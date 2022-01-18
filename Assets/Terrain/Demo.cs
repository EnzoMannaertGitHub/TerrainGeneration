using TMPro;
using UnityEngine;

public class Demo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _iterations;
    [SerializeField] TextMeshProUGUI _DemoText;
    [SerializeField] private bool _isdemo = true;
    [SerializeField] private float _waitSec = .1f;
    [SerializeField] private int _thermalIterations = 100;
    [SerializeField] private int _hydraulicIterations = 10;

    private Terrain _landscape = new Terrain();
    private NoiseGenerator _noiseGenerator;
    private ErosionGenerator _erosionGenerator;

    private int _currentNoiseIndex = 0;
    private int _currentThermalIterations = 0;
    private int _currentHydraulicIterations = 0;

    private float[,] _demoHeights;
    private float[,] _allHeights;

    private float _elapsedWaitSec = 0f;
    private bool _isFinished = false;
    private float _presentSec = 5f;

    void Start()
    {
        _noiseGenerator = GetComponent<NoiseGenerator>();
        _erosionGenerator = GetComponent<ErosionGenerator>();
        _landscape = GetComponent<Terrain>();

        _erosionGenerator._isDemo = _isdemo;

        float[,] zeroHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _demoHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
        _allHeights = new float[_noiseGenerator._width, _noiseGenerator._height];

        _allHeights = _landscape.terrainData.GetHeights(0, 0, _noiseGenerator._width, _noiseGenerator._height);
        _landscape.terrainData.SetHeights(0, 0, zeroHeights);

        _erosionGenerator._iterationsThermal = 1;
        _erosionGenerator._iterationsHydraulic = 1;
    }

    void Update()
    {
        if (_isFinished)
        {
            HandleFinish();
            return;
        }
        if (_currentNoiseIndex == _noiseGenerator._width)
        {
            HandleErosion();
            return;
        }

        HandleNoise();
    }

    void OnApplicationQuit()
    {
        _landscape.terrainData.SetHeights(0, 0, _allHeights);
    }
    void HandleErosion()
    {
        if (_elapsedWaitSec >= _waitSec * 3)
        {
            _elapsedWaitSec = 0;
            if (_currentThermalIterations < _thermalIterations)
            {
                _DemoText.SetText("Manipulating landscape using Thermal erosion");
                _erosionGenerator.ExecuteThermalErosion();
                _currentThermalIterations++;
                _iterations.SetText($"Iterations: " + _currentThermalIterations);
            }
            else if (_currentHydraulicIterations < _hydraulicIterations)
            {
                _DemoText.SetText("Manipulating landscape using Hydraulic erosion");
                _erosionGenerator.ExecuteHudraulicErosion();
                _currentHydraulicIterations++;
                _iterations.SetText($"Iterations: " + _currentHydraulicIterations);
            }
            else
                _isFinished = true;
        }
        else
        {
            _elapsedWaitSec += Time.deltaTime;
        }
    }
    void HandleNoise()
    {
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
    void HandleFinish()
    {
        if (_elapsedWaitSec >= _presentSec)
        {
            _isFinished = false;
            _elapsedWaitSec = 0;
            _currentHydraulicIterations = 0;
            _currentThermalIterations = 0;
            _currentNoiseIndex = 0;
            _erosionGenerator.IsDemoMapsReset = false;
            _noiseGenerator.RegenerateTerrain();
            _allHeights = _noiseGenerator.GetHeightValues();
            float[,] zeroHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
            _landscape.terrainData.SetHeights(0, 0, zeroHeights);
            _demoHeights = new float[_noiseGenerator._width, _noiseGenerator._height];
            _DemoText.SetText("Creating landscape using Noise");
            _iterations.SetText("");
        }
        else
            _elapsedWaitSec += Time.deltaTime;
    }
}
