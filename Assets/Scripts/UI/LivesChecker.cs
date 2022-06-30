using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LivesChecker : MonoBehaviour
{
    public void CallLivesOnCharacterSelectionController()
    {
        CharacterSelectionController.Instance.SetNumLives();
    }
}
