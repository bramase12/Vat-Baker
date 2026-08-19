using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VATSystem
{
    public class VATLauncherWindow : EditorWindow
    {
        private enum Page
        {
            Dashboard, Character, Animation, Baking, Prefab, Database, Material, Texture,
            Runtime, Debug, Utilities, Settings, About
        }

        private Page currentPage = Page.Dashboard;
        private string searchText = "";
        private Vector2 sidebarScroll, contentScroll;

        private static readonly string[] PageNames = {
            "Dashboard", "Character", "Animation", "Baking", "Prefab",
            "Database", "Material", "Texture", "Runtime", "Debug",
            "Utilities", "Settings", "About"
        };

        [MenuItem("ExLib/VAT/VAT Launcher")]
        public static void ShowWindow()
        {
            var window = GetWindow<VATLauncherWindow>("VAT Launcher");
            window.minSize = new Vector2(800, 500);
        }

        private void OnGUI()
        {
            var visiblePages = string.IsNullOrEmpty(searchText)
                ? PageNames
                : PageNames.Where(p => p.ToLower().Contains(searchText.ToLower())).ToArray();

            EditorGUILayout.BeginHorizontal();

            // Sidebar
            EditorGUILayout.BeginVertical(GUILayout.Width(150));
            EditorGUILayout.LabelField("Search", EditorStyles.miniLabel);
            searchText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);
            sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);
            foreach (var pageName in visiblePages)
            {
                Page page = (Page)System.Enum.Parse(typeof(Page), pageName);
                GUI.backgroundColor = currentPage == page ? Color.cyan : Color.white;
                if (GUILayout.Button(pageName, EditorStyles.toolbarButton, GUILayout.Height(25)))
                    currentPage = page;
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Content
            EditorGUILayout.BeginVertical();
            contentScroll = EditorGUILayout.BeginScrollView(contentScroll);
            switch (currentPage)
            {
                case Page.Dashboard: DrawDashboard(); break;
                case Page.Character: DrawCharacter(); break;
                case Page.Animation: DrawAnimation(); break;
                case Page.Baking: DrawBaking(); break;
                case Page.Prefab: DrawPrefab(); break;
                case Page.Database: DrawDatabase(); break;
                case Page.Material: DrawMaterial(); break;
                case Page.Texture: DrawTexture(); break;
                case Page.Runtime: DrawRuntime(); break;
                case Page.Debug: DrawDebug(); break;
                case Page.Utilities: DrawUtilities(); break;
                case Page.Settings: DrawSettings(); break;
                case Page.About: DrawAbout(); break;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            DrawStatusBar();
        }

        // ---------- DASHBOARD ----------
        private void DrawDashboard()
        {
            EditorGUILayout.LabelField("VAT System Dashboard", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Unity: {Application.unityVersion}");
            EditorGUILayout.LabelField($"VAT Version: 2.0 (Prefab‑based)");

            int totalAnimPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/VAT/AnimationPrefabs" }).Length;
            int totalChar = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/VAT/Character" }).Length;
            int totalDB = AssetDatabase.FindAssets("t:VATAnimationDatabase").Length;
            int totalTex = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/VAT/Textures" }).Length;

            EditorGUILayout.LabelField($"Animation Prefabs: {totalAnimPrefabs}");
            EditorGUILayout.LabelField($"Character Prefabs: {totalChar}");
            EditorGUILayout.LabelField($"Databases: {totalDB}");
            EditorGUILayout.LabelField($"Textures: {totalTex}");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bake Animations")) OpenBakingWindow();
            if (GUILayout.Button("Create Character")) VATCharacterCreator.CreateCharacterPrefab();
            if (GUILayout.Button("Rebuild Database")) RebuildDatabase();
            if (GUILayout.Button("Validate Project")) VATValidator.Validate(Selection.activeGameObject);
            if (GUILayout.Button("Refresh")) AssetDatabase.Refresh();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCharacter()
        {
            EditorGUILayout.LabelField("Character Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Character Prefab")) VATCharacterCreator.CreateCharacterPrefab();
            if (GUILayout.Button("Open Character Folder")) EditorUtility.RevealInFinder("Assets/VAT/Character");
        }

        private void DrawAnimation()
        {
            EditorGUILayout.LabelField("Animation Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Animation Prefabs Folder")) EditorUtility.RevealInFinder("Assets/VAT/AnimationPrefabs");
        }

        private void DrawBaking()
        {
            EditorGUILayout.LabelField("VAT Baking", EditorStyles.boldLabel);
            if (GUILayout.Button("Open VAT Baking Window")) OpenBakingWindow();
            EditorGUILayout.HelpBox("Use the VAT Baking Window to sample animations and generate prefabs.", MessageType.Info);
        }

        private void OpenBakingWindow() => VATBakingWindow.ShowWindow();

        private void DrawPrefab()
        {
            EditorGUILayout.LabelField("Prefab Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Generate Animation Prefab")) OpenBakingWindow();
            if (GUILayout.Button("Generate Character Prefab")) VATCharacterCreator.CreateCharacterPrefab();
            if (GUILayout.Button("Repair Missing Reference"))
            {
                var go = Selection.activeObject as GameObject;
                if (go != null && VATValidator.ValidatePrefab(go))
                    EditorUtility.DisplayDialog("Valid", "No missing references.", "OK");
                else
                    EditorUtility.DisplayDialog("Error", "Prefab has issues. Check Console.", "OK");
            }
            if (GUILayout.Button("Open Prefabs Folder")) EditorUtility.RevealInFinder("Assets/VAT/Character");
        }

        private void DrawDatabase()
        {
            EditorGUILayout.LabelField("Animation Database", EditorStyles.boldLabel);
            if (GUILayout.Button("Rebuild Database")) RebuildDatabase();
            if (GUILayout.Button("Open Database Folder")) EditorUtility.RevealInFinder("Assets/VAT/AnimationDatabase");
        }

        private void RebuildDatabase()
        {
            string dbPath = "Assets/VAT/AnimationDatabase/VATAnimationDatabase.asset";
            var db = AssetDatabase.LoadAssetAtPath<VATAnimationDatabase>(dbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<VATAnimationDatabase>();
                AssetDatabase.CreateAsset(db, dbPath);
            }
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/VAT/AnimationPrefabs" });
            db.animations.Clear();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null) db.AddOrUpdateAnimation(prefab.name, prefab);
            }
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Database", "Rebuilt successfully.", "OK");
        }

        private void DrawMaterial()
        {
            EditorGUILayout.LabelField("Material Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Materials Folder")) EditorUtility.RevealInFinder("Assets/VAT/Materials");
        }

        private void DrawTexture()
        {
            EditorGUILayout.LabelField("Texture Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Textures Folder")) EditorUtility.RevealInFinder("Assets/VAT/Textures");
            if (GUILayout.Button("Fix All Texture Import Settings"))
            {
                VATTextureGenerator.FixAllTextureImports();
            }
        }

        private void DrawRuntime()
        {
            EditorGUILayout.LabelField("Runtime Preview", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Play the scene to test animations.", MessageType.Info);
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Stop")) EditorApplication.isPlaying = false;
            }
            else
            {
                if (GUILayout.Button("Play")) EditorApplication.isPlaying = true;
            }
        }

        private void DrawDebug()
        {
            EditorGUILayout.LabelField("Debug & Validation", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate Selection")) VATValidator.Validate(Selection.activeGameObject);
            if (GUILayout.Button("Validate Prefab")) VATValidator.ValidatePrefab(Selection.activeObject as GameObject);
        }

        private void DrawUtilities()
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh AssetDatabase")) AssetDatabase.Refresh();
            if (GUILayout.Button("Reimport VAT Assets")) AssetDatabase.ImportAsset("Assets/VAT", ImportAssetOptions.ImportRecursive);
            if (GUILayout.Button("Clear Output Folder"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Delete all contents of Assets/VAT?", "Yes", "No"))
                {
                    if (Directory.Exists("Assets/VAT"))
                        Directory.Delete("Assets/VAT", true);
                    AssetDatabase.Refresh();
                }
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Output Root: Assets/VAT");
        }

        private void DrawAbout()
        {
            EditorGUILayout.LabelField("VAT System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Version 2.0 – Prefab-based");
            EditorGUILayout.LabelField("Based on Fuqunaga/VatBaker");
        }

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            bool charOk = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/VAT/Character" }).Length > 0;
            bool animOk = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/VAT/AnimationPrefabs" }).Length > 0;
            bool dbOk = AssetDatabase.FindAssets("t:VATAnimationDatabase").Length > 0;
            bool matOk = AssetDatabase.FindAssets("t:Material", new[] { "Assets/VAT" }).Length > 0;
            bool texOk = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/VAT/Textures" }).Length > 0;

            DrawStatusIcon(charOk, "Character");
            DrawStatusIcon(animOk, "Animation");
            DrawStatusIcon(dbOk, "Database");
            DrawStatusIcon(matOk, "Material");
            DrawStatusIcon(texOk, "Texture");

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Status", GUILayout.Width(40));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusIcon(bool ok, string label)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.normal.textColor = ok ? Color.green : Color.red;
            EditorGUILayout.LabelField(label, style, GUILayout.Width(70));
        }

        private void ComparePrefabs()
        {
            var orig = Selection.objects.FirstOrDefault() as GameObject;
            if (orig == null) { Debug.LogError("Select original prefab first."); return; }

            var vat = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VAT/Character/YourCharacter.prefab");
            if (vat == null) { Debug.LogError("VAT character prefab not found."); return; }

            bool match = VATValidator.ValidateAgainstOriginal(orig, vat);
            Debug.Log(match ? "Prefabs are visually identical." : "Differences found. Check Console.");
        }

        private void CompareCharacterWithDefaultAnimation()
        {
            var db = AssetDatabase.LoadAssetAtPath<VATAnimationDatabase>("Assets/VAT/AnimationDatabase/VATAnimationDatabase.asset");
            if (db == null) return;

            var defaultPrefab = db.GetAnimationPrefab("Idle") ?? db.animations[0].animationPrefab;
            var charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VAT/Character/YourCharacter.prefab");

            if (defaultPrefab != null && charPrefab != null)
            {
                bool match = VATValidator.ValidateAgainstOriginal(defaultPrefab, charPrefab);
                Debug.Log(match ? "Character matches default animation prefab." : "Mismatch detected!");
            }
        }
    }
}