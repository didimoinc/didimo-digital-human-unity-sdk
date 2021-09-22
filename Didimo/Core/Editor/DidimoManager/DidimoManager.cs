using System.Collections.Generic;
using System.Data;
using System.Linq;
using DigitalSalmon;
using DigitalSalmon.Extensions;
using DigitalSalmon.UI;
using UnityEditor;
using UnityEngine;

public class DidimoManager : EditorWindow
{
    private static string doNotAutoOpenPlayerPrefsKey = "DIDIMO_MANAGER_DO_NOT_AUTO_OPEN_WINDOW";

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void DidReloadScripts()
    {
        bool shouldAutoOpen = PlayerPrefs.GetInt(doNotAutoOpenPlayerPrefsKey, 0) == 0;
        if (shouldAutoOpen)
        {
            ShowWindow();
            PlayerPrefs.SetInt(doNotAutoOpenPlayerPrefsKey, 1);
        }
    }

    protected const int HEADER_HEIGHT = 50;
    protected const int PADDING       = 10;
    protected const int PADDING_SMALL = 5;

    protected static Color HEADER_BACKGROUND => Colours.GREY16;
    protected static Color ACCENT => Colours.BLUE_VIBRANT;

    protected static Color LABEL_H1 => Colours.WHITE;

    [SerializeField]
    private int SelectedTab = 0;

    private static List<DidimoManagerTab> tabs = new List<DidimoManagerTab>();

    public static void AddTab(DidimoManagerTab tab)
    {
        tabs.Add(tab);
        tabs = tabs.OrderBy(x => x.GetIndex()).ToList();
    }

    protected void OnGUI()
    {
        Rect headerRect = new Rect(0, 0, position.width, HEADER_HEIGHT);
        GUILayout.Space(HEADER_HEIGHT + PADDING_SMALL);
        DrawHeader(ref headerRect, "Didimo Manager");
        SelectedTab = GUILayout.Toolbar(SelectedTab, tabs.Select(x => x.GetTabName()).ToArray());
        GUILayout.Space(PADDING);
        tabs[SelectedTab].Draw(this);
    }

    protected void DrawHeader(ref Rect area, string headerLabel)
    {
        Rect headerArea = area.WithHeight(HEADER_HEIGHT);
        Style.GUI.Box(headerArea, HEADER_BACKGROUND);
        Rect textArea = headerArea.WithPadding(PADDING);

        GUILayout.BeginArea(textArea);
        Style.GUI.LayoutLabel(headerLabel, LABEL_H1, TextAnchor.MiddleLeft, FontWeights.Bold, Style.Text.FontSize.Header);
        GUILayout.EndArea();

        Style.GUI.Box(headerArea.WithHeight(3, RectAnchor.Bottom), ACCENT);

        area = area.WithTrimY(HEADER_HEIGHT + PADDING, RectAnchor.Bottom);
    }

    [MenuItem("Didimo/Didimo Manager")]
    public static void ShowWindow()
    {
        DidimoManager window = GetWindow<DidimoManager>();
        window.minSize = new Vector2(200, 200);
    }
}