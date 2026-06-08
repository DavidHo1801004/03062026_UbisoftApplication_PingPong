using UnityEngine;
using UnityEngine.UI;

namespace Developer.UI
{
    [RequireComponent(typeof(LayoutElement))]
    [AddComponentMenu("Developer/UI/Sizeable Display")]
    public class SizeableDisplay : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private LayoutElement layoutElement;



#if UNITY_EDITOR
        private void Reset()
        {
            if (!layoutElement) 
                layoutElement = GetComponent<LayoutElement>();
        }
#endif

        private void Awake()
        {
            if (!layoutElement)
                layoutElement = GetComponent<LayoutElement>();
        }


        public void SetPreferredSize(Vector2 _Value)
        {
            layoutElement.preferredWidth = _Value.x;
            layoutElement.preferredHeight = _Value.y;
        }

        public void SetMaxSize(Vector2 _Value)
        {
            layoutElement.minWidth = _Value.x;
            layoutElement.minHeight = _Value.y;
        }
    }
}
