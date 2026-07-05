using NUnit.Framework;
using MapEditor.Domain;
using SceneEditor.Domain;

namespace Tests
{
    public class MapTests
    {
        private Map _map;

        [SetUp]
        public void Setup()
        {
            _map = new Map { Id = "map-1", Name = "Test Map" };
        }

        // ── Initial state ──────────────────────────────────────────────────────

        [Test]
        public void NewMap_StartsWithEmptyCollections()
        {
            Assert.IsEmpty(_map.Objects);
            Assert.IsEmpty(_map.Tokens);
            Assert.IsEmpty(_map.Structures);
        }

        [Test]
        public void NewMap_StoresIdAndName()
        {
            Assert.AreEqual("map-1", _map.Id);
            Assert.AreEqual("Test Map", _map.Name);
        }

        // ── AddObject ──────────────────────────────────────────────────────────

        [Test]
        public void AddObject_AppearsInObjectsList()
        {
            var obj = new SceneObject { Id = "obj-1", DisplayName = "Chair" };
            _map.AddObject(obj);
            Assert.Contains(obj, _map.Objects);
        }

        [Test]
        public void AddObject_IncreasesObjectCount()
        {
            _map.AddObject(new SceneObject { Id = "a" });
            _map.AddObject(new SceneObject { Id = "b" });
            Assert.AreEqual(2, _map.Objects.Count);
        }

        [Test]
        public void AddObject_FiresOnObjectAdded()
        {
            SceneObject received = null;
            _map.OnObjectAdded += o => received = o;
            var obj = new SceneObject { Id = "obj-1" };
            _map.AddObject(obj);
            Assert.AreEqual(obj, received);
        }

        // ── RemoveObject ───────────────────────────────────────────────────────

        [Test]
        public void RemoveObject_DisappearsFromObjectsList()
        {
            var obj = new SceneObject { Id = "obj-1" };
            _map.AddObject(obj);
            _map.RemoveObject(obj);
            Assert.IsFalse(_map.Objects.Contains(obj));
        }

        [Test]
        public void RemoveObject_FiresOnObjectRemoved()
        {
            SceneObject received = null;
            _map.OnObjectRemoved += o => received = o;
            var obj = new SceneObject { Id = "obj-1" };
            _map.AddObject(obj);
            _map.RemoveObject(obj);
            Assert.AreEqual(obj, received);
        }

        [Test]
        public void RemoveObject_LeavesOtherObjectsIntact()
        {
            var a = new SceneObject { Id = "a" };
            var b = new SceneObject { Id = "b" };
            _map.AddObject(a);
            _map.AddObject(b);
            _map.RemoveObject(a);
            Assert.IsFalse(_map.Objects.Contains(a));
            Assert.IsTrue(_map.Objects.Contains(b));
        }

        [Test]
        public void RemoveObject_NotInList_DoesNotThrow()
        {
            var obj = new SceneObject { Id = "ghost" };
            Assert.DoesNotThrow(() => _map.RemoveObject(obj));
        }
    }
}
