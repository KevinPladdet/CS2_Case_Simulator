using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminHandler : MonoBehaviour
{

    [SerializeField] private GameObject adminButton;

    // Konami Code
    private KeyCode[] sequence = { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A, KeyCode.Return };

    private int sequenceIndex;

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(sequence[sequenceIndex]))
            {
                sequenceIndex++;

                if (sequenceIndex == sequence.Length)
                {
                    sequenceIndex = 0;

                    // You just unlocked admin controls:
                    adminButton.SetActive(true);
                }
            }
            else
            {
                sequenceIndex = 0;
            }
        }
    }
}
