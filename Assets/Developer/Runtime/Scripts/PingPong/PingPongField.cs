using System;
using System.Collections.Generic;
using LMK.Attribute;
using UnityEngine;

namespace Developer.PingPong
{
    public class PingPongField : MonoBehaviour
    {
        public static PingPongField Instance { get; private set; }

        [Header("DATA")]
        [SerializeField]
        private Vector2 fieldSize;

        [Header("REFERENCES")]
        [SerializeField]
        private Transform extensionContainer;
        [SerializeField]
        private SpriteRenderer mainRenderer;
        [SerializeField]
        private List<FieldExtension> extensions = new();

        private readonly Dictionary<Type, List<FieldExtension>> extensionMap = new();

        public Vector2 Size
            => fieldSize;



        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            fieldSize = GameManager.GameplaySettings.fieldSize;

            InitializeProperties();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, fieldSize);
        }
#endif



        public FieldExtension[] GetExtensions<T>()
        {
            var type = typeof(T);
            if (!extensionMap.TryGetValue(type, out var extensions)) return null;
            return extensions.ToArray();
        }

        public void AddExtension(FieldExtension _Extension)
        {
            _Extension.transform.SetParent(extensionContainer);
            _Extension.Attach(this);

            var type = _Extension.GetType();
            extensionMap.TryAdd(type, new());
            extensionMap[type].Add(_Extension);

            if (!extensions.Contains(_Extension))
            {
                extensions.Add(_Extension);
            }
        }

        public void RemoveExtension(FieldExtension _Extension)
        {
            if (!extensions.Remove(_Extension)) return;

            _Extension.Attach(null);
        }

        [Button("Update Preview")]
        private void InitializeProperties()
        {
            UpdateSizeDisplay();

            foreach (var extension in extensions)
            {
                AddExtension(extension);
                extension.OnFieldUpdated(this);
            }
        }

        private void UpdateSizeDisplay()
        {
            if (mainRenderer)
            {
                mainRenderer.size = fieldSize;
            }
        }
    }
}
