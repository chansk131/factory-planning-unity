using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

public class Field : MonoBehaviour
{
    public GameObject[] go_raw;
    public GameObject field;
    public GameObject[] arrowGroup;
    // public Text[] positionText;
    public Text materialHandlingCostText;
    public Text suggestionText;
    // public Text[] nameText;

    private int[] currentMachine;
    private int[] immediateMachine;
    private double[] outputWeight;
    // private int[] objectCollisionIndex = new int[6];

    private Vector3[] go_points;
    private GameObject[] go_n;
    private LineRenderer[] lineChild;

    private void Start()
    {
        currentMachine = new int[] { 1, 1, 2, 2, 3, 3, 4, 4, 5 };
        immediateMachine = new int[] { 2, 3, 4, 6, 4, 6, 5, 6, 0 };
        outputWeight = new double[] { 0.6, 0.4, 0.5, 0.1, 0.3, 0.1, 0.5, 0.3, 0 };
        lineChild = new LineRenderer[currentMachine.Length - 1];


        for (int i = 0; i < currentMachine.Length - 1; i++)
        {
            lineChild[i] = new GameObject("Fractal Child" + i).AddComponent<LineRenderer>();
            lineChild[i].positionCount = 2;
            lineChild[i].material = new Material(Shader.Find("Mobile/Particles/Additive"));
            lineChild[i].transform.parent = field.transform;
        }
    }

    private void Update()
    {
        GetAllAvailablePoints();
        //CheckCollision();
        Calculation();
        SuggestionArrow();
        DrawLines();
    }

    private void GetAllAvailablePoints()
    {
        // Store all available points on the array
        List<Vector3> vertices3DList = new List<Vector3>();
        List<GameObject> oLlist = new List<GameObject>();

        for (int i = 0; i < go_raw.Length; i++)
        {
            if (go_raw[i] != null)
            {
                vertices3DList.Add(new Vector3(go_raw[i].transform.position.x, go_raw[i].transform.position.y, go_raw[i].transform.position.z));
                oLlist.Add(go_raw[i]);
                //positionText[i].text = "x: " + go_raw[i].transform.position.x + "\n" + "y: " + go_raw[i].transform.position.y + "\n" + "z: " + go_raw[i].transform.position.z + "\n"
                //    + "rotation x:" + go_raw[i].transform.eulerAngles.x + "\n" + "rotation y:" + go_raw[i].transform.eulerAngles.y + "\n" + "rotation z:" + go_raw[i].transform.eulerAngles.z;
                //positionText[i].text = go_raw[i].name;
                arrowGroup[i].transform.position = go_raw[i].transform.position;
                arrowGroup[i].transform.rotation =go_raw[i].transform.rotation;
            }
        }

        go_points = vertices3DList.ToArray();
        go_n = oLlist.ToArray();
    }

    /*private void CheckCollision()
    {
        for (int i = 0; i < nameText.Length; i++)
        {
            if (nameText[i].text == "1")
            {
                objectCollisionIndex[i] = 1;
            }
            else
            {
                objectCollisionIndex[i] = 0;
            }
        }
    }*/

    private void Calculation()
    {
        Results results = new Results
        {
            materialCost = 0f,
            materialDistance = 0f,
            maxMaterialCost = 0f,
            maxMaterialDistance = 0f
        };

        if (go_points.Length == 6)
        {
            results = DistanceCalculation(go_points);
        }

        String initText = "Handling Cost: " + Math.Round(results.materialCost * 100, 1) + "\n"
            + "Total distance: " + Math.Round(results.materialDistance * 100, 1) + "\n";

        materialHandlingCostText.text = initText;
    }

    private void SuggestionArrow()
    {
        if (go_points.Length == 6)
        {
            for (int i = 0; i < go_points.Length; i++)
            {
                List<float> materialDistanceList = new List<float>();
                List<float> materialCostList = new List<float>();

                int suggestionFlag = -1;
                for (int direction = 0; direction < 4; direction++)
                {
                    materialDistanceList.Add(DistanceCalculation(go_points, i, direction).materialDistance);
                    materialCostList.Add(DistanceCalculation(go_points, i, direction).materialCost);
                }

                suggestionFlag = materialCostList.IndexOf(materialCostList.Min());

                if (suggestionFlag > -1)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        if (direction == suggestionFlag)
                        {
                            arrowGroup[i].gameObject.transform.GetChild(direction).gameObject.SetActive(true);
                        }
                        else
                        {
                            arrowGroup[i].gameObject.transform.GetChild(direction).gameObject.SetActive(false);
                        }
                    }
                }
                // positionText[i].text = "suggestionFlag: " + suggestionFlag;
            }
        }
    }

    private void DrawLines()
    {

        if (go_points.Length == 6)
        {
            for (int i = 0; i < currentMachine.Length - 1; i++)
            {
                //lineChild[i].material.color = Color.Lerp(Color.white, Color.red, (float)outputWeight[i]);
                lineChild[i].startColor = Color.Lerp(Color.white, Color.red, (float)(outputWeight[i] / outputWeight.Max()));
                lineChild[i].endColor = Color.Lerp(Color.white, Color.red, (float)outputWeight[i]);
                lineChild[i].startWidth = (float)(outputWeight[i] / 100);
                lineChild[i].SetPosition(0, new Vector3(go_n[currentMachine[i] - 1].transform.position.x, go_n[currentMachine[i] - 1].transform.position.y, go_n[currentMachine[i] - 1].transform.position.z));
                lineChild[i].SetPosition(1, new Vector3(go_n[immediateMachine[i] - 1].transform.position.x, go_n[immediateMachine[i] - 1].transform.position.y, go_n[immediateMachine[i] - 1].transform.position.z));

            }
        }
    }

    private Results DistanceCalculation(Vector3[] positions, int item = 0, int direction = 4)
    {
        Results results = new Results
        {
            materialCost = 0f,
            materialDistance = 0f,
            maxMaterialCost = 0f,
            maxMaterialDistance = 0f
        };

        float distanceEach = 0f;
        float costEach = 0f;

        // moving points arround (gameMove = 24 when no move)
        if (direction != 4)
        {
            switch (direction)
            {
                case 0:
                    positions[item].x = positions[item].x + 0.001f;
                    break;
                case 1:
                    positions[item].x = positions[item].x - 0.001f;
                    break;
                case 2:
                    positions[item].z = positions[item].z + 0.001f;
                    break;
                case 3:
                    positions[item].z = positions[item].z - 0.001f;
                    break;
            }
        }



        // calculate materialCost and materialDistance
        for (int i = 0; i < currentMachine.Length - 1; i++)
        {
            distanceEach = Math.Abs(positions[currentMachine[i] - 1].x - positions[immediateMachine[i] - 1].x) + Math.Abs(positions[currentMachine[i] - 1].z - positions[immediateMachine[i] - 1].z);
            results.materialDistance = results.materialDistance + distanceEach;
            costEach = distanceEach * (float)outputWeight[i];
            results.materialCost = results.materialCost + costEach;

            // store maxMaterialDistance
            if (results.maxMaterialDistance == 0f)
            {
                results.maxMaterialDistance = distanceEach;
            }
            else if (distanceEach < results.maxMaterialDistance)
            {
                results.maxMaterialDistance = distanceEach;
            }

            // store maxMaterialCost
            if (results.maxMaterialCost == 0f)
            {
                results.maxMaterialCost = costEach;
            }
            else if (costEach < results.maxMaterialCost)
            {
                results.maxMaterialCost = costEach;
            }

        }

        return results;
    }
}

public struct Results
{
    public float materialDistance;
    public float materialCost;
    public float maxMaterialDistance;
    public float maxMaterialCost;
}