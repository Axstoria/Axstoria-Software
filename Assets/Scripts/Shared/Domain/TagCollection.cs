using System;
using System.Collections.Generic;

namespace Shared.Domain
{
    public class TagCollection
    {
        private readonly List<Tag> _tags = new();

        public IReadOnlyList<Tag> All => _tags;

        public event Action<Tag> OnTagAdded;
        public event Action<Tag> OnTagRemoved;

        public void Add(Tag tag)
        {
            _tags.Add(tag);
            OnTagAdded?.Invoke(tag);
        }

        public bool Remove(string tagId)
        {
            var tag = GetById(tagId);
            if (tag == null) return false;
            _tags.Remove(tag);
            OnTagRemoved?.Invoke(tag);
            return true;
        }

        public Tag GetById(string tagId)
        {
            foreach (var t in _tags)
                if (t.Id == tagId) return t;
            return null;
        }

        public void Clear()
        {
            var copy = new List<Tag>(_tags);
            _tags.Clear();
            foreach (var t in copy)
                OnTagRemoved?.Invoke(t);
        }
    }
}
