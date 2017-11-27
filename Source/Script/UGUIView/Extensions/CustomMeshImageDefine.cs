using UnityEngine;

namespace UBlockly.UGUI
{
    [RequireComponent(typeof(CustomMeshImage))]
    public class CustomMeshImageDefine : MonoBehaviour
    {
        [SerializeField] private Vector4[] m_DrawDimensions;

        private void Awake()
        {
            GetComponent<CustomMeshImage>().SetDrawDimensions(m_DrawDimensions);
        }
    }
}