﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

/// <summary>
/// Base class for setting panels that use a list of elements.
/// </summary>
public abstract class BaseElementListSettingsPanel : BaseSettingsPanel
{
    public TMP_InputField monitorInput;
    public GameObject ElementListContainer;
    public GameObject ElementPrefab;
    public TMP_InputField audioInput;
    public TMP_InputField speechBubbleInput;
    public Toggle automaticAudioToggle;
    public TMP_Dropdown skippableDropdown;
    public Transform warningContainer;
    public TextMeshProUGUI warningMessage;

    protected string labelTemplate;
    protected int maxNumObjects;

    protected void Awake()
    {
        monitorInput.onEndEdit.AddListener(delegate { SaveSettings(); });
        InitInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
        warningContainer.gameObject.SetActive(false);
    }

    protected override void SetUpByProperties()
    {
        SetSkippableDropdown(skippableDropdown);
        SetTextInputField(monitorInput, "textMonitor");
        List<JObject> sortableObjects = GetSortableObjects("items");
        ElementListContainer.transform.DestroyImmediateAllChildren("Preview");
        // no sortable object available, instantiate the first empty container for adding some.
        if (sortableObjects.Count == 0)
            InitElement(false,true,true);
        else
            for (int i = 0; i < sortableObjects.Count; i++)
                InitElement(i == maxNumObjects - 1, i == sortableObjects.Count - 1, sortableObjects.Count == 1,
                    sortableObjects[i]);
        SetInstructorSettings(audioInput, speechBubbleInput, automaticAudioToggle);
    }

    protected override void SetJSON()
    {
        SubTask subTask = relatedSubTaskContainer.SubTaskData;
        JObject json = new JObject
        {
            {"textMonitor", monitorInput.text}
        };
        SetSkippableDropdownJSON(json, skippableDropdown);
        SetInstructorJSON(json, audioInput, speechBubbleInput, automaticAudioToggle);
        JArray sortablesJson = new JArray();
        foreach (Transform child in ElementListContainer.transform)
        {
            BaseListElement container = child.GetComponent<BaseListElement>();
            if (!container.ValuesMissing())
            {
                sortablesJson.Add(container.SetItemJson());
            }
        }
        json.Add("items", sortablesJson);
        subTask.properties = json.ToString();
    }
    
    protected virtual void AdditionalInitElement(BaseListElement newContainer)
    {
    }

    protected abstract void InitElement(bool isMax, bool isLast, bool isSingle, JObject element = null);
    

    private void UpdateAllSelectionElements()
    {
        if (ElementListContainer.transform.childCount < 1)
            InitElement(false, true, true);
        else
        {
            foreach (Transform element in ElementListContainer.transform)
            {
                BaseListElement container = element.GetComponent<BaseListElement>();
                bool isLastElement = element.GetSiblingIndex() == ElementListContainer.transform.childCount - 1;
                bool isMaxElement = element.GetSiblingIndex() == maxNumObjects - 1;
                container.btnAdd.gameObject.SetActive(isLastElement && !isMaxElement);
                container.btnRemove.gameObject.SetActive(ElementListContainer.transform.childCount > 1);
                container.label.text = string.Format(labelTemplate, element.GetSiblingIndex() + 1);
            }
        }
    }

    protected void AddSelectionElement()
    {
        InitElement(false,false,false);
        UpdateAllSelectionElements();
    }

    protected void RemoveSelectionElement(GameObject element)
    {
        DestroyImmediate(element);
        UpdateAllSelectionElements();
    }
}