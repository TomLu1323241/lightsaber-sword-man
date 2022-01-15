using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct singleGrid
{
    public singleGrid(GameObject square, bool filled)
    {
        this.square = square;
        this.filled = filled;
    }
    public GameObject square;
    public bool filled;
}

public class UpgradeSystem : MonoBehaviour
{

    public GameObject square;
    public float margin = 0.95f;
    public List<GameObject> upgrades;
    [HideInInspector]
    public float oneUnit;
    [HideInInspector]
    public Vector2 topLeft;

    // Like 90% sure
    // [row, col]
    // [0, 0] * * * [0, 4]
    //    *   * * *    *
    //    *   * * *    *
    //    *   * * *    *
    // [4, 0] * * * [4, $]
    [HideInInspector]
    public singleGrid[,] grid = new singleGrid[5,5];
    [HideInInspector]
    public Dictionary<UpgradeType, int> currentUpgrades;

    private List<Vector2> availableSlots = new List<Vector2>();

    [HideInInspector]
    public PlayerController playerController;

    private readonly byte ALPHA = 90;
    void Start()
    {
        currentUpgrades = new Dictionary<UpgradeType, int>();
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            currentUpgrades.Add(type, 0);
        }
        playerController = GetComponentInParent<PlayerController>();

        RectTransform canvasRect = GetComponent<RectTransform>();
        oneUnit = canvasRect.rect.height / 5;
        topLeft = new Vector2(-oneUnit * 2, oneUnit * 2);
        LayoutGrid();
        CalculateAvailableSlots();
    }

    private void CalculateAvailableSlots()
    {
        RectTransform canvasRect = GetComponent<RectTransform>();
        float sideWidth = (canvasRect.rect.width - canvasRect.rect.height) / 2;
        float startingWidth = -sideWidth / 2f - oneUnit * 2.5f;
        float startingHeight = canvasRect.rect.height / 3;
        availableSlots.Add(new Vector2(startingWidth, startingHeight));
        availableSlots.Add(new Vector2(startingWidth, startingHeight - canvasRect.rect.height / 3f));
        availableSlots.Add(new Vector2(startingWidth, startingHeight - canvasRect.rect.height / 3f * 2));
        startingWidth = sideWidth / 2f + oneUnit * 2.5f;
        availableSlots.Add(new Vector2(startingWidth, startingHeight));
        availableSlots.Add(new Vector2(startingWidth, startingHeight - canvasRect.rect.height / 3f));
        availableSlots.Add(new Vector2(startingWidth, startingHeight - canvasRect.rect.height / 3f * 2));
        for (int i = 0; i < upgrades.Count; i++)
        {
            GameObject temp = Instantiate(upgrades[i], transform);
            temp.transform.localPosition = availableSlots[i];
            temp.GetComponent<RectTransform>().localScale = Vector3.one * 0.2f;
            temp.GetComponent<UpgradePiece>().orignalPos = availableSlots[i];
        }
        //foreach (Vector2 pos in availableSlots)
        //{
        //    GameObject temp = Instantiate(square, transform);
        //    temp.transform.localPosition = pos;
        //    RectTransform temp2 = temp.GetComponent<RectTransform>();
        //    temp2.sizeDelta = new Vector2(oneUnit * margin, oneUnit * margin);
        //}
    }

    private void LayoutGrid()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameObject temp = Instantiate(square, transform);
                temp.transform.localPosition = new Vector2(topLeft.x + i * oneUnit, topLeft.y - j * oneUnit);
                RectTransform temp2 = temp.GetComponent<RectTransform>();
                temp2.sizeDelta = new Vector2(oneUnit * margin, oneUnit * margin);
                grid[j,i] = new singleGrid(temp, false);
            }
        }
        grid[1, 1].square.GetComponent<Image>().color = new Color32(90, 90, 90, ALPHA);
        grid[1, 1].filled = true;

        grid[1, 3].square.GetComponent<Image>().color = new Color32(90, 90, 90, ALPHA);
        grid[1, 3].filled = true;

        grid[3, 1].square.GetComponent<Image>().color = new Color32(90, 90, 90, ALPHA);
        grid[3, 1].filled = true;

        grid[3, 3].square.GetComponent<Image>().color = new Color32(90, 90, 90, ALPHA);
        grid[3, 3].filled = true;
    }


    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        for (int i = 0;i < 5; i++)
    //        {
    //            Debug.Log($"{grid[i, 0].filled} : {grid[i, 1].filled} : {grid[i, 2].filled} : {grid[i, 3].filled} : {grid[i, 4].filled}");
    //        }
    //        Debug.Log($"----------------------------------");
    //        for (int i = 0; i < currentUpgrades.Count; i++)
    //        {
    //            Debug.Log($"{currentUpgrades[i]}");
    //        }
    //    }
    //}
}
