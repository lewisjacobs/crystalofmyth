using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CalibrationMenu : MonoBehaviour
{
    public float scaleFactor;
    private Canvas titleCanvas;

    void Start()
    {
        GestureController.Instance.BeginProcessing = true;
        GestureController.Instance.SuccessfullGestureteCallback += SuccessfullGestureteCallback;

        titleCanvas = GameObject.Find("Title Canvas").GetComponent<Canvas>();

        float wFactor = ((100f / 1920f) * Screen.width) / 100f;
        float hFactor = ((100f / 1080f) * Screen.height) / 100f;
        if (wFactor < hFactor) scaleFactor = hFactor;
        else scaleFactor = wFactor;

        titleCanvas.scaleFactor = scaleFactor;
    }

    public void SuccessfullGestureteCallback( Type gestureType )
    {
        if ( gestureType == typeof( TPoseGesture ) )
        {
            GestureController.Instance.SuccessfullGestureteCallback -= SuccessfullGestureteCallback;
            GameController.Instance.LoadScene( Scene.Lobby );
        }
    }
}
