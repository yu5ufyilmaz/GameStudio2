using UnityEngine;
using UnityEngine.UI;

public class CardInfoChange : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private Sprite frontSprite;

    [SerializeField]
    private Sprite backSprite;

    private bool isFront = true;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ToggleCardFace()
    {
        animator.SetTrigger("Toggle");
        if (isFront)
        {
            image.sprite = backSprite;
            isFront = false;
        }
        else
        {
            image.sprite = frontSprite;
            isFront = true;
        }
    }
}
