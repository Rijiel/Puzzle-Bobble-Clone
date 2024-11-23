using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private float rotationPower = 10f;
    [SerializeField] private Transform rotator;

    public GameObject nextShotColorIndicator;

    float angle;

    private void Update()
    {
        HandleRotation();
    }

    private void HandleRotation()
    {
        float inputFloat = GameInput.Instance.GetInputFloat();
        angle += -1 * (inputFloat * rotationPower * Time.deltaTime);

        // Clamp rotation angle
        float angleDegreeMax = 60f;
        angle = Mathf.Clamp(angle, -angleDegreeMax, angleDegreeMax);

        rotator.rotation = Quaternion.Euler(0, 0, angle);
    }

    public Vector2 GetCurrentDirection()
    {
        return rotator.up;
    }

    public void SetNextShotColorIndicator(Sprite colorSprite)
    {
        nextShotColorIndicator.GetComponent<SpriteRenderer>().sprite = colorSprite;
    }
}
