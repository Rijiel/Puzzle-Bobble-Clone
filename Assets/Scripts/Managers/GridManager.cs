using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Bubble;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public Vector3 OriginPosition { get { return originPosition; } }
    public GridXY<GridObject> GridXY { get; private set; }
    public class GridObject { }

    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private GameObject hexagonPrefab;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Vector3 originPosition;
    [SerializeField] private int bubbleCountMax = 21;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 16;
    [SerializeField] private bool showGrid;

    private float cellSize;
    BubblesManager bubbleManager;
    private int addedRowCount;

    private void Awake()
    {
        Instance = this;
        cellSize = hexagonPrefab.transform.localScale.x;
        gridParent.gameObject.SetActive(showGrid);

        PopulateGridCells();
    }

    private void Start()
    {
        bubbleManager = BubblesManager.Instance;
        PopulateGridWithBubbles(bubbleCountMax);
    }

    private void PopulateGridCells()
    {
        // Create a new GridXY object with the specified width, height, cell size, origin position, and grid object factory function
        // Iterate over the grid rows and columns to instantiate hexagon game objects and set text values
        GridXY = new GridXY<GridObject>(width, height, cellSize, originPosition, (GridXY<GridObject> g, int x, int Y) => new GridObject());

        for (int y = 0; y < height; y++)
        {
            // Add an extra cell if the row is even
            int currentRowWidth = y % 2 == 0 ? width + 1 : width;

            for (int x = 0; x < currentRowWidth; x++)
            {
                // Instantiate a hexagon prefab at the calculated position for debugging
                GameObject hexObj = Instantiate(hexagonPrefab, GridXY.GetWorldPosition(x, y), hexagonPrefab.transform.rotation);
                hexObj.transform.SetParent(gridParent);

                // Set the text value of the cell base on its vector2 value
                var text = hexObj.GetComponentInChildren<TextMeshProUGUI>();
                text.text = x.ToString() + ", " + y.ToString();
            }
        }
    }

    private void PopulateGridCellsWithOffset(int offset)
    {
        // Remove existing grid gameobjects
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // Move all rows below by 1 row
        Vector3 newOrigin = new(originPosition.x, originPosition.y - (GridXY.GetHeightPerRow() * offset), originPosition.z);
        GridXY = new GridXY<GridObject>(width, height, cellSize, newOrigin, (GridXY<GridObject> g, int x, int Y) => new GridObject());

        // Update the starting height base on all added rows
        for (int y = offset; y < height; y++)
        {
            int currentRowWidth = y % 2 == 0 ? width + 1 : width;

            for (int x = 0; x < currentRowWidth; x++)
            {
                GameObject hexObj = Instantiate(hexagonPrefab, GridXY.GetWorldPosition(x, y), hexagonPrefab.transform.rotation);
                hexObj.transform.SetParent(gridParent);
                var text = hexObj.GetComponentInChildren<TextMeshProUGUI>();
                text.text = x.ToString() + ", " + y.ToString();
            }
        }
    }

    private void PopulateGridWithBubbles(int count)
    {
        int bubbleCount = 0;
        for (int y = height - 1; y < height && bubbleCount < count; y--)
        {
            int currentRowWidth = y % 2 == 0 ? width + 1 : width;
            for (int x = 0; x < currentRowWidth && bubbleCount < count; x++)
            {
                // Get random bubble type and color sprite
                bubbleManager.GetRandomBubbleTypeAndColorSprite(out BubbleType bubbleType, out Sprite colorSprite);

                // Create a bubble with the generated type and sprite
                Bubble bubble = CreateBubble(bubblePrefab, bubbleType, GridXY.GetWorldPosition(x, y), colorSprite);
                bubble.SetBubbleName(x, y);
                bubbleCount++;

                Vector2Int gridPosition = new Vector2Int(x, y);
                // Check if the grid position is not already occupied, then add the bubble to the dictionary
                if (!bubbleManager.BubbleDictionary.ContainsKey(gridPosition))
                    bubbleManager.BubbleDictionary.Add(gridPosition, bubble.transform);
            }
        }
    }

    // Create new top row of bubbles
    public void GenerateNewRowOfBubbles(int rowOffset)
    {
        float heightPerRow = GridXY.GetHeightPerRow();
        var updatedDictionary = new Dictionary<Vector2Int, Transform>();

        // Move all transforms by 1 row down
        foreach (var key in bubbleManager.BubbleDictionary.Keys)
        {
            Transform value = bubbleManager.BubbleDictionary[key];
            value.position = new Vector2(value.position.x, value.position.y - heightPerRow);
        }

        Physics2D.SyncTransforms();

        // Create new grid with adjusted position
        addedRowCount++;
        height++;
        PopulateGridCellsWithOffset(addedRowCount);

        int bubbleCountPerRowMax = width + (rowOffset == 1 ? 1 : 0);
        PopulateGridWithBubbles(bubbleCountPerRowMax);

        // Get the lowest bubble after moving down and check if it is out of bounds
        Transform lowestBubble = bubbleManager.GetLowestBubbleTransform();
        if (lowestBubble != null && bubbleManager.IsOutOfBounds(lowestBubble))
            GameManager.Instance.GameOver(false);
    }

    public bool IsFloatingGrid(Vector2Int gridPosition)
    {
        // Verify if the grid position is not in the top row
        if (gridPosition.y == height - 1)
            return false;

        // Return true if it has no neighbor
        return GridXY.GetNeighborXYList(gridPosition).
            All(neighbor => !bubbleManager.BubbleDictionary.ContainsKey(neighbor));
    }

    // Get the starting cell value
    public Vector2Int GetBaseCellXY()
    {
        int baseX = 0;
        int baseY = 0 + addedRowCount;
        Vector2Int baseXY = new(baseX, baseY);
        return baseXY;
    }
}
