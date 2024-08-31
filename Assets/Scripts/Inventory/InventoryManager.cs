using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq; // Using this for sorting

public class InventoryManager : MonoBehaviour
{
    // Separate lists for keys and skins
    public List<GameObject> itemShowcases; // List of all itemShowcases (should be 45)
    public List<Case> cases; // List of all available cases
    public List<Skin> skins; // List of all available skins

    private List<Key> keysInventory = new List<Key>(); // List to store keys in the inventory
    private List<Skin> skinsInventory = new List<Skin>(); // List to store skins in the inventory

    public TMP_Text pageNumberText;
    public Button previousButton;
    public Button nextButton;
    public TMP_Text previousText;
    public TMP_Text nextText;
    public TMP_Dropdown sortDropdown;

    private int currentPage = 1;
    private int totalPages = 1;

    // Page button colors
    public Color activeColor;
    public Color inactiveColor;

    private void Start()
    {
        // Populate keys inventory from cases
        foreach (Case caseItem in cases)
        {
            for (int i = 0; i < caseItem.keys; i++)
            {
                keysInventory.Add(new Key { caseName = caseItem.caseName, keyImage = caseItem.keyImage });
            }
        }

        sortDropdown.onValueChanged.AddListener(OnSortOptionChanged); // Add listener for dropdown changes
        RefreshKeysInventory();
    }

    // Method to populate the inventory UI with keys for the current page
    private void PopulateInventory(List<Key> inventory)
    {
        int showcaseIndex = 0;
        int startKeyIndex = (currentPage - 1) * itemShowcases.Count;

        // Deactivate all showcases first
        foreach (GameObject showcase in itemShowcases)
        {
            showcase.SetActive(false);
        }

        // Loop through the keys and display them for the current page
        for (int i = startKeyIndex; i < startKeyIndex + itemShowcases.Count && i < inventory.Count; i++)
        {
            UpdateCaseUI(itemShowcases[showcaseIndex], inventory[i]);
            itemShowcases[showcaseIndex].SetActive(true);
            showcaseIndex++;
        }

        UpdatePageButtons(inventory.Count);
    }

    // Updates the image and name of the key
    private void UpdateCaseUI(GameObject itemShowcase, Key key)
    {
        Image keyImage = itemShowcase.transform.Find("SkinImage").GetComponent<Image>();
        TMP_Text keyNameText = itemShowcase.transform.Find("ItemTypeText").GetComponent<TMP_Text>();

        keyImage.sprite = key.keyImage;
        keyNameText.text = key.caseName + " Key";
    }

    // Refreshes the keys inventory UI
    public void RefreshKeysInventory()
    {
        CalculateTotalPages(keysInventory.Count);
        PopulateInventory(keysInventory);
    }

    // Refreshes the skins inventory UI
    public void RefreshSkinsInventory()
    {
        CalculateTotalPages(skinsInventory.Count);
        PopulateInventory(skinsInventory.Select(s => new Key { caseName = s.skinName, keyImage = s.skinImage }).ToList()); // Temporary conversion to use PopulateInventory
    }

    // Calculates how many pages there will be in total based on the inventory size
    private void CalculateTotalPages(int itemCount)
    {
        totalPages = Mathf.CeilToInt((float)itemCount / itemShowcases.Count);
        if (totalPages < 1) totalPages = 1;

        if (currentPage > totalPages) currentPage = totalPages;
    }

    private void UpdatePageButtons(int itemCount)
    {
        bool isOnFirstPage = currentPage == 1;
        bool isOnLastPage = currentPage == totalPages;

        previousButton.interactable = !isOnFirstPage;
        nextButton.interactable = !isOnLastPage;

        previousText.color = isOnFirstPage ? inactiveColor : activeColor;
        nextText.color = isOnLastPage ? inactiveColor : activeColor;

        pageNumberText.text = $"{currentPage} / {totalPages}";
    }

    public void OnPreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            RefreshKeysInventory();
        }
    }

    public void OnNextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            RefreshKeysInventory();
        }
    }

    // Every dropdown sorting option
    public void OnSortOptionChanged(int optionIndex)
    {
        switch (optionIndex)
        {
            case 0: // Newest
                // Put sorting by newest here
                break;
            case 1: // Increasing Rarity
                // Put sorting by increasing rarity here
                break;
            case 2: // Decreasing Rarity
                // Put sorting by decreasing rarity here
                break;
            case 3: // Alphabetic A-Z
                keysInventory = keysInventory.OrderBy(k => k.caseName).ToList();
                break;
            case 4: // Alphabetic Z-A
                keysInventory = keysInventory.OrderByDescending(k => k.caseName).ToList();
                break;
        }

        currentPage = 1; // Reset to the first page after sorting
        RefreshKeysInventory(); // Refresh inventory display
    }
}

// Helper classes to represent Key and Skin objects
[System.Serializable]
public class Key
{
    public string caseName;
    public Sprite keyImage;
}

[System.Serializable]
public class Skin
{
    public string skinName;
    public Sprite skinImage;
}
