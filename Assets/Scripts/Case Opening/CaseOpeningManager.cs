using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CaseOpeningManager : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponShowcasePrefab; // Empty Weapon Showcase prefab
    public Transform openingContents; // Every Weapon Showcase will be under this transform
    public Transform winLine; // The WinLine that indicates the winning position
    public GameObject floatValueObject;
    public GameObject closeOpeningButton;

    // These ones are enabled / disabled to go through menus
    [Header("Menu Stuff")]
    public GameObject CaseMenu; // The menu where you see the case and all the skins
    public GameObject caseMenuBackground; // Enables when you go back to the menu
    public GameObject openingMenu; // Contains "coverBackground", "WinLine" and "SkinFloatValue"
    public GameObject blurring; // This is an image that blurs the background behind case contents

    [Header("Case Info")]
    public TextMeshProUGUI UnlockCaseText;
    public Image caseImage; // Image component to display the case image

    [Header("Case Roll Settings")]
    [SerializeField] private int amountOfSkins = 50; // The number of Weapon Showcases to spawn
    private float spacing = 373f; // The spacing between each WeaponShowcase (should be 373)
    [SerializeField] private float initialSpeed = 5000f; // Initial speed of the scrolling
    [SerializeField] private float slowdownDistance = 6000f; // Distance from the WinLine where the slowdown starts
    [SerializeField] private float minimumSpeed = 200f; // The minimum speed the scrolling can go

    private int winningIndex; // The index of the winning Weapon Showcase

    private float winLineOffDistance; // This decides where it lands on the Winning Showcase
    private float floatValue; // Float values is one of the best things in CS

    [Header("Cases")]
    public Case currentCase; // Reference to the selected case

    private Dictionary<Color32, float> rarityDropChances;

    void Start()
    {
        if (currentCase != null)
        {
            // Set case menu elements
            UnlockCaseText.text = "Unlock <b>" + currentCase.caseName + "</b>";
            caseImage.sprite = currentCase.caseImage;

            // Initialize rarity drop chances
            rarityDropChances = new Dictionary<Color32, float>()
            {
                { new Color32(81, 103, 241, 255), 79.92f },  // Blue
                { new Color32(132, 73, 247, 255), 15.98f },  // Purple
                { new Color32(190, 48, 205, 255), 3.2f },    // Pink
                { new Color32(206, 73, 74, 255), 0.64f },    // Red
                { new Color32(239, 215, 55, 255), 0.26f }    // Gold
            };

            blurring.SetActive(true);
        }
        else
        {
            Debug.LogError("No case selected.");
        }
    }

    public void OpenCase()
    {
        CaseMenu.SetActive(false);
        caseMenuBackground.SetActive(false);
        blurring.SetActive(false);

        closeOpeningButton.SetActive(false);
        openingMenu.SetActive(true);

        winLineOffDistance = Random.Range(-0.75f, 0.15f); // winLine can land anywhere on the winning weapon showcase
        floatValue = Random.Range(0f, 1f);

        // Spawn weapon showcases with randomized skins
        SpawnWeaponShowcases();

        StartCoroutine(CaseOpeningRoutine());
    }

    public void ExitCase()
    {
        floatValueObject.SetActive(false);
        closeOpeningButton.SetActive(false);
        openingMenu.SetActive(false);

        CaseMenu.SetActive(true);
        caseMenuBackground.SetActive(true);
        blurring.SetActive(true);
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
        // Clear existing showcases
        foreach (Transform child in openingContents)
        {
            Destroy(child.gameObject);
        }

        // Instantiate and set up 50 weapon showcases with randomized skins
        for (int i = 0; i < amountOfSkins; i++)
        {
            Vector3 position = new Vector3(-800f + (i * spacing), 15.8f, 0);
            GameObject showcase = Instantiate(weaponShowcasePrefab, position, Quaternion.identity);
            showcase.transform.SetParent(openingContents, false);

            // Set a random skin based on rarity
            ShowcaseWeapon selectedSkin = GetRandomSkinByRarity();
            WeaponDisplay display = showcase.GetComponent<WeaponDisplay>();
            if (display != null)
            {
                display.skin = selectedSkin;
            }

            // Name each showcase
            showcase.name = "WeaponShowcase_" + (i + 1);

            // Determine the winning showcase index
            if (i == amountOfSkins - 5) // Set the winning showcase position
            {
                winningIndex = i;
                showcase.name = "Winning Showcase"; // Name the winning showcase
            }
        }
    }

    ShowcaseWeapon GetRandomSkinByRarity()
    {
        if (currentCase == null)
        {
            Debug.LogError("No case selected.");
            return null;
        }

        // Calculate the total weight for rarity drop chances
        float totalWeight = 0f;
        foreach (ShowcaseWeapon skin in currentCase.caseSkins)
        {
            totalWeight += rarityDropChances[skin.skinRarity];
        }

        // Pick a random skin based on rarity
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (ShowcaseWeapon skin in currentCase.caseSkins)
        {
            cumulativeWeight += rarityDropChances[skin.skinRarity];
            if (randomValue < cumulativeWeight)
            {
                return skin;
            }
        }

        // Fallback if something goes wrong
        return currentCase.caseSkins[0];
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
            Vector3 worldCenter = winningShowcaseRect.TransformPoint(localCenter);
            float winningShowcaseCenterX = worldCenter.x;

            // Calculate the distance to the WinLine
            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

            // Adjust the speed based on the distance
            if (distanceToWinLine <= slowdownDistance)
            {
                float t = Mathf.Clamp01(distanceToWinLine / slowdownDistance);
                speed = Mathf.Lerp(minimumSpeed, initialSpeed, t);
            }
            else
            {
                speed = initialSpeed;
            }

            // Determine the target range for the WinLine to stop within
            float minTargetX = winningShowcaseCenterX - showcasePositionRect.rect.width / 2;
            float maxTargetX = winningShowcaseCenterX + showcasePositionRect.rect.width / 2;

            // Check if the WinLine is within the target range of the winning showcase
            if (winLineCenterX >= minTargetX && winLineCenterX <= maxTargetX)
            {
                // Everything below this happens when the skin is pulled
                floatValueObject.SetActive(true);
                floatValueObject.GetComponent<TextMeshProUGUI>().text = "" + floatValue.ToString("F7");

                // Enable weapon type text + skin name text
                TextMeshProUGUI[] textComponents = winningShowcase.GetComponentsInChildren<TextMeshProUGUI>();

                foreach (TextMeshProUGUI textComponent in textComponents)
                {
                    if (textComponent.gameObject.name == "WeaponTypeText" ||
                        textComponent.gameObject.name == "WeaponSkinText")
                    {
                        textComponent.enabled = true;
                    }
                }

                closeOpeningButton.SetActive(true);

                break;
            }

            yield return null;
        }
    }
}
