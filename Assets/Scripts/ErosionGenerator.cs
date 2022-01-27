using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class ErosionGenerator : MonoBehaviour
{
    private Stopwatch _s = new Stopwatch();

    public int _iterationsThermal = 100;
    public int _iterationsHydraulic = 15;
    public float _t = .025f;
    public bool _showTime = true;
    public bool _isDemo = false;
    public float[,] _erosionHeights = new float[256, 256];

    private NoiseGenerator _noiseGenerator;
    private Terrain _landscape = new Terrain();

    private int _width = 256;
    private int _height = 256;

    private float[,] _waterMap = new float[256, 256];
    private float[,] _sedimentMap = new float[256, 256];
    List<float> heightsInvolved = new List<float>();

    private bool _isDemoMapsReset = false;

    public bool IsDemoMapsReset
    {
        get { return _isDemoMapsReset = false; }
        set { _isDemoMapsReset = value; }
    }

    private void ThermalErosion()
    {
        if (_landscape == null)
            _landscape = GetComponent<Terrain>();

        float[,] heights = _landscape.terrainData.GetHeights(0, 0, _width, _height);
        float[,] tempHeights = heights;
        for (int n = 0; n < _iterationsThermal; n++)
        {
            //Distribute to lowest neighbor
            //Using a Von Neuman neighborhood
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    float currentHeight = heights[j, i];
                    Vector2 lowestIndex = new Vector2(-1, -1);

                    //Get height difference with all neighbors (4)
                    float leftHeightDiff = 0;
                    float rightHeightDiff = 0;
                    if (j != 0)
                    {
                        leftHeightDiff = currentHeight - heights[j - 1, i]; //left
                        if (leftHeightDiff > _t)
                        {
                            lowestIndex = new Vector2(j - 1, i);
                            leftHeightDiff = rightHeightDiff;
                        }
                    }

                    if (j != _width - 1)
                    {
                        rightHeightDiff = currentHeight - heights[j + 1, i]; //right
                        if (rightHeightDiff > _t && rightHeightDiff > leftHeightDiff)
                            lowestIndex = new Vector2(j + 1, i);
                    }

                    float upHeightDiff = 0;
                    float downHeightDiff = 0;
                    if (i != _height - 1)
                    {
                        upHeightDiff = currentHeight - heights[j, i + 1]; //up
                        if (upHeightDiff > _t && upHeightDiff >leftHeightDiff && upHeightDiff > leftHeightDiff)
                            lowestIndex = new Vector2(j, i + 1);
                    }

                    if (i != 0)
                    {
                        downHeightDiff = currentHeight - heights[j, i - 1]; //down
                        if (downHeightDiff > _t && downHeightDiff > leftHeightDiff && downHeightDiff > leftHeightDiff && downHeightDiff > upHeightDiff)
                            lowestIndex = new Vector2(j, i - 1);
                    }

                    if (lowestIndex.x == -1)
                        continue;

                    do
                    {
                        tempHeights[j, i] -= .001f;
                        tempHeights[(int)lowestIndex.x, (int)lowestIndex.y] += .001f;
                    } while (tempHeights[j, i] - tempHeights[(int)lowestIndex.x, (int)lowestIndex.y] >= _t);
                }
            }
            heights = tempHeights;
        }
        _erosionHeights = heights;

        _landscape.terrainData.SetHeights(0, 0, heights);
    }
    private void HydraulicErosion()
    {
        if (_landscape == null)
            _landscape = GetComponent<Terrain>();

        //ResetMaps();
        float[,] heights = _landscape.terrainData.GetHeights(0, 0, _width, _height);

        for (int n = 0; n < _iterationsHydraulic; n++)
        {
            const float water = 0.01f;
            const float solubility = 0.01f;
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    //Step 1, simulate rain
                    _waterMap[j, i] += water;

                    //step 2, proportion of height is converted into sediment
                    heights[j, i] -= solubility * _waterMap[j, i];
                    _sedimentMap[j, i] += solubility * _waterMap[j, i];
                }
            }

            //step 3, for each neighbor whose height is less than that of the curren cell, water is moved to that neighbor
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    int leftneighborIndex = 1;
                    int rightneighborIndex = 1;
                    int upneighborIndex = 1;
                    int bottomneighborIndex = 1;

                    if (i == 0) bottomneighborIndex = 0;
                    if (i == _height - 1) upneighborIndex = 0;
                    if (j == 0) leftneighborIndex = 0;
                    if (j == _width - 1) rightneighborIndex = 0;

                    float currentHeight = heights[j, i];
                    float dTotal = 0;

                    float heightLeftNeighbor = heights[j - leftneighborIndex, i];
                    float heightRighttNeighbor = heights[j + rightneighborIndex, i];
                    float heightUpNeighbor = heights[j, i + upneighborIndex];
                    float heightDownNeighbor = heights[j, i - bottomneighborIndex];
                    GetHeightsInvolved(currentHeight, heightLeftNeighbor, heightRighttNeighbor, heightUpNeighbor, heightDownNeighbor
                                       ,ref dTotal);

                    float sum = 0;
                    foreach (float h in heightsInvolved)
                    {
                        sum += h;
                    }
                    float deltaHeight = currentHeight - (sum / heightsInvolved.Count);

                    float deltaWater = 0;
                    if (heightLeftNeighbor < currentHeight)
                    {
                        deltaWater += Mathf.Min(_waterMap[j, i], deltaHeight) * ((heights[j, i] - heights[j - leftneighborIndex, i]) / dTotal);
                        if (deltaWater > _waterMap[j, i])
                        {
                            float w = _waterMap[j, i] / heightsInvolved.Count;
                            _waterMap[j, i] -= w;
                            _waterMap[j - leftneighborIndex, i] += w;
                        }
                        else
                        {
                            if ((heights[j, i] - deltaHeight) > 0)
                            {
                                deltaWater = heights[j, i] - deltaHeight;
                                _waterMap[j, i] -= deltaWater;
                                _waterMap[j - leftneighborIndex, i] += deltaWater;
                            }
                            else
                            {
                                float w = _waterMap[j, i] / heightsInvolved.Count;
                                _waterMap[j, i] -= w;
                                _waterMap[j - leftneighborIndex, i] += w;
                            }
                        }

                        float deltaSediment = _sedimentMap[j, i] * (deltaWater / _waterMap[j, i]);
                        _sedimentMap[j - leftneighborIndex, i] += deltaSediment;
                        _sedimentMap[j, i] -= deltaSediment;
                    }
                    if (heightRighttNeighbor < currentHeight)
                    {
                        deltaWater += Mathf.Min(_waterMap[j, i], deltaHeight) * ((heights[j, i] - heights[j + rightneighborIndex, i]) / dTotal);
                        if (deltaWater > _waterMap[j, i])
                        {
                            float w = _waterMap[j, i];
                            _waterMap[j, i] -= w;
                            _waterMap[j + rightneighborIndex, i] += w;

                        }
                        else
                        {
                            if ((heights[j, i] - deltaHeight) > 0)
                            {
                                deltaWater = heights[j, i] - deltaHeight;
                                _waterMap[j, i] -= deltaWater;
                                _waterMap[j - leftneighborIndex, i] += deltaWater;
                            }
                            else
                            {
                                float w = _waterMap[j, i] / heightsInvolved.Count;
                                _waterMap[j, i] -= w;
                                _waterMap[j - leftneighborIndex, i] += w;
                            }
                        }

                        float deltaSediment = _sedimentMap[j, i] * (deltaWater / _waterMap[j, i]);
                        _sedimentMap[j + rightneighborIndex, i] += deltaSediment;
                        _sedimentMap[j, i] -= deltaSediment;
                    }
                    if (heightUpNeighbor < currentHeight)
                    {
                        deltaWater += Mathf.Min(_waterMap[j, i], deltaHeight) * ((heights[j, i] - heights[j, i + upneighborIndex]) / dTotal);
                        if (deltaWater > _waterMap[j, i])
                        {
                            float w = _waterMap[j, i];
                            _waterMap[j, i] -= w;
                            _waterMap[j, i + upneighborIndex] += w;

                        }
                        else
                        {
                            if ((heights[j, i] - deltaHeight) > 0)
                            {
                                deltaWater = heights[j, i] - deltaHeight;
                                _waterMap[j, i] -= deltaWater;
                                _waterMap[j - leftneighborIndex, i] += deltaWater;
                            }
                            else
                            {
                                float w = _waterMap[j, i] / heightsInvolved.Count;
                                _waterMap[j, i] -= w;
                                _waterMap[j - leftneighborIndex, i] += w;
                            }
                        }
                        float deltaSediment = _sedimentMap[j, i] * (deltaWater / _waterMap[j, i]);
                        _sedimentMap[j, i + upneighborIndex] += deltaSediment;
                        _sedimentMap[j, i] -= deltaSediment;
                    }
                    if (heightDownNeighbor < currentHeight)
                    {
                        deltaWater += Mathf.Min(_waterMap[j, i], deltaHeight) * ((heights[j, i] - heights[j, i - bottomneighborIndex]) / dTotal);
                        if (deltaWater > _waterMap[j, i])
                        {
                            float w = _waterMap[j, i];
                            _waterMap[j, i] -= w;
                            _waterMap[j, i - bottomneighborIndex] += w;

                        }
                        else
                        {
                            if ((heights[j, i] - deltaHeight) > 0)
                            {
                                deltaWater = heights[j, i] - deltaHeight;
                                _waterMap[j, i] -= deltaWater;
                                _waterMap[j - leftneighborIndex, i] += deltaWater;
                            }
                            else
                            {
                                float w = _waterMap[j, i] / heightsInvolved.Count;
                                _waterMap[j, i] -= w;
                                _waterMap[j - leftneighborIndex, i] += w;
                            }
                        }

                        float deltaSediment = _sedimentMap[j, i] * (deltaWater / _waterMap[j, i]);
                        _sedimentMap[j, i - bottomneighborIndex] += deltaSediment;
                        _sedimentMap[j, i] -= deltaSediment;
                    }
                }
            }
        }
        _landscape.terrainData.SetHeights(0, 0, heights);
    }
    public void ExecuteThermalErosion()
    {
        if (_noiseGenerator == null)
            _noiseGenerator = GetComponent<NoiseGenerator>();
        if (!_isDemo)
            _noiseGenerator.RegenerateTerrain();

        _width = _noiseGenerator._width;
        _height = _noiseGenerator._height;

        _s.Start();
        ThermalErosion();
        _s.Stop();

        PrintTime();
    }
    public void ExecuteHudraulicErosion()
    {
        if (_noiseGenerator == null)
            _noiseGenerator = GetComponent<NoiseGenerator>();

        if (!_isDemo)
        {
            _noiseGenerator.RegenerateTerrain();
            ResetMaps();
        }
        else
        {
            if (!_isDemoMapsReset)
            {
                ResetMaps();
                _isDemoMapsReset = true;
            }
        }

        _width = _noiseGenerator._width;
        _height = _noiseGenerator._height;

        _s.Start();
        HydraulicErosion();
        _s.Stop();

         PrintTime();
    }
    public void ExecuteAllErosion()
    {
        if (_noiseGenerator == null)
            _noiseGenerator = GetComponent<NoiseGenerator>();

        //_noiseGenerator.RegenerateTerrain();

        _width = _noiseGenerator._width;
        _height = _noiseGenerator._height;
        _waterMap = new float[_width, _height];
        _sedimentMap = new float[_width, _height];
        _erosionHeights = new float[_width, _height];

        _s.Start();
             ThermalErosion();
             HydraulicErosion();
            _s.Stop();

        PrintTime();
    }
    void GetHeightsInvolved(float currentHeight, float heightLeftNeighbor, float heightRighttNeighbor, float heightUpNeighbor,
                            float heightDownNeighbor, ref float dTotal)
    {
        heightsInvolved.Clear();

        if (heightLeftNeighbor < currentHeight)
        {
            heightsInvolved.Add(heightLeftNeighbor);
            dTotal += currentHeight - heightLeftNeighbor;
        }
        if (heightRighttNeighbor < currentHeight)
        {
            heightsInvolved.Add(heightRighttNeighbor);
            dTotal += currentHeight - heightRighttNeighbor;
        }
        if (heightUpNeighbor < currentHeight)
        {
            heightsInvolved.Add(heightUpNeighbor);
            dTotal += currentHeight - heightUpNeighbor;
        }
        if (heightDownNeighbor < currentHeight)
        {
            heightsInvolved.Add(heightDownNeighbor);
            dTotal += currentHeight - heightDownNeighbor;
        }
    }
    void PrintTime()
    {
        if (!_showTime)
            return;

        TimeSpan ts = _s.Elapsed;
        string elapsedTime = String.Format("{0:00}:{1:00}",
            ts.Seconds,
            ts.Milliseconds);
        UnityEngine.Debug.Log($"It took " + elapsedTime);
        _s.Reset();
    }
    public void ResetMaps()
    {
        _waterMap = new float[_width, _height];
        _sedimentMap = new float[_width, _height];
        _erosionHeights = new float[_width, _height];
    }
}