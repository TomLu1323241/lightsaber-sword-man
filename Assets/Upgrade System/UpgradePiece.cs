using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Pos
{
    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public int x;
    public int y;
}

public enum UpgradeType
{
    ExtraDash,
    IncreaseDashRecovery,
    IncreaseSlowDown,
    ReflectProjectiles,
    GODMODE
}

public enum Direction
{
    Up,
    Right,
    Down,
    Left
}

public class UpgradePiece : MonoBehaviour
{
    public GameObject singleGrid;
    public UpgradeType type;
    public Color32 color;

    [Header("Upgrade Shape")]
    public int up;
    public int right;
    public int down;
    public int left;

    private RectTransform rectTransform;
    private float oneUnit;
    private Vector2 topLeft;
    private singleGrid[,] grid;
    [HideInInspector]
    public Vector2 orignalPos;
    private List<Pos> takenPos;
    private UpgradeSystem upgradeSystem;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        upgradeSystem = GetComponentInParent<UpgradeSystem>();
        oneUnit = upgradeSystem.oneUnit;
        topLeft = upgradeSystem.topLeft;
        grid = upgradeSystem.grid;

        color = new Color32(color.r, color.g, color.b, 200);
        GameObject baseSquare = Instantiate(singleGrid, transform);
        baseSquare.GetComponent<Image>().color = color;
        baseSquare.GetComponent<Image>().raycastTarget = true;
        baseSquare.GetComponent<RectTransform>().sizeDelta = new Vector2(oneUnit, oneUnit);

        buildWithDirection(up, Direction.Up);
        buildWithDirection(right, Direction.Right);
        buildWithDirection(down, Direction.Down);
        buildWithDirection(left, Direction.Left);
    }

    private void buildWithDirection(int ammount, Direction direction)
    {
        for (int i = 1; i < ammount + 1; i++)
        {
            GameObject temp = Instantiate(singleGrid, transform);
            switch (direction)
            {
                case Direction.Up:
                    temp.transform.localPosition = new Vector2(0, oneUnit * i);
                    break;
                case Direction.Right:
                    temp.transform.localPosition = new Vector2(oneUnit * i, 0);
                    break;
                case Direction.Down:
                    temp.transform.localPosition = new Vector2(0, -oneUnit * i);
                    break;
                case Direction.Left:
                    temp.transform.localPosition = new Vector2(-oneUnit * i, 0);
                    break;
            }
            temp.GetComponent<Image>().color = color;
            temp.GetComponent<Image>().raycastTarget = true;
            RectTransform temp2 = temp.GetComponent<RectTransform>();
            temp2.sizeDelta = new Vector2(oneUnit, oneUnit);
        }
    }

    public void dragging(Vector2 movement)
    {
        rectTransform.anchoredPosition += movement;
    }

    public void dragBegin()
    {
        if ((Vector2)transform.localPosition == orignalPos)
        {
            removeUpgrade(false);
        } else
        {
            removeUpgrade(true);
        }
        rectTransform.localScale = Vector3.one;
    }

    public void dragDrop()
    {
        Vector2 snapPos = Vector2.positiveInfinity;
        int x = 0;
        int y = 0;
        for (int i = 0; i < 5; i++)
        {
            if (Mathf.Abs(topLeft.x + i * oneUnit - rectTransform.anchoredPosition.x) < snapPos.x) 
            {
                snapPos.x = Mathf.Abs(topLeft.x + i * oneUnit - rectTransform.anchoredPosition.x);
                x = i;
            }
            if (Mathf.Abs(topLeft.y - i * oneUnit - rectTransform.anchoredPosition.y) < snapPos.y)
            {
                snapPos.y = Mathf.Abs(topLeft.y - i * oneUnit - rectTransform.anchoredPosition.y);
                y = i;
            }
        }
        if (snapPos.x > oneUnit || snapPos.y > oneUnit)
        {
            removeUpgrade(false);
            rectTransform.localScale = Vector3.one * 0.2f;
            transform.localPosition = orignalPos;
            return;
        }
        Vector2 newPos = topLeft;
        newPos.y -= y * oneUnit;
        newPos.x += x * oneUnit;
        if (validFit(x, y))
        {
            // Add upgrade
            rectTransform.anchoredPosition = newPos;
            takenPos = new List<Pos>();
            grid[x, y].filled = true;
            takenPos.Add(new Pos(x, y));
            takeUpGrid(x, y, up, Direction.Up);
            takeUpGrid(x, y, right, Direction.Right);
            takeUpGrid(x, y, down, Direction.Down);
            takeUpGrid(x, y, left, Direction.Left);
            if (upgradeSystem.currentUpgrades.ContainsKey(type)) {
                upgradeSystem.currentUpgrades[type]++;
            } else
            {
                upgradeSystem.currentUpgrades.Add(type, 1);
            }
            upgradeSystem.playerController.UpdateUpgrades(upgradeSystem.currentUpgrades);
        } else
        {
            removeUpgrade(false);
            rectTransform.localScale = Vector3.one * 0.2f;
            transform.localPosition = orignalPos;
            return;
        }
    }

    private void removeUpgrade(bool onBoard)
    {
        if (takenPos != null)
        {
            foreach (Pos pos in takenPos)
            {
                grid[pos.x, pos.y].filled = false;
            }
            takenPos = null;
        }
        if (onBoard)
        {
            upgradeSystem.currentUpgrades[type]--;
        }
        upgradeSystem.playerController.UpdateUpgrades(upgradeSystem.currentUpgrades);
    }

    private void takeUpGrid(int x, int y, int ammount, Direction direction)
    {
        for (int i = 1; i < ammount + 1; i++)
        {
            switch (direction)
            {
                case Direction.Up:
                    grid[x, y - i].filled = true;
                    takenPos.Add(new Pos(x, y - i));
                    break;
                case Direction.Right:
                    grid[x + i, y].filled = true;
                    takenPos.Add(new Pos(x + i, y));
                    break;
                case Direction.Down:
                    grid[x, y + i].filled = true;
                    takenPos.Add(new Pos(x, y + i));
                    break;
                case Direction.Left:
                    grid[x - i, y].filled = true;
                    takenPos.Add(new Pos(x - i, y));
                    break;
            }
        }
    }

    private bool validFit(int x, int y)
    {
        try
        {
            if (checkDirection(x, y, up, Direction.Up) &&
                checkDirection(x, y, right, Direction.Right) &&
                checkDirection(x, y, down, Direction.Down) &&
                checkDirection(x, y, left, Direction.Left) &&
                !grid[x, y].filled)
            {
                return true;
            }
            return false;
        } catch
        {
            return false;
        }
    }

    private bool checkDirection(int x, int y, int ammount, Direction direction)
    {
        for (int i = 1; i < ammount + 1; i++)
        {
            switch (direction)
            {
                case Direction.Up:
                    if(grid[x, y - i].filled)
                    {
                        return false;
                    }
                    break;
                case Direction.Right:
                    if (grid[x + i, y].filled)
                    {
                        return false;
                    }
                    break;
                case Direction.Down:
                    if (grid[x, y + i].filled)
                    {
                        return false;
                    }
                    break;
                case Direction.Left:
                    if (grid[x - i, y].filled)
                    {
                        return false;
                    }
                    break;
            }
        }
        return true;
    }
}
