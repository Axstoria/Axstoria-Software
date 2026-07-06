using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasPhysicalSizeScaler : MonoBehaviour
    {
        private const float ReferenceDpi = 96f;

        private void Awake()
        {
            float dpi = Screen.dpi;
            if (dpi <= 0f) return;
            GetComponent<CanvasScaler>().scaleFactor = dpi / ReferenceDpi;
        }
    }
}
