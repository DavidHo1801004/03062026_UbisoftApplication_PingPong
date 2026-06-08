using UnityEngine;
using UnityEngine.UIElements;

namespace LMK.UIElements
{
    /// <summary>
    /// Alternate wrapper display of <see cref="PlaceholderTextField"/> for search fields.
    /// </summary>
    [UxmlElement("SearchBar", libraryPath = "GASU/Search Bar", visibility = LibraryVisibility.Visible)]
    public partial class SearchBar : PlaceholderTextField
    {
        private const string USS_CLASS = "search-bar";

        private const string USS_CLASS_ICON = USS_CLASS + "__icon";

        private readonly Image searchIcon;



        public SearchBar() : base()
        {
            var styleSheet = Resources.Load<StyleSheet>("SearchBarUSS");
            if (styleSheet) styleSheets.Add(styleSheet);

            AddToClassList(USS_CLASS);

            // Search icon
            searchIcon = new();
            searchIcon.AddToClassList(USS_CLASS_ICON);

            // Handler large image downscale
            var rt = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Trilinear
            };

            Graphics.Blit(Resources.Load<Texture2D>("Sprites/search-interface-symbol-white"), rt);

            searchIcon.image = rt;
            searchIcon.scaleMode = ScaleMode.ScaleToFit;

            Add(searchIcon);
        }
    }
}
