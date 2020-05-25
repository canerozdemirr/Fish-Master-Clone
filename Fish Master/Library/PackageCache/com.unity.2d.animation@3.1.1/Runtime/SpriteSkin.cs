#pragma warning disable 0168 // variable declared but not used.
#if ENABLE_ANIMATION_COLLECTION && ENABLE_ANIMATION_BURST
#define ENABLE_SPRITESKIN_COMPOSITE
#endif

using UnityEngine.Scripting;
using UnityEngine.U2D.Common;
using Unity.Collections;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.U2D.Animation
{
    struct DeformVerticesBuffer
    {
        public const int k_DefaultBufferSize = 2;
        int m_BufferCount;
        int m_CurrentBuffer;
        NativeArray<byte>[] m_DeformedVertices;

        public DeformVerticesBuffer(int bufferCount)
        {
            m_BufferCount = bufferCount;
            m_DeformedVertices = new NativeArray<byte>[m_BufferCount];
            for (int i = 0; i < m_BufferCount; ++i)
            {
                m_DeformedVertices[i] = new NativeArray<byte>(1, Allocator.Persistent);
            }
            m_CurrentBuffer = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < m_BufferCount; ++i)
            {
                if (m_DeformedVertices[i].IsCreated)
                    m_DeformedVertices[i].Dispose();
            }
        }

        public ref NativeArray<byte> GetBuffer(int expectedSize)
        {
            m_CurrentBuffer = (m_CurrentBuffer + 1) % m_BufferCount;
            if (m_DeformedVertices[m_CurrentBuffer].IsCreated)
                m_DeformedVertices[m_CurrentBuffer].Dispose();
            m_DeformedVertices[m_CurrentBuffer] = new NativeArray<byte>(expectedSize, Allocator.Persistent);
            return ref m_DeformedVertices[m_CurrentBuffer];

        }
    }

    /// <summary>
    /// Deforms the Sprite that is currently assigned to the SpriteRenderer in the same GameObject
    /// </summary>
    [Preserve]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("2D Animation/Sprite Skin")]
    [MovedFrom("UnityEngine.U2D.Experimental.Animation")]

    public sealed partial class SpriteSkin : MonoBehaviour
    {
        [SerializeField]
        private Transform m_RootBone;
        [SerializeField]
        private Transform[] m_BoneTransforms = new Transform[0];
        [SerializeField]
        private Bounds m_Bounds;
        [SerializeField]
        private bool m_UseBatching = true;

        // The deformed m_SpriteVertices stores all 'HOT' channels only in single-stream and essentially depends on Sprite  Asset data.
        // The order of storage if present is POSITION, NORMALS, TANGENTS.
        private DeformVerticesBuffer m_DeformedVertices;
        private int m_CurrentDeformVerticesLength = 0;
        private SpriteRenderer m_SpriteRenderer;
        private Sprite m_CurrentDeformSprite;
        private bool m_ForceSkinning;
        private bool m_BatchSkinning = false;
        bool m_IsValid;
        int m_TransformsHash = 0;

        internal bool batchSkinning
        {
            get { return m_BatchSkinning; }
            set { m_BatchSkinning = value; }
        }

#if UNITY_EDITOR
        internal static Events.UnityEvent onDrawGizmos = new Events.UnityEvent();
        private void OnDrawGizmos() { onDrawGizmos.Invoke(); }

        private bool m_IgnoreNextSpriteChange = true;
        internal bool ignoreNextSpriteChange
        {
            get { return m_IgnoreNextSpriteChange; }
            set { m_IgnoreNextSpriteChange = value; }
        }
#endif


        void OnEnable()
        {
            CacheValidFlag();
            UpdateSpriteDeform();
            OnEnableBatch();
            m_DeformedVertices = new DeformVerticesBuffer(DeformVerticesBuffer.k_DefaultBufferSize);
        }

        void CacheValidFlag()
        {
            m_IsValid = isValid;
        }

        void Reset()
        {
            CacheValidFlag();
            OnResetBatch();
        }

        internal void UseBatching(bool value)
        {
            m_UseBatching = value;
            UseBatchingBatch();
        }

        internal ref NativeArray<byte> GetDeformedVertices(int spriteVertexCount)
        {
            if (sprite != null)
            {
                if (m_CurrentDeformVerticesLength != spriteVertexCount)
                {
                    m_TransformsHash = 0;
                    m_CurrentDeformVerticesLength = spriteVertexCount;
                }
            }
            else
            {
                m_CurrentDeformVerticesLength = 0;
            }
            return ref m_DeformedVertices.GetBuffer(m_CurrentDeformVerticesLength);
        }

        void OnDisable()
        {
            DeactivateSkinning();
            m_DeformedVertices.Dispose();
            OnDisableBatch();
        }

#if ENABLE_SPRITESKIN_COMPOSITE
        internal void OnLateUpdate()
#else
        void LateUpdate()
#endif
        {
            if (m_CurrentDeformSprite != sprite)
            {
                DeactivateSkinning();
                m_CurrentDeformSprite = sprite;
                UpdateSpriteDeform();
            }
            if (isValid && !batchSkinning && this.enabled)
            {
                var transformHash = SpriteSkinUtility.CalculateTransformHash(this);
                var spriteVertexCount = sprite.GetVertexStreamSize() * sprite.GetVertexCount();
                if (spriteVertexCount > 0 && m_TransformsHash != transformHash)
                {
                    var inputVertices = GetDeformedVertices(spriteVertexCount);
                    SpriteSkinUtility.Deform(sprite, gameObject.transform.worldToLocalMatrix, boneTransforms, ref inputVertices);
                    SpriteSkinUtility.UpdateBounds(this, inputVertices);
                    InternalEngineBridge.SetDeformableBuffer(spriteRenderer, inputVertices);
                    m_TransformsHash = transformHash;
                    m_CurrentDeformSprite = sprite;
                }
            }
        }

        internal Sprite sprite
        {
            get
            {
                if (spriteRenderer == null)
                    return null;
                return spriteRenderer.sprite;
            }
        }

        internal SpriteRenderer spriteRenderer
        {
            get
            {
                if (m_SpriteRenderer == null)
                    m_SpriteRenderer = GetComponent<SpriteRenderer>();
                return m_SpriteRenderer;
            }
        }

        /// <summary>
        /// Returns the Transform Components that is used for deformation
        /// </summary>
        /// <returns>An array of Transform Components</returns>
        public Transform[] boneTransforms
        {
            get { return m_BoneTransforms; }
            internal set
            {
                m_BoneTransforms = value;
                CacheValidFlag();
                OnBoneTransformChanged();
            }
        }

        /// <summary>
        /// Returns the Transform Component that represents the root bone for deformation
        /// </summary>
        /// <returns>A Transform Component</returns>
        public Transform rootBone
        {
            get { return m_RootBone; }
            internal set
            {
                m_RootBone = value;
                CacheValidFlag();
                OnRootBoneTransformChanged();
            }
        }

        internal Bounds bounds
        {
            get { return m_Bounds; }
            set { m_Bounds = value; }
        }

        internal bool isValid
        {
            get { return this.Validate() == SpriteSkinValidationResult.Ready; }
        }

        void OnDestroy()
        {
            DeactivateSkinning();
        }

        internal void DeactivateSkinning()
        {
            var sprite = spriteRenderer.sprite;

            if (sprite != null)
                InternalEngineBridge.SetLocalAABB(spriteRenderer, sprite.bounds);

            SpriteRendererDataAccessExtensions.DeactivateDeformableBuffer(spriteRenderer);
        }

        internal void ResetSprite()
        {
            m_CurrentDeformSprite = null;
            CacheValidFlag();
        }

    }
}
