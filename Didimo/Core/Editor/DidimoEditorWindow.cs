using System;
using System.Collections.Generic;
using DigitalSalmon;
using DigitalSalmon.Extensions;
using DigitalSalmon.UI;
using UnityEditor;
using UnityEngine;

public abstract class DidimoEditorWindow : EditorWindow
{
    protected const int HEADER_HEIGHT = 76;
    protected const int PADDING       = 10;
    protected const int PADDING_SMALL = 5;

    protected static Color HEADER_BACKGROUND => Colours.GREY16;
    protected static Color ACCENT => Colours.BLUE_VIBRANT;

    protected static Color LABEL_H1 => Colours.WHITE;
    protected static Color LABEL_H2 => Colours.GREY90;
    protected static Color LABEL_BODY => Colours.GREY70;

    private readonly Queue<Action> layoutQueue  = new Queue<Action>();
    private readonly Queue<Action> repaintQueue = new Queue<Action>();

    public static bool MouseIsDown { get; private set; }

    protected virtual void OnGUI()
    {
        if (Event.current.type == EventType.Layout) DequeueActions(layoutQueue);
        if (Event.current.type == EventType.Repaint) DequeueActions(repaintQueue);

        if (Event.current.type != EventType.MouseDown) return;
        GUI.FocusControl(null);
        GUIUtility.hotControl = 0;
    }

    protected void DrawDividerText(ref Rect area, string label)
    {
        const int DIVIDER_HEIGHT = 30;
        const int DIVIDER_LINE_HEIGHT = 2;

        area = area.WithTrimY(PADDING, RectAnchor.Bottom);

        GUIStyle style = Style.Text.GetStyle(TextAnchor.MiddleCenter, FontWeights.Bold);
        style.CalcMinMaxWidth(new GUIContent(label), out float minWidth, out float maxWidth);

        Rect fullArea = area.WithHeight(DIVIDER_HEIGHT);
        float lineWidth = fullArea.width / 2 - maxWidth / 2 - PADDING - PADDING;

        Style.GUI.Box(fullArea.WithOffsetX(PADDING).WithWidth(lineWidth).WithHeight(DIVIDER_LINE_HEIGHT, RectAnchor.Center), Colours.GREY40);
        Style.GUI.Box(fullArea.WithOffsetX(-PADDING).WithWidth(lineWidth, RectAnchor.Right).WithHeight(DIVIDER_LINE_HEIGHT, RectAnchor.Center), Colours.GREY40);
        Style.GUI.Label(fullArea, label, LABEL_H2, style);

        area = area.WithTrimY(DIVIDER_HEIGHT, RectAnchor.Bottom);
    }

    protected void DrawHeader(ref Rect area, string headerLabel, string subheaderLabel)
    {
        Rect headerArea = area.WithHeight(HEADER_HEIGHT);
        Style.GUI.Box(headerArea, HEADER_BACKGROUND);
        Rect textArea = headerArea.WithPadding(PADDING);

        GUILayout.BeginArea(textArea);
        Style.GUI.LayoutLabel(headerLabel, LABEL_H1, TextAnchor.MiddleLeft, FontWeights.Bold, Style.Text.FontSize.Header);
        Style.GUI.LayoutLabel(subheaderLabel, LABEL_BODY, TextAnchor.MiddleLeft);
        GUILayout.EndArea();

        Style.GUI.Box(headerArea.WithHeight(3, RectAnchor.Bottom), ACCENT);

        area = area.WithTrimY(HEADER_HEIGHT + PADDING, RectAnchor.Bottom);
    }

    protected bool DrawFullWidthButton(ref Rect area, string label)
    {
        Rect buttonArea = area.WithHeight(60).WithPadding(PADDING);
        bool clicked = Style.GUI.Button(buttonArea, label, FontWeights.Bold);
        area = area.WithTrimY(buttonArea.height + PADDING_SMALL, RectAnchor.Bottom);
        return clicked;
    }

    protected void DrawFooter(ref Rect area)
    {
        Rect footerArea = area.WithHeight(10, RectAnchor.Bottom);
        Style.GUI.Box(footerArea, Colours.GREY18);
    }

    protected void EnqueueLayoutAction(Action action) { layoutQueue.Enqueue(action); }

    protected void EnqueueRepaintAction(Action action) { repaintQueue.Enqueue(action); }

    private void DequeueActions(Queue<Action> actionQueue)
    {
        while (actionQueue.Count > 0) actionQueue.Dequeue()();
    }
}