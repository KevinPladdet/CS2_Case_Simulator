using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaseOpeningManager : MonoBehaviour
{
    public GameObject weaponShowcasePrefab; // Empty WeaponShowcase prefab
    public int amountOfSkins; // The number of WeaponShowcases it will spawn
    public float spacing = 30f; // The spacing between each WeaponShowcase (should be 30)
    public Transform openingContents; // Every WeaponShowcase will be under this transform
    public Transform winLine; // The WinLine that indicates the winning position
    public float initialSpeed = 500f; // Initial speed of the scrolling

    public List<ShowcaseWeapon> possibleSkins; // List of every skin from the case

    private Dictionary<Color32, float> rarityDropChances;
    private int winningIndex; // The index of the winning WeaponShowcase

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

        // Start the case opening process
        StartCoroutine(CaseOpeningRoutine());
    }

    void SpawnWeaponShowcases()
    {
        float startX = -800f;

        for (int i = 0; i < amountOfSkins; i++)
        {
            Vector3 position = new Vector3(startX + (i * spacing), 2.1f, 0);
            GameObject showcase = Instantiate(weaponShowcasePrefab, position, Quaternion.identity);
            showcase.transform.SetParent(openingContents, false);

            ShowcaseWeapon selectedSkin = GetRandomSkinByRarity();
            WeaponDisplay display = showcase.GetComponent<WeaponDisplay>();
            if (display != null)
            {
                display.skin = selectedSkin;
            }

            // Name each showcase
            showcase.name = "WeaponShowcase_" + (i + 1);

            // Determine the winning showcase
            if (i == amountOfSkins - 5) // The index of the winning showcase
            {
                winningIndex = i;
                showcase.name = "Winning Showcase"; // Name the winning showcase
            }
        }
    }

    ShowcaseWeapon GetRandomSkinByRarity()
    {
        float totalWeight = 0f;

        foreach (ShowcaseWeapon skin in possibleSkins)
        {
            totalWeight += rarityDropChances[skin.skinRarity];
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (ShowcaseWeapon skin in possibleSkins)
        {
            cumulativeWeight += rarityDropChances[skin.skinRarity];
            if (randomValue < cumulativeWeight)
            {
                return skin;
            }
        }

        return possibleSkins[0];
    }

    IEnumerator CaseOpeningRoutine()
    {
        float constantSpeedDuration = 2f; // Duration of constant speed
        float slowdownDuration = 4f; // Duration of slowdown
        float totalDuration = constantSpeedDuration + slowdownDuration; // Total time for the routine
        float timer = 0f; // Timer to track elapsed time

        float speed = initialSpeed;

        // Calculate the width of a WeaponShowcase using RectTransform (if it's a UI element)
        RectTransform exampleRect = weaponShowcasePrefab.GetComponentInChildren<RectTransform>();
        if (exampleRect == null)
        {
            Debug.LogError("WeaponShowcasePrefab does not have a RectTransform in its children.");
            yield break;
        }
        float showcaseWidth = exampleRect.rect.width;

        // Calculate the center position of the WinLine
        float winLineCenterX = winLine.position.x;

        while (true)
        {
            timer += Time.deltaTime; // Update timer

            if (timer <= constantSpeedDuration)
            {
                // Constant speed phase
                speed = initialSpeed;
            }
            else if (timer <= totalDuration)
            {
                // Gradual slowdown phase
                float t = (timer - constantSpeedDuration) / slowdownDuration; // Normalize time
                speed = Mathf.Lerp(initialSpeed, 0f, t); // Lerp speed to zero
            }
            else
            {
                // Ensure speed is exactly zero after total duration
                speed = 0f;
            }

            foreach (Transform showcase in openingContents)
            {
                showcase.transform.Translate(Vector3.left * speed * Time.deltaTime);
            }

            Transform winningShowcase = openingContents.GetChild(winningIndex);
            Transform showcasePosition = winningShowcase.Find("ShowcasePosition"); // Find the "ShowcasePosition" child
            RectTransform winningShowcaseRect = showcasePosition.GetComponent<RectTransform>();
            if (winningShowcaseRect == null)
            {
                Debug.LogError("Winning Showcase's 'ShowcasePosition' does not have a RectTransform.");
                yield break;
            }

            float winningShowcaseCenterX = winningShowcaseRect.position.x + (showcaseWidth / 2f);
            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

            // Check if the winning showcase is very close to the win line and speed is very low
            if (distanceToWinLine < 0.1f && speed <= 0.1f)
            {
                Debug.Log("Winning showcase reached the WinLine!");
                winningShowcaseRect.position = new Vector3(winLineCenterX - (showcaseWidth / 2f), winningShowcaseRect.position.y, winningShowcaseRect.position.z);
                yield break; // Stop the routine
            }

            // Debug log for tracking time and position
            Debug.Log($"Time Elapsed: {timer:F2}s");
            Debug.Log($"Winning Showcase Position: {winningShowcaseRect.position.x}");
            Debug.Log($"WinLine Position: {winLineCenterX}");
            Debug.Log($"Distance to WinLine: {distanceToWinLine}");
            Debug.Log($"Current Speed: {speed}");

            yield return null; // Wait until the next frame
        }
    }
}
