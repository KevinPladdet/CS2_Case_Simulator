using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class ShowcaseWeapon : ScriptableObject
{

    public string weaponType;
    public string skinName;
    public Color32 skinRarity;

    public Sprite skinImage;
    public Sprite weaponBackground;

}
