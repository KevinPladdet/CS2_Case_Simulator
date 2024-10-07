using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CaseOpeningManager : MonoBehaviour
{
    [Header("References")]
    public GameObject weaponShowcasePrefab;
    public Transform openingContents; // Contains every weapon showcase for the opening menu
    [SerializeField] private Transform winLine;
    
    [Header("After Pull Menu")]
    [SerializeField] private GameObject afterPullMenu;
    [SerializeField] private TextMeshProUGUI floatValueText;
    [SerializeField] private TextMeshProUGUI exteriorText;
    [SerializeField] private TextMeshProUGUI patternText;

    [Header("Menu Stuff")]
    public GameObject CaseMenu;
    public GameObject caseMenuBackground;
    public GameObject openingMenu;

    [Header("Case Info")]
    public TextMeshProUGUI UnlockCaseText; // Case name text
    public Image caseImage; // Case image

    [Header("Case Roll Settings")]
    [SerializeField] private int amountOfSkins = 50; // Number of skins that will be shown during the case opening
    private float spacing = 373f; // Spacing between each skin showcase
    [SerializeField] private float initialSpeed = 5000f; // Initial speed of the case opening
    [SerializeField] private float slowdownDistance = 6000f; // Distance at which the showcases start slowing down
    [SerializeField] private float minimumSpeed = 200f; // Minimum speed of the case opening as it slows down

    private int winningIndex; // Index of the winning showcase

    private float winLineOffDistance;
    private float floatValue;
    private float patternValue;

    [Header("Cases")]
    public Case currentCase; // The case that is currently selected in the case menu

    private Dictionary<Color32, float> rarityDropChances; // Dictionary for drop chances based on rarity
    private Coroutine caseOpeningRoutine; // Coroutine for the case opening animation

    [Header("Case Menu Weapon Showcases")]
    public List<GameObject> caseMenuWeaponShowcases = new List<GameObject>(); // List of showcases for displaying skins in the case menu

    [Header("Key Management")]
    public TextMeshProUGUI keyCountText; // UI text for displaying the number of keys
    public Image keyImage; // UI image for the keys
    public TextMeshProUGUI keyNameText; // UI text for the key name
    public TextMeshProUGUI keyNameTextTwo; // UI text for the key name
    public GameObject buyKeysPanel; // UI panel for buying keys
    public TMP_Dropdown buyKeysDropdown; // Dropdown menu for selecting the number of keys to buy
    public Button buyKeysButton; // Button to confirm key purchase
    public GameObject UnlockButton; // Reference to the Unlock button

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip itemdrop_blue;
    public AudioClip itemdrop_purple;
    public AudioClip itemdrop_pink;
    public AudioClip itemdrop_red;
    public AudioClip itemdrop_gold;
    public AudioClip case_scroll;

    private Dictionary<Transform, bool> showcasePassedWinLine = new Dictionary<Transform, bool>(); // To track whether each showcase has passed the win line

    void Start()
    {
        if (currentCase != null)
        {
            UnlockCaseText.text = "Unlock <b>" + currentCase.caseName + "</b>"; // Set the unlock case text
            caseImage.sprite = currentCase.caseImage; // Set the case image

            // Define the drop chances based on rarity
            rarityDropChances = new Dictionary<Color32, float>()
            {
                { new Color32(81, 103, 241, 255), 79.92f },  // Blue
                { new Color32(132, 73, 247, 255), 15.98f },  // Purple
                { new Color32(190, 48, 205, 255), 3.2f },    // Pink
                { new Color32(206, 73, 74, 255), 0.64f },    // Red
                { new Color32(239, 215, 55, 255), 0.26f }    // Gold
            };

            UpdateCaseMenuShowcases(); // Update the showcases with skins for the case menu
        }
        else
        {
            Debug.LogError("No case selected.");
        }

        UpdateKeyUI(); // Update the key UI at the start

        // Add a listener to the dropdown to handle updates
        buyKeysDropdown.onValueChanged.AddListener(UpdateBuyKeysButtonText);

        // Initialize the BuyKeysButton text based on the default dropdown selection
        UpdateBuyKeysButtonText(buyKeysDropdown.value);
    }

    // Method to update the BuyKeysButton text based on the selected dropdown option
    public void UpdateBuyKeysButtonText(int dropdownValue)
    {
        if (buyKeysButton != null)
        {
            // Get the number of keys selected from the dropdown
            int keysToBuy = int.Parse(buyKeysDropdown.options[dropdownValue].text);

            // Calculate the total price
            float totalPrice = keysToBuy * 2.35f;

            // Update the button text
            buyKeysButton.GetComponentInChildren<TextMeshProUGUI>().text = $"{totalPrice:0.00}€"; // Format to two decimal places
        }
    }

    public void OpenCase()
    {
        if (currentCase == null || !currentCase.UseKey())
        {
            ShowBuyKeysPanel(); // Show the buy keys panel if there are no keys available
            return;
        }

        UpdateKeyUI(); // Update the key UI after using a key

        CaseMenu.SetActive(false);
        caseMenuBackground.SetActive(false);

        afterPullMenu.SetActive(false);

        openingMenu.SetActive(true);

        RemoveWeaponShowcases(); // Remove any existing weapon showcases
        SpawnWeaponShowcases(); // Spawn the weapon showcases for the case opening

        // Randomizes winLine off distance and the float value
        winLineOffDistance = Random.Range(-0.75f, 0.15f);
        floatValue = Random.Range(0f, 1f);
        patternValue = Random.Range(0f, 1000f);

        // Reset the initial positions of all showcases to ensure proper animation
        for (int i = 0; i < openingContents.childCount; i++)
        {
            Transform showcase = openingContents.GetChild(i);
            Vector3 position = showcase.localPosition;
            position.x = -800f + (i * spacing);
            showcase.localPosition = position;
        }

        // Initialize showcasePassedWinLine dictionary for tracking
        showcasePassedWinLine.Clear();
        foreach (Transform showcase in openingContents)
        {
            showcasePassedWinLine.Add(showcase, false);
        }

        // Stop any ongoing case opening routine
        if (caseOpeningRoutine != null)
        {
            StopCoroutine(caseOpeningRoutine);
        }
        caseOpeningRoutine = StartCoroutine(CaseOpeningRoutine()); // Starts the case opening routine
    }

    public void ExitCase()
    {
        afterPullMenu.SetActive(false);
        openingMenu.SetActive(false);

        CaseMenu.SetActive(true);
        caseMenuBackground.SetActive(true);

        // Update key UI when entering the case menu
        UpdateKeyUI();

        if (currentCase.keys == 0)
        {
            ShowBuyKeysPanel();
        }
        else
        {
            CloseBuyKeysPanel();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    // Remove all the weapon showcases from the case opening menu (so it can be respawned to open the case again)
    void RemoveWeaponShowcases()
    {
        foreach (Transform child in openingContents)
        {
            Destroy(child.gameObject);
        }
        openingContents.DetachChildren(); // Ensure all children are fully detached after destroying
    }

    // Spawn weapon showcases in the case opening scene
    void SpawnWeaponShowcases()
    {
        for (int i = 0; i < amountOfSkins; i++)
        {
            // Calculate the position for the showcase
            Vector3 position = new Vector3(-800f + (i * spacing), 15.8f, 0);
            GameObject showcase = Instantiate(weaponShowcasePrefab, position, Quaternion.identity);
            showcase.transform.SetParent(openingContents, false); // Set the showcase as a child of the opening contents

            ShowcaseWeapon selectedSkin;

            // Allow the chance for gold on only the Winning Showcase
            if (i == amountOfSkins - 5)
            {
                winningIndex = i;
                showcase.name = "Winning Showcase";

                // Winning Showcase can get the gold
                selectedSkin = GetRandomSkinByRarity(includeGold: true);
            }
            else
            {
                // All other Showcases can not get the gold
                selectedSkin = GetRandomSkinByRarity(includeGold: false);
            }

            WeaponDisplay display = showcase.GetComponent<WeaponDisplay>();
            if (display != null)
            {
                display.skin = selectedSkin; // Set the skin in the showcase
                display.UpdateCaseContents(); // Ensure the skin is updated immediately
            }

            showcase.name = "WeaponShowcase_" + (i + 1); // Name the showcase
        }
    }

    // Get a random skin from the current case based on rarity
    ShowcaseWeapon GetRandomSkinByRarity(bool includeGold = true)
    {
        if (currentCase == null)
        {
            Debug.LogError("No case selected.");
            return null;
        }

        // Adjust rarityDropChances to enable / disable the chance for a gold
        Dictionary<Color32, float> adjustedRarityDropChances = new Dictionary<Color32, float>(rarityDropChances);

        if (!includeGold)
        {
            adjustedRarityDropChances.Remove(new Color32(239, 215, 55, 255)); // Remove gold from the chances
        }

        // Calculate the total weight of all skins
        float totalWeight = 0f;
        foreach (ShowcaseWeapon skin in currentCase.caseSkins)
        {
            if (adjustedRarityDropChances.ContainsKey(skin.skinRarity))
            {
                totalWeight += adjustedRarityDropChances[skin.skinRarity];
            }
        }

        // Select a random value and pick the corresponding skin.
        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (ShowcaseWeapon skin in currentCase.caseSkins)
        {
            if (adjustedRarityDropChances.ContainsKey(skin.skinRarity))
            {
                cumulativeWeight += adjustedRarityDropChances[skin.skinRarity];
                if (randomValue < cumulativeWeight)
                {
                    return skin;
                }
            }
        }

        return currentCase.caseSkins[0]; // Fallback in case no skin is selected
    }

    // Case menu weapon showcases get updated here
    void UpdateCaseMenuShowcases()
    {
        if (caseMenuWeaponShowcases.Count == 0)
        {
            Debug.LogError("No weapon showcases assigned to the case menu.");
            return;
        }

        // Deactivate all showcases
        foreach (GameObject showcase in caseMenuWeaponShowcases)
        {
            showcase.SetActive(false);
        }

        int skinCount = currentCase.caseSkins.Count; // Gets the number of skins in the selected case
        for (int i = 0; i < skinCount; i++)
        {
            if (i >= caseMenuWeaponShowcases.Count)
                break;

            GameObject showcase = caseMenuWeaponShowcases[i];
            showcase.SetActive(true); // Activate just enough showcases for the skins and keeps the rest deactivated

            WeaponDisplay display = showcase.GetComponent<WeaponDisplay>();
            if (display != null)
            {
                display.skin = currentCase.caseSkins[i]; // Set the skin in the showcase
                display.UpdateCaseContents(); // Update the showcase with the current skin
            }
        }
    }

    // Method to update the key UI with the current number of keys for the selected case
    public void UpdateKeyUI()
    {
        if (keyCountText != null && currentCase != null)
        {
            keyCountText.text = currentCase.keys.ToString();
        }

        if (UnlockButton != null)
        {
            UnlockButton.SetActive(currentCase != null && currentCase.keys > 0); // Enable UnlockButton if keys are available, otherwise disable it
        }
    }

    // Show the panel for buying more keys
    public void ShowBuyKeysPanel()
    {
        if (buyKeysPanel != null)
        {
            buyKeysPanel.SetActive(true);
        }
    }

    // Close the panel for buying more keys
    public void CloseBuyKeysPanel()
    {
        if (buyKeysPanel != null)
        {
            buyKeysPanel.SetActive(false);
        }
    }

    // Function to be called when the player buys keys
    public void BuyKeys()
    {
        if (currentCase != null)
        {
            int keysToBuy = int.Parse(buyKeysDropdown.options[buyKeysDropdown.value].text); // Get the number of keys selected in the dropdown
            currentCase.AddKeys(keysToBuy); // Add the purchased keys to the current case
            UpdateKeyUI(); // Update the key UI after the purchase
            CloseBuyKeysPanel(); // Close the buy keys panel after the purchase
        }
    }

    // Case opening animation happens here
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
            float winLineCenterX = winLine.position.x; // Gets the position of the winLine

            // Moves all showcases to the left
            foreach (Transform showcase in openingContents)
            {
                showcase.transform.Translate(Vector3.left * speed * Time.deltaTime);

                // Play the scroll sound effect for each showcase as it passes the win line
                PlayScrollSound(showcase);
            }

            // Ensure winningIndex is within bounds
            if (winningIndex < 0 || winningIndex >= openingContents.childCount)
            {
                Debug.LogError("Winning showcase index is out of bounds.");
                yield break;
            }

            // Gets the position of the winning showcase
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

            // Calculate the world position of the winning showcase's center.
            Vector3 localCenter = new Vector3(winningShowcaseRect.rect.width * (winLineOffDistance + winningShowcaseRect.pivot.x), 0, 0);
            Vector3 worldCenter = winningShowcaseRect.TransformPoint(localCenter);
            float winningShowcaseCenterX = worldCenter.x;

            // Calculate the distance to the win line.
            float distanceToWinLine = Mathf.Abs(winLineCenterX - winningShowcaseCenterX);

            // Slow down the showcases as they approach the win line.
            if (distanceToWinLine <= slowdownDistance)
            {
                float t = Mathf.Clamp01(distanceToWinLine / slowdownDistance);
                speed = Mathf.Lerp(minimumSpeed, initialSpeed, t);
            }
            else
            {
                speed = initialSpeed;
            }

            // Check if the winning showcase is at the win line.
            float minTargetX = winningShowcaseCenterX - showcasePositionRect.rect.width / 2;
            float maxTargetX = winningShowcaseCenterX + showcasePositionRect.rect.width / 2;

            if (winLineCenterX >= minTargetX && winLineCenterX <= maxTargetX)
            {
                // Pulled skin / case animation ended
                afterPullMenu.SetActive(true);
                floatValueText.text = "<b>Wear Rating</b>: " + floatValue.ToString("F7");

                patternText.text = "<b>Pattern Template</b>: " + patternValue.ToString("F0");

                // Check Exterior
                string condition = GetPreciseCondition(floatValue);

                // Update exteriorText with GetPreciseCondition
                exteriorText.text = "<b>Exterior</b>: " + condition;

                // Enable the text components of the winning showcase.
                TextMeshProUGUI[] textComponents = winningShowcase.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI textComponent in textComponents)
                {
                    if (textComponent.gameObject.name == "WeaponTypeText" ||
                        textComponent.gameObject.name == "WeaponSkinText")
                    {
                        textComponent.enabled = true;
                    }
                }

                // Play the appropriate sound effect for the winning showcase
                PlayItemDropSound(winningShowcase);
                
                break;
            }

            yield return null;
        }
    }

    // Updates exteriorText with a precise float
    private string GetPreciseCondition(float floatValue)
    {
        // Makes it so that if a skin is (for exampel) 7.00000001 it will be Minimal Wear
        float roundedValue = Mathf.Round(floatValue * 1000000f) / 1000000f;

        if (roundedValue <= 0.07f)
        {
            return "Factory New";
        }
        else if (roundedValue > 0.07f && roundedValue <= 0.15f)
        {
            if (Mathf.Approximately(floatValue, 0.07f))
            {
                return "Factory New";
            }
            else
            {
                return "Minimal Wear";
            }
        }
        else if (roundedValue > 0.15f && roundedValue <= 0.37f)
        {
            if (Mathf.Approximately(floatValue, 0.15f))
            {
                return "Minimal Wear";
            }
            else
            {
                return "Field-Tested";
            }
        }
        else if (roundedValue > 0.37f && roundedValue <= 0.45f)
        {
            if (Mathf.Approximately(floatValue, 0.37f))
            {
                return "Field-Tested";
            }
            else
            {
                return "Well-Worn";
            }
        }
        else if (roundedValue > 0.45f && roundedValue <= 1.0f)
        {
            if (Mathf.Approximately(floatValue, 0.45f))
            {
                return "Well-Worn";
            }
            else
            {
                return "Battle-Scarred";
            }
        }

        return "Invalid";
    }

    void PlayScrollSound(Transform showcase)
    {
        // Calculate the current distance of the showcase to the win line
        float showcaseCenterX = showcase.position.x;

        RectTransform showcasePositionRect = showcase.Find("ShowcasePosition")?.GetComponent<RectTransform>();
        if (showcasePositionRect == null)
        {
            Debug.LogError("Showcase does not have a 'ShowcasePosition' RectTransform.");
            return;
        }

        Vector3 localRightEdge = new Vector3(showcasePositionRect.rect.width * (3.11f + showcasePositionRect.pivot.x), 0, 0);
        Vector3 worldRightEdge = showcasePositionRect.TransformPoint(localRightEdge);
        float showcaseRightX = worldRightEdge.x;

        // Check if the showcase has passed the win line from right to left
        if (showcaseRightX < winLine.position.x && !showcasePassedWinLine[showcase])
        {
            audioSource.PlayOneShot(case_scroll);
            showcasePassedWinLine[showcase] = true; // Mark this showcase as having passed the win line
        }
    }

    void PlayItemDropSound(Transform winningShowcase)
    {
        WeaponDisplay display = winningShowcase.GetComponent<WeaponDisplay>();
        if (display != null)
        {
            Color32 rarityColor = display.skin.skinRarity;
            if (rarityColor.Equals(new Color32(81, 103, 241, 255)))
            {
                audioSource.PlayOneShot(itemdrop_blue);
            }
            else if (rarityColor.Equals(new Color32(132, 73, 247, 255)))
            {
                audioSource.PlayOneShot(itemdrop_purple);
            }
            else if (rarityColor.Equals(new Color32(190, 48, 205, 255)))
            {
                audioSource.PlayOneShot(itemdrop_pink);
            }
            else if (rarityColor.Equals(new Color32(206, 73, 74, 255)))
            {
                audioSource.PlayOneShot(itemdrop_red);
            }
            else if (rarityColor.Equals(new Color32(239, 215, 55, 255)))
            {
                audioSource.PlayOneShot(itemdrop_gold);
            }
        }
    }

    // This method is on used for when you click on a case to select it
    public void SetCurrentCase(Case selectedCase)
    {
        currentCase = selectedCase;

        if (currentCase != null)
        {
            UnlockCaseText.text = "Unlock <b>" + currentCase.caseName + " Case</b>"; // Sets the unlock case text
            caseImage.sprite = currentCase.caseImage; // Sets the case image

            keyImage.sprite = currentCase.keyImage; // Sets the key image

            keyNameText.text = currentCase.caseName + " Key"; // Sets keyNameText
            keyNameTextTwo.text = currentCase.caseName + " Key";

            UpdateCaseMenuShowcases(); // Update the showcases with skins for the case menu
            UpdateKeyUI(); // Update the key UI for the selected case

            if (currentCase.keys == 0)
            {
                ShowBuyKeysPanel();
            }
            else
            {
                CloseBuyKeysPanel();
            }
        }
        else
        {
            Debug.LogError("No case selected.");
        }
    }
}