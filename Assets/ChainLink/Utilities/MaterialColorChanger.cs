using Sirenix.OdinInspector;
using UnityEngine;

namespace ChainLink.Core
{
    public class MaterialColorChanger : MonoBehaviour
    {
        private const string DefaultColorProperty = "_Color";
        private const string DefaultTilingProperty = "_MainTex_ST";

        private string ColorProperty => !string.IsNullOrEmpty(materialPropertyOverride) ? materialPropertyOverride : DefaultColorProperty;
        private MaterialPropertyBlock Block {
            get {
                if (_block == null)
                    _block = new MaterialPropertyBlock();
                return _block;
            }
        }
        [SerializeField, TitleGroup("Colors")]
        private Color color;
        [SerializeField, TitleGroup("Colors")]
        private string materialPropertyOverride;

        [SerializeField, TitleGroup("Tiling")]
        private Vector4 tiling;
        
        [SerializeField]
        private Renderer renderer;

        private MaterialPropertyBlock _block;

        public void SetColor(Color color)
        {
            renderer.GetPropertyBlock(Block);
            Block.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(Block);
        }

        private void SetTilingInfo()
        {
            if (tiling != Vector4.zero) {
                renderer.GetPropertyBlock(Block);
                Block.SetVector(DefaultTilingProperty, tiling);
                renderer.SetPropertyBlock(Block);
            }
        }

        private void OnValidate()
        {
            if (renderer != null) { 
                SetColor(color);
                SetTilingInfo();
            }
        }

        private void Awake()
        {
            if(renderer == null)
                renderer = GetComponentInChildren<Renderer>();
        }
    }
}