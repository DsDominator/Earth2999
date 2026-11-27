using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class CreateUpgradeStatsUI
{
    [MenuItem("Tools/Upgrade Menu/Create Stats UI")]
    public static void Create()
    {
        const string scenePath = "Assets/Scenes/SampleScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        var menu = Object.FindFirstObjectByType<UpgradeMenu>();
        if (menu == null)
            throw new System.InvalidOperationException("UpgradeMenu not found in SampleScene.");

        var serializedMenu = new SerializedObject(menu);
        var panel = serializedMenu.FindProperty("upgradePanel").objectReferenceValue as GameObject;
        if (panel == null)
            throw new System.InvalidOperationException("Upgrade panel is not assigned.");

        var existing = panel.transform.Find("PlayerStatsView");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);
        existing = panel.transform.Find("ViewStatsButton");
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);

        var statsButton = MakeButton("ViewStatsButton", "View Stats", panel.transform,
            new Vector2(1f, 0f), new Vector2(-120f, 24f));

        var statsView = new GameObject("PlayerStatsView", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        statsView.transform.SetParent(panel.transform, false);
        var viewRect = statsView.GetComponent<RectTransform>();
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = Vector2.zero;
        viewRect.offsetMax = Vector2.zero;
        var background = statsView.GetComponent<Image>();
        background.color = new Color(0.04f, 0.07f, 0.1f, 0.97f);

        var statsObject = new GameObject("PlayerStatsText", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        statsObject.transform.SetParent(statsView.transform, false);
        var statsRect = statsObject.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.5f, 0.5f);
        statsRect.anchorMax = new Vector2(0.5f, 0.5f);
        statsRect.pivot = new Vector2(0.5f, 0.5f);
        statsRect.anchoredPosition = new Vector2(0f, 20f);
        statsRect.sizeDelta = new Vector2(500f, 440f);
        var statsText = statsObject.GetComponent<TextMeshProUGUI>();
        statsText.font = TMP_Settings.defaultFontAsset;
        statsText.fontSize = 28f;
        statsText.color = Color.white;
        statsText.alignment = TextAlignmentOptions.Top;
        statsText.raycastTarget = false;

        var backButton = MakeButton("BackToUpgradesButton", "Back to Upgrades",
            statsView.transform, new Vector2(0f, 1f), new Vector2(28f, -28f));
        statsView.SetActive(false);

        serializedMenu.Update();
        serializedMenu.FindProperty("statsButton").objectReferenceValue = statsButton;
        serializedMenu.FindProperty("statsView").objectReferenceValue = statsView;
        serializedMenu.FindProperty("playerStatsDisplay").objectReferenceValue = statsText;
        serializedMenu.FindProperty("backButton").objectReferenceValue = backButton;
        serializedMenu.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Created editable stats UI in SampleScene.");
    }

    private static Button MakeButton(string name, string label, Transform parent,
        Vector2 anchor, Vector2 position)
    {
        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200f, 48f);
        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.23f, 0.81f, 0.56f, 1f);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        var labelObject = new GameObject("Label", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var text = labelObject.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.text = label;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 22f;
        text.raycastTarget = false;
        return button;
    }
}
