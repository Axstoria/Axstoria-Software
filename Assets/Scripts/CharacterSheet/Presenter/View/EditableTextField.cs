using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CharacterSheet.Presenter.View
{
    [RequireComponent(typeof(TMP_InputField))]
    public class EditableTextField : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler,
        IEndDragHandler
    {
        private TMP_InputField _inputField;
        private IDragHandler _parentDragHandler;
        private IBeginDragHandler _parentBeginDragHandler;
        private IEndDragHandler _parentEndDragHandler;

        private void Awake()
        {
            _inputField = GetComponent<TMP_InputField>();

            _inputField.interactable = false;

            if (transform.parent != null) {
                _parentBeginDragHandler = transform.parent.GetComponentInParent<IBeginDragHandler>();
                _parentDragHandler = transform.parent.GetComponentInParent<IDragHandler>();
                _parentEndDragHandler = transform.parent.GetComponentInParent<IEndDragHandler>();
            }

            _inputField.onDeselect.AddListener(OnDeselectField);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2) {
                _inputField.interactable = true;
                _inputField.ActivateInputField();
            }
            else {
                Transform parent = transform.parent;

                if (parent != null) {
                    ExecuteEvents.ExecuteHierarchy(parent.gameObject, eventData, ExecuteEvents.pointerClickHandler);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_inputField.interactable && _parentBeginDragHandler != null)
                _parentBeginDragHandler.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_inputField.interactable && _parentDragHandler != null)
                _parentDragHandler.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_inputField.interactable && _parentEndDragHandler != null)
                _parentEndDragHandler.OnEndDrag(eventData);
        }

        private void OnDeselectField(string text)
        {
            StartCoroutine(DisableInteractableDelayed());
        }

        private System.Collections.IEnumerator DisableInteractableDelayed()
        {
            yield return new WaitForEndOfFrame();

            if (_inputField != null) {
                _inputField.interactable = false;
            }
        }
    }
}