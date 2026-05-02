using System;
using System.Collections.Generic;

namespace Shared.Domain
{
    public class CommandHistory
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public event Action OnHistoryChanged;

        public bool   CanUndo   => _undoStack.Count > 0;
        public bool   CanRedo   => _redoStack.Count > 0;
        public string UndoLabel => CanUndo ? _undoStack.Peek().Label : "";
        public string RedoLabel => CanRedo ? _redoStack.Peek().Label : "";
        public int    UndoCount => _undoStack.Count;
        public int    RedoCount => _redoStack.Count;

        public void Record(ICommand cmd)
        {
            cmd.Execute();
            _undoStack.Push(cmd);
            _redoStack.Clear();
            OnHistoryChanged?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            OnHistoryChanged?.Invoke();
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redoStack.Pop();
            cmd.Redo();
            _undoStack.Push(cmd);
            OnHistoryChanged?.Invoke();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnHistoryChanged?.Invoke();
        }
    }
}
