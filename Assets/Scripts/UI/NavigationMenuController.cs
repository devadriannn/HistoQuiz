using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NavigationMenuController : MonoBehaviour
{
    [Header("Sidebar Layout")]
    [SerializeField] private RectTransform sidebarRect;
    [SerializeField] private float fixedSidebarWidth = 260f;
    [SerializeField] private float contentInset = 24f;
    [SerializeField] private bool hideToggleButton = true;

    [Header("Menu Elements")]
    [SerializeField] private GameObject[] menuLabels;
    [SerializeField] private Button menuToggleButton;
    [SerializeField] private GameObject dashboardPanel;

    [Header("Scaling Elements")]
    [SerializeField] private RectTransform logoRect;
    [SerializeField] private Vector2 fixedLogoSize = new Vector2(120f, 120f);
    [SerializeField] private RectTransform[] buttonIcons;
    [SerializeField] private Vector2 fixedIconSize = new Vector2(32f, 32f);

    private void Start()
    {
        ApplyFixedLayout();
    }

    private void OnEnable()
    {
        ApplyFixedLayout();
    }

    public void OnMenuToggle()
    {
        ApplyFixedLayout();
    }

    public void SetMenuExpanded(bool expand)
    {
        ApplyFixedLayout();
    }

    private void ApplyFixedLayout()
    {
        if (sidebarRect == null)
        {
            sidebarRect = GetComponent<RectTransform>();
        }

        if (sidebarRect != null)
        {
            sidebarRect.anchorMin = new Vector2(0f, 0f);
            sidebarRect.anchorMax = new Vector2(0f, 1f);
            sidebarRect.pivot = new Vector2(0f, 0.5f);
            sidebarRect.anchoredPosition = Vector2.zero;
            sidebarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedSidebarWidth);
        }

        VerticalLayoutGroup sidebarLayout = GetComponent<VerticalLayoutGroup>();
        if (sidebarLayout != null)
        {
            sidebarLayout.padding = new RectOffset(18, 18, 24, 24);
            sidebarLayout.spacing = 14;
            sidebarLayout.childAlignment = TextAnchor.UpperCenter;
            sidebarLayout.childControlWidth = true;
            sidebarLayout.childControlHeight = false;
            sidebarLayout.childForceExpandWidth = true;
            sidebarLayout.childForceExpandHeight = false;
        }

        if (dashboardPanel != null)
        {
            RectTransform dashboardRect = dashboardPanel.GetComponent<RectTransform>();
            if (dashboardRect != null)
            {
                dashboardRect.anchorMin = Vector2.zero;
                dashboardRect.anchorMax = Vector2.one;
                dashboardRect.offsetMin = new Vector2(fixedSidebarWidth + contentInset, contentInset);
                dashboardRect.offsetMax = new Vector2(-contentInset, -contentInset);
            }
        }

        if (logoRect != null)
        {
            logoRect.sizeDelta = fixedLogoSize;
        }

        for (int i = 0; i < buttonIcons.Length; i++)
        {
            if (buttonIcons[i] != null)
            {
                buttonIcons[i].sizeDelta = fixedIconSize;
            }
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (menuToggleButton != null && buttons[i] == menuToggleButton)
            {
                continue;
            }

            LayoutElement layoutElement = buttons[i].GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = buttons[i].gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredHeight = 68f;
            layoutElement.flexibleWidth = 1f;

            HorizontalLayoutGroup buttonLayout = buttons[i].GetComponent<HorizontalLayoutGroup>();
            if (buttonLayout != null)
            {
                buttonLayout.childAlignment = TextAnchor.MiddleLeft;
                buttonLayout.padding = new RectOffset(18, 18, 0, 0);
                buttonLayout.spacing = 14;
                buttonLayout.childControlWidth = false;
                buttonLayout.childControlHeight = false;
                buttonLayout.childForceExpandWidth = false;
                buttonLayout.childForceExpandHeight = false;
            }

            TMP_Text label = buttons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.enableWordWrapping = false;
            }
        }

        if (menuToggleButton != null && hideToggleButton)
        {
            menuToggleButton.gameObject.SetActive(false);
        }

        for (int i = 0; i < menuLabels.Length; i++)
        {
            if (menuLabels[i] != null)
            {
                menuLabels[i].SetActive(true);
            }
        }
    }
}
