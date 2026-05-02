using System;
using SceneEditor.Domain;
using UnityEngine;

namespace SceneEditor.Presenter.View
{
    public class SceneObjectView : MonoBehaviour
    {
        public SceneObject DomainObject { get; private set; }

        public void Init(SceneObject domainObject)
        {
            DomainObject = domainObject;
            DomainObject.OnTransformChanged += OnDomainTransformChanged;

            if (DomainObject.Transform != null)
                ApplyTransform(DomainObject.Transform);
        }

        private void OnDestroy()
        {
            if (DomainObject != null)
                DomainObject.OnTransformChanged -= OnDomainTransformChanged;
        }

        private void OnDomainTransformChanged(object sender, EventArgs e)
        {
            if (DomainObject?.Transform != null)
                ApplyTransform(DomainObject.Transform);
        }

        private void ApplyTransform(TransformModel t)
        {
            transform.position   = new Vector3(t.Position.x, t.Position.y, t.Position.z);
            transform.rotation   = new Quaternion(t.Rotation.x, t.Rotation.y, t.Rotation.z, t.Rotation.w);
            transform.localScale = new Vector3(t.Scale.x, t.Scale.y, t.Scale.z);
        }
    }
}
