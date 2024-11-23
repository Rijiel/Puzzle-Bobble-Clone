using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BubbleCollision : MonoBehaviour
{
    public event Action OnPlayerBubbleLanded;

    Bubble bubble;
    bool hasCollided;

    private void Awake()
    {
        bubble = GetComponent<Bubble>();

        if (!TryGetComponent(out Collider2D collider) || !TryGetComponent(out Rigidbody2D rigidbody2D))
            Debug.LogError("MISSING COMPONENTS");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Prevent multiple collision detection
        if (!hasCollided)
        {
            hasCollided = true;
            HandleCollision(collision.transform);
        }
    }

    public void HandleCollision(Transform collidedObject)
    {
        // Check if the collision is with a wall or a bubble
        if (collidedObject != null && !collidedObject.TryGetComponent(out Bubble collidedBubble))
        {
            hasCollided = false;
            Player.Instance.FlipBubble = !Player.Instance.FlipBubble;
        }
        else
        {
            // Place the collided bubble to the closest grid
            Player.Instance.StopPlayerBubble();
            bubble.PlaceToClosestGrid();    

            var bubblesManager = BubblesManager.Instance;

            // Get the grid coordinates
            var gridXY = GridManager.Instance.GridXY;

            // Get a list of neighboring bubbles of the same type
            List<Transform> neighborsByType = bubblesManager.GetClusterByTypeList(transform, bubble.GetBubbleType());
            neighborsByType.Remove(transform);
                        
            int minClusterCount = 2; // Including the player bubble

            // Check if the cluster is big enough
            if (neighborsByType.Count < minClusterCount)
            {
                TriggerOnCollisionEvent();                
                
                return;
            }

            // Remove the current bubble from the dictionary
            var bubbleDictionary = bubblesManager.BubbleDictionary;
            bubbleDictionary.Remove(bubbleDictionary.FirstOrDefault(x => x.Value == transform).Key);

            // Remove neighbor bubbles
            float totalClusterShrinkDuration = 1;
            bubblesManager.RemoveBubbleCluster(neighborsByType, totalClusterShrinkDuration);
                
            // Remove floating bubbles
            List<Transform> floatingBubbles = bubblesManager.GetFloatingBubbles();
            bubblesManager.RemoveBubbleCluster(floatingBubbles, totalClusterShrinkDuration);

            bubble.RemoveBubble(totalClusterShrinkDuration, TriggerOnCollisionEvent);
        }
    }

    private void TriggerOnCollisionEvent()
    {
        OnPlayerBubbleLanded?.Invoke();
        Destroy(this);
    }
}
