using System;
using System.Collections.Generic;
using UnityEngine;

public class GridXY<TGridObject>
{
    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = .86f;

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public GridXY(int width, int height, float cellSize, Vector3 originPosition, Func<GridXY<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                //gridArray[x, y] = createGridObject(this, x, y);

                if (y % 2 == 0 && x == width)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
                else if (x < width)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }
        }
    }

    public float GetHeightPerRow() => HEX_VERTICAL_OFFSET_MULTIPLIER * cellSize;

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public void GetFirstCellXY()
    {
        foreach (var gridArray in gridArray)
            Debug.Log(gridArray.ToString());
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, 0, 0) * cellSize + 
            new Vector3(0, y, 0) * cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER +
            ((y % 2) == 1 ? new Vector3(1, 0, 0) * cellSize * .5f : Vector3.zero) + 
            originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.RoundToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.RoundToInt((worldPosition - originPosition).y / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER);
    }

    public List<Vector2Int> GetNeighborXYList(Vector2Int gridPosition)
    {
        bool oddRow = gridPosition.y % 2 == 1;
        List<Vector2Int> neighborList = new List<Vector2Int>
        {
            gridPosition + new Vector2Int(-1, 0), // Left
            gridPosition + new Vector2Int(1, 0), // Right

            gridPosition + new Vector2Int(oddRow ? +1 : -1, +1), // Top Right
            gridPosition + new Vector2Int(0, 1), // Top Left

            gridPosition + new Vector2Int(oddRow ? +1 : -1, -1), // Bottom Right
            gridPosition + new Vector2Int(0, -1) // Bottom Left
        };

        List<Vector2Int> invalidNeighborList = neighborList.FindAll(x => !IsValidGridPositionCustomWidth(x));
        foreach (var invalidNeighbor in invalidNeighborList)
        {
            neighborList.Remove(invalidNeighbor);
        }

        return neighborList;
    }

    public Vector2Int GetClosestNeighborByX(int x, List<Vector2Int> neighbors)
    {
        Vector2Int closestNeighbor = neighbors[0];
        foreach (var neighbor in neighbors)
        {
            if (x - neighbor.x < closestNeighbor.x - x)
                closestNeighbor = neighbor;
        }
        return closestNeighbor;
    }    

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            TriggerGridObjectChanged(x, y);
        }
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public Vector2Int ValidateGridPosition(Vector2Int gridPosition)
    {
        return new Vector2Int(
            Mathf.Clamp(gridPosition.x, 0, width - 1),
            Mathf.Clamp(gridPosition.y, 0, height - 1)
        );
    }       

    public bool IsValidGridPosition(Vector2Int gridPosition)
    {
        int x = gridPosition.x;
        int y = gridPosition.y;

        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsValidGridPositionCustomWidth(Vector2Int gridPosition)
    {
        int x = gridPosition.x;
        int y = gridPosition.y;

        int customWidth = y % 2 == 0 ? width + 1: width;

        if (x >= 0 && y >= 0 && x < customWidth && y < height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsValidGridPositionWithPadding(Vector2Int gridPosition)
    {
        Vector2Int padding = new Vector2Int(2, 2);
        int x = gridPosition.x;
        int y = gridPosition.y;

        if (x >= padding.x && y >= padding.y && x < width - padding.x && y < height - padding.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
