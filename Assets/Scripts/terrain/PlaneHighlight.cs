using UnityEngine;

namespace terrain
{
    public class PlanHighlighter : MonoBehaviour
    {
        [SerializeField] private Renderer planRenderer;
        [SerializeField] private Color highlightColor = Color.yellow;
        private Material _originalMat;
        private Material _highlightMat;

        void Awake()
        {
            _originalMat = planRenderer.material;
            _highlightMat = new Material(_originalMat);
            _highlightMat.color = highlightColor;
        }

        public void Highlight(bool on)
        {
            planRenderer.material = on ? _highlightMat : _originalMat;
        }
    }

}

