using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponDisplay : MonoBehaviour
{
    public ShowcaseWeapon skin;

    public TextMeshProUGUI weaponTypeText;
    public TextMeshProUGUI skinNameText;

    public Image skinImage;
    public Image weaponBackground;
    public Image rarityImage;

    void Start()
    {
        UpdateCaseContents();
    }

    public void UpdateCaseContents()
    {
        weaponTypeText.text = skin.weaponType;
        skinNameText.text = skin.skinName;

        skinImage.sprite = skin.skinImage;
        weaponBackground.sprite = skin.weaponBackground;
        rarityImage.color = skin.skinRarity;
    }
}
