using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HashTableTest : MonoBehaviour
{
    private OpenAddressingHashTable<string, int> open;
    private ChainingHashTable<string, int> chain;

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
       open = new OpenAddressingHashTable<string, int>();
    }


}
