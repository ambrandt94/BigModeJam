using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace ChainLink.Core
{
    public class MaterialUtility : MonoBehaviour
    {
        private const string DefaultColorProperty = "_Color";
        private const string DefaultTextureProperty = "_MainTex";
        private const string DefaultTilingProperty = "_MainTex_ST";

        private string ColorProperty => !string.IsNullOrEmpty(materialPropertyOverride) ? materialPropertyOverride : DefaultColorProperty;
        private MaterialPropertyBlock Block {
            get {
                if (_block == null)
                    _block = new MaterialPropertyBlock();
                return _block;
            }
        }

        public string Id;
        
        [SerializeField, TitleGroup("Colors")]
        private Color color;
        [SerializeField, TitleGroup("Colors")]
        private string materialPropertyOverride;

        [SerializeField, TitleGroup("Tiling")]
        private Vector4 tiling;

        [SerializeField]
        private Renderer renderer;
        [SerializeField]
        private Texture overrideTexture;

        private MaterialPropertyBlock _block;

        public static List<MaterialUtility> GetUtilitiesWithId(string id, Transform parent = null)
        {
            List<MaterialUtility> utilities = new List<MaterialUtility>();

            //Global
            if (parent == null) {
                foreach (MaterialUtility u in FindObjectsByType(typeof(MaterialUtility), FindObjectsInactive.Include, FindObjectsSortMode.None))
                    utilities.Add(u);
            } else { 
            foreach(MaterialUtility u in parent.GetComponentsInChildren<MaterialUtility>(true))
                    utilities.Add(u);
            }
            
            return utilities;
        }

        public void SetColor(Color color)
        {
            if (color == Color.clear)
                return;
            renderer.GetPropertyBlock(Block);
            Block.SetColor(ColorProperty, color);
            renderer.SetPropertyBlock(Block);
        }

        public void SetTexture(Texture texture)
        {
            renderer.GetPropertyBlock(Block);
            Block.SetTexture(DefaultTextureProperty, texture);
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
                if (overrideTexture != null) {
                    SetTexture(overrideTexture);
                    overrideTexture = null;
                }
            }
        }

        private void Awake()
        {
            if (renderer == null)
                renderer = GetComponentInChildren<Renderer>();
        }
    }
}