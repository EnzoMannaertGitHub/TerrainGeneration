using UnityEngine;

public class Erosionenerator : MonoBehaviour
{
    private Terrain _landscape = new Terrain();
    private int _width = 256;
    private int _height = 256;
    [SerializeField] private int _iterations = 100;
    [SerializeField] private float _t = .025f;
    [SerializeField] private NoiseGenerator _noiseGenerator;

    public void ThermalErosion()
    {
        float[,] heights = _noiseGenerator.GetHeightValues();
        float[,] tempHeights = heights;
        for (int n = 0; n < _iterations; n++)
        {
            //Distribute to lowest neighbor
            //Using a Von Neuman neighborhood
            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    float currentHeight = heights[j, i];
                    Vector2 lowestIndex = new Vector2(-1, -1);
                    float biggestHeightDifference = 0;

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
                        {
                            lowestIndex = new Vector2(j + 1, i);
                            biggestHeightDifference = rightHeightDiff;
                        }
                    }

                    float upHeightDiff = 0;
                    float downHeightDiff = 0;
                    if (i != _height - 1)
                    {
                        upHeightDiff = currentHeight - heights[j, i + 1]; //up
                        if (upHeightDiff > _t && upHeightDiff >leftHeightDiff && upHeightDiff > leftHeightDiff)
                        {
                            lowestIndex = new Vector2(j, i + 1);
                            biggestHeightDifference = upHeightDiff;
                        }
                    }

                    if (i != 0)
                    {
                        downHeightDiff = currentHeight - heights[j, i - 1]; //down
                        if (downHeightDiff > _t && downHeightDiff > leftHeightDiff && downHeightDiff > leftHeightDiff && downHeightDiff > upHeightDiff)
                        {
                            lowestIndex = new Vector2(j, i - 1);
                            biggestHeightDifference = downHeightDiff;
                        }
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
        _noiseGenerator.Landscape.terrainData.SetHeights(0, 0, heights);
    }
    public void HydraulicErosion()
    {

    }
}