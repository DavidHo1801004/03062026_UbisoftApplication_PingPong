using UnityEngine;
using UnityEngine.UIElements;

namespace GASU.UIElements
{
    /// <summary>
    /// Display a menu item separator with a prefix label.
    /// </summary>
    [UxmlElement("NamedSeparator", libraryPath = "GASU/Named Separator", visibility = LibraryVisibility.Visible)]
    public partial class NamedSeparator : VisualElement
    {
        private string text;
        [UxmlAttribute(name = "text")]
        public string Text
        {
            get => text;
            set
            {
                text = value;
                label.text = text;
                label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private Color color = Color.white;
        [UxmlAttribute(name = "color")]
        public Color Color
        {
            get => color;
            set
            {
                color = value;
                label.style.color = color;
                separator.style.backgroundColor = color;
            }
        }



        private const string USS_CLASS = "named_separator";

        private const string USS_CLASS_LABEL = USS_CLASS + "__label";

        private const string USS_CLASS_SEPARATOR = USS_CLASS + "__separator";

        private readonly Label label;

        private readonly VisualElement separator;



        public NamedSeparator() 
            : this(string.Empty) { }

        public NamedSeparator(string _Text)
        {
            var styleSheet = Resources.Load<StyleSheet>("NamedSeparatorUSS");
            if (styleSheet) styleSheets.Add(styleSheet);

            AddToClassList(USS_CLASS);

            label = new Label();
            label.AddToClassList(USS_CLASS_LABEL);
            label.text = _Text;

            separator = new VisualElement();
            separator.AddToClassList(USS_CLASS_SEPARATOR);

            Add(label);
            Add(separator);

            Text = _Text;
        }
    }
}
