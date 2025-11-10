using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    [Header("Hover Effect")]
    [Range(0f, 1f)] public float hoverDarkenAmount = 0.8f;

    private Image image;
    private Color originalColor;
    private bool isHovered = false;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
            if (normalSprite == null)
                normalSprite = image.sprite;
        }
    }

    void OnEnable()
    {
        // Reset ketika tombol diaktifkan kembali
        if (image != null)
        {
            image.color = originalColor;
            image.sprite = normalSprite;
            isHovered = false;
        }
    }

    void OnDisable()
    {
        // Reset juga ketika tombol dinonaktifkan agar tidak menyimpan kondisi hover
        if (image != null)
        {
            image.color = originalColor;
            image.sprite = normalSprite;
            isHovered = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        Darken(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        Darken(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pressedSprite != null)
            image.sprite = pressedSprite;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (normalSprite != null)
            image.sprite = normalSprite;

        // kalau masih hover setelah dilepas
        if (isHovered)
            Darken(true);
        else
            Darken(false);
    }

    private void Darken(bool enable)
    {
        if (enable)
            image.color = originalColor * hoverDarkenAmount;
        else
            image.color = originalColor;
    }
}
