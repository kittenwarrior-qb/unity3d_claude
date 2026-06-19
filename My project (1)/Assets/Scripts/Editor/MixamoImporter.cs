// This file is in an Editor/ folder — Unity automatically excludes it from builds.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using CozyStroll.Core;
using CozyStroll.Player;

namespace CozyStroll.EditorTools
{
    /// <summary>
    /// Step-by-step wizard for plugging a Mixamo character into Cozy Stroll:
    ///   Step 1 – Select the character FBX and set its rig to Humanoid.
    ///   Step 2 – Assign / auto-detect Idle, Walk, Run, Jump clips.
    ///   Step 3 – Generate an AnimatorController and save the config.
    ///            Next time you press Play the real model replaces the capsule.
    /// Open via the menu: CozyStroll → Mixamo Importer
    /// </summary>
    public sealed class MixamoImporter : EditorWindow
    {
        private const string MenuPath = "CozyStroll/Mixamo Importer";
        private const string ConfigResourcePath = "Assets/Resources/CozyStrollConfig.asset";
        private const string ConfigLoadKey = "CozyStrollConfig"; // Resources.Load key

        // ── Step 1 ────────────────────────────────────────────────────────────
        private GameObject _charPrefab;   // character FBX prefab (T-pose)

        // ── Step 2 ────────────────────────────────────────────────────────────
        private AnimationClip _clipIdle;
        private AnimationClip _clipWalk;  // optional — skipped if null
        private AnimationClip _clipRun;
        private AnimationClip _clipJump;  // optional — skipped if null

        // ── Step 3 ────────────────────────────────────────────────────────────
        private string _controllerPath = "Assets/Animations/CozyPlayer.controller";

        // ── UI state ──────────────────────────────────────────────────────────
        private Vector2 _scroll;
        private string _statusMsg = "Kéo file FBX nhân vật (T-pose) vào ô Bước 1.";
        private MessageType _statusType = MessageType.Info;

        // ── Cached styles (built lazily to avoid constructor-time GUISkin access) ──
        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;

        // ─────────────────────────────────────────────────────────────────────

        [MenuItem(MenuPath)]
        public static void Open()
        {
            var w = GetWindow<MixamoImporter>("Mixamo Importer");
            w.minSize = new Vector2(430f, 580f);
        }

        private void OnGUI()
        {
            BuildStyles();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawHeader();
            GUILayout.Space(6f);

            DrawStep1();
            GUILayout.Space(4f);

            DrawStep2();
            GUILayout.Space(4f);

            DrawStep3();
            GUILayout.Space(8f);

            DrawStatus();

            EditorGUILayout.EndScrollView();
        }

        // ── Header ────────────────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("🌤  Cozy Stroll — Mixamo Importer", _headerStyle);
            EditorGUILayout.HelpBox(
                "Làm theo từng bước. Sau khi Lưu Config, bấm Play — nhân vật thật sẽ xuất hiện.",
                MessageType.None);
        }

        // ── Step 1 ────────────────────────────────────────────────────────────

        private void DrawStep1()
        {
            EditorGUILayout.LabelField("Bước 1 — Nhân vật (FBX T-pose)", _sectionStyle);

            EditorGUI.BeginChangeCheck();
            _charPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Character FBX", _charPrefab, typeof(GameObject), false);
            bool fbxChanged = EditorGUI.EndChangeCheck();

            // Auto-detect clips whenever a new FBX is assigned.
            if (fbxChanged && _charPrefab != null)
                TryAutoDetectFromPath(AssetDatabase.GetAssetPath(_charPrefab));

            using (new EditorGUI.DisabledScope(_charPrefab == null))
            {
                if (GUILayout.Button("Set Humanoid & Reimport"))
                    DoSetHumanoid();
            }

            if (_charPrefab != null)
            {
                string path = AssetDatabase.GetAssetPath(_charPrefab);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null && importer.animationType == ModelImporterAnimationType.Human)
                    EditorGUILayout.LabelField("  ✔ Humanoid", EditorStyles.miniLabel);
                else if (importer != null)
                    EditorGUILayout.HelpBox("Rig chưa phải Humanoid — bấm nút trên.", MessageType.Warning);
            }
        }

        // ── Step 2 ────────────────────────────────────────────────────────────

        private void DrawStep2()
        {
            EditorGUILayout.LabelField("Bước 2 — Clip hoạt ảnh", _sectionStyle);

            using (new EditorGUI.DisabledScope(_charPrefab == null))
            {
                if (GUILayout.Button("Auto-detect clips từ FBX / thư mục cùng cấp"))
                    TryAutoDetectFromPath(AssetDatabase.GetAssetPath(_charPrefab));
            }

            GUILayout.Space(2f);

            _clipIdle = ClipField("Idle  (bắt buộc)", _clipIdle);
            _clipWalk = ClipField("Walk  (tuỳ chọn)", _clipWalk);
            _clipRun  = ClipField("Run   (bắt buộc)", _clipRun);
            _clipJump = ClipField("Jump  (tuỳ chọn)", _clipJump);
        }

        private static AnimationClip ClipField(string label, AnimationClip current)
        {
            return (AnimationClip)EditorGUILayout.ObjectField(label, current, typeof(AnimationClip), false);
        }

        // ── Step 3 ────────────────────────────────────────────────────────────

        private void DrawStep3()
        {
            EditorGUILayout.LabelField("Bước 3 — Build & Lưu Config", _sectionStyle);

            _controllerPath = EditorGUILayout.TextField("Controller path", _controllerPath);

            bool canBuild = _charPrefab != null && (_clipIdle != null || _clipRun != null);
            using (new EditorGUI.DisabledScope(!canBuild))
            {
                if (GUILayout.Button("Tạo AnimatorController & Lưu Config"))
                    DoBuild();
            }

            if (!canBuild)
                EditorGUILayout.HelpBox("Cần có Character FBX + ít nhất 1 clip (Idle hoặc Run).",
                    MessageType.None);
            else
                EditorGUILayout.HelpBox("Sau khi lưu: bấm Play → nhân vật thật thay capsule tự động.",
                    MessageType.None);
        }

        // ── Status ────────────────────────────────────────────────────────────

        private void DrawStatus()
        {
            if (!string.IsNullOrEmpty(_statusMsg))
                EditorGUILayout.HelpBox(_statusMsg, _statusType);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Operations
        // ═════════════════════════════════════════════════════════════════════

        private void DoSetHumanoid()
        {
            if (_charPrefab == null) return;

            string path = AssetDatabase.GetAssetPath(_charPrefab);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                SetStatus("Không thể đọc ModelImporter từ file này.", MessageType.Error);
                return;
            }

            importer.animationType = ModelImporterAnimationType.Human;
            importer.optimizeGameObjects = false; // keep the rig hierarchy accessible
            importer.SaveAndReimport();
            AssetDatabase.Refresh();

            SetStatus($"✔ Đã set Humanoid cho '{_charPrefab.name}'. Reimporting...", MessageType.Info);
        }

        // ── Auto-detect clips ─────────────────────────────────────────────────

        private void TryAutoDetectFromPath(string charFbxPath)
        {
            if (string.IsNullOrEmpty(charFbxPath)) return;

            // Collect candidate clips: first from the character FBX itself (embedded),
            // then from all other FBX files in the same folder.
            var allClips = new List<AnimationClip>();
            allClips.AddRange(ExtractClips(charFbxPath));

            string dir = Path.GetDirectoryName(charFbxPath);
            if (!string.IsNullOrEmpty(dir))
            {
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { dir });
                foreach (string guid in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(guid);
                    if (p == charFbxPath) continue; // already added above
                    allClips.AddRange(ExtractClips(p));
                }
            }

            if (allClips.Count == 0)
            {
                SetStatus("Không tìm thấy AnimationClip nào. Gán tay vào các ô ở Bước 2.", MessageType.Warning);
                return;
            }

            // Match by name (case-insensitive).
            foreach (var clip in allClips)
            {
                string n = clip.name.ToLowerInvariant();
                if (MatchesKeyword(n, "idle"))       _clipIdle = _clipIdle ?? clip;
                else if (MatchesKeyword(n, "walk"))  _clipWalk = _clipWalk ?? clip;
                else if (MatchesKeyword(n, "run"))   _clipRun  = _clipRun  ?? clip;
                else if (MatchesKeyword(n, "jump"))  _clipJump = _clipJump ?? clip;
            }

            // Second pass: looser name match (Mixamo uses "Walking", "Running", etc.)
            foreach (var clip in allClips)
            {
                string n = clip.name.ToLowerInvariant();
                if (MatchesKeyword(n, "walking") || MatchesKeyword(n, "walk"))  _clipWalk = _clipWalk ?? clip;
                if (MatchesKeyword(n, "running") || MatchesKeyword(n, "run"))   _clipRun  = _clipRun  ?? clip;
                if (MatchesKeyword(n, "jumping") || MatchesKeyword(n, "jump"))  _clipJump = _clipJump ?? clip;
            }

            int found = (_clipIdle != null ? 1 : 0) + (_clipWalk != null ? 1 : 0)
                      + (_clipRun  != null ? 1 : 0) + (_clipJump != null ? 1 : 0);
            SetStatus($"Auto-detect xong: tìm thấy {found}/4 clip. Kiểm tra lại ô Bước 2.", MessageType.Info);
            Repaint();
        }

        private static List<AnimationClip> ExtractClips(string assetPath)
        {
            var result = new List<AnimationClip>();
            UnityEngine.Object[] assets;
            try { assets = AssetDatabase.LoadAllAssetsAtPath(assetPath); }
            catch { return result; }

            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                    result.Add(clip);
            }
            return result;
        }

        private static bool MatchesKeyword(string name, string keyword)
            => name.Contains(keyword);

        // ── Build ─────────────────────────────────────────────────────────────

        private void DoBuild()
        {
            try
            {
                var ctrl = BuildAnimatorController();
                if (ctrl == null) return;

                SaveConfig(ctrl);

                SetStatus("✔ Xong! AnimatorController & Config đã lưu. Bấm Play để thấy nhân vật thật.",
                    MessageType.Info);
            }
            catch (Exception ex)
            {
                SetStatus($"Lỗi: {ex.Message}", MessageType.Error);
                Debug.LogException(ex);
            }
        }

        private AnimatorController BuildAnimatorController()
        {
            // ── 1. Ensure output folder exists ────────────────────────────────
            EnsureFolder(_controllerPath);

            // ── 2. Delete old file if exists ──────────────────────────────────
            if (File.Exists(_controllerPath))
                AssetDatabase.DeleteAsset(_controllerPath);

            // ── 3. Create controller ──────────────────────────────────────────
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(_controllerPath);
            if (ctrl == null)
            {
                SetStatus($"Không tạo được controller tại: {_controllerPath}", MessageType.Error);
                return null;
            }

            // ── 4. Parameters ─────────────────────────────────────────────────
            ctrl.AddParameter("Speed",      AnimatorControllerParameterType.Float);
            ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("Jump",       AnimatorControllerParameterType.Trigger);

            var rootSM = ctrl.layers[0].stateMachine;

            // ── 5. Locomotion blend tree ──────────────────────────────────────
            var locoState = ctrl.CreateBlendTreeInController("Locomotion", out BlendTree locoTree);
            locoTree.blendType      = BlendTreeType.Simple1D;
            locoTree.blendParameter = "Speed";
            locoTree.useAutomaticThresholds = false;

            // Collect available loco clips and spread thresholds evenly.
            var locoMotions = new List<(float threshold, Motion clip)>();
            if (_clipIdle != null) locoMotions.Add((0f,   _clipIdle));
            if (_clipWalk != null) locoMotions.Add((0.4f, _clipWalk));
            if (_clipRun  != null) locoMotions.Add((1f,   _clipRun));

            // If walk is present but idle is not, push walk to threshold 0.
            // If run is present but idle is not, it already gets 1f.
            if (locoMotions.Count == 1)
            {
                // Only one clip — still valid; just set at 0.
                locoMotions[0] = (0f, locoMotions[0].clip);
            }
            else if (locoMotions.Count == 2 && _clipWalk == null)
            {
                // Idle + Run only — spread to 0 and 1.
                locoMotions[0] = (0f, locoMotions[0].clip);
                locoMotions[1] = (1f, locoMotions[1].clip);
            }

            foreach (var (threshold, motion) in locoMotions)
                locoTree.AddChild(motion, threshold);

            rootSM.defaultState = locoState;

            // ── 6. Jump state (optional — only if clip exists) ────────────────
            if (_clipJump != null)
            {
                var jumpState = rootSM.AddState("Jump");
                jumpState.motion = _clipJump;

                // AnyState → Jump when trigger fires (can't transition to self).
                var toJump = rootSM.AddAnyStateTransition(jumpState);
                toJump.hasExitTime     = false;
                toJump.duration        = 0.1f;
                toJump.canTransitionToSelf = false;
                toJump.AddCondition(AnimatorConditionMode.If, 0f, "Jump");

                // Jump → Locomotion when grounded again.
                var toLoco = jumpState.AddTransition(locoState);
                toLoco.hasExitTime = false;
                toLoco.duration    = 0.2f;
                toLoco.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return ctrl;
        }

        // ── Config ────────────────────────────────────────────────────────────

        private void SaveConfig(RuntimeAnimatorController ctrl)
        {
            EnsureFolder(ConfigResourcePath);

            // Load existing or create new.
            var cfg = AssetDatabase.LoadAssetAtPath<CozyStrollConfig>(ConfigResourcePath);
            if (cfg == null)
            {
                cfg = CreateInstance<CozyStrollConfig>();
                AssetDatabase.CreateAsset(cfg, ConfigResourcePath);
            }

            cfg.playerModel    = _charPrefab;
            cfg.playerAnimator = ctrl;

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void EnsureFolder(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(dir) || AssetDatabase.IsValidFolder(dir)) return;

            string[] parts = dir.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private void SetStatus(string msg, MessageType type)
        {
            _statusMsg  = msg;
            _statusType = type;
            Repaint();
        }

        // ── Styles ────────────────────────────────────────────────────────────

        private void BuildStyles()
        {
            if (_headerStyle != null) return; // already built

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 14,
                alignment = TextAnchor.MiddleLeft,
            };
            _headerStyle.margin.top = 6;

            _sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
            };
            _sectionStyle.margin.top = 8;
        }
    }
}
