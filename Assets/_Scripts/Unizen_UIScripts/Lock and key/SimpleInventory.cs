using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SimpleInventory : MonoBehaviour
{
   
    [SerializeField]
    private TMP_Text keysText;
    public static SimpleInventory instance;

    private List<string> ObjNames;
    private void Awake()
    {
        instance = this;
        ObjNames = new List<string>();
    }

    public void AddItem(string name)
    {
        if (!ObjNames.Contains(name))
        {
            ObjNames.Add(name);
            Debug.Log("Added " + name);
            if (name == "10") {
                keysText.text = "Objective: Escape!";
            } else {
                keysText.text = "Objective: Find Keys " + ObjNames.Count + "/10";
            }
        }
    }

    public bool CheckKey(string name)
    {
        foreach (string str in ObjNames)
        {
            if (str == name)
                return true;
        }
        return false;
    }
}
