using UnityEngine;

namespace CozyStroll.Core
{
    /// <summary>
    /// Persisted config written by the Mixamo Importer (Editor tool) and read at
    /// runtime by SceneBootstrapper to swap the placeholder capsule for a real model.
    /// Save the asset to Assets/Resources/CozyStrollConfig.asset so it loads via
    /// Resources.Load without needing a scene reference.
    /// </summary>
    [CreateAssetMenu(fileName = "CozyStrollConfig", menuName = "CozyStroll/Config")]
    public class CozyStrollConfig : ScriptableObject
    {
        [Header("Player model (set by Mixamo Importer)")]
        [Tooltip("The Mixamo character FBX prefab (humanoid rig).")]
        public GameObject playerModel;

        [Tooltip("AnimatorController created by the Mixamo Importer.")]
        public RuntimeAnimatorController playerAnimator;
    }
}
