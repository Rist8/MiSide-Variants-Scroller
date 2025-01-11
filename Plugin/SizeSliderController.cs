using UnityEngine;
using UnityEngine.UI;

namespace VarinatsScroller
{
    public class SizeSliderController
    {
        public Slider sizeSlider; // Reference to the size slider UI
        public List<GameObject> variantsLists;
        public float minSize = 64f; // Minimum size for the variants
        public float maxSize = 96f; // Maximum size for the variants

        public SizeSliderController(Slider sizeSlider)
        {
            this.sizeSlider = sizeSlider;
            variantsLists = new List<GameObject>();
        }

        public void Start()
        {
            if (sizeSlider == null || variantsLists == null)
            {
                Debug.LogError("SizeSlider or VariantsContent is not assigned!");
                return;
            }

            // Initialize slider values
            sizeSlider.minValue = minSize;
            sizeSlider.maxValue = maxSize;
            sizeSlider.wholeNumbers = true;
            sizeSlider.value = 64f; // Start with the largest size
            sizeSlider.gameObject.transform.Find("Handle Slide Area")
                .Find("Handle").Find("Text").gameObject
                .GetComponent<Text>().text = "64";

            // Add listener to handle slider value changes
            sizeSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnSizeSliderChanged);
        }

        // Called whenever the slider value changes
        public void OnSizeSliderChanged(float newSize)
        {
            sizeSlider.gameObject.transform.Find("Handle Slide Area")
                .Find("Handle").Find("Text").gameObject
                .GetComponent<Text>().text = newSize.ToString();

            foreach (GameObject variantsList in variantsLists)
            {
                foreach (MenuClothes_Case caseCloth in VariantsScroller.locationClothes.GetComponentsInChildren<MenuClothes_Case>(true))
                {
                    caseCloth.transform.Find("VariantsList").localScale = new Vector3(newSize / minSize, newSize / minSize, newSize / minSize);
                    caseCloth.transform.Find("VariantsList").localPosition = new Vector3(-80 + minSize - newSize, 0, 0);
                }
            }
        }
    }

}