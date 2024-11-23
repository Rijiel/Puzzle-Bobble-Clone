using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Bubble;

public class Player : MonoBehaviour
{
    public event Action OnPlayerBubbleShot;

    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private GameObject bubblePrefab;    
    [SerializeField] private float shootingPower;
    [SerializeField] private SpriteRenderer[] bubbleQueueSprites;
        
    private Bubble playerBubble;
    private Sprite playerBubbleColorSprite;
    private BubbleType playerBubbleType;
    private Vector2 currentDirection;
    private Vector2 shotDirection;
    private Transform shootingPoint;
    private bool directionSet;
    private bool canShoot = true;
    private bool stopBubble;    
    private int shotCount;
    private int totalShotCount;
    private int rowOffset = 3;

    // Bubbles queue
    private BubbleType[] queuedBubbleTypes;
    private Sprite[] queuedBubbleSprites;

    public static Player Instance { get; private set; }
    public bool FlipBubble { get; set; }

    public void StopPlayerBubble() => stopBubble = true;

    public bool IsPlayerBubbleActive() => playerBubble != null;

    public Bubble GetPlayerBubble() => playerBubble;

    private void Awake()
    {
        Instance = this;
}

    private void Start()
    {
        shootingPoint = playerVisual.nextShotColorIndicator.transform;

        // Initialize player bubbles
        CreateNewPlayerBubbles(true);
    }

    private void OnEnable()
    {
        GameInput.Instance.OnClickAction += GameInput_OnClick;
        OnPlayerBubbleShot += Player_OnPlayerBubbleShot;
    }

    private void Player_OnPlayerBubbleShot()
    {
        // Subscribe to instantiated bubble collision
        BubbleCollision bubbleCollision = playerBubble.GetComponent<BubbleCollision>();
        bubbleCollision.OnPlayerBubbleLanded += BubbleCollision_OnPlayerBubbleLanded;
    }

    private void BubbleCollision_OnPlayerBubbleLanded()
    {
        if (playerBubble == null)
        {
            Debug.LogError("PLAYER BUBBLE IS NULL");
            return;
        }

        SetPlayerBubbleInactive();

        // Create new top row after reaching shot count
        shotCount++;
        int shotCountBeforeNewRow = 15;
        if (shotCount == shotCountBeforeNewRow)
        {
            shotCount = 0;
            GridManager.Instance.GenerateNewRowOfBubbles(rowOffset);
            rowOffset = rowOffset == 2 ? 1 : 2;
        }
    }

    private void FixedUpdate()
    {
        if (IsPlayerBubbleActive())
        {
            if (stopBubble) return;
                        
            if (!playerBubble.DidHitTopRow())
            {
                // Prevent shot bubble from also turning when turning the rotator
                if (!directionSet)
                {
                    directionSet = true;
                    currentDirection = playerVisual.GetCurrentDirection();
                    shotDirection = currentDirection;
                }

                // Flip current direction when hitting wall
                if (!FlipBubble)
                    playerBubble.MoveBubble(shootingPower, currentDirection);
                else
                    playerBubble.ToggleFlipDirection(shootingPower, currentDirection);
            }
            // If current Y position is the max height, place to nearest cell
            else
            {
                BubbleCollision bubbleCollision = playerBubble.GetComponent<BubbleCollision>();
                bubbleCollision.HandleCollision(null);
            }
        }
    }

    // Shoot a bubble from shooting point when clicking or pressing a key
    private void GameInput_OnClick()
    {
        if (canShoot && GameManager.Instance.IsGamePlaying())
        {
            canShoot = false;
            ShootPlayerBubble();
            CreateNewPlayerBubbles(false);

            OnPlayerBubbleShot?.Invoke();
        }
    }

    // Create new player bubble from array queue
    private void ShootPlayerBubble()
    {
        totalShotCount++;
        playerBubble = CreateBubble(bubblePrefab, playerBubbleType, shootingPoint.position, playerBubbleColorSprite);
        playerBubble.transform.name = "PlayerBubble " + totalShotCount;
        playerBubble.gameObject.AddComponent<BubbleCollision>();    
    }

    // Create new array for bubbles in queue
    private void CreateNewPlayerBubbles(bool isFirstInitialization)
    {
        if (isFirstInitialization)
        {
            int ArrayLength = bubbleQueueSprites.Length + 1; // Including next shot

            queuedBubbleTypes = new BubbleType[ArrayLength];
            queuedBubbleSprites = new Sprite[ArrayLength];

            for (int i = 0; i < ArrayLength; i++)
            {
                BubblesManager.Instance.GetRandomBubbleTypeAndColorSprite(out BubbleType bubbleType, out Sprite sprite);
                queuedBubbleTypes[i] = bubbleType;
                queuedBubbleSprites[i] = sprite;
            }
        }
        else
        {
            // New array holder for updated queue
            BubbleType[] shiftedQueuedBubbleTypes = new BubbleType[queuedBubbleTypes.Length - 1];
            Sprite[] shiftedQueuedBubbleSprites = new Sprite[queuedBubbleSprites.Length - 1];

            // Shift array elements to the left by 1 and remove the last element
            Array.Copy(queuedBubbleTypes, 1, shiftedQueuedBubbleTypes, 0, queuedBubbleTypes.Length - 1);
            Array.Copy(queuedBubbleSprites, 1, shiftedQueuedBubbleSprites, 0, queuedBubbleSprites.Length - 1);

            queuedBubbleTypes = shiftedQueuedBubbleTypes;
            queuedBubbleSprites = shiftedQueuedBubbleSprites;        

            BubblesManager.Instance.GetRandomBubbleTypeAndColorSprite(out BubbleType bubbleType, out Sprite sprite);

            // Add an empty element to the end of the array for the deleted element
            Array.Resize(ref queuedBubbleTypes, queuedBubbleTypes.Length + 1);
            Array.Resize(ref queuedBubbleSprites, queuedBubbleSprites.Length + 1);

            // Update the last element with the generated random type
            queuedBubbleTypes[queuedBubbleTypes.Length - 1] = bubbleType;
            queuedBubbleSprites[queuedBubbleSprites.Length - 1] = sprite;
        }

        // Update queued bubble types and sprites
        for (int i = 0; i < bubbleQueueSprites.Length; i++)
        {
            bubbleQueueSprites[i].sprite = queuedBubbleSprites[i + 1];
        }

        // Update player next shot indicator sprite
        playerBubbleType = queuedBubbleTypes[0];
        Sprite nextShotSprite = queuedBubbleSprites[0];
        playerBubbleColorSprite = nextShotSprite;
        
        playerVisual.SetNextShotColorIndicator(nextShotSprite);
    }

    private void SetPlayerBubbleInactive()
    {
        playerBubble.StopBubble();
        directionSet = false;
        stopBubble = false;
        playerBubble = null;
        canShoot = true;
        FlipBubble = false;
    }
}
