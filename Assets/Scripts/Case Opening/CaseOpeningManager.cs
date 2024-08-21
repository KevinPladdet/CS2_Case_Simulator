using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseOpeningManager : MonoBehaviour
{
    public GameObject weaponShowcasePrefab; // Empty WeaponShowcase prefab
    public int amountOfSkins; // The number of WeaponShowcases it will spawn
    public float spacing; // The spacing between each WeaponShowcase (should be 30)
    public Transform openingContents; // Every WeaponShowcase will be under this transform

    public List<ShowcaseWeapon> possibleSkins; // List of every skin from the case

    private Dictionary<Color32, float> rarityDropChances;

    void Start()
    {
        // Drop chances are based on the rarity of the skin
        rarityDropChances = new Dictionary<Color32, float>()
        {
            { new Color32(81, 103, 241, 255), 79.92f },  // Blue
            { new Color32(132, 73, 247, 255), 15.98f },  // Purple
            { new Color32(190, 48, 205, 255), 3.2f },    // Pink
            { new Color32(206, 73, 74, 255), 0.64f },    // Red
            { new Color32(239, 215, 55, 255), 0.26f }    // Gold
        };

        SpawnWeaponShowcases();
    }

    void SpawnWeaponShowcases()
    {
        // WeaponShowcase starting position (has to be enough to the left of the scene)
        float startX = -800f;

        for (int i = 0; i < amountOfSkins; i++)
        {
            // Calculate the position for each WeaponShowcase
            Vector3 position = new Vector3(startX + (i * spacing), 2.1f, 0);

            // Instantiate the prefab
            GameObject showcase = Instantiate(weaponShowcasePrefab, position, Quaternion.identity);

            // Set the parent
            showcase.transform.SetParent(openingContents, false);

            // Randomly assign a skin to the WeaponShowcase
            ShowcaseWeapon selectedSkin = GetRandomSkinByRarity();
            WeaponDisplay display = showcase.GetComponent<WeaponDisplay>();
            if (display != null)
            {
                display.skin = selectedSkin;
            }

            // Naming is key to clean code :)
            showcase.name = "WeaponShowcase_" + (i + 1);
        }
    }


    ShowcaseWeapon GetRandomSkinByRarity() // Basically a legal lottery :)
    {
        // Total weight is every skin in the case combined (will always be 100%)
        float totalWeight = 0f;

        // Calculate the total weight based on the rarity
        foreach (ShowcaseWeapon skin in possibleSkins)
        {
            totalWeight += rarityDropChances[skin.skinRarity];
        }

        float randomValue = Random.Range(0, totalWeight);

        // Goes through each skin and makes sure it doesn't go past a 100% weight)
        float cumulativeWeight = 0f;
        foreach (ShowcaseWeapon skin in possibleSkins)
        {
            cumulativeWeight += rarityDropChances[skin.skinRarity];
            if (randomValue < cumulativeWeight)
            {
                return skin;
            }
        }

        // Will get run if there are any errors
        return possibleSkins[0];
    }
}
