using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CaseOpeningManager : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponShowcasePrefab; // Empty Weapon Showcase prefab
    public Transform openingContents; // Every Weapon Showcase will be under this transform
    public Transform winLine; // The WinLine that indicates the winning position
    public GameObject floatValueObject;

    // These ones are enabled / disabled to go through menus
    [Header("Menu Stuff")]
    public GameObject CaseMenu; // The menu where you see the case and all the skins
    public GameObject caseMenuBackground; // Enables when you go back to the menu
    public GameObject OpeningMenu; // Contains "coverBackground", "WinLine" and "SkinFloatValue"

    [Header("Case Settings")]
    [SerializeField] private int amountOfSkins; // The number of Weapon Showcases it will spawn
    private float spacing = 373f; // The spacing between each WeaponShowcase (should be 373)
    [SerializeField] private float initialSpeed = 5000f; // Initial speed of the scrolling
    [SerializeField] private float slowdownDistance = 6000f; // Distance from the WinLine where the slowdown starts
    [SerializeField] private float minimumSpeed = 200f; // The minimum speed the scrolling can go

    private GameObject winningShowcase;

    private float winLineOffDistance; // This decides where it lands on the Winning Showcase
    private float floatValue; // Float values is one of the best things in CS

    [Header("Cases")]
    // Different cases (for now there is only the wildfire case)
    public List<ShowcaseWeapon> wildfireCaseSkins; // List of every skin from the case

    private Dictionary<Color32, float> rarityDropChances;
    private int winningIndex; // The index of the winning Weapon Showcase

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

    public void OpenCase()
    {
        CaseMenu.SetActive(false);
        caseMenuBackground.SetActive(false);

        OpeningMenu.SetActive(true);

        winLineOffDistance = Random.Range(-0.75f, 0.15f);

        floatValue = Random.Range(0f, 1f);

        StartCoroutine(CaseOpeningRoutine());
    }

    public void ExitCase()
    {
        floatValueObject.SetActive(false);
        OpeningMenu.SetActive(false);

        CaseMenu.SetActive(true);
        caseMenuBackground.SetActive(true);
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

    ShowcaseWeapon GetRandomSkinByRarity() // This method is for the wildfire case, copy this method to make a new case
    {
        float totalWeight = 0f;

        foreach (ShowcaseWeapon skin in wildfireCaseSkins)
        {
            totalWeight += rarityDropChances[skin.skinRarity];
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (ShowcaseWeapon skin in wildfireCaseSkins)
        {
            cumulativeWeight += rarityDropChances[skin.skinRarity];
            if (randomValue < cumulativeWeight)
            {
                return skin;
            }
        }

        return wildfireCaseSkins[0];
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
            Vector3 localCenter = new Vector3(winningShowcaseRect.rect.width * (winLineOffDistance + winningShowcaseRect.pivot.x), 0, 0);

            // Convert local center to world position
            Vector3 worldCenter = winningShowcaseRect.TransformPoint(localCenter);

            // Get the X position of the world center
            float winningShowcaseCenterX = worldCenter.x;

            // Calculate the distance to the WinLine
            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

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

            // Determine the target range for the WinLine to stop within
            float minTargetX = winningShowcaseCenterX - showcasePositionRect.rect.width * 0.5f;
            float maxTargetX = winningShowcaseCenterX + showcasePositionRect.rect.width * 0.5f;

            // Check if the WinLine is within the target range
            if (winLine.position.x >= minTargetX && winLine.position.x <= maxTargetX)
            {
                // Winline has stopped and the skin has been pulled
                pulledSkin();

                yield break;
            }

            yield return null; // Wait until the next frame
        }
    }

    void pulledSkin()
    {
        floatValueObject.SetActive(true);
        floatValueObject.GetComponent<TextMeshProUGUI>().text = "" + floatValue.ToString("F7");
        winningShowcase = GameObject.Find("Winning Showcase");

        TextMeshProUGUI[] textComponents = winningShowcase.GetComponentsInChildren<TextMeshProUGUI>();

        // Loop through children and enable both TextMeshProUGUI
        foreach (TextMeshProUGUI textComponent in textComponents)
        {
            if (textComponent.gameObject.name == "WeaponTypeText" ||
                textComponent.gameObject.name == "WeaponSkinText")
            {
                textComponent.enabled = true;
            }
        }

        // Also make it so that it shows the weapon type and skin name (enable gameobjects)
        // Maybe make it here so that there is a go back / close button which will bring you back to the case menu
    }
}