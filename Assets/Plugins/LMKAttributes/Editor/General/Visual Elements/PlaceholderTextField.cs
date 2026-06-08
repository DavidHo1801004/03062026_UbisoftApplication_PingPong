using UnityEngine;
using UnityEngine.UIElements;

namespace LMK.UIElements
{
    /// <summary>
    /// Alternate wrapper display of <see cref="TextField"/> 
    /// which adds a placeholder text while input value is empty.
    /// </summary>
    [UxmlElement("PlaceholderTextField", libraryPath = "GASU/Placeholder Text Field", visibility = LibraryVisibility.Visible)]
    public partial class PlaceholderTextField : TextField
    {
        private string placeholderText = "Search";
        [UxmlAttribute(name = "placeholder-text")]
        public string PlaceholderText
        {
            get => placeholderText;
            set
            {
                placeholderText = value;
                placeholderLabel.text = value;
            }
        }



        private const string USS_CLASS = "placeholder-text-field";

        private const string USS_CLASS_PLACEHOLDER = USS_CLASS + "__placeholder";

        private readonly Label placeholderLabel;



        public PlaceholderTextField() : base()
        {
            AddToClassList(USS_CLASS);

            // Input field
            RegisterCallback<FocusInEvent>((e) =>
            {
                placeholderLabel.style.display = DisplayStyle.None;
            });
            RegisterCallback<FocusOutEvent>((e) => {
                if (string.IsNullOrEmpty(value))
                    placeholderLabel.style.display = DisplayStyle.Flex;
            });

            // Placeholder label
            placeholderLabel = new()
            {
                pickingMode = PickingMode.Ignore,
                focusable = false,
                text = PlaceholderText,
            };
            placeholderLabel.AddToClassList(USS_CLASS_PLACEHOLDER);
            placeholderLabel.style.position = Position.Absolute;
            placeholderLabel.style.top = 0;
            placeholderLabel.style.right = 0;
            placeholderLabel.style.bottom = 0;
            placeholderLabel.style.left = 2;
            placeholderLabel.style.color = new Color(0.3f, 0.3f, 0.3f);

            textInputBase.Add(placeholderLabel);
        }
    }
}
