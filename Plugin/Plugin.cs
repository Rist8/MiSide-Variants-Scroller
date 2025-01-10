using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VariantsScroller : MonoBehaviour
{

    static GameObject locationClothes = null;
    static string currentSceneName = null;
    static int previousVariantIndex = -1;
    static string prevCaseName = null;
    static bool clothCaseChanged = true;

    private void Start()
    {
        SceneManager.sceneLoaded += ((UnityAction<Scene, LoadSceneMode>)OnSceneChanged);
    }

    void Update()
    {
        if (currentSceneName == "SceneMenu" && locationClothes.activeInHierarchy
            && UnityEngine.Input.GetMouseButtonUp(0))
        {
            foreach (MenuClothes_Case caseCloth in locationClothes.GetComponentsInChildren<MenuClothes_Case>(true))
            {
                if (caseCloth.open)
                {
                    if (prevCaseName != GlobalGame.clothMita)
                    {
                        prevCaseName = GlobalGame.clothMita;
                        clothCaseChanged = true;
                        previousVariantIndex = -1;
                    }
                    else
                    {
                        clothCaseChanged = false;
                    }

                    var variantsList = caseCloth.gameObject.transform.Find("VariantsList");
                    variantsList.gameObject.SetActive(true);

                    // Hide the highlight on the previously selected variant
                    if (!clothCaseChanged && previousVariantIndex >= 0 && GlobalGame.clothVariantMita != previousVariantIndex)
                    {
                        Transform prevVariant = variantsList.GetChild(0).GetChild(0).GetChild(previousVariantIndex);
                        prevVariant.Find("Highlight").gameObject.SetActive(false);
                    }

                    // Show the highlight on the newly selected variant
                    if (!clothCaseChanged)
                    {
                        Transform currentVariant = variantsList.GetChild(0).GetChild(0).GetChild(GlobalGame.clothVariantMita);
                        currentVariant.Find("Highlight").gameObject.SetActive(true);
                    }

                    previousVariantIndex = GlobalGame.clothVariantMita;
                    prevCaseName = GlobalGame.clothMita;
                }
                else
                {
                    caseCloth.gameObject.transform.Find("VariantsList").gameObject.SetActive(false);
                }
            }
        }
    }


    void OnSceneChanged(Scene current, LoadSceneMode mode)
    {
        currentSceneName = current.name;
        if (currentSceneName == "SceneMenu")
        {
            AddScrollMenu();
        }
    }

    void AddScrollMenu()
    {
        // Locate the main container for clothes
        locationClothes = Reflection.FindObjectsOfType<MenuClothes>(true)[0].gameObject;
        if (locationClothes == null)
        {
            Debug.LogError("locationClothes not found!");
            return;
        }

        var parent = locationClothes.transform.parent;

        // Find the ScrollRect template
        var locationOptionsChange = parent.Find("Location OptionsChange");
        var scrollRect = locationOptionsChange?.GetComponentInChildren<ScrollRect>();
        if (scrollRect == null)
        {
            Debug.LogError("ScrollRect template not found!");
            return;
        }
        var templateVariantsList = scrollRect.gameObject;

        // Loop through all clothing cases
        foreach (MenuClothes_Case caseCloth in locationClothes.GetComponentsInChildren<MenuClothes_Case>(true))
        {
            // Create a new ScrollRect for each case
            var variantsList = GameObject.Instantiate(templateVariantsList, caseCloth.transform);
            variantsList.name = "VariantsList";

            // Configure RectTransform
            var rect = variantsList.GetComponent<RectTransform>();
            ConfigureRectTransform(rect);

            // Find and configure content container
            var content = variantsList.GetComponentInChildren<ScrollRect>().content;
            if (content == null)
            {
                Debug.LogError("Content container missing in template!");
                continue;
            }
            ConfigureContent(content);

            UnityEngine.Component.Destroy(caseCloth.transform.FindChild("VairantChange").gameObject.GetComponent<Image>());

            // Add variants to content
            AddVariantsToContent(caseCloth, content);

            // Activate the ScrollRect after setup
            variantsList.SetActive(true);
        }
    }

    // Helper to configure RectTransform
    void ConfigureRectTransform(RectTransform rect)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1); // Top-left anchor
        rect.anchoredPosition3D = new Vector3(-80, 0, 0);
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.sizeDelta = new Vector2(64, 300);
    }

    // Helper to configure content
    void ConfigureContent(Transform content)
    {
        content.transform.localPosition = Vector3.zero;

        UnityEngine.Object.Destroy(content.GetComponent<MenuScrolac>());
        UnityEngine.Object.Destroy(content.Find("Change").gameObject);
        UnityEngine.Object.Destroy(content.Find("ChangeTarget").gameObject);
        UnityEngine.Object.Destroy(content.Find("ButtonChange").gameObject);

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 0;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Helper to add variants to content
    void AddVariantsToContent(MenuClothes_Case caseCloth, Transform content)
    {
        foreach (MenuCloth_CaseVariant variant in caseCloth.GetComponentsInChildren<MenuCloth_CaseVariant>(true))
        {
            variant.gameObject.SetActive(true);
            variant.transform.SetParent(content, false);
            variant.transform.localScale = Vector3.one;

            // Remove any unnecessary elements
            UnityEngine.Object.Destroy(variant.transform.Find("Back")?.gameObject);
            // Get the Image component for the variant and set its default properties
            var image = variant.GetComponent<Image>();
            image.color = Color.red;
            image.transform.localScale = Vector3.one;

            // Configure the variant's RectTransform
            var buttonRect = variant.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = Vector2.zero;
            buttonRect.sizeDelta = new Vector2(64, 64);

            // Add a highlight image (child object)
            GameObject highlight = new GameObject("Highlight");
            var highlightImage = highlight.AddComponent<Image>();

            // Assign a circular sprite to the highlight image
            highlightImage.sprite = image.sprite; // Replace with your sprite path
            highlightImage.color = new Color(1, 1, 1, 0.15f); // Yellow, semi-transparent
            highlightImage.raycastTarget = false; // Prevent blocking clicks
            UnityEngine.Component.Destroy(image);

            // Set highlight object as a child of the variant and configure its RectTransform
            highlight.transform.SetParent(variant.transform, false);
            var highlightRect = highlight.GetComponent<RectTransform>();
            highlightRect.anchorMin = new Vector2(0.5f, 0.5f);
            highlightRect.anchorMax = new Vector2(0.5f, 0.5f);
            highlightRect.pivot = new Vector2(0.5f, 0.5f);
            highlightRect.anchoredPosition = Vector2.zero;
            highlightRect.sizeDelta = new Vector2(68, 68); // Slightly larger than the variant

            // Initially hide the highlight
            highlight.SetActive(false);
        }
    }






}
