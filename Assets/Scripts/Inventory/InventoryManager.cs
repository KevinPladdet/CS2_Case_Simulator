﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public List<GameObject> itemShowcases; // List of all itemShowcases (should be 45)
    public List<Case> cases; // List of all available cases
    public List<Skin> skins; // List of all available skins

    public List<Key> keysInventory = new List<Key>(); // List to store keys in the inventory
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

    // Rarity colors
    private readonly Dictionary<Color32, int> rarityOrder = new Dictionary<Color32, int>()
    {
        { new Color32(177, 197, 218, 255), 0 }, // Grey
        { new Color32(81, 103, 241, 255), 1 }, // Blue
        { new Color32(132, 73, 247, 255), 2 }, // Purple
        { new Color32(190, 48, 205, 255), 3 }, // Pink
        { new Color32(206, 73, 74, 255), 4 }, // Red
        { new Color32(239, 215, 55, 255), 5 }  // Gold
    };

    private void Start()
    {
        sortDropdown.onValueChanged.AddListener(OnSortOptionChanged); // Add listener for dropdown changes
        RefreshKeysInventory();
    }

    // Method to populate the inventory UI with keys for the current page
    private void PopulateInventory()
    {
        int showcaseIndex = 0;
        int startKeyIndex = (currentPage - 1) * itemShowcases.Count;

        // Deactivate all showcases first
        foreach (GameObject showcase in itemShowcases)
        {
            showcase.SetActive(false);
        }

        // Loop through the keys and display them for the current page
        for (int i = startKeyIndex; i < startKeyIndex + itemShowcases.Count && i < keysInventory.Count; i++)
        {
            UpdateCaseUI(itemShowcases[showcaseIndex], keysInventory[i]);
            itemShowcases[showcaseIndex].SetActive(true);
            showcaseIndex++;
        }

        UpdatePageButtons(keysInventory.Count);
    }

    // Updates the image, name, and rarity color of the key
    private void UpdateCaseUI(GameObject itemShowcase, Key key)
    {
        Image keyImage = itemShowcase.transform.Find("SkinImage").GetComponent<Image>();
        TMP_Text keyNameText = itemShowcase.transform.Find("ItemTypeText").GetComponent<TMP_Text>();
        Image rarityImage = itemShowcase.transform.Find("RarityImage").GetComponent<Image>();

        keyImage.sprite = key.keyImage;
        keyNameText.text = key.caseName + " Key";

        // Set the RarityImage color to grey for keys
        rarityImage.color = new Color32(177, 197, 218, 255); // Grey color
    }

    // Refreshes the keys inventory UI
    public void RefreshKeysInventory()
    {
        OnSortOptionChanged(sortDropdown.value); // Saves sorting index
    }

    public void RefreshSortKeysInventory()
    {
        CalculateTotalPages(keysInventory.Count);
        PopulateInventory();
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
            RefreshSortKeysInventory();
        }
    }

    public void OnNextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            RefreshSortKeysInventory();
        }
    }

    // Dropdown sorting options
    public void OnSortOptionChanged(int optionIndex)
    {
        switch (optionIndex)
        {
            case 0: // Newest
                // Sorting by newest will be implemented later
                Debug.Log("sorting newest");
                BuildKeysInventory();
                break;
            case 1: // Increasing Rarity
                Debug.Log("sorting increasing rarity");
                BuildKeysInventory();
                keysInventory = keysInventory.OrderBy(k => GetRarityValue(k)).ToList();
                break;
            case 2: // Decreasing Rarity
                Debug.Log("sorting decreasing rarity");
                BuildKeysInventory();
                keysInventory = keysInventory.OrderByDescending(k => GetRarityValue(k)).ToList();
                break;
            case 3: // Alphabetic A-Z
                Debug.Log("sorting A-Z");
                BuildKeysInventory();
                keysInventory = keysInventory.OrderBy(k => k.caseName).ToList();
                break;
            case 4: // Alphabetic Z-A
                Debug.Log("sorting Z-A");
                BuildKeysInventory();
                keysInventory = keysInventory.OrderByDescending(k => k.caseName).ToList();
                break;
        }

        currentPage = 1; // Reset to the first page after sorting
        RefreshSortKeysInventory(); // Refresh inventory display
    }

    // Build the keysInventory list once
    private void BuildKeysInventory()
    {
        keysInventory.Clear();

        // Rebuild keysInventory from the current cases
        foreach (Case caseItem in cases)
        {
            for (int i = 0; i < caseItem.keys; i++)
            {
                keysInventory.Add(new Key { caseName = caseItem.caseName, keyImage = caseItem.keyImage });
            }
        }
    }

    // Helper method to get the rarity value based on the color
    private int GetRarityValue(Key key)
    {
        // Find the corresponding showcase to get the RarityImage color
        var itemShowcase = itemShowcases.FirstOrDefault(showcase =>
            showcase.activeSelf &&
            showcase.transform.Find("ItemTypeText").GetComponent<TMP_Text>().text == key.caseName + " Key");

        if (itemShowcase != null)
        {
            Color32 rarityColor = itemShowcase.transform.Find("RarityImage").GetComponent<Image>().color;
            if (rarityOrder.TryGetValue(rarityColor, out int rarityValue))
            {
                return rarityValue;
            }
        }

        return int.MaxValue; // If no matching color is found, put it at the end of the list
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