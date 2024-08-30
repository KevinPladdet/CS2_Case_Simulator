using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public List<GameObject> itemShowcases; // List of all itemShowcases (should be 45)
    public List<Case> cases; // List of all available cases

    public TMP_Text pageNumberText;
    public Button previousButton;
    public Button nextButton;
    public TMP_Text previousText;
    public TMP_Text nextText;

    private int currentPage = 1;
    private int totalPages = 1;

    // Page button colors
    public Color activeColor;
    public Color inactiveColor;

    private void Start()
    {
        CalculateTotalPages();
        PopulateInventory(); // Populate the inventory UI with keys
        UpdatePageButtons(); // Update button states and text
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

        for (int i = startKeyIndex; i < cases.Count && showcaseIndex < itemShowcases.Count; i++)
        {
            Case caseItem = cases[i];
            for (int j = 0; j < caseItem.keys; j++)
            {
                if (showcaseIndex < itemShowcases.Count)
                {
                    UpdateCaseUI(itemShowcases[showcaseIndex], caseItem);
                    itemShowcases[showcaseIndex].SetActive(true);
                    showcaseIndex++;
                }
            }
        }

        UpdatePageButtons();
    }

    // Updates the image and name of the key
    private void UpdateCaseUI(GameObject itemShowcase, Case caseItem)
    {
        Image keyImage = itemShowcase.transform.Find("SkinImage").GetComponent<Image>();
        TMP_Text keyNameText = itemShowcase.transform.Find("ItemTypeText").GetComponent<TMP_Text>();

        keyImage.sprite = caseItem.keyImage;
        keyNameText.text = caseItem.caseName + " Key";
    }

    // Method to refresh the inventory UI
    public void RefreshInventory()
    {
        CalculateTotalPages();
        PopulateInventory();
    }

    // Calculates how many pages there will be in total based on how many keys there are
    private void CalculateTotalPages()
    {
        int totalKeys = 0;

        foreach (Case caseItem in cases)
        {
            totalKeys += caseItem.keys;
        }

        totalPages = Mathf.CeilToInt((float)totalKeys / itemShowcases.Count);
        if (totalPages < 1) totalPages = 1;

        if (currentPage > totalPages) currentPage = totalPages;
    }

    private void UpdatePageButtons()
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
            PopulateInventory();
        }
    }

    public void OnNextPage()
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            PopulateInventory();
        }
    }
}