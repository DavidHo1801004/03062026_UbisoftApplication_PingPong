using System.Collections.Generic;

using UnityEditor;
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

using GASU.UIElements;

namespace GASU.Editor.Utilities
{
    /// <summary>
    /// Helper class which defines a dropdown list of items with search handling.
    /// </summary>
    public class SearchableDropdown : PopupWindowContent
    {
        public event Action<int> OnSelectedItemChanged;

        private readonly GUIContent[] options;

        private SearchBar searchBar;
        private ScrollView contentScrollView;

        private readonly Dictionary<string, VisualElement> menuItemLookup = new();
        private readonly Dictionary<VisualElement, int> menuElementIndexLookup = new();

        public int Index { get; private set; }



        public SearchableDropdown(params GUIContent[] _Options)
        {
            options = _Options;
        }

        public SearchableDropdown(params string[] _Options)
        {
            options = _Options
                .Select(e => new GUIContent(e))
                .ToArray();
        }



        public override VisualElement CreateGUI()
        {
            var visualTree = Resources.Load<VisualTreeAsset>("SearchableDropdownUXML");
            if (!visualTree) return base.CreateGUI();

            var root = visualTree.CloneTree();

            // Search bar
            searchBar = root.Q<SearchBar>("search-bar");
            searchBar.RegisterValueChangedCallback(OnSearchBarValueChanged);

            // Main content scroll view
            contentScrollView = root.Q<ScrollView>();
            for (int i = 0; i < options.Length; i++)
            {
                var menuItem = CreateMenuItem(options[i]);

                menuItemLookup.TryAdd(options[i].text, menuItem);
                menuElementIndexLookup.TryAdd(menuItem, i);

                contentScrollView.Add(menuItem);
            }

            // Footer
            root.Q<Label>("footer").text = $"{options.Length} items";

            return root;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(100, 200);
        }



        /// <summary>
        /// Get menu item name at the given index.
        /// </summary>
        /// <returns>
        /// A string represents the item name if valid; <br/>
        /// otherwise, return <see langword="null"/>
        /// </returns>
        public string GetMenuItemNameAt(int _Index)
        {
            if (_Index < 0 || _Index >= options.Length) return null;

            return options[_Index].text;
        }

        #region Helper
        // Create a new menu item based on the given content.
        private VisualElement CreateMenuItem(GUIContent _Content)
        {
            VisualElement root = new();
            root.AddToClassList("menu-item__container");
            root.tooltip = _Content.tooltip;
            root.RegisterCallback<PointerDownEvent>((e) =>
            {
                if (e.button != 0) return;

                Index = menuElementIndexLookup[root];

                OnSelectedItemChanged?.Invoke(Index);
                editorWindow.Close();
            });

            Image icon = new();
            icon.image = _Content.image;
            icon.AddToClassList("menu-item__icon");
            root.Add(icon);

            Label itemName = new();
            itemName.text = _Content.text;
            itemName.AddToClassList("menu-item__label");
            root.Add(itemName);

            return root;
        }

        // Update visible items based on search bar value.
        private void OnSearchBarValueChanged(ChangeEvent<string> _Event)
        {
            foreach (var menuItem in menuItemLookup)
            {
                menuItem.Value.style.display = menuItem.Key.Contains(_Event.newValue) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        #endregion
    }
}
