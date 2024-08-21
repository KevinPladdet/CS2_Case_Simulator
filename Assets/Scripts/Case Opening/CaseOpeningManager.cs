using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaseOpeningManager : MonoBehaviour
{
    public GameObject weaponShowcasePrefab; // Empty WeaponShowcase prefab
    public int amountOfSkins; // The number of WeaponShowcases it will spawn
    public float spacing = 30f; // The spacing between each WeaponShowcase (should be 30)
    public Transform openingContents; // Every WeaponShowcase will be under this transform
    public Transform winLine; // The WinLine that indicates the winning position
    public float initialSpeed = 3000f; // Initial speed of the scrolling
    public float slowdownDistance = 500f; // Distance from the WinLine where the slowdown starts
    public float minimumSpeed = 200f; // The minimum speed the scrolling can go

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    void SpawnWeaponShowcases()
    {
        float startX = -800f;

        for (int i = 0; i < amountOfSkins; i++)
        {
            Vector3 position = new Vector3(startX + (i * spacing), 15.8f, 0);
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
        float speed = initialSpeed;

        // Ensure the RectTransform of the ShowcasePosition is correctly found
        RectTransform showcasePositionRect = weaponShowcasePrefab.transform.Find("ShowcasePosition")?.GetComponent<RectTransform>();
        if (showcasePositionRect == null)
        {
            Debug.LogError("WeaponShowcasePrefab's 'ShowcasePosition' child does not have a RectTransform.");
            yield break;
        }

        // Calculate the center position of the WinLine in world space
        float winLineCenterX = winLine.position.x;

        while (true)
        {
            // Move all showcases with current speed
            foreach (Transform showcase in openingContents)
            {
                showcase.transform.Translate(Vector3.left * speed * Time.deltaTime);
            }

            // Check the distance from the winning showcase to the win line
            Transform winningShowcase = openingContents.GetChild(winningIndex);
            Transform showcasePosition = winningShowcase.Find("ShowcasePosition");
            if (showcasePosition == null)
            {
                Debug.LogError("Winning Showcase does not have a child named 'ShowcasePosition'.");
                yield break;
            }

            RectTransform winningShowcaseRect = showcasePosition.GetComponent<RectTransform>();
            if (winningShowcaseRect == null)
            {
                Debug.LogError("ShowcasePosition does not have a RectTransform.");
                yield break;
            }

            // Calculate the center of the winning showcase in world coordinates
            Vector3 localCenter = new Vector3(
                winningShowcaseRect.rect.width * (0.5f - winningShowcaseRect.pivot.x),
                0,
                0
            );

            // Convert local center to world position
            Vector3 worldCenter = winningShowcaseRect.TransformPoint(localCenter);

            // Get the X position of the world center
            float winningShowcaseCenterX = worldCenter.x;

            // Calculate the distance to the WinLine
            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

            // Log the distance to the console
            Debug.Log($"Distance to WinLine: {distanceToWinLine}");

            // Adjust the speed based on the distance
            if (distanceToWinLine <= slowdownDistance)
            {
                // Normalize the distance to a value between 0 and 1
                float t = Mathf.Clamp01(distanceToWinLine / slowdownDistance);
                // Lerp speed from initialSpeed to minimumSpeed based on the normalized distance
                speed = Mathf.Lerp(minimumSpeed, initialSpeed, t);
            }
            else
            {
                // Ensure speed remains constant if distance is greater than slowdownDistance
                speed = initialSpeed;
            }

            // Stop when the winning showcase is close enough
            if (distanceToWinLine < 0.4f && speed <= minimumSpeed + 0.4f)
            {
                Debug.Log("Winning showcase reached the WinLine!");

                // Align the center of the winning showcase with the WinLine
                Vector3 newPosition = winningShowcaseRect.position;
                newPosition.x = winLineCenterX;
                winningShowcaseRect.position = newPosition;

                // Stop the coroutine
                yield break;
            }

            yield return null; // Wait until the next frame
        }
    }
}
