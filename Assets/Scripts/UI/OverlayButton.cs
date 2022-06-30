using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class OverlayButton : MonoBehaviour
{
    public Text text;
    public int buttonIndex;
    public bool popupButton;
    public bool Highlighted { get; set; }

    void Start()
    {
        Highlighted = false;
    }

    public virtual void TurnRed()
    {

    }

    public virtual void RevertColour()
    {

    }
    
    public virtual void TextClicked()
    {

    }
}
