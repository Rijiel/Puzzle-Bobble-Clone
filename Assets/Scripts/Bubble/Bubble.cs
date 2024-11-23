using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private BubbleVisual bubbleVisual;
    
    public enum BubbleType
    {
        Brown,
        Gray,
        Yellow,
        Red,
        Green,
        Blue
    }

    public BubbleType bubbleType;

    public BubbleType GetBubbleType() => bubbleType;

    public BubbleVisual GetBubbleVisual() => bubbleVisual;

    public void SetBubbleName(int x, int y)
    {
        transform.name = "Bubble(" + x.ToString() + ", " + y.ToString() + ")";
    }

    public void MoveBubble(float shootingPower, Vector2 direction)
    {
        Rigidbody2D bubbleRb = GetComponent<Rigidbody2D>();
        bubbleRb.velocity = direction * shootingPower;
    }
    
    // Multiply current direction X by -1
    public void ToggleFlipDirection(float shootingPower, Vector2 originDirection)
    {
        Rigidbody2D bubbleRb = GetComponent<Rigidbody2D>();
        Vector2 flippedDirection = new(-originDirection.x, originDirection.y);
        bubbleRb.velocity = flippedDirection * shootingPower;
    }

    public void PlaceToClosestGrid()
    {
        // Get the grid position
        var bubblesManager = BubblesManager.Instance;
        var gridXY = GridManager.Instance.GridXY;
        gridXY.GetXY(transform.position, out int x, out int y);
        Vector2Int gridPositionXY = new(x, y);
        
        // If not a valid grid position, find the closest empty and valid cell
        if (!IsValidBubblePosiion(gridPositionXY))
        {
            List<Vector2Int> neighborXYList = gridXY.GetNeighborXYList(gridPositionXY);

            // Reorder list by distance in x
            neighborXYList.Sort((a, b) => Math.Abs(x - a.x).CompareTo(Math.Abs(x - b.x)));

            foreach (var neighborXY in neighborXYList)
            {
                if (IsValidBubblePosiion(neighborXY))
                {
                    gridPositionXY = neighborXY;
                    break;
                }
            }
        }
        transform.position = gridXY.GetWorldPosition(gridPositionXY.x, gridPositionXY.y);
        
        // Update the dictionary
        if (!bubblesManager.BubbleDictionary.ContainsKey(gridPositionXY))
            bubblesManager.BubbleDictionary.Add(gridPositionXY, transform);
    }

    // Return true if the grid position is in range, if empty, and if it is not floating
    private bool IsValidBubblePosiion(Vector2Int gridPositionXY)
    {
        var bubblesManager = BubblesManager.Instance;
        var gridXY = GridManager.Instance.GridXY;

        if (!gridXY.IsValidGridPositionCustomWidth(gridPositionXY) ||
            bubblesManager.BubbleDictionary.ContainsKey(gridPositionXY) ||
            GridManager.Instance.IsFloatingGrid(gridPositionXY))
        {
            return false;
        }
        return true;
    }

    public void StopBubble()
    {
        Rigidbody2D bubbleRb = GetComponent<Rigidbody2D>();
        bubbleRb.velocity = Vector2.zero;
    }

    public static Bubble CreateBubble(GameObject bubblePrefab, BubbleType bubbleType, Vector2 position, Sprite colorSprite)
    {
        Transform bubbleTransform = Instantiate(bubblePrefab).transform;
        Bubble bubble = bubbleTransform.GetComponent<Bubble>();
        bubble.bubbleType = bubbleType;
        bubbleTransform.position = position;

        bubble.bubbleVisual.SetColorSprite(colorSprite);
        return bubble;
    }

    public void RemoveBubble(float totalDuration, Action onComplete)
    {
        StartCoroutine(RemoveBubbleRoutine(totalDuration, onComplete));
    }

    private IEnumerator RemoveBubbleRoutine(float totalDuration, Action onComplete)
    {
        GetBubbleVisual().ShrinkBubble(gameObject, totalDuration, null);

        // Add a small delay to ensure the bubble is fully shrunk
        yield return new WaitForSeconds(totalDuration + .2f); 
        onComplete?.Invoke();
        Destroy(gameObject);
    }

    public bool DidHitTopRow()
    {
        int topRowY = GridManager.Instance.GridXY.GetHeight() - 1;

        GridManager.Instance.GridXY.GetXY(transform.position, out int x, out int y);
        Vector2Int currentGridPosition = new(x, y);

        if (currentGridPosition.y >= topRowY)
            return true;

        return false;
    }
}


