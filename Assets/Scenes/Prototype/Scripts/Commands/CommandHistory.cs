using System;
using System.Collections.Generic;
using UnityEngine;
using VTT.UI;

namespace VTT
{
    /// <summary>
    /// Central undo/redo stack. Ctrl+Z = Undo | Ctrl+Y / Ctrl+Shift+Z = Redo.
    /// </summary>
    [AddComponentMenu("VTT/Command History")]
    public class CommandHistory : MonoBehaviour
    {
        public static CommandHistory Instance { get; private set; }

        [SerializeField] private int maxHistory = 100;

        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public event Action OnHistoryChanged;

        public bool   CanUndo   => _undoStack.Count > 0;
        public bool   CanRedo   => _redoStack.Count > 0;
        public string UndoLabel => CanUndo ? _undoStack.Peek().Label : "";
        public string RedoLabel => CanRedo ? _redoStack.Peek().Label : "";
        public int    UndoCount => _undoStack.Count;
        public int    RedoCount => _redoStack.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (VTTPanelUI.IsMouseOverUI) return;
            bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);
            if (ctrl && !shift && Input.GetKeyDown(KeyCode.Z)) Undo();
            if (ctrl && (Input.GetKeyDown(KeyCode.Y) || (shift && Input.GetKeyDown(KeyCode.Z)))) Redo();
        }

        public void Record(ICommand cmd)
        {
            _undoStack.Push(cmd);
            _redoStack.Clear();

            if (_undoStack.Count > maxHistory)
            {
                var tmp = new Stack<ICommand>(_undoStack);
                _undoStack.Clear();
                int keep = 0;
                foreach (var c in tmp)
                    if (keep++ < maxHistory) _undoStack.Push(c);
            }

            OnHistoryChanged?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            OnHistoryChanged?.Invoke();
            Debug.Log($"[VTT] Undo: {cmd.Label}");
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redoStack.Pop();
            cmd.Redo();
            _undoStack.Push(cmd);
            OnHistoryChanged?.Invoke();
            Debug.Log($"[VTT] Redo: {cmd.Label}");
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnHistoryChanged?.Invoke();
        }
    }
}