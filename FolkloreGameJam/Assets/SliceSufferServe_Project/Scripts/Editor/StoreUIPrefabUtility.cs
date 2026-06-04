#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class StoreUIPrefabUtility
{
    private const string PrefabFolder = "Assets/SliceSufferServe_Project/Prefabs/UI/Store";
    private const string StoreCanvasPath = "Assets/SliceSufferServe_Project/Prefabs/UI/StoreCanvas.prefab";
    private const string AutoCreateSessionKey = "SliceSufferServe.StoreUIPrefabUtility.AutoCreatedV2";

    static StoreUIPrefabUtility()
    {
        EditorApplication.delayCall += AutoCreateTemplatesIfMissing;
    }

    [MenuItem("Slice Suffer Serve/Store/Create And Assign Store UI Prefabs")]
    public static void CreateAndAssignStoreUIPrefabs()
    {
        EnsureFolder(PrefabFolder);

        StoreItemsContentView contentPrefab = SavePrefab(CreateContentView(), $"{PrefabFolder}/StoreItemsContent.prefab").GetComponent<StoreItemsContentView>();
        TextMeshProUGUI sectionTitlePrefab = SavePrefab(CreateSectionTitle(), $"{PrefabFolder}/StoreSectionTitle.prefab").GetComponent<TextMeshProUGUI>();
        StoreItemRowView humanRowPrefab = SavePrefab(CreateHumanRow(), $"{PrefabFolder}/HumanStoreItemRow.prefab").GetComponent<StoreItemRowView>();
        StoreItemRowView weaponRowPrefab = SavePrefab(CreateWeaponRow(), $"{PrefabFolder}/WeaponStoreItemRow.prefab").GetComponent<StoreItemRowView>();

        AssignTemplatesToStoreCanvas(contentPrefab, sectionTitlePrefab, humanRowPrefab, weaponRowPrefab);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created and assigned Store UI prefabs.");
    }

    private static void AutoCreateTemplatesIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        GameObject storeCanvas = AssetDatabase.LoadAssetAtPath<GameObject>(StoreCanvasPath);
        if (storeCanvas == null)
        {
            return;
        }

        bool hasTemplates = storeCanvas.transform.Find("StoreUITemplates") != null;
        bool hasHumanTab = FindDescendant(storeCanvas.transform, "HumanTabButton") != null;
        bool hasHumanPanel = FindDescendant(storeCanvas.transform, "HumanPanel") != null;
        if (!hasTemplates || !hasHumanTab || !hasHumanPanel)
        {
            if (SessionState.GetBool(AutoCreateSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(AutoCreateSessionKey, true);
            CreateAndAssignStoreUIPrefabs();
        }
    }

    private static void AssignTemplatesToStoreCanvas(
        StoreItemsContentView contentPrefab,
        TextMeshProUGUI sectionTitlePrefab,
        StoreItemRowView humanRowPrefab,
        StoreItemRowView weaponRowPrefab)
    {
        GameObject storeCanvas = PrefabUtility.LoadPrefabContents(StoreCanvasPath);
        if (storeCanvas == null)
        {
            Debug.LogWarning("Created Store UI prefabs, but could not load StoreCanvas.prefab.");
            return;
        }

        try
        {
            StoreUIManager manager = storeCanvas.GetComponentInChildren<StoreUIManager>(true);
            if (manager == null)
            {
                Debug.LogWarning("Created Store UI prefabs, but could not find StoreUIManager on StoreCanvas.prefab.");
                return;
            }

            Button humanTabButton = EnsureHumanTabButton(storeCanvas.transform);
            GameObject humanPanel = EnsureHumanPanel(storeCanvas.transform);

            Transform templatesRoot = FindChild(storeCanvas.transform, "StoreUITemplates");
            if (templatesRoot == null)
            {
                GameObject templatesObject = new GameObject("StoreUITemplates", typeof(RectTransform));
                templatesObject.transform.SetParent(storeCanvas.transform, false);
                templatesRoot = templatesObject.transform;
            }

            templatesRoot.gameObject.SetActive(false);
            ClearChildren(templatesRoot);

            StoreItemsContentView contentTemplate = InstantiateTemplate(contentPrefab, templatesRoot, "StoreItemsContentTemplate");
            TextMeshProUGUI sectionTitleTemplate = InstantiateTemplate(sectionTitlePrefab, templatesRoot, "StoreSectionTitleTemplate");
            StoreItemRowView humanRowTemplate = InstantiateTemplate(humanRowPrefab, templatesRoot, "HumanStoreItemRowTemplate");
            StoreItemRowView weaponRowTemplate = InstantiateTemplate(weaponRowPrefab, templatesRoot, "WeaponStoreItemRowTemplate");

            SerializedObject serializedManager = new SerializedObject(manager);
            EnsureTab(serializedManager, "human", humanTabButton, humanPanel);
            serializedManager.FindProperty("storeItemsContentPrefab").objectReferenceValue = contentTemplate;
            serializedManager.FindProperty("sectionTitlePrefab").objectReferenceValue = sectionTitleTemplate;
            serializedManager.FindProperty("humanItemPrefab").objectReferenceValue = humanRowTemplate;
            serializedManager.FindProperty("weaponItemPrefab").objectReferenceValue = weaponRowTemplate;
            serializedManager.ApplyModifiedPropertiesWithoutUndo();

            RepositionStoreTabButtons(serializedManager);

            PrefabUtility.SaveAsPrefabAsset(storeCanvas, StoreCanvasPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(storeCanvas);
        }
    }

    private static GameObject CreateContentView()
    {
        GameObject scrollObject = new GameObject("StoreItemsContent", typeof(RectTransform), typeof(ScrollRect), typeof(StoreItemsContentView));
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(60f, 36f);
        scrollRectTransform.offsetMax = new Vector2(-60f, -36f);

        GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewportObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewportObject.GetComponent<Mask>().showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        contentObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        StoreItemsContentView view = scrollObject.GetComponent<StoreItemsContentView>();
        view.ResolveReferences();
        return scrollObject;
    }

    private static GameObject CreateSectionTitle()
    {
        GameObject titleObject = new GameObject("StoreSectionTitle", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        TextMeshProUGUI title = titleObject.GetComponent<TextMeshProUGUI>();
        title.text = "Weapons";
        title.fontSize = 32;
        title.alignment = TextAlignmentOptions.Left;
        title.color = new Color(1f, 0.92f, 0.78f, 1f);
        title.raycastTarget = false;
        titleObject.GetComponent<LayoutElement>().preferredHeight = 44f;
        return titleObject;
    }

    private static GameObject CreateHumanRow()
    {
        GameObject row = CreateRowRoot("HumanStoreItemRow", 176f, new Color(0.08f, 0.07f, 0.06f, 0.78f), 18, 18, 12, 12, 18f);
        CreatePreview(row.transform, 190f, 148f);
        CreateRowText(row.transform, "TitleText", "Part Name", 26, TextAlignmentOptions.Left, new Color(0.95f, 0.9f, 0.82f, 1f), 1f, 148f);
        CreateRowText(row.transform, "PriceText", "1,000", 24, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f), 130f, 148f);
        CreateBuyButton(row.transform, 150f, 62f, 26);
        row.GetComponent<StoreItemRowView>().ResolveReferences();
        return row;
    }

    private static GameObject CreateWeaponRow()
    {
        GameObject row = CreateRowRoot("WeaponStoreItemRow", 198f, new Color(0.62f, 0.52f, 0.39f, 0.95f), 32, 32, 22, 22, 34f);
        row.GetComponent<LayoutElement>().preferredWidth = 1030f;
        CreatePreview(row.transform, 176f, 154f);
        CreateWeaponInfo(row.transform);
        CreateRowText(row.transform, "PriceText", "5,000", 44, TextAlignmentOptions.Center, new Color(1f, 0.82f, 0.36f, 1f), 145f, 78f);
        CreateBuyButton(row.transform, 160f, 78f, 41);
        row.GetComponent<StoreItemRowView>().ResolveReferences();
        return row;
    }

    private static GameObject CreateRowRoot(string name, float height, Color color, int left, int right, int top, int bottom, float spacing)
    {
        GameObject row = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(StoreItemRowView));
        row.GetComponent<Image>().color = color;

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(left, right, top, bottom);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        row.GetComponent<LayoutElement>().preferredHeight = height;
        return row;
    }

    private static void CreatePreview(Transform parent, float width, float height)
    {
        GameObject preview = new GameObject("Preview", typeof(RectTransform), typeof(LayoutElement), typeof(RectMask2D));
        preview.transform.SetParent(parent, false);
        preview.GetComponent<LayoutElement>().preferredWidth = width;
        preview.GetComponent<LayoutElement>().preferredHeight = height;

        GameObject imageObject = new GameObject("PreviewImage", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(preview.transform, false);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;
        imageObject.GetComponent<Image>().preserveAspect = true;
        imageObject.GetComponent<Image>().raycastTarget = false;
    }

    private static TextMeshProUGUI CreateRowText(Transform parent, string name, string value, int fontSize, TextAlignmentOptions alignment, Color color, float width, float height)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;

        LayoutElement layout = textObject.GetComponent<LayoutElement>();
        if (width <= 1f)
        {
            layout.flexibleWidth = width;
        }
        else
        {
            layout.preferredWidth = width;
        }

        layout.preferredHeight = height;
        return text;
    }

    private static void CreateWeaponInfo(Transform parent)
    {
        GameObject info = new GameObject("Info", typeof(RectTransform), typeof(LayoutElement));
        info.transform.SetParent(parent, false);
        info.GetComponent<LayoutElement>().preferredWidth = 450f;
        info.GetComponent<LayoutElement>().preferredHeight = 154f;

        TextMeshProUGUI title = CreateAbsoluteText(info.transform, "TitleText", "CLAW", 46, TextAlignmentOptions.Left, new Color(0.12f, 0.08f, 0.05f, 1f));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(0f, 58f);
        title.enableWordWrapping = false;
        title.overflowMode = TextOverflowModes.Overflow;

        TextMeshProUGUI description = CreateAbsoluteText(info.transform, "DescriptionText", "Sharp and deadly. A classic weapon for close encounters.", 30, TextAlignmentOptions.TopLeft, new Color(0.15f, 0.11f, 0.08f, 1f));
        RectTransform descriptionRect = description.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0f, 0f);
        descriptionRect.anchorMax = new Vector2(1f, 1f);
        descriptionRect.pivot = new Vector2(0f, 1f);
        descriptionRect.offsetMin = Vector2.zero;
        descriptionRect.offsetMax = new Vector2(0f, -58f);
        description.enableWordWrapping = true;
        description.overflowMode = TextOverflowModes.Overflow;
    }

    private static TextMeshProUGUI CreateAbsoluteText(Transform parent, string name, string value, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateBuyButton(Transform parent, float width, float height, int fontSize)
    {
        GameObject buttonObject = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<LayoutElement>().preferredWidth = width;
        buttonObject.GetComponent<LayoutElement>().preferredHeight = height;
        buttonObject.GetComponent<Image>().color = new Color(0.65f, 0.22f, 0.16f, 1f);

        TextMeshProUGUI text = CreateAbsoluteText(buttonObject.transform, "BuyButtonText", "Buy", fontSize, TextAlignmentOptions.Center, new Color(1f, 0.94f, 0.82f, 1f));
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    private static Button EnsureHumanTabButton(Transform storeCanvas)
    {
        Button existing = FindDescendant(storeCanvas, "HumanTabButton")?.GetComponent<Button>();
        if (existing != null)
        {
            SetButtonText(existing, "Human");
            return existing;
        }

        Transform tabsRoot = FindDescendant(storeCanvas, "Tabs");
        Button templateButton = FindDescendant(storeCanvas, "ItemsTabButton")?.GetComponent<Button>()
            ?? FindDescendant(storeCanvas, "DisableAdsTabButton")?.GetComponent<Button>();
        if (tabsRoot == null || templateButton == null)
        {
            return null;
        }

        GameObject buttonObject = Object.Instantiate(templateButton.gameObject, tabsRoot, false);
        buttonObject.name = "HumanTabButton";
        Button button = buttonObject.GetComponent<Button>();
        SetButtonText(button, "Human");

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.name = "HumanTabText";
        }

        return button;
    }

    private static GameObject EnsureHumanPanel(Transform storeCanvas)
    {
        Transform existing = FindDescendant(storeCanvas, "HumanPanel");
        if (existing != null)
        {
            return existing.gameObject;
        }

        Transform contentRoot = FindDescendant(storeCanvas, "ContentRoot");
        GameObject templatePanel = FindDescendant(storeCanvas, "ItemsPanel")?.gameObject
            ?? FindDescendant(storeCanvas, "DisableAdsPanel")?.gameObject;
        if (contentRoot == null || templatePanel == null)
        {
            return null;
        }

        GameObject panel = new GameObject("HumanPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(contentRoot, false);

        RectTransform templateRect = templatePanel.GetComponent<RectTransform>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (templateRect != null)
        {
            panelRect.anchorMin = templateRect.anchorMin;
            panelRect.anchorMax = templateRect.anchorMax;
            panelRect.pivot = templateRect.pivot;
            panelRect.anchoredPosition = templateRect.anchoredPosition;
            panelRect.sizeDelta = templateRect.sizeDelta;
            panelRect.offsetMin = templateRect.offsetMin;
            panelRect.offsetMax = templateRect.offsetMax;
        }

        Image image = panel.GetComponent<Image>();
        Image templateImage = templatePanel.GetComponent<Image>();
        image.color = templateImage != null ? templateImage.color : new Color(0.13f, 0.18f, 0.15f, 0.96f);
        panel.SetActive(false);
        return panel;
    }

    private static void EnsureTab(SerializedObject serializedManager, string tabId, Button tabButton, GameObject panel)
    {
        if (tabButton == null || panel == null)
        {
            return;
        }

        SerializedProperty tabsProperty = serializedManager.FindProperty("tabs");
        for (int i = 0; i < tabsProperty.arraySize; i++)
        {
            SerializedProperty element = tabsProperty.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("tabId").stringValue == tabId)
            {
                element.FindPropertyRelative("tabButton").objectReferenceValue = tabButton;
                element.FindPropertyRelative("panel").objectReferenceValue = panel;
                return;
            }
        }

        tabsProperty.InsertArrayElementAtIndex(tabsProperty.arraySize);
        SerializedProperty newElement = tabsProperty.GetArrayElementAtIndex(tabsProperty.arraySize - 1);
        newElement.FindPropertyRelative("tabId").stringValue = tabId;
        newElement.FindPropertyRelative("tabButton").objectReferenceValue = tabButton;
        newElement.FindPropertyRelative("panel").objectReferenceValue = panel;
    }

    private static void RepositionStoreTabButtons(SerializedObject serializedManager)
    {
        serializedManager.Update();
        SerializedProperty tabsProperty = serializedManager.FindProperty("tabs");
        float spacing = 310f;
        float startX = -spacing * (tabsProperty.arraySize - 1) * 0.5f;

        for (int i = 0; i < tabsProperty.arraySize; i++)
        {
            Button button = tabsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("tabButton").objectReferenceValue as Button;
            RectTransform rectTransform = button != null ? button.GetComponent<RectTransform>() : null;
            if (rectTransform == null)
            {
                continue;
            }

            rectTransform.anchorMin = new Vector2(0.5f, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(0.5f, rectTransform.anchorMax.y);
            rectTransform.anchoredPosition = new Vector2(startX + spacing * i, rectTransform.anchoredPosition.y);
        }
    }

    private static void SetButtonText(Button button, string value)
    {
        if (button == null)
        {
            return;
        }

        TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = value;
        }
    }

    private static T InstantiateTemplate<T>(T prefabComponent, Transform parent, string name) where T : Component
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabComponent.gameObject, parent);
        instance.name = name;
        T component = instance.GetComponent<T>();

        if (component is StoreItemsContentView contentView)
        {
            contentView.ResolveReferences();
        }
        else if (component is StoreItemRowView rowView)
        {
            rowView.ResolveReferences();
        }

        return component;
    }

    private static Transform FindChild(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child != null && child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform FindDescendant(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindDescendant(parent.GetChild(i), childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static GameObject SavePrefab(GameObject instance, string path)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
        Object.DestroyImmediate(instance);
        return prefab;
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
