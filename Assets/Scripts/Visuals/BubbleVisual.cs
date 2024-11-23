using System;
using System.Collections;
using UnityEngine;

public class BubbleVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetColorSprite(Sprite colorSprite)
    {
        spriteRenderer.sprite = colorSprite;
    }

    public void ShrinkBubble(GameObject bubbleGO, float totalBubblesDuration, Action onComplete)
    {
        StartCoroutine(ShrinkBubbleRoutine(bubbleGO, totalBubblesDuration, onComplete));
    }

    private IEnumerator ShrinkBubbleRoutine(GameObject bubbleGO, float totalBubblesDuration, Action onComplete)
    {
        // Ensure that the duration will not exceed the total duration of all bubbles
        float durationMax = totalBubblesDuration * 0.5f; 
        float duration = 0f;

        // Randomize the start delay
        float startDelay = UnityEngine.Random.Range(0, durationMax);
        yield return new WaitForSeconds(startDelay);

        while (duration < durationMax)
        {
            float progress = duration / durationMax;
            bubbleGO.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, progress);

            duration += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        onComplete?.Invoke();
    }
}
