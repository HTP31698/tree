using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashTableTest : MonoBehaviour
{
    private OpenAddressingHashTable<string, int> openAddressingHashTable;
    private ChainingHashTable<string, int> chainingHashTable;

    [Header("Dropdown")]
    public TMP_Dropdown First;
    public TMP_Dropdown Second;

    [Header("InputField")]
    public TMP_InputField KeyInput;
    public TMP_InputField ValueInput;

    [Header("Buttons")]
    public Button AddButton;
    public Button RemoveButton;
    public Button ClearButton;

    [Header("Text")]
    public TextMeshProUGUI Output;



    private void Start()
    {
       
    }

    private void openAddressingHashTableOn()
    {
        if (chainingHashTable != null)
        {
            chainingHashTable = null;
        }
        openAddressingHashTable = new OpenAddressingHashTable<string, int>();
    }
    private void chainingHashTableOn()
    {
        if (openAddressingHashTable != null)
        {
            openAddressingHashTable = null;
        }
        chainingHashTable = new ChainingHashTable<string, int>();
    }

}
