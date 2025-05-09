using UnityEngine;
using UnityEngine.UI;

public class SinMenu : MonoBehaviour
{
    [SerializeField]
    private bool isFront;

    [SerializeField]
    private bool isOpen;

    public void ShowCard(GameObject selectedButton)
    {
        Animator anim = selectedButton.GetComponent<Animator>();
        if (isOpen == false && isFront == true)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject != selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Disabled");
                    }
                }
                else
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Show");
                        anim.SetBool("isOpen", true);
                        isOpen = anim.GetBool("isOpen");
                    }
                }
            }
        }
        else if (isOpen == true && isFront == true)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject == selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();

                    if (animator != null)
                    {
                        animator.SetTrigger("Turn");
                        anim.SetBool("isFront", false);
                        isFront = anim.GetBool("isFront");
                    }
                }
            }
        }
        else if (isOpen == true && isFront == false)
        {
            foreach (Transform child in transform)
            {
                if (child.gameObject != selectedButton)
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Normal");
                        animator.ResetTrigger("Disabled");
                    }
                }
                else
                {
                    Animator animator = child.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger("Normal");
                        anim.SetBool("isOpen", false);
                        anim.SetBool("isFront", true);
                        isFront = anim.GetBool("isFront");
                        isOpen = anim.GetBool("isOpen");
                    }
                }
            }
        }
    }
}
