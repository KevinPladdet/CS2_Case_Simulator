using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public List<GameObject> itemShowcases; // List of all itemShowcases (should be 45)

    public List<Case> cases; // List of all available cases

    private void Start()
    {
        PopulateInventory(); // Populate the inventory UI with keys
    }

    // Method to populate the inventory UI with keys
    private void PopulateInventory()
    {
        int showcaseIndex = 0;

        foreach (Case caseItem in cases)
        {
            // Each key will be filled in an ItemShowcase
            for (int i = 0; i < caseItem.keys; i++)
            {
                if (showcaseIndex < itemShowcases.Count)
                {
                    UpdateCaseUI(itemShowcases[showcaseIndex], caseItem);
                    showcaseIndex++;
                }
                else
                {
                    Debug.LogWarning("Not enough ItemShowcases to display all keys.");
                    return;
                }
            }
        }

        // Deactivate unused ItemShowcases
        for (int i = showcaseIndex; i < itemShowcases.Count; i++)
        {
            itemShowcases[i].SetActive(false);
        }
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
        // Clear current showcases
        foreach (GameObject itemShowcase in itemShowcases)
        {
            itemShowcase.SetActive(false);
        }

        // Repopulate the inventory
        PopulateInventory();
    }
}
