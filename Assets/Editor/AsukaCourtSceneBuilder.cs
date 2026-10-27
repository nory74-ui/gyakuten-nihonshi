#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsukaHistoryCourt.EditorTools
{
    /// <summary>
    /// Resources/Images 内のイラスト画像を自動的にUI用Spriteとして設定し、
    /// Resources/Fonts 内の日本語フォントをビルド同梱可能なDynamic設定にするインポーター
    /// </summary>
    public class AsukaIllustrationImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.Contains("Resources/Images/"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
            }
        }
    }

    /// <summary>
    /// Unity未経験の方でも迷わず1クリック（または自動）でシーンのセットアップや
    /// Webブラウザ版（WebGL）・Mac版・Windows版の書き出しができるエディタ拡張
    /// </summary>
    [InitializeOnLoad]
    public static class AsukaCourtSceneBuilder
    {
        private const string BundledFontAssetPath = "Assets/Resources/Fonts/JpGameFont.otf";

        static AsukaCourtSceneBuilder()
        {
            EditorApplication.delayCall += AutoSetupIfNeeded;
        }

        private static void AutoSetupIfNeeded()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            ConfigureBundledFontIfNeeded();

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded) return;

            if (Object.FindFirstObjectByType<AsukaCourtGameManager>() == null)
            {
                SetupSceneInternal(showDialog: false);
            }
        }

        private static void ConfigureBundledFontIfNeeded()
        {
            TrueTypeFontImporter fontImporter = AssetImporter.GetAtPath(BundledFontAssetPath) as TrueTypeFontImporter;
            if (fontImporter != null)
            {
                bool changed = false;
                if (fontImporter.fontTextureCase != FontTextureCase.Dynamic)
                {
                    fontImporter.fontTextureCase = FontTextureCase.Dynamic;
                    changed = true;
                }
                if (!fontImporter.includeFontData)
                {
                    fontImporter.includeFontData = true;
                    changed = true;
                }
                if (changed)
                {
                    fontImporter.SaveAndReimport();
                }
            }
        }

        [MenuItem("逆転日本史/① 1クリックでゲームをシーンに配置＆保存", false, 10)]
        public static void MenuSetupScene()
        {
            SetupSceneInternal(showDialog: true);
        }

        [MenuItem("逆転日本史/② 今すぐテストプレイ開始（▶）", false, 20)]
        public static void MenuPlayGameNow()
        {
            if (!EditorApplication.isPlaying)
            {
                SetupSceneInternal(showDialog: false);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("逆転日本史/🌐 ③ Webブラウザ版（WebGL）を書き出す（スマホ・他PC両対応）", false, 40)]
        public static void MenuBuildWebGL()
        {
            ExecuteBuild(
                BuildTarget.WebGL,
                "WebGL",
                "",
                "Webブラウザ版（WebGL）"
            );
        }

        [MenuItem("逆転日本史/💻 ④ Mac用アプリ（.app）を書き出す", false, 50)]
        public static void MenuBuildMac()
        {
            ExecuteBuild(
                BuildTarget.StandaloneOSX,
                "Mac",
                "GyakutenNihonshi.app",
                "Mac用アプリ（.app）"
            );
        }

        [MenuItem("逆転日本史/🪟 ⑤ Windows用アプリ（.exe）を書き出す", false, 60)]
        public static void MenuBuildWindows()
        {
            ExecuteBuild(
                BuildTarget.StandaloneWindows64,
                "Windows",
                "GyakutenNihonshi.exe",
                "Windows用アプリ（.exe）"
            );
        }

        [MenuItem("逆転日本史/🚀 ⑥ GitHub Pages公開用（docsフォルダ）を更新", false, 70)]
        public static void MenuUpdateDocsForGitHubPages()
        {
            ExecuteBuild(
                BuildTarget.WebGL,
                "WebGL",
                "",
                "GitHub Pages公開用Web版"
            );
        }

        private static void SetupSceneInternal(bool showDialog)
        {
            ConfigureBundledFontIfNeeded();

            AsukaCourtGameManager existing = Object.FindFirstObjectByType<AsukaCourtGameManager>();
            if (existing == null)
            {
                GameObject rootObj = new GameObject("AsukaHistoryCourtSystem");
                rootObj.AddComponent<RetroSoundPlayer>();
                rootObj.AddComponent<AsukaCourtGameManager>();
                Undo.RegisterCreatedObjectUndo(rootObj, "Create AsukaHistoryCourtSystem");
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }

            EnsureActiveSceneInBuildSettings();

            if (showDialog && !Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "『逆転日本史』セットアップ完了！",
                    "『逆転日本史 〜第1話：逆転の飛鳥時代〜』のセットアップが完了しました！\n\n" +
                    "・画面上部の「▶（再生ボタン）」を押すとすぐに遊べます。\n" +
                    "・他のPCやスマホで遊べるように書き出す場合は、上部メニュー『逆転日本史』の③〜⑥を選んでください！",
                    "OK"
                );
            }
        }

        private static string EnsureActiveSceneInBuildSettings()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && string.IsNullOrEmpty(activeScene.path))
            {
                string defaultSceneDir = "Assets/Scenes";
                if (!Directory.Exists(defaultSceneDir))
                {
                    Directory.CreateDirectory(defaultSceneDir);
                }
                string savePath = Path.Combine(defaultSceneDir, "SampleScene.unity");
                EditorSceneManager.SaveScene(activeScene, savePath);
                activeScene = SceneManager.GetActiveScene();
            }

            string scenePath = activeScene.path;
            if (!string.IsNullOrEmpty(scenePath))
            {
                var existingScenes = EditorBuildSettings.scenes;
                bool found = false;
                for (int i = 0; i < existingScenes.Length; i++)
                {
                    if (existingScenes[i].path == scenePath)
                    {
                        existingScenes[i].enabled = true;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    var nextScenes = new EditorBuildSettingsScene[existingScenes.Length + 1];
                    nextScenes[0] = new EditorBuildSettingsScene(scenePath, true);
                    for (int i = 0; i < existingScenes.Length; i++)
                    {
                        nextScenes[i + 1] = existingScenes[i];
                    }
                    EditorBuildSettings.scenes = nextScenes;
                }
                else
                {
                    EditorBuildSettings.scenes = existingScenes;
                }
            }
            return scenePath;
        }

        private static void ExecuteBuild(BuildTarget target, string subFolder, string executableName, string label)
        {
            if (EditorApplication.isPlaying)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("プレイ中はビルドできません", "一度再生ボタン（▶）を止めてから実行してください。", "OK");
                }
                return;
            }

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            if (!BuildPipeline.IsBuildTargetSupported(targetGroup, target))
            {
                string moduleName = target == BuildTarget.WebGL
                    ? "Web Build Support (WebGL)"
                    : (target == BuildTarget.StandaloneWindows64 ? "Windows Build Support (Mono)" : "Mac Build Support");

                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        $"{label} モジュールが未インストールです",
                        $"Unity Hub で「{moduleName}」を追加すると、1クリックで書き出せるようになります！\n\n" +
                        "【追加手順（約2分）】\n" +
                        "1. Unity Hub を開く\n" +
                        "2. 左メニュー「インストール（Installs）」を開く\n" +
                        "3. 使用中の Unity（6000.x）の右にある歯車（⚙）＞「モジュールを加える」をクリック\n" +
                        $"4. 「{moduleName}」にチェックを入れてインストール",
                        "OK"
                    );
                }
                else
                {
                    Debug.LogError($"[BuildError] {moduleName} is not installed!");
                }
                return;
            }

            SetupSceneInternal(showDialog: false);
            EditorSceneManager.SaveOpenScenes();
            string scenePath = EnsureActiveSceneInBuildSettings();

            PlayerSettings.productName = "逆転日本史";
            PlayerSettings.companyName = "AsukaHistoryCourt";
            PlayerSettings.runInBackground = true;

            if (target == BuildTarget.WebGL)
            {
                // どの無料公開サイト（GitHub Pages / itch.io / Netlify Drop）でもそのまま動くよう非圧縮に設定
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.template = "APPLICATION:Default";
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildFolder = Path.Combine(projectRoot, "Builds", subFolder);
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string locationPathName = string.IsNullOrEmpty(executableName)
                ? buildFolder
                : Path.Combine(buildFolder, executableName);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { scenePath },
                locationPathName = locationPathName,
                target = target,
                options = BuildOptions.None
            };

            Debug.Log($"[Build] Starting build for {target} -> {locationPathName}");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] Successfully built {label} to: {locationPathName}");

                // WebGLビルドの場合は、GitHub Pages公開用の docs/ フォルダへ自動同期＆.nojekyll生成
                if (target == BuildTarget.WebGL)
                {
                    string docsFolder = Path.Combine(projectRoot, "docs");
                    SyncDirectory(buildFolder, docsFolder);
                    File.WriteAllText(Path.Combine(docsFolder, ".nojekyll"), "");
                    Debug.Log($"[Build] Synced WebGL build to docs/ for GitHub Pages (.nojekyll created)");
                }

                if (!Application.isBatchMode)
                {
                    EditorUtility.RevealInFinder(locationPathName);

                    string extraGuide = target == BuildTarget.WebGL
                        ? "\n\n【GitHub Pagesで誰でも遊べるようにするには？】\n" +
                          "プロジェクト直下の「docs」フォルダが自動更新されました！\n" +
                          "GitHubにプッシュし、リポジトリ設定（Settings > Pages）で「Folder: /docs」を選ぶだけで、世界中の誰もがURLひとつでブラウザから遊べます！"
                        : "\n\n書き出されたアプリファイルをそのまま他のパソコンにコピー（またはZIPで共有）して起動できます！";

                    EditorUtility.DisplayDialog(
                        $"🎉 {label} の書き出し完了！",
                        $"書き出し先:\n{locationPathName}" + extraGuide,
                        "OK"
                    );
                }
            }
            else
            {
                Debug.LogError($"[Build] Failed: {summary.result}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        $"{label} の書き出しに失敗しました",
                        $"結果: {summary.result}\nConsoleウィンドウのエラーログを確認してください。",
                        "OK"
                    );
                }
            }
        }

        private static void SyncDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(targetDir, fileName), true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDir);
                SyncDirectory(subDir, Path.Combine(targetDir, subDirName));
            }
        }
    }
}
#endif

