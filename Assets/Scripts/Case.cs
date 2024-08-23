using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Case", menuName = "Case")]
public class Case : ScriptableObject
{
    public string caseName;
    public Sprite caseImage;
    public List<ShowcaseWeapon> caseSkins;
}