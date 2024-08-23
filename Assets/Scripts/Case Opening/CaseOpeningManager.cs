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
    private Coroutine caseOpeningRoutine;

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

        floatValueObject.SetActive(false);
        closeOpeningButton.SetActive(false);

        openingMenu.SetActive(true);

        // Remove and re-add weapon showcases
        RemoveWeaponShowcases();
        SpawnWeaponShowcases();

        // Reset the float value and win line offset
        winLineOffDistance = Random.Range(-0.75f, 0.15f);
        floatValue = Random.Range(0f, 1f);

        // Stop any existing routine and start a new one
        if (caseOpeningRoutine != null)
        {
            StopCoroutine(caseOpeningRoutine);
        }
        caseOpeningRoutine = StartCoroutine(CaseOpeningRoutine());
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

    void RemoveWeaponShowcases()
    {
        // Destroy all existing weapon showcases
        foreach (Transform child in openingContents)
        {
            Destroy(child.gameObject);
        }
    }

    void SpawnWeaponShowcases()
    {
        // Instantiate and set up weapon showcases
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
        RectTransform showcasePositionRect = weaponShowcasePrefab.transform.Find("ShowcasePosition")?.GetComponent<RectTransform>();

        if (showcasePositionRect == null)
        {
            Debug.LogError("WeaponShowcasePrefab's 'ShowcasePosition' child does not have a RectTransform.");
            yield break;
        }

        while (true)
        {
            float winLineCenterX = winLine.position.x; // Ensure this is updated each frame

            // Move all showcases with current speed
            foreach (Transform showcase in openingContents)
            {
                showcase.transform.Translate(Vector3.left * speed * Time.deltaTime);
            }

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

            Vector3 localCenter = new Vector3(winningShowcaseRect.rect.width * (winLineOffDistance + winningShowcaseRect.pivot.x), 0, 0);
            Vector3 worldCenter = winningShowcaseRect.TransformPoint(localCenter);
            float winningShowcaseCenterX = worldCenter.x;

            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

            if (distanceToWinLine <= slowdownDistance)
            {
                float t = Mathf.Clamp01(distanceToWinLine / slowdownDistance);
                speed = Mathf.Lerp(minimumSpeed, initialSpeed, t);
            }
            else
            {
                speed = initialSpeed;
            }

            float minTargetX = winningShowcaseCenterX - showcasePositionRect.rect.width / 2;
            float maxTargetX = winningShowcaseCenterX + showcasePositionRect.rect.width / 2;

            if (winLineCenterX >= minTargetX && winLineCenterX <= maxTargetX)
            {
                floatValueObject.SetActive(true);
                floatValueObject.GetComponent<TextMeshProUGUI>().text = floatValue.ToString("F7");

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
