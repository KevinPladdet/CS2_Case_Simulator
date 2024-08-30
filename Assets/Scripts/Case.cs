using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Case", menuName = "Case")]
public class Case : ScriptableObject
{
    public string caseName;
    public Sprite caseImage;
    public Sprite keyImage;
    public int keys; // Tracks the number of keys available for this case
    public List<ShowcaseWeapon> caseSkins;

    // Method to add keys to the case
    public void AddKeys(int amount)
    {
        keys += amount;
    }

    // Method to use a key for this case
    public bool UseKey()
    {
        if (keys > 0)
        {
            keys--;
            return true;
        }
        return false;
    }
}
