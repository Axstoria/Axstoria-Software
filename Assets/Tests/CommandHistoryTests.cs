using System.Collections.Generic;
using NUnit.Framework;
using Shared.Domain;

namespace Tests
{
    public class CommandHistoryTests
    {
        private CommandHistory _history;
        private List<string> _log;

        [SetUp]
        public void Setup()
        {
            _history = new CommandHistory();
            _log = new List<string>();
        }

        private ICommand Cmd(string label) => new LoggingCommand(label, _log);

        // ── Initial state ──────────────────────────────────────────────────────

        [Test]
        public void InitialState_CannotUndoOrRedo()
        {
            Assert.IsFalse(_history.CanUndo);
            Assert.IsFalse(_history.CanRedo);
        }

        [Test]
        public void InitialState_CountsAreZero()
        {
            Assert.AreEqual(0, _history.UndoCount);
            Assert.AreEqual(0, _history.RedoCount);
        }

        [Test]
        public void InitialState_LabelsAreEmpty()
        {
            Assert.AreEqual("", _history.UndoLabel);
            Assert.AreEqual("", _history.RedoLabel);
        }

        // ── Record ─────────────────────────────────────────────────────────────

        [Test]
        public void Record_ExecutesCommand()
        {
            _history.Record(Cmd("cmd"));
            Assert.Contains("Execute:cmd", _log);
        }

        [Test]
        public void Record_MakesUndoAvailable()
        {
            _history.Record(Cmd("cmd"));
            Assert.IsTrue(_history.CanUndo);
            Assert.AreEqual("cmd", _history.UndoLabel);
        }

        [Test]
        public void Record_DoesNotMakeRedoAvailable()
        {
            _history.Record(Cmd("cmd"));
            Assert.IsFalse(_history.CanRedo);
        }

        [Test]
        public void Record_ClearsRedoStack()
        {
            _history.Record(Cmd("a"));
            _history.Undo();
            Assert.IsTrue(_history.CanRedo);
            _history.Record(Cmd("b"));
            Assert.IsFalse(_history.CanRedo);
        }

        [Test]
        public void Record_FiresOnHistoryChanged()
        {
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Record(Cmd("cmd"));
            Assert.AreEqual(1, fired);
        }

        // ── Undo ───────────────────────────────────────────────────────────────

        [Test]
        public void Undo_CallsCommandUndo()
        {
            _history.Record(Cmd("cmd"));
            _history.Undo();
            Assert.Contains("Undo:cmd", _log);
        }

        [Test]
        public void Undo_MovesCommandToRedoStack()
        {
            _history.Record(Cmd("cmd"));
            _history.Undo();
            Assert.IsFalse(_history.CanUndo);
            Assert.IsTrue(_history.CanRedo);
            Assert.AreEqual("cmd", _history.RedoLabel);
        }

        [Test]
        public void Undo_WhenEmpty_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _history.Undo());
        }

        [Test]
        public void Undo_WhenEmpty_DoesNotFireEvent()
        {
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Undo();
            Assert.AreEqual(0, fired);
        }

        [Test]
        public void Undo_FiresOnHistoryChanged()
        {
            _history.Record(Cmd("cmd"));
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Undo();
            Assert.AreEqual(1, fired);
        }

        [Test]
        public void MultipleRecords_UndoInLIFOOrder()
        {
            _history.Record(Cmd("a"));
            _history.Record(Cmd("b"));
            Assert.AreEqual("b", _history.UndoLabel);
            _history.Undo();
            Assert.AreEqual("a", _history.UndoLabel);
        }

        // ── Redo ───────────────────────────────────────────────────────────────

        [Test]
        public void Redo_CallsCommandRedo()
        {
            _history.Record(Cmd("cmd"));
            _history.Undo();
            _history.Redo();
            Assert.Contains("Redo:cmd", _log);
        }

        [Test]
        public void Redo_MovesCommandBackToUndoStack()
        {
            _history.Record(Cmd("cmd"));
            _history.Undo();
            _history.Redo();
            Assert.IsTrue(_history.CanUndo);
            Assert.IsFalse(_history.CanRedo);
        }

        [Test]
        public void Redo_WhenEmpty_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _history.Redo());
        }

        [Test]
        public void Redo_WhenEmpty_DoesNotFireEvent()
        {
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Redo();
            Assert.AreEqual(0, fired);
        }

        [Test]
        public void Redo_FiresOnHistoryChanged()
        {
            _history.Record(Cmd("cmd"));
            _history.Undo();
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Redo();
            Assert.AreEqual(1, fired);
        }

        // ── Clear ──────────────────────────────────────────────────────────────

        [Test]
        public void Clear_EmptiesBothStacks()
        {
            _history.Record(Cmd("a"));
            _history.Undo();
            _history.Clear();
            Assert.IsFalse(_history.CanUndo);
            Assert.IsFalse(_history.CanRedo);
        }

        [Test]
        public void Clear_FiresOnHistoryChanged()
        {
            int fired = 0;
            _history.OnHistoryChanged += () => fired++;
            _history.Clear();
            Assert.AreEqual(1, fired);
        }

        // ── Full round-trip ────────────────────────────────────────────────────

        [Test]
        public void UndoThenRedo_CommandCallsInOrder()
        {
            _history.Record(Cmd("x"));
            _history.Undo();
            _history.Redo();
            CollectionAssert.AreEqual(new[] { "Execute:x", "Undo:x", "Redo:x" }, _log);
        }

        // ── Helper ─────────────────────────────────────────────────────────────

        private class LoggingCommand : ICommand
        {
            private readonly List<string> _log;
            public string Label { get; }

            public LoggingCommand(string label, List<string> log)
            {
                Label = label;
                _log = log;
            }

            public void Execute() => _log.Add($"Execute:{Label}");
            public void Undo()    => _log.Add($"Undo:{Label}");
            public void Redo()    => _log.Add($"Redo:{Label}");
        }
    }
}
