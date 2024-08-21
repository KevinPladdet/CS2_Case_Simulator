using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OpenCase : MonoBehaviour
{

    public GameObject coverBackground; // this background will cover the screen when the unlock container button is pressed

    public GameObject WeaponShowcase; // empty weaponshowcase which would be assigned a scriptable object somewhere in the code at some point

    public void OpeningCase()
    {
        Debug.Log("Unlocking Container");
        coverBackground.SetActive(true);
    }
}
