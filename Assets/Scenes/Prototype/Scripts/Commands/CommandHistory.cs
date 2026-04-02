using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTT
{
    /// <summary>
    /// Undo/redo stack. Call Record() to execute and store a command.
    /// Ctrl+Z = Undo, Ctrl+Y / Ctrl+Shift+Z = Redo.
    /// TODO: replace keyboard shortcuts with configurable InputAction asset.
    /// </summary>
    [AddComponentMenu("VTT/Command History")]
    public class CommandHistory : MonoBehaviour
    {
        public static CommandHistory Instance { get; private set; }

        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        /// <summary>
        /// Fires on any stack change.
        /// </summary>
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
            bool ctrl  = Input.GetKey(KeyCode.LeftControl)  || Input.GetKey(KeyCode.RightControl);
            bool shift = Input.GetKey(KeyCode.LeftShift)    || Input.GetKey(KeyCode.RightShift);

            // TODO: replace with a configurable InputAction asset when input remapping is added.
            if (ctrl && !shift && Input.GetKeyDown(KeyCode.Z))                          Undo();
            if (ctrl && (Input.GetKeyDown(KeyCode.Y) || shift && Input.GetKeyDown(KeyCode.Z))) Redo();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Execute a command and push it onto the undo stack.
        /// </summary>
        public void Record(ICommand cmd)
        {
            cmd.Execute();

            _undoStack.Push(cmd);
            _redoStack.Clear(); // a new action invalidates the redo branch

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

        /// <summary>
        /// Wipe both stacks
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnHistoryChanged?.Invoke();
        }

    }
}
