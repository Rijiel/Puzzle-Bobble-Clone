using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static Bubble;

public class BubblesManager : MonoBehaviour
{
    // Keys for bubble dictionary
    private const string COLOR_BROWN = "brown";
    private const string COLOR_GRAY = "gray";
    private const string COLOR_YELLOW = "yellow";
    private const string COLOR_RED = "red";
    private const string COLOR_GREEN = "green";
    private const string COLOR_BLUE = "blue";

    [Serializable]
    public struct ColorAndType
    {
        public BubbleType bubbleType;
        public Sprite sprite;
        public string colorName;
    }

    [SerializeField] private List<ColorAndType> colorSpriteAndTypeList;

    private GridManager gridManager;

    public static BubblesManager Instance { get; private set; }    
    public Dictionary<string, BubbleType> colorDictionary = new();    
    public Dictionary<Vector2Int, Transform> BubbleDictionary { get; set; } = new();
        
    private void Awake()
    {
        Instance = this;
        
        colorDictionary.Add(COLOR_BROWN, BubbleType.Brown);
        colorDictionary.Add(COLOR_GRAY, BubbleType.Gray);
        colorDictionary.Add(COLOR_YELLOW, BubbleType.Yellow);      
        colorDictionary.Add(COLOR_RED, BubbleType.Red);      
        colorDictionary.Add(COLOR_GREEN, BubbleType.Green);      
        colorDictionary.Add(COLOR_BLUE, BubbleType.Blue);
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
    }

    private void OnEnable()
    {
        Player.Instance.OnPlayerBubbleShot += Player_OnPlayerBubbleShot;
    }

    private void Player_OnPlayerBubbleShot()
    {
        // Subscribe to instantiated bubble collision
        BubbleCollision bubbleCollision = Player.Instance.GetPlayerBubble().GetComponent<BubbleCollision>();
        bubbleCollision.OnPlayerBubbleLanded += BubbleCollision_OnPlayerBubbleLanded;
    }

    private void Update()
    {
        // For debugging purposes
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            foreach (var entry in BubbleDictionary)            
                Debug.Log(entry.Value.name + " in " + entry.Key);            
        }
    }

    private void BubbleCollision_OnPlayerBubbleLanded()
    {
        // Lowest bubble will not be checked on if it's part of explosion
        // since it was removed in the dictionary before this event
        Transform lowestBubble = GetLowestBubbleTransform();
        if (lowestBubble != null && IsOutOfBounds(lowestBubble))
        {            
            GameManager.Instance.GameOver(false);
        }
        // Count remaining bubbles
        else
        {
            int remainingBubbles = BubbleDictionary.Count;
            if (remainingBubbles == 0)
                GameManager.Instance.GameOver(true);
        }
    }    

    public void GetRandomBubbleTypeAndColorSprite(out BubbleType bubbleType, out Sprite sprite)
    {
        int randomInt = UnityEngine.Random.Range(0, colorSpriteAndTypeList.Count);
        bubbleType = colorDictionary[colorSpriteAndTypeList[randomInt].colorName.ToLower()];
        sprite = colorSpriteAndTypeList[randomInt].sprite;
    }

    public Sprite GetColorSpriteFromType(BubbleType bubbleType)
    {
        // Find sprite in the dictionary using the type value
        foreach (var colorSpriteAndType in colorSpriteAndTypeList)
        {
            if (colorSpriteAndType.bubbleType == bubbleType)
                return colorSpriteAndType.sprite;
        }
        return null;
    }

    public List<Transform> GetNeighborsTransformList(Transform startTransform)
    {
        // Get the grid position of the transform
        Vector2Int gridPosition = BubbleDictionary.FirstOrDefault(x => x.Value == startTransform).Key;
        if (gridPosition == null)
        {
            Debug.LogError("Not found in list.");
            return null;
        }

        // Get the XY in 4 directions and check if it contains a bubble
        List<Vector2Int> neighborsXY = gridManager.GridXY.GetNeighborXYList(gridPosition);
        List<Transform> neighborTransforms = neighborsXY.Select(x => BubbleDictionary.FirstOrDefault(y => y.Key == x).Value).ToList();

        return neighborTransforms;
    }

    public List<Transform> GetNeighborsByTypeList(Transform startTransform, BubbleType bubbleType)
    {
        // Get the grid position of the transform
        Vector2Int gridPosition = BubbleDictionary.FirstOrDefault(x => x.Value == startTransform).Key;
        if (gridPosition == null)
        {
            Debug.LogError("Not found in list.");
            return null;
        }

        // Get the XY in 4 directions and check if it contains a bubble with the same type
        List<Vector2Int> neighborsXY = gridManager.GridXY.GetNeighborXYList(gridPosition);

        // Remove all neighbors that are not of the same type
        List<Transform> neighborTransforms = neighborsXY.Where(x => BubbleDictionary.ContainsKey(x)).Select(x => BubbleDictionary[x]).ToList();
        List<Transform> neighborsWithSameType = neighborTransforms.Where(x => x.GetComponent<Bubble>().bubbleType == bubbleType).ToList();

        return neighborsWithSameType;
    }

    public List<Transform> GetClusterByTypeList(Transform startTransform, BubbleType bubbleType)
    {
        List<Transform> allNeighbors = new();
        Queue<Transform> queue = new();
        HashSet<Transform> visited = new();

        // Enqueue the starting transform
        queue.Enqueue(startTransform);
        visited.Add(startTransform);

        // Check if the queue is not empty
        while (queue.Count > 0)
        {
            // Get the next transform in queue
            Transform current = queue.Dequeue();
            allNeighbors.Add(current);

            // Get the neighbors of the same type and enqueue each if it has not been visited
            List<Transform> neighbors = GetNeighborsByTypeList(current, bubbleType);
            foreach (Transform neighbor in neighbors)
            {                
                if (!visited.Contains(neighbor) && neighbor != startTransform)
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        return allNeighbors;
    }

    public List<Transform> GetFloatingBubbles()
    {
        // Get all the bubbles in the dictionary
        List<Transform> allBubbles = BubbleDictionary.Values.ToList();

        List<Transform> nonTopRowBubbles = new();
        // Filter out the bubbles that are not in the top row
        foreach (var bubble in allBubbles)
        {
            Vector2Int gridPosition = BubbleDictionary.FirstOrDefault(x => x.Value == bubble).Key;
            if (gridPosition.y < gridManager.GridXY.GetHeight() - 1)
                nonTopRowBubbles.Add(bubble);
        }

        List<Transform> floatingBubbles = new();
        // Check if the bubble is connected to the top row
        foreach (var nonTopRowBubble in nonTopRowBubbles)
        {
            if (!IsConnectedToTopRowRecursive(nonTopRowBubble))
                floatingBubbles.Add(nonTopRowBubble);
        }
        return floatingBubbles;
    }

    private bool IsConnectedToTopRowRecursive(Transform bubble, int depth = 0, int depthMax = 10)
    {
        // Prevent stack overflow
        if (depth >= depthMax)
            return false;

        // Check if the bubble is in the top row
        Vector2Int gridPosition = BubbleDictionary.FirstOrDefault(x => x.Value == bubble).Key;
        if (gridPosition.y == gridManager.GridXY.GetHeight() - 1)
        {
            return true;
        }
        // Get all the neighbors and add in queue to flood fill the list
        foreach (Transform neighbor in GetNeighborsTransformList(bubble))
        {
            if (IsConnectedToTopRowRecursive(neighbor, depth + 1))
                return true;
        }

        return false;
    }

    // Check if the bubble is within the row of out of bounds
    public bool IsOutOfBounds(Transform transform)
    {
        Vector2Int gridPosition = BubbleDictionary.FirstOrDefault(x => x.Value == transform).Key;

        if (transform == null || gridPosition == null) return false;

        int startingCellY = GridManager.Instance.GetBaseCellXY().y;
        int rowNumber = 3;
        int outOfBoundsRow = startingCellY + rowNumber; // 3rd row

        return gridPosition.y <= outOfBoundsRow;
    }

    public Transform GetLowestBubbleTransform()
    {
        Transform lowestBubble = BubbleDictionary.OrderBy(t => t.Key.y).FirstOrDefault().Value;

        return lowestBubble;
    }

    // TODO: Object pooling
    public void RemoveBubbleCluster(List<Transform> cluster, float totalDuration)
    {
        // Shuffle the list for additional randomness
        cluster = cluster.OrderBy(x => Guid.NewGuid()).ToList();

        foreach (var bubble in cluster)
        {
            BubbleDictionary.Remove(BubbleDictionary.FirstOrDefault(x => x.Value == bubble).Key);
            BubbleVisual bubbleVisual = bubble.GetComponent<Bubble>().GetBubbleVisual();
            bubbleVisual.ShrinkBubble(bubble.gameObject, totalDuration, () => Destroy(bubble.gameObject));
        }
    }
}
