using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Case", menuName = "Case")]
public class Case : ScriptableObject
{
    public string caseName;
    public Sprite caseImage;
    public Sprite keyImage;

    // I want to make it so that each case will keep track of how many keys they have
    // This makes it so that for each case you need to buy the specific keys
    // Currently does nothing / haven't worked on it yet
    public int keys; 

    public List<ShowcaseWeapon> caseSkins;
}
