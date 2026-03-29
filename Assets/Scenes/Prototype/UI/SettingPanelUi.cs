using UnityEngine;
using UnityEngine.UIElements;

public class VTTUIManager : MonoBehaviour
{
    [Header("Scene References")]
    public Light directionalLight;
    public MeshRenderer gridRenderer;

    private VisualElement _root;

    private void OnEnable()
    {
        // 1. Get the UI Document component
        var uiDocument = GetComponent<UIDocument>();
        _root = uiDocument.rootVisualElement;

        // 2. Bind UI Elements by the 'name' set in UXML
        SetupCameraControls();
        SetupGridControls();
    }

    private void SetupCameraControls()
    {
        // Example: Link a slider to an action
        var orbitSlider = _root.Q<Slider>("orbit-sensitivity");
        orbitSlider?.RegisterValueChangedCallback(evt => {
            Debug.Log($"Orbit Sensitivity changed to: {evt.newValue}");
            // Add your camera reference logic here
        });
    }

    private void SetupGridControls()
    {
        // Link the Grid Opacity slider to the Shader
        var opacitySlider = _root.Q<Slider>("grid-opacity");
        opacitySlider?.RegisterValueChangedCallback(evt => {
            if (gridRenderer != null)
            {
                gridRenderer.material.SetFloat("_Grid_Opacity", evt.newValue);
            }
        });

        // Link the Background Color field
        var transToggle = _root.Q<Toggle>("transparent-toggle");
        transToggle?.RegisterValueChangedCallback(evt => {
            if (gridRenderer != null)
            {
                gridRenderer.material.SetFloat("_Transparent", evt.newValue ? 1f : 0f);
            }
        });
        
        // Regenerate Button
        var regenBtn = _root.Q<Button>("regen-btn");
        regenBtn.clicked += () => Debug.Log("Regenerating Map...");
    }
}
