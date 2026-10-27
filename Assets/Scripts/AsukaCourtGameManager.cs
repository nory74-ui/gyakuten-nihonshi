using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace AsukaHistoryCourt
{
    /// <summary>
    /// 『逆転日本史 〜第1話：逆転の飛鳥時代〜』
    /// 添付された『逆転裁判』のタイトル画面（黒背景＋白指差しシルエット＋紫グラデ白縁ロゴ）および
    /// 法廷・探偵プレイ画面（フルスクリーン背景＋大判立ち絵＋木製手すり＋紫紺台形ネームプレート付き半透明黒メッセージ枠＋ギザギザ爆発吹き出し『異議あり！』『待った！』）を完全再現。
    /// </summary>
    public class AsukaCourtGameManager : MonoBehaviour
    {
        public enum GamePhase
        {
            Title,
            Opening,
            Investigation,
            Court,
            Ending
        }

        public enum CourtCameraAngle
        {
            WitnessStand, // 証人席（蘇我 馬子）
            DefenseBench, // 弁護席（史堂 レイ）
            CoCounsel     // 助手席（古文 ミオ）
        }

        public enum CharacterEmotion
        {
            Normal,   // 通常
            Smile,    // 笑顔・自信
            Sweat,    // 焦り・冷や汗
            Shocked,  // 驚き・ショック
            Thinking, // 考え中・考察
            Pointing, // 指差し（異議あり！）
            Damage,   // 大ショック・論破ダメージ
            Laugh     // 高笑い・嘲笑
        }

        [Header("現在のゲーム状態")]
        public GamePhase currentPhase = GamePhase.Title;

        // エピソード管理システム（第1話：飛鳥時代 / 第2話：奈良時代）
        public const string ClearedEpisodePrefKey = "Gyakuten_ClearedEpisode";
        private int _currentEpisodeNumber = 1;
        private EpisodeData _currentEpisodeData;
        private GameObject _episodeSelectModalPanel;
        private Text _endingHeaderText;
        private Text _endingSummarySheetText;
        private Button _endingNextEraBtn;
        private GameObject _ep2LockedMask;
        private Text _ep2StatusBadge;
        private Button _ep2PlayBtn;
        private Button _ep2CourtBtn;

        // データベース
        private List<EvidenceItem> _allEvidences;
        private HashSet<string> _collectedEvidenceIds = new HashSet<string>();
        private HashSet<string> _inspectedSpotIds = new HashSet<string>();
        private HashSet<string> _talkedTopicIds = new HashSet<string>();
        private List<LocationData> _locations;
        private List<CrossExaminationRound> _courtRounds;

        // サウンド＆フォント＆画像キャッシュ
        private RetroSoundPlayer _sound;
        private Font _jpFont;
        private Sprite _whiteSprite;
        private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        // ルートUI要素
        private Canvas _mainCanvas;
        private RectTransform _shakeContainer;
        private Image _flashOverlay;

        // 各パートのパネル
        private GameObject _titlePanel;
        private GameObject _openingPanel;
        private GameObject _investigationPanel;
        private GameObject _courtPanel;
        private GameObject _endingPanel;
        private GameObject _evidenceModalPanel;
        private GameObject _cutInOverlayPanel;

        // オープニング演出用UI（アニメ調：教室で居眠り➔時空の渦➔飛鳥時代へタイムスリップ）
        private Image _opBgImage;
        private Image _opIllustrationImage;
        private RectTransform _opIlluRect;
        private Image _opCharacterImage;
        private RectTransform _opCharRect;
        private RectTransform _opTopEyelidBar;
        private RectTransform _opBottomEyelidBar;
        private GameObject _opEpisodeTitleBanner;
        private Text _opEpisodeTitleText;
        private Text _opEraYearText;
        private Text _opVisualArtText;
        private Text _opSpeakerText;
        private Text _opBodyText;
        private Coroutine _opCoroutine;
        private Coroutine _opBgMotionCoroutine;
        private bool _opAdvanceRequested = false;
        private bool _isTypingText = false;
        private bool _skipCurrentTyping = false;

        // 探偵パート用UI
        private int _currentLocationIndex = 0;
        private Image _invFieldBackgroundImage;
        private Image _invStandingCharImage;
        private Text _invLocationTitleText;
        private Text _invProgressText;
        private RectTransform _invHotspotContainer;
        private GameObject _invMoveOverlayPanel;
        private GameObject _invTalkOverlayPanel;
        private Transform _invTalkListContainer;
        private Text _invSpeakerText;
        private Text _invDialogueText;
        private Button _invNextLineButton;
        private Button _invGoToCourtButton;
        private List<Button> _locationTabButtons = new List<Button>();
        private Queue<string> _invDialogueQueue = new Queue<string>();
        private Action _onInvDialogueFinished;
        private Coroutine _typingCoroutine;

        // 法廷パート用UI（参照画像1〜4 完全再現）
        private int _currentRoundIndex = 0;
        private int _currentStatementIndex = 0;
        private int _playerTrustLife = 5;
        private const int MaxTrustLife = 5;
        private bool _isInCourtConversation = false;
        private Image[] _penaltyBarSegments = new Image[MaxTrustLife];
        private Button _topLeftPressBtn;
        private Button _topRightPresentBtn;
        private RectTransform _courtBgRect;
        private GameObject _witnessStageGroup;
        private GameObject _defenseStageGroup;
        private GameObject _counselStageGroup;
        private Image _witnessPortraitImage;
        private Image _defensePortraitImage;
        private Image _counselPortraitImage;
        private Text _witnessEffectEmojiText;
        private Text _statementIndexDotsText;
        private Image _courtSpeakerPlateImg;
        private Text _courtSpeakerText;
        private Text _courtDialogueText;
        private GameObject _courtCrossExamArrows;
        private Button _courtNextLineBtn;
        private Button _courtWholeBoxNextBtn;
        private Queue<string> _courtDialogueQueue = new Queue<string>();
        private Action _onCourtDialogueFinished;

        // 法廷記録（史料ファイル）モーダル用UI
        private bool _isSelectingEvidenceToPresent = false;
        private string _selectedEvidenceId = "";
        private Transform _evidenceListContainer;
        private Text _evidenceModalHeaderText;
        private Image _evidenceDetailPreviewImage;
        private Text _evidenceDetailIconText;
        private Text _evidenceDetailTitleText;
        private Text _evidenceDetailSubText;
        private Text _evidenceDetailDescText;
        private Text _evidenceDetailExamText;
        private Button _presentEvidenceConfirmButton;

        // ギザギザ爆発吹き出しカットイン（「異議あり！」「待った！」）用UI
        private Image _cutInBurstImage;
        private RectTransform _cutInBurstRect;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootOnPlay()
        {
            if (FindFirstObjectByType<AsukaCourtGameManager>() == null)
            {
                GameObject sysObj = new GameObject("AsukaHistoryCourtSystem");
                sysObj.AddComponent<AsukaCourtGameManager>();
            }
        }

        public int GetMaxClearedEpisode()
        {
            return PlayerPrefs.GetInt(ClearedEpisodePrefKey, 0);
        }

        public void MarkEpisodeCleared(int epNum)
        {
            int cur = GetMaxClearedEpisode();
            if (epNum > cur)
            {
                PlayerPrefs.SetInt(ClearedEpisodePrefKey, epNum);
                PlayerPrefs.Save();
            }
        }

        public void LoadEpisode(int epNum)
        {
            _currentEpisodeNumber = epNum;
            _currentEpisodeData = HistoryDatabase.GetEpisode(epNum);
            _allEvidences = _currentEpisodeData.evidences;
            _locations = _currentEpisodeData.locations;
            _courtRounds = _currentEpisodeData.courtRounds;

            _collectedEvidenceIds.Clear();
            _inspectedSpotIds.Clear();
            _talkedTopicIds.Clear();
            _currentLocationIndex = 0;
            _currentRoundIndex = 0;
            _currentStatementIndex = 0;
            _playerTrustLife = MaxTrustLife;
        }

        private void Awake()
        {
            InitializeResources();
            LoadEpisode(1);
            EnsureCameraAndEventSystem();
            BuildAllGameUI();
            SwitchPhase(GamePhase.Title);
        }

        private void InitializeResources()
        {
            _sound = gameObject.GetComponent<RetroSoundPlayer>();
            if (_sound == null)
            {
                _sound = gameObject.AddComponent<RetroSoundPlayer>();
            }
            _sound.Initialize();

            // 1. プロジェクトに同梱された日本語フォント (Assets/Resources/Fonts/JpGameFont.otf) を最優先でロード
            //    これにより WebGL（ブラウザ版）・スマホ（iOS/Android）・他のPC（Windows/Mac）でも文字化けせず確実に表示！
            _jpFont = Resources.Load<Font>("Fonts/JpGameFont");

            // 2. 万が一見つからない場合はOSフォントから動的生成
            if (_jpFont == null)
            {
                string[] jpFontCandidates = new string[]
                {
                    "Hiragino Sans",
                    "Hiragino Kaku Gothic ProN",
                    "YuGothic",
                    "Yu Gothic",
                    "Meiryo",
                    "MS Gothic",
                    "Noto Sans CJK JP",
                    "Arial Unicode MS",
                    "Arial"
                };
                _jpFont = Font.CreateDynamicFontFromOSFont(jpFontCandidates, 40);
            }

            if (_jpFont == null)
            {
                _jpFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        }

        private Sprite LoadIllustrationSprite(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName)) return null;
            if (_spriteCache.TryGetValue(resourceName, out Sprite cached) && cached != null)
            {
                return cached;
            }

            string path = "Images/" + resourceName;
            Sprite sp = Resources.Load<Sprite>(path);
            if (sp == null)
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                {
                    sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            if (sp != null)
            {
                _spriteCache[resourceName] = sp;
            }
            return sp;
        }

        private void LoadDatabase()
        {
            _allEvidences = AsukaEpisodeDatabase.GetAllEvidences();
            _locations = AsukaEpisodeDatabase.GetAllLocations();
            _courtRounds = AsukaEpisodeDatabase.GetCourtRounds();
        }

        private void EnsureCameraAndEventSystem()
        {
            if (Camera.main == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                Camera cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                camObj.AddComponent<AudioListener>();
            }

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// 発言者名を逆転裁判のネームプレート風（『レイ』『ミオ』『馬子』など短く端的な表記）に整形
        /// </summary>
        private string FormatShortSpeakerName(string rawSpeaker)
        {
            if (string.IsNullOrEmpty(rawSpeaker)) return "";
            if (rawSpeaker.Contains("史堂") || rawSpeaker == "レイ") return "レイ";
            if (rawSpeaker.Contains("古文") || rawSpeaker == "ミオ") return "ミオ";
            if (rawSpeaker.Contains("馬子")) return "馬子";
            if (rawSpeaker.Contains("妹子")) return "小野妹子";
            if (rawSpeaker.Contains("中大兄")) return "中大兄皇子";
            if (rawSpeaker.Contains("仲麻呂") || rawSpeaker.Contains("藤原")) return "仲麻呂";
            if (rawSpeaker.Contains("行基")) return "行基";
            if (rawSpeaker.Contains("鑑真")) return "鑑真";
            if (rawSpeaker.Contains("農民")) return "農民";
            if (rawSpeaker.Contains("裁判長")) return "裁判長";
            return rawSpeaker;
        }

        // =====================================================================
        // 全パートのUI自動構築
        // =====================================================================
        private void BuildAllGameUI()
        {
            Transform oldCanvas = transform.Find("GameCanvas");
            if (oldCanvas != null)
            {
                Destroy(oldCanvas.gameObject);
            }

            GameObject canvasObj = new GameObject("GameCanvas");
            canvasObj.transform.SetParent(transform, false);
            _mainCanvas = canvasObj.AddComponent<Canvas>();
            _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _mainCanvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            _shakeContainer = CreatePanel(canvasObj.transform, "ShakeContainer", new Color(0, 0, 0, 0));
            SetStretch(_shakeContainer);

            _titlePanel = BuildTitlePanel(_shakeContainer);
            _openingPanel = BuildOpeningPanel(_shakeContainer);
            _investigationPanel = BuildInvestigationPanel(_shakeContainer);
            _courtPanel = BuildCourtPanel(_shakeContainer);
            _endingPanel = BuildEndingPanel(_shakeContainer);
            _evidenceModalPanel = BuildEvidenceModalPanel(_shakeContainer);
            _cutInOverlayPanel = BuildCutInOverlay(_shakeContainer);
            _episodeSelectModalPanel = BuildEpisodeSelectModalPanel(_shakeContainer);

            RectTransform flashRect = CreatePanel(canvasObj.transform, "FlashOverlay", new Color(1f, 1f, 1f, 0f));
            SetStretch(flashRect);
            _flashOverlay = flashRect.GetComponent<Image>();
            _flashOverlay.raycastTarget = false;
        }

        // =====================================================================
        // ① タイトル画面の構築（参照画像5: 初代『逆転裁判』タイトル画面を完全再現！）
        // =====================================================================
        private GameObject BuildTitlePanel(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "TitlePanel", Color.black);
            SetStretch(panel);

            // 中央：『逆転日本史』ロゴ＋白い指差しシルエット＋黄色い閃光スター
            RectTransform logoRt = CreatePanel(panel, "TitleLogo", Color.white);
            SetAnchor(logoRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1280f, 680f));
            Image logoImg = logoRt.GetComponent<Image>();
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;
            Sprite logoSp = LoadIllustrationSprite("title_logo_gyakuten");
            if (logoSp != null)
            {
                logoImg.sprite = logoSp;
            }

            // メインボタン1：『はじめから（第1話スタート）』
            Button startBtn = CreateSpriteOrTextButton(
                panel,
                "StartBtn_Hajimekara",
                "title_btn_start",
                "① 第1話から",
                new Vector2(-220f, -230f),
                new Vector2(360f, 84f),
                46
            );
            startBtn.onClick.AddListener(() =>
            {
                _sound.PlayObjection();
                StartNewGameEpisode(1);
            });

            // メインボタン2：『エピソード選択（時代の復習）』
            Button epSelectBtn = CreateAceCornerButton(
                panel,
                "EpisodeSelectBtn",
                "📖 エピソード選択（復習）",
                new Color(0.12f, 0.44f, 0.38f, 0.96f),
                new Vector2(220f, -230f),
                new Vector2(400f, 84f),
                34
            );
            epSelectBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                OpenEpisodeSelectModal();
            });

            // サブボタン1：『法廷パートから』（直接尋問対決へ）
            Button courtDirectBtn = CreateSpriteOrTextButton(
                panel,
                "CourtDirectBtn",
                "title_btn_court",
                "⚖️ 法廷へ直行",
                new Vector2(-200f, -335f),
                new Vector2(320f, 64f),
                32
            );
            courtDirectBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                StartCourtDirectly();
            });

            // サブボタン2：『法廷記録』（史料図鑑）
            Button recordDirectBtn = CreateSpriteOrTextButton(
                panel,
                "RecordDirectBtn",
                "title_btn_record",
                "📁 法廷記録",
                new Vector2(200f, -335f),
                new Vector2(260f, 64f),
                32
            );
            recordDirectBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                UnlockAllEvidences();
                OpenEvidenceModal(false);
            });

            // 最下部コピーライト表記（参照画像5完全再現）
            Text copyText = CreateText(
                panel,
                "CopyrightText",
                "©GYAKUTEN NIHONSHI PROJECT 2026 ALL RIGHTS RESERVED.",
                32,
                FontStyle.Bold,
                Color.white,
                TextAnchor.MiddleCenter
            );
            SetAnchor(copyText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 42f), new Vector2(1600f, 50f));

            return panel.gameObject;
        }

        private Button CreateSpriteOrTextButton(Transform parent, string name, string spriteName, string fallbackText, Vector2 pos, Vector2 size, int fontSize)
        {
            RectTransform rt = CreatePanel(parent, name, Color.white);
            SetAnchor(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Image img = rt.GetComponent<Image>();
            Sprite sp = LoadIllustrationSprite(spriteName);
            if (sp != null)
            {
                img.sprite = sp;
                img.preserveAspect = true;
            }
            else
            {
                img.color = new Color(0, 0, 0, 0);
                Text txt = CreateText(rt, "Label", fallbackText, fontSize, FontStyle.Bold, new Color(0.32f, 0.24f, 0.72f), TextAnchor.MiddleCenter);
                Outline o = txt.gameObject.AddComponent<Outline>();
                o.effectColor = Color.white;
                o.effectDistance = new Vector2(3f, -3f);
            }

            Button btn = rt.gameObject.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 0.96f, 0.65f, 1f);
            cb.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            btn.colors = cb;
            return btn;
        }

        // =====================================================================
        // ② アニメ調オープニング演出パネル
        //    （現代の教室で居眠り ➔ まぶたが閉じる ➔ 時空の渦 ➔ 目覚めると飛鳥時代！）
        // =====================================================================
        private GameObject BuildOpeningPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "OpeningPanel", Color.black);
            SetStretch(panel);
            _opBgImage = panel.GetComponent<Image>();

            // フルスクリーン背景イラスト（アニメカメラワーク用に少し大きめに配置）
            RectTransform illuRect = CreatePanel(panel, "OpIllustration", Color.white);
            SetAnchor(illuRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2060f, 1160f));
            _opIlluRect = illuRect;
            _opIllustrationImage = illuRect.GetComponent<Image>();

            // 大判キャラクター立ち絵
            RectTransform charRect = CreatePanel(panel, "OpCharSprite", Color.white);
            SetAnchor(charRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 460f), new Vector2(820f, 860f));
            _opCharRect = charRect;
            _opCharacterImage = charRect.GetComponent<Image>();
            _opCharacterImage.preserveAspect = true;
            _opCharacterImage.raycastTarget = false;

            // アニメの「まぶた開閉（居眠り〜目覚め）」＆シネマスコープ演出用の黒バー（上下）
            RectTransform topBar = CreatePanel(panel, "OpTopEyelidBar", Color.black);
            SetAnchor(topBar, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 0f));
            topBar.pivot = new Vector2(0.5f, 1f);
            topBar.GetComponent<Image>().raycastTarget = false;
            _opTopEyelidBar = topBar;

            RectTransform bottomBar = CreatePanel(panel, "OpBottomEyelidBar", Color.black);
            SetAnchor(bottomBar, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 0f));
            bottomBar.pivot = new Vector2(0.5f, 0f);
            bottomBar.GetComponent<Image>().raycastTarget = false;
            _opBottomEyelidBar = bottomBar;

            // 左上：アニメ場面・時代テロップボックス
            RectTransform eraBox = CreateAceDialogueStyleBox(panel, "OpEraBox", new Color(0.03f, 0.05f, 0.12f, 0.88f));
            SetAnchor(eraBox, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(610f, -68f), new Vector2(1140f, 108f));
            _opEraYearText = CreateText(eraBox, "OpEraYearText", "", 34, FontStyle.Bold, new Color(0.35f, 1f, 0.50f), TextAnchor.UpperLeft);
            SetPadding(_opEraYearText.rectTransform, 26f, 26f, 12f, 52f);
            _opVisualArtText = CreateText(eraBox, "OpVisualArtText", "", 27, FontStyle.Bold, new Color(1f, 0.94f, 0.72f), TextAnchor.LowerLeft);
            SetPadding(_opVisualArtText.rectTransform, 26f, 26f, 54f, 12f);

            // 中央：アニメ第1話サブタイトル・アイキャッチバナー（クライマックスで表示）
            RectTransform epBanner = CreateAceDialogueStyleBox(panel, "OpEpisodeTitleBanner", new Color(0.08f, 0.06f, 0.22f, 0.96f));
            SetAnchor(epBanner, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1560f, 124f));
            _opEpisodeTitleText = CreateText(epBanner, "OpEpisodeTitleText", "🎬 第1話：逆転の飛鳥時代 〜聖徳太子から大化の改新へ〜", 42, FontStyle.Bold, new Color(1f, 0.92f, 0.38f), TextAnchor.MiddleCenter);
            SetPadding(_opEpisodeTitleText.rectTransform, 24f, 24f, 12f, 12f);
            _opEpisodeTitleBanner = epBanner.gameObject;
            _opEpisodeTitleBanner.SetActive(false);

            // 右上スキップボタン（大きく見やすい文字サイズ）
            Button skipBtn = CreateAceCornerButton(panel, "SkipOpBtn", "SKIP ▶▶", new Color(0.16f, 0.18f, 0.34f, 0.94f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(skipBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-135f, -54f), new Vector2(210f, 64f));
            skipBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                FinishOpeningAndStartInvestigation();
            });

            // 逆転裁判メッセージウィンドウ（クリックまたは『▶』ボタンでも読み進め可能）
            BuildAceDialogueWindow(panel, "OpDialogueBox", out _, out _opSpeakerText, out _opBodyText, out Button opNextBtn, out Button opWholeBoxBtn);
            opNextBtn.onClick.AddListener(OnClickAdvanceOpening);
            opWholeBoxBtn.onClick.AddListener(OnClickAdvanceOpening);

            return panel.gameObject;
        }

        private void OnClickAdvanceOpening()
        {
            if (_isTypingText)
            {
                _skipCurrentTyping = true;
            }
            else
            {
                _sound.PlayClick();
                _opAdvanceRequested = true;
            }
        }

        // =====================================================================
        // ③ 探偵パート（フルスクリーン背景＋古文ミオ立ち絵＋逆転裁判メッセージ枠＋4コマンド）
        // =====================================================================
        private GameObject BuildInvestigationPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "InvestigationPanel", Color.black);
            SetStretch(panel);

            // フルスクリーン背景イラスト
            RectTransform bgImgRect = CreatePanel(panel, "LocationIllustration", Color.white);
            SetStretch(bgImgRect);
            _invFieldBackgroundImage = bgImgRect.GetComponent<Image>();

            // 右側に助手・古文ミオ（または史堂レイ）の大判立ち絵
            RectTransform standingCharRect = CreatePanel(panel, "InvStandingChar", Color.white);
            SetAnchor(standingCharRect, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-310f, 440f), new Vector2(640f, 760f));
            _invStandingCharImage = standingCharRect.GetComponent<Image>();
            _invStandingCharImage.preserveAspect = true;
            _invStandingCharImage.raycastTarget = false;
            Sprite mioSp = LoadIllustrationSprite("char_mio");
            if (mioSp != null) _invStandingCharImage.sprite = mioSp;

            // ホットスポット配置コンテナ
            _invHotspotContainer = CreatePanel(panel, "HotspotContainer", new Color(0, 0, 0, 0));
            SetStretch(_invHotspotContainer);

            // 左上：現在地プレート＆入手史料数（文字サイズ拡大）
            RectTransform locPlate = CreateAceDialogueStyleBox(panel, "InvLocationPlate", new Color(0.05f, 0.06f, 0.14f, 0.90f));
            SetAnchor(locPlate, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, -50f), new Vector2(680f, 72f));
            _invLocationTitleText = CreateText(locPlate, "InvLocTitle", "① 飛鳥の朝廷（604年頃）", 29, FontStyle.Bold, new Color(0.40f, 1f, 0.55f), TextAnchor.MiddleLeft);
            SetPadding(_invLocationTitleText.rectTransform, 20f, 175f, 6f, 6f);
            _invProgressText = CreateText(locPlate, "InvProgress", "史料 0/7", 28, FontStyle.Bold, new Color(1f, 0.92f, 0.45f), TextAnchor.MiddleRight);
            SetPadding(_invProgressText.rectTransform, 480f, 20f, 6f, 6f);

            // 右上：『法廷パートへ進む』＆『法廷記録』アイコンボタン（文字サイズ拡大）
            _invGoToCourtButton = CreateAceCornerButton(panel, "GoToCourtBtn", "⚖️ 法廷へ進む", new Color(0.75f, 0.12f, 0.15f, 0.95f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(_invGoToCourtButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-365f, -50f), new Vector2(320f, 68f));
            _invGoToCourtButton.onClick.AddListener(() =>
            {
                _sound.PlayObjection();
                UnlockAllEvidences();
                StartCourtMode();
            });

            Button topRecordBtn = CreateAceCornerButton(panel, "InvRecordBtn", "📁 法廷記録", new Color(0.32f, 0.24f, 0.12f, 0.95f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(topRecordBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-105f, -50f), new Vector2(185f, 68f));
            topRecordBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                OpenEvidenceModal(false);
            });

            // 『移動する』サブメニューオーバーレイ
            RectTransform moveOverlay = CreateAceDialogueStyleBox(panel, "MoveOverlayPanel", new Color(0.04f, 0.06f, 0.14f, 0.96f));
            SetAnchor(moveOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, 130f), new Vector2(980f, 430f));
            _invMoveOverlayPanel = moveOverlay.gameObject;

            CreateText(moveOverlay, "MoveHeader", "【 移動する場所を選択 】", 32, FontStyle.Bold, new Color(1f, 0.92f, 0.45f), TextAnchor.UpperCenter)
                .rectTransform.anchoredPosition = new Vector2(0f, -16f);

            _locationTabButtons.Clear();
            string[] tabNames = new string[]
            {
                "① 飛鳥の朝廷（604年・聖徳太子の政治）",
                "② 難波津の港（607年・小野妹子と遣隋使）",
                "③ 斑鳩・法隆寺（607年・世界最古の木造建築）",
                "④ 飛鳥板蓋宮（645年・大化の改新と公地公民）"
            };
            for (int i = 0; i < tabNames.Length; i++)
            {
                int idx = i;
                Button locBtn = CreateAceCornerButton(moveOverlay, "MoveLoc_" + i, tabNames[i], new Color(0.18f, 0.24f, 0.46f, 0.96f), Vector2.zero, Vector2.zero, 29);
                float yOffset = 98f - (i * 78f);
                SetAnchor(locBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, yOffset), new Vector2(880f, 64f));
                locBtn.onClick.AddListener(() =>
                {
                    _sound.PlayClick();
                    _invMoveOverlayPanel.SetActive(false);
                    SelectInvestigationLocation(idx);
                });
                _locationTabButtons.Add(locBtn);
            }
            _invMoveOverlayPanel.SetActive(false);

            // 『話す』サブメニューオーバーレイ
            RectTransform talkOverlay = CreateAceDialogueStyleBox(panel, "TalkOverlayPanel", new Color(0.04f, 0.08f, 0.14f, 0.96f));
            SetAnchor(talkOverlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, 130f), new Vector2(980f, 370f));
            _invTalkOverlayPanel = talkOverlay.gameObject;

            CreateText(talkOverlay, "TalkHeader", "【 ミオと話す 】", 32, FontStyle.Bold, new Color(0.45f, 1f, 0.75f), TextAnchor.UpperCenter)
                .rectTransform.anchoredPosition = new Vector2(0f, -16f);

            RectTransform talkList = CreatePanel(talkOverlay, "TalkListContainer", new Color(0, 0, 0, 0));
            SetStretch(talkList);
            _invTalkListContainer = talkList;
            _invTalkOverlayPanel.SetActive(false);

            // 逆転裁判 探偵パート4コマンドバー（メッセージウィンドウのすぐ上・見やすい30px文字）
            RectTransform cmdBar = CreatePanel(panel, "InvCommandStrip", new Color(0, 0, 0, 0));
            SetAnchor(cmdBar, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(140f, 332f), new Vector2(1220f, 60f));

            string[] cmdLabels = { "調べる", "移動する", "話す", "つきつける" };
            Action[] cmdActions =
            {
                () =>
                {
                    _sound.PlayClick();
                    _invMoveOverlayPanel.SetActive(false);
                    _invTalkOverlayPanel.SetActive(false);
                    _invHotspotContainer.gameObject.SetActive(true);
                    StartInvestigationDialogue("ミオ", new string[] { "ミオ：「画面内の『🔍 調べる』カードをタップすると、その人物や史料を調査できるよ！」" }, null);
                },
                () =>
                {
                    _sound.PlayClick();
                    _invTalkOverlayPanel.SetActive(false);
                    bool showMove = !_invMoveOverlayPanel.activeSelf;
                    _invMoveOverlayPanel.SetActive(showMove);
                    _invHotspotContainer.gameObject.SetActive(!showMove);
                },
                () =>
                {
                    _sound.PlayClick();
                    _invMoveOverlayPanel.SetActive(false);
                    ToggleTalkMenuOverlay();
                },
                () =>
                {
                    _sound.PlayClick();
                    OpenEvidenceModal(false);
                }
            };

            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                Button cBtn = CreateAceCornerButton(cmdBar, "InvCmd_" + i, cmdLabels[i], new Color(0.16f, 0.20f, 0.38f, 0.95f), Vector2.zero, Vector2.zero, 30);
                float xPos = -435f + (i * 290f);
                SetAnchor(cBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(xPos, 0f), new Vector2(270f, 56f));
                cBtn.onClick.AddListener(() => cmdActions[idx]());
            }

            // 逆転裁判メッセージウィンドウ（参照画像1 & 3 完全再現）
            BuildAceDialogueWindow(panel, "InvDialogueBox", out _, out _invSpeakerText, out _invDialogueText, out _invNextLineButton, out Button invWholeBoxBtn);
            _invNextLineButton.onClick.AddListener(OnClickNextInvestigationDialogue);
            invWholeBoxBtn.onClick.AddListener(OnClickNextInvestigationDialogue);

            return panel.gameObject;
        }

        // =====================================================================
        // ④ 法廷パート（参照画像1〜4 の『逆転裁判』プレイ画面を完全再現！）
        // =====================================================================
        private GameObject BuildCourtPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "CourtPanel", Color.black);
            SetStretch(panel);

            // 1. フルスクリーン法廷背景（カメラアングル切替時に左右へパン）
            RectTransform bgRect = CreatePanel(panel, "CourtBgPanning", Color.white);
            SetAnchor(bgRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2240f, 1120f));
            _courtBgRect = bgRect;
            Sprite courtBgSp = LoadIllustrationSprite("bg_courtroom");
            if (courtBgSp != null)
            {
                bgRect.GetComponent<Image>().sprite = courtBgSp;
            }

            // --- カメラ①：証人席（正面・蘇我 馬子） ---
            RectTransform witnessGroup = CreatePanel(panel, "WitnessStageGroup", new Color(0, 0, 0, 0));
            SetStretch(witnessGroup);
            _witnessStageGroup = witnessGroup.gameObject;

            RectTransform witnessCharRt = CreatePanel(witnessGroup, "WitnessCharSprite", Color.white);
            SetAnchor(witnessCharRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 480f), new Vector2(780f, 860f));
            _witnessPortraitImage = witnessCharRt.GetComponent<Image>();
            _witnessPortraitImage.preserveAspect = true;
            _witnessPortraitImage.raycastTarget = false;
            Sprite umakoSp = LoadIllustrationSprite("char_umako");
            if (umakoSp != null) _witnessPortraitImage.sprite = umakoSp;

            _witnessEffectEmojiText = CreateText(witnessGroup, "WitnessEffectEmoji", "", 84, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetAnchor(_witnessEffectEmojiText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(260f, 220f), new Vector2(220f, 140f));

            // --- カメラ②：弁護席（参照画像1 & 2 & 4 完全再現：左側に史堂レイが大きく立ち、右へ指を突き出す構図！） ---
            RectTransform defenseGroup = CreatePanel(panel, "DefenseStageGroup", new Color(0, 0, 0, 0));
            SetStretch(defenseGroup);
            _defenseStageGroup = defenseGroup.gameObject;

            RectTransform defenseCharRt = CreatePanel(defenseGroup, "DefenseCharSprite", Color.white);
            SetAnchor(defenseCharRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-80f, 520f), new Vector2(1060f, 880f));
            _defensePortraitImage = defenseCharRt.GetComponent<Image>();
            _defensePortraitImage.preserveAspect = true;
            _defensePortraitImage.raycastTarget = false;
            Sprite reiSp = LoadIllustrationSprite("char_rei");
            if (reiSp != null) _defensePortraitImage.sprite = reiSp;

            // --- カメラ③：助手席（古文 ミオ） ---
            RectTransform counselGroup = CreatePanel(panel, "CounselStageGroup", new Color(0, 0, 0, 0));
            SetStretch(counselGroup);
            _counselStageGroup = counselGroup.gameObject;

            RectTransform counselCharRt = CreatePanel(counselGroup, "CounselCharSprite", Color.white);
            SetAnchor(counselCharRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 495f), new Vector2(780f, 840f));
            _counselPortraitImage = counselCharRt.GetComponent<Image>();
            _counselPortraitImage.preserveAspect = true;
            _counselPortraitImage.raycastTarget = false;
            Sprite mioSp = LoadIllustrationSprite("char_mio");
            if (mioSp != null) _counselPortraitImage.sprite = mioSp;

            // -----------------------------------------------------------------
            // 2. 逆転裁判の法廷木製手すり・机（参照画像2・3・4の下部木製バー完全再現）
            // -----------------------------------------------------------------
            RectTransform woodRail = CreatePanel(panel, "CourtWoodenRailing", new Color(0.46f, 0.24f, 0.08f, 1f));
            SetAnchor(woodRail, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 280f), new Vector2(0f, 74f));
            RectTransform woodHighlight = CreatePanel(woodRail, "WoodTopHighlight", new Color(0.68f, 0.38f, 0.14f, 1f));
            SetAnchor(woodHighlight, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -6f), new Vector2(0f, 12f));
            RectTransform woodShadow = CreatePanel(woodRail, "WoodBottomShadow", new Color(0.22f, 0.10f, 0.03f, 1f));
            SetAnchor(woodShadow, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(0f, 20f));

            // -----------------------------------------------------------------
            // 3. 上部左右コーナーUI（大きく見やすい文字サイズに改善）
            // -----------------------------------------------------------------
            _topLeftPressBtn = CreateAceCornerButton(panel, "TopLeftPressBtn", "待った! (ゆさぶる)", new Color(0.14f, 0.44f, 0.28f, 0.95f), Vector2.zero, Vector2.zero, 29);
            SetAnchor(_topLeftPressBtn.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(175f, -48f), new Vector2(300f, 66f));
            _topLeftPressBtn.onClick.AddListener(OnClickPressStatement);

            RectTransform gaugeFrame = CreateAceDialogueStyleBox(panel, "PenaltyGaugeFrame", new Color(0.06f, 0.08f, 0.14f, 0.90f));
            SetAnchor(gaugeFrame, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(475f, -48f), new Vector2(260f, 56f));
            for (int i = 0; i < MaxTrustLife; i++)
            {
                RectTransform seg = CreatePanel(gaugeFrame, "Seg_" + i, new Color(0.22f, 0.96f, 0.38f, 1f));
                float segX = -92f + (i * 46f);
                SetAnchor(seg, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(segX, 0f), new Vector2(38f, 28f));
                _penaltyBarSegments[i] = seg.GetComponent<Image>();
            }

            // 中央上：尋問中の証言ドット表示（● ○ ○）
            RectTransform dotsBox = CreateAceDialogueStyleBox(panel, "StatementDotsBox", new Color(0.04f, 0.06f, 0.14f, 0.88f));
            SetAnchor(dotsBox, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-20f, -48f), new Vector2(350f, 56f));
            _statementIndexDotsText = CreateText(dotsBox, "DotsText", "証言  ● ○ ○", 29, FontStyle.Bold, new Color(0.35f, 1f, 0.50f), TextAnchor.MiddleCenter);

            // 右上コーナー：『異議あり！（つきつける）』＋『法廷記録』＋『相談』
            _topRightPresentBtn = CreateAceCornerButton(panel, "TopRightPresentBtn", "異議あり! (つきつける)", new Color(0.82f, 0.12f, 0.14f, 0.96f), Vector2.zero, Vector2.zero, 29);
            SetAnchor(_topRightPresentBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-525f, -48f), new Vector2(340f, 66f));
            _topRightPresentBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                OpenEvidenceModal(true);
            });

            Button topRecordBtn = CreateAceCornerButton(panel, "TopCourtRecordBtn", "📁 法廷記録", new Color(0.32f, 0.22f, 0.12f, 0.95f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(topRecordBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-245f, -48f), new Vector2(190f, 66f));
            topRecordBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                OpenEvidenceModal(!_isInCourtConversation);
            });

            Button topHintBtn = CreateAceCornerButton(panel, "TopMioHintBtn", "💡 ミオ", new Color(0.16f, 0.36f, 0.46f, 0.95f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(topHintBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-75f, -48f), new Vector2(130f, 66f));
            topHintBtn.onClick.AddListener(OnClickPartnerHint);

            // -----------------------------------------------------------------
            // 4. 逆転裁判 下部メッセージウィンドウ（参照画像1・3・4 完全再現！）
            // -----------------------------------------------------------------
            BuildAceDialogueWindow(
                panel,
                "CourtDialogueBox",
                out _courtSpeakerPlateImg,
                out _courtSpeakerText,
                out _courtDialogueText,
                out _courtNextLineBtn,
                out _courtWholeBoxNextBtn
            );
            _courtNextLineBtn.onClick.AddListener(OnClickNextCourtConversationLine);
            _courtWholeBoxNextBtn.onClick.AddListener(() =>
            {
                if (_isTypingText)
                {
                    _skipCurrentTyping = true;
                    return;
                }
                if (_isInCourtConversation)
                {
                    OnClickNextCourtConversationLine();
                }
                else
                {
                    ChangeStatement(1);
                }
            });

            // 尋問中の証言送り＆下部クイック操作バー（文字サイズ28pxに拡大＆見やすい配置）
            RectTransform examStrip = CreatePanel(panel, "CrossExamQuickStrip", new Color(0, 0, 0, 0));
            SetAnchor(examStrip, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-530f, 326f), new Vector2(960f, 56f));
            _courtCrossExamArrows = examStrip.gameObject;

            Button prevBtn = CreateAceCornerButton(examStrip, "PrevStmtBtn", "◀ 前の証言", new Color(0.14f, 0.18f, 0.34f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(prevBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-360f, 0f), new Vector2(205f, 54f));
            prevBtn.onClick.AddListener(() => ChangeStatement(-1));

            Button nextBtn = CreateAceCornerButton(examStrip, "NextStmtBtn", "次の証言 ▶", new Color(0.14f, 0.18f, 0.34f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(nextBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, 0f), new Vector2(205f, 54f));
            nextBtn.onClick.AddListener(() => ChangeStatement(1));

            Button botPressBtn = CreateAceCornerButton(examStrip, "BotPressBtn", "ゆさぶる", new Color(0.14f, 0.44f, 0.28f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(botPressBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(75f, 0f), new Vector2(195f, 54f));
            botPressBtn.onClick.AddListener(OnClickPressStatement);

            Button botPresentBtn = CreateAceCornerButton(examStrip, "BotPresentBtn", "つきつける", new Color(0.82f, 0.12f, 0.14f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(botPresentBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(295f, 0f), new Vector2(220f, 54f));
            botPresentBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                OpenEvidenceModal(true);
            });

            return panel.gameObject;
        }

        /// <summary>
        /// 参照画像1〜4の逆転裁判メッセージウィンドウを完全再現する共通ビルダー。
        /// 文字が小さくなりすぎないよう大判40pxフォントに統一し、枠内で綺麗に改行して表示！
        /// </summary>
        private RectTransform BuildAceDialogueWindow(
            Transform parent,
            string name,
            out Image speakerPlateImage,
            out Text speakerNameText,
            out Text bodyText,
            out Button rightArrowBoxBtn,
            out Button wholeBoxClickBtn)
        {
            RectTransform container = CreatePanel(parent, name, new Color(0, 0, 0, 0));
            SetAnchor(container, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 154f), new Vector2(1840f, 284f));

            // メインの黒半透明メッセージボックス（白銀の細枠つき）
            RectTransform mainBox = CreateAceDialogueStyleBox(container, "MainTextBox", new Color(0.03f, 0.03f, 0.06f, 0.90f));
            SetPadding(mainBox, 0f, 150f, 0f, 0f);

            wholeBoxClickBtn = mainBox.gameObject.AddComponent<Button>();
            wholeBoxClickBtn.transition = Selectable.Transition.None;

            // 左上にピタッと重なる紫紺の台形ネームプレート（『ナルホド』『ミツルギ』完全再現）
            RectTransform namePlate = CreatePanel(mainBox, "TrapezoidNamePlate", Color.white);
            SetAnchor(namePlate, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(170f, 26f), new Vector2(340f, 58f));
            speakerPlateImage = namePlate.GetComponent<Image>();
            Sprite trapSp = LoadIllustrationSprite("ui_nameplate_trap");
            if (trapSp != null)
            {
                speakerPlateImage.sprite = trapSp;
            }
            else
            {
                speakerPlateImage.color = new Color(0.27f, 0.29f, 0.56f, 0.96f);
            }

            speakerNameText = CreateText(namePlate, "SpeakerName", "レイ", 34, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetPadding(speakerNameText.rectTransform, 14f, 26f, 4f, 4f);
            speakerNameText.resizeTextForBestFit = true;
            speakerNameText.resizeTextMinSize = 28;
            speakerNameText.resizeTextMaxSize = 34;
            speakerNameText.verticalOverflow = VerticalWrapMode.Truncate;

            // メインセリフテキスト：大判40px固定（最小でも36px以上）＆枠内で自然改行してくっきり表示！
            bodyText = CreateText(mainBox, "DialogueBodyText", "", 40, FontStyle.Bold, Color.white, TextAnchor.UpperLeft);
            SetPadding(bodyText.rectTransform, 36f, 52f, 22f, 18f);
            bodyText.lineSpacing = 1.14f;
            bodyText.supportRichText = true;
            bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyText.verticalOverflow = VerticalWrapMode.Truncate;
            bodyText.resizeTextForBestFit = true;
            bodyText.resizeTextMinSize = 36;
            bodyText.resizeTextMaxSize = 40;

            Shadow textShadow = bodyText.gameObject.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0f, 0f, 0f, 0.95f);
            textShadow.effectDistance = new Vector2(2f, -2f);

            // メイン枠内右下の小さな白い三角マーク『▶』
            Text smallArrow = CreateText(mainBox, "BottomRightSmallArrow", "▶", 28, FontStyle.Bold, new Color(0.92f, 0.92f, 0.96f, 0.9f), TextAnchor.LowerRight);
            SetPadding(smallArrow.rectTransform, 20f, 20f, 20f, 14f);

            // 右端に並ぶ独立した『▶』送りボタン枠
            RectTransform rightBox = CreateAceDialogueStyleBox(container, "RightArrowBox", new Color(0.05f, 0.06f, 0.12f, 0.92f));
            SetAnchor(rightBox, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-70f, 0f), new Vector2(140f, 0f));
            rightArrowBoxBtn = rightBox.gameObject.AddComponent<Button>();
            CreateText(rightBox, "BigPlayArrow", "▶", 56, FontStyle.Bold, new Color(0.95f, 0.92f, 0.80f), TextAnchor.MiddleCenter);

            return container;
        }

        private RectTransform CreateAceDialogueStyleBox(Transform parent, string name, Color fillColor)
        {
            RectTransform outer = CreatePanel(parent, name, new Color(0.84f, 0.86f, 0.92f, 0.95f));
            RectTransform inner = CreatePanel(outer, "InnerFill", fillColor);
            SetAnchor(inner, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-5f, -5f));
            inner.GetComponent<Image>().raycastTarget = false;
            return outer;
        }

        private Button CreateAceCornerButton(Transform parent, string name, string label, Color bgColor, Vector2 pos, Vector2 size, int fontSize)
        {
            RectTransform rt = CreateAceDialogueStyleBox(parent, name, bgColor);
            SetAnchor(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Button btn = rt.gameObject.AddComponent<Button>();
            Text txt = CreateText(rt, "Label", label, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            SetPadding(txt.rectTransform, 10f, 10f, 4f, 4f);
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(24, fontSize - 4);
            txt.resizeTextMaxSize = fontSize;
            return btn;
        }

        // =====================================================================
        // ⑤ エンディング ＆ 飛鳥時代テスト対策まとめシート
        // =====================================================================
        private GameObject BuildEndingPanel(Transform parent)
        {
            RectTransform panel = CreatePanel(parent, "EndingPanel", new Color(0.04f, 0.06f, 0.12f, 1f));
            SetStretch(panel);

            Sprite horyujiSp = LoadIllustrationSprite("loc_horyuji");
            if (horyujiSp != null)
            {
                RectTransform bgArt = CreatePanel(panel, "EndingBgArt", new Color(0.25f, 0.28f, 0.38f, 1f));
                SetStretch(bgArt);
                bgArt.GetComponent<Image>().sprite = horyujiSp;
            }

            _endingHeaderText = CreateText(panel, "EndingHeader", "🎊 完全勝訴！『逆転日本史』クリア！ 🎊", 44, FontStyle.Bold, new Color(1f, 0.90f, 0.38f), TextAnchor.MiddleCenter);
            SetAnchor(_endingHeaderText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1740f, 68f));

            RectTransform summaryBox = CreateAceDialogueStyleBox(panel, "SummarySheetBox", new Color(0.04f, 0.06f, 0.14f, 0.94f));
            SetAnchor(summaryBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 25f), new Vector2(1740f, 660f));

            _endingSummarySheetText = CreateText(summaryBox, "SummarySheetText", "", 29, FontStyle.Bold, new Color(0.95f, 1f, 0.92f), TextAnchor.MiddleLeft);
            SetPadding(_endingSummarySheetText.rectTransform, 36f, 36f, 20f, 20f);
            _endingSummarySheetText.lineSpacing = 1.15f;
            _endingSummarySheetText.resizeTextForBestFit = true;
            _endingSummarySheetText.resizeTextMinSize = 24;
            _endingSummarySheetText.resizeTextMaxSize = 29;

            // 中央：『次の時代へタイムスリップ！』（第1話クリア時）
            _endingNextEraBtn = CreateAceCornerButton(panel, "NextEraBtn", "⚡ 次の時代へタイムスリップ！（第2話・奈良時代へ）", new Color(0.85f, 0.20f, 0.15f, 0.98f), Vector2.zero, Vector2.zero, 32);
            SetAnchor(_endingNextEraBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 95f), new Vector2(860f, 76f));
            _endingNextEraBtn.onClick.AddListener(() =>
            {
                _sound.PlayObjection();
                StartNewGameEpisode(2);
            });

            Button openFileBtn = CreateAceCornerButton(panel, "ReviewEvidencesBtn", "📁 法廷記録を見返す", new Color(0.16f, 0.44f, 0.32f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(openFileBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-360f, 25f), new Vector2(420f, 54f));
            openFileBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                UnlockAllEvidences();
                OpenEvidenceModal(false);
            });

            Button returnTitleBtn = CreateAceCornerButton(panel, "ReturnTitleBtn", "🏠 タイトル画面へ戻る", new Color(0.24f, 0.28f, 0.45f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(returnTitleBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 25f), new Vector2(420f, 54f));
            returnTitleBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                SwitchPhase(GamePhase.Title);
            });

            return panel.gameObject;
        }

        // =====================================================================
        // 📁 法廷記録モーダルウィンドウ（文字を大きく・枠内で改行して表示）
        // =====================================================================
        private GameObject BuildEvidenceModalPanel(Transform parent)
        {
            RectTransform modalBg = CreatePanel(parent, "EvidenceModalPanel", new Color(0f, 0f, 0f, 0.85f));
            SetStretch(modalBg);

            RectTransform window = CreateAceDialogueStyleBox(modalBg, "ModalWindow", new Color(0.06f, 0.08f, 0.16f, 0.98f));
            SetAnchor(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1760f, 940f));

            RectTransform headerBar = CreatePanel(window, "ModalHeader", new Color(0.27f, 0.29f, 0.56f, 1f));
            SetAnchor(headerBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -44f), new Vector2(-6f, 82f));

            _evidenceModalHeaderText = CreateText(headerBar, "ModalHeaderText", "📁 【 法廷記録 】飛鳥時代の重要史料ファイル", 34, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            SetAnchor(_evidenceModalHeaderText.rectTransform, new Vector2(0f, 0f), new Vector2(0.8f, 1f), new Vector2(35f, 0f), new Vector2(-35f, 0f));

            Button closeBtn = CreateAceCornerButton(headerBar, "CloseModalBtn", "✖ 戻る", new Color(0.22f, 0.14f, 0.20f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(closeBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(180f, 58f));
            closeBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                _evidenceModalPanel.SetActive(false);
            });

            RectTransform listPanel = CreateAceDialogueStyleBox(window, "EvidenceListPanel", new Color(0.04f, 0.05f, 0.10f, 1f));
            SetAnchor(listPanel, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(345f, -44f), new Vector2(630f, -120f));
            _evidenceListContainer = listPanel;

            RectTransform detailPanel = CreateAceDialogueStyleBox(window, "EvidenceDetailPanel", new Color(0.09f, 0.11f, 0.20f, 1f));
            SetAnchor(detailPanel, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-540f, -44f), new Vector2(1020f, -120f));

            RectTransform previewBox = CreateAceDialogueStyleBox(detailPanel, "DetailPreviewBox", new Color(0.04f, 0.05f, 0.10f, 1f));
            SetAnchor(previewBox, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(195f, -125f), new Vector2(310f, 190f));
            RectTransform previewInner = CreatePanel(previewBox, "DetailPreviewImg", Color.white);
            SetAnchor(previewInner, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-6f, -6f));
            _evidenceDetailPreviewImage = previewInner.GetComponent<Image>();
            _evidenceDetailPreviewImage.preserveAspect = true;

            _evidenceDetailIconText = CreateText(previewBox, "IconText", "👑", 42, FontStyle.Bold, Color.white, TextAnchor.LowerRight);
            SetAnchor(_evidenceDetailIconText.rectTransform, Vector2.zero, Vector2.one, new Vector2(-10f, 8f), new Vector2(-16f, -16f));

            _evidenceDetailTitleText = CreateText(detailPanel, "DetailTitle", "史料を選択してください", 40, FontStyle.Bold, new Color(1f, 0.92f, 0.45f), TextAnchor.MiddleLeft);
            SetAnchor(_evidenceDetailTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(175f, -85f), new Vector2(-390f, 70f));

            _evidenceDetailSubText = CreateText(detailPanel, "DetailSub", "", 28, FontStyle.Bold, new Color(0.45f, 1f, 0.65f), TextAnchor.MiddleLeft);
            SetAnchor(_evidenceDetailSubText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(175f, -150f), new Vector2(-390f, 50f));

            _evidenceDetailDescText = CreateText(detailPanel, "DetailDesc", "", 34, FontStyle.Bold, Color.white, TextAnchor.UpperLeft);
            SetAnchor(_evidenceDetailDescText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -5f), new Vector2(-80f, 270f));
            _evidenceDetailDescText.lineSpacing = 1.16f;
            _evidenceDetailDescText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _evidenceDetailDescText.resizeTextForBestFit = true;
            _evidenceDetailDescText.resizeTextMinSize = 30;
            _evidenceDetailDescText.resizeTextMaxSize = 34;

            RectTransform examBox = CreateAceDialogueStyleBox(detailPanel, "ExamPointBox", new Color(0.18f, 0.12f, 0.04f, 0.98f));
            SetAnchor(examBox, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(940f, 128f));
            _evidenceDetailExamText = CreateText(examBox, "ExamPointText", "", 30, FontStyle.Bold, new Color(1f, 0.90f, 0.38f), TextAnchor.MiddleCenter);
            SetPadding(_evidenceDetailExamText.rectTransform, 20f, 20f, 10f, 10f);
            _evidenceDetailExamText.lineSpacing = 1.14f;
            _evidenceDetailExamText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _evidenceDetailExamText.resizeTextForBestFit = true;
            _evidenceDetailExamText.resizeTextMinSize = 26;
            _evidenceDetailExamText.resizeTextMaxSize = 30;

            _presentEvidenceConfirmButton = CreateAceCornerButton(detailPanel, "PresentConfirmBtn", "👉 【 つきつける 】（異議あり！）", new Color(0.84f, 0.12f, 0.14f, 0.98f), Vector2.zero, Vector2.zero, 36);
            SetAnchor(_presentEvidenceConfirmButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(860f, 88f));
            _presentEvidenceConfirmButton.onClick.AddListener(OnConfirmPresentEvidence);

            modalBg.gameObject.SetActive(false);
            return modalBg.gameObject;
        }

        // =====================================================================
        // 📖 エピソード選択・時代復習モーダル画面の構築
        // =====================================================================
        private GameObject BuildEpisodeSelectModalPanel(Transform parent)
        {
            RectTransform modalBg = CreatePanel(parent, "EpisodeSelectModalPanel", new Color(0f, 0f, 0f, 0.88f));
            SetStretch(modalBg);

            RectTransform window = CreateAceDialogueStyleBox(modalBg, "ModalWindow", new Color(0.06f, 0.08f, 0.16f, 0.98f));
            SetAnchor(window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1760f, 940f));

            RectTransform headerBar = CreatePanel(window, "ModalHeader", new Color(0.20f, 0.35f, 0.55f, 1f));
            SetAnchor(headerBar, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -44f), new Vector2(-6f, 82f));

            Text headerTxt = CreateText(headerBar, "ModalHeaderText", "📖 【 エピソード選択・時代の復習 】クリアした時代をいつでも再挑戦！", 34, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            SetAnchor(headerTxt.rectTransform, new Vector2(0f, 0f), new Vector2(0.8f, 1f), new Vector2(35f, 0f), new Vector2(-35f, 0f));

            Button closeBtn = CreateAceCornerButton(headerBar, "CloseModalBtn", "✖ 閉じる", new Color(0.22f, 0.14f, 0.20f, 0.96f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(closeBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(180f, 58f));
            closeBtn.onClick.AddListener(() =>
            {
                CloseEpisodeSelectModal();
            });

            Text subDesc = CreateText(window, "SubDesc", "各時代の重要史料と法廷バトルを復習して、中学歴史の要点を完璧にマスターしよう！", 26, FontStyle.Normal, new Color(0.85f, 0.90f, 1f), TextAnchor.MiddleCenter);
            SetAnchor(subDesc.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1600f, 40f));

            // -----------------------------------------------------------------
            // 左カード：第1話（飛鳥時代）
            // -----------------------------------------------------------------
            RectTransform card1 = CreateAceDialogueStyleBox(window, "EpisodeCard1", new Color(0.08f, 0.14f, 0.26f, 0.95f));
            SetAnchor(card1, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420f, -40f), new Vector2(800f, 680f));

            Text ep1Title = CreateText(card1, "Ep1Title", "🌸 第1話：逆転の飛鳥時代", 36, FontStyle.Bold, new Color(1f, 0.90f, 0.40f), TextAnchor.MiddleCenter);
            SetAnchor(ep1Title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(760f, 50f));

            Text ep1Sub = CreateText(card1, "Ep1Sub", "西暦604年〜645年頃（推古朝・聖徳太子・大化の改新）", 23, FontStyle.Normal, new Color(0.80f, 0.88f, 0.95f), TextAnchor.MiddleCenter);
            SetAnchor(ep1Sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(760f, 35f));

            Text ep1Badge = CreateText(card1, "Ep1Badge", "🔓 解放中（いつでも復習可能）", 26, FontStyle.Bold, new Color(0.35f, 0.95f, 0.45f), TextAnchor.MiddleCenter);
            SetAnchor(ep1Badge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(760f, 35f));

            RectTransform ep1PointsBox = CreatePanel(card1, "PointsBox1", new Color(0.04f, 0.08f, 0.16f, 0.85f));
            SetAnchor(ep1PointsBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(720f, 300f));
            string ep1Text = "📝【歴史の重要学習ポイント】\n" +
                "・聖徳太子の政治（冠位十二階・十七条の憲法）\n" +
                "・小野妹子の遣隋使派遣と法隆寺（飛鳥文化）\n" +
                "・中大兄皇子と中臣鎌足の大化の改新（公地公民）\n" +
                "・大豪族・蘇我馬子のウソを暴き、正しい歴史を取り戻せ！";
            Text ep1PointsTxt = CreateText(ep1PointsBox, "Ep1PointsTxt", ep1Text, 25, FontStyle.Normal, new Color(0.92f, 0.96f, 1f), TextAnchor.UpperLeft);
            SetPadding(ep1PointsTxt.rectTransform, 25f, 25f, 20f, 20f);
            ep1PointsTxt.lineSpacing = 1.25f;

            Button ep1PlayBtn = CreateAceCornerButton(card1, "Ep1PlayBtn", "🎬 最初から（探偵＆法廷）", new Color(0.82f, 0.18f, 0.15f, 0.98f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(ep1PlayBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 60f), new Vector2(360f, 64f));
            ep1PlayBtn.onClick.AddListener(() =>
            {
                _sound.PlayObjection();
                _episodeSelectModalPanel.SetActive(false);
                StartNewGameEpisode(1);
            });

            Button ep1CourtBtn = CreateAceCornerButton(card1, "Ep1CourtBtn", "⚖️ 法廷パート直行", new Color(0.15f, 0.45f, 0.35f, 0.98f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(ep1CourtBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 60f), new Vector2(360f, 64f));
            ep1CourtBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                _episodeSelectModalPanel.SetActive(false);
                LoadEpisode(1);
                UnlockAllEvidences();
                StartCourtMode();
            });

            // -----------------------------------------------------------------
            // 右カード：第2話（奈良時代）
            // -----------------------------------------------------------------
            RectTransform card2 = CreateAceDialogueStyleBox(window, "EpisodeCard2", new Color(0.14f, 0.10f, 0.26f, 0.95f));
            SetAnchor(card2, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(420f, -40f), new Vector2(800f, 680f));

            Text ep2Title = CreateText(card2, "Ep2Title", "🌸 第2話：咲くやこの花、逆転の奈良時代", 36, FontStyle.Bold, new Color(1f, 0.90f, 0.40f), TextAnchor.MiddleCenter);
            SetAnchor(ep2Title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(760f, 50f));

            Text ep2Sub = CreateText(card2, "Ep2Sub", "西暦710年〜752年頃（平城京・天平文化・大仏造立）", 23, FontStyle.Normal, new Color(0.80f, 0.88f, 0.95f), TextAnchor.MiddleCenter);
            SetAnchor(ep2Sub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(760f, 35f));

            _ep2StatusBadge = CreateText(card2, "Ep2Badge", "🔒 未解放", 26, FontStyle.Bold, new Color(0.95f, 0.55f, 0.45f), TextAnchor.MiddleCenter);
            SetAnchor(_ep2StatusBadge.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(760f, 35f));

            RectTransform ep2PointsBox = CreatePanel(card2, "PointsBox2", new Color(0.08f, 0.04f, 0.16f, 0.85f));
            SetAnchor(ep2PointsBox, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(720f, 300f));
            string ep2Text = "📝【歴史の重要学習ポイント】\n" +
                "・平城京遷都（710年・長安を手本）と大宝律令・和同開珎\n" +
                "・農民の重税（租・庸・調・防人）と逃亡の真相\n" +
                "・墾田永年私財法（743年）と荘園（私有地）の始まり\n" +
                "・聖武天皇の鎮護国家と大仏造立、行基・鑑真の活躍！";
            Text ep2PointsTxt = CreateText(ep2PointsBox, "Ep2PointsTxt", ep2Text, 25, FontStyle.Normal, new Color(0.92f, 0.96f, 1f), TextAnchor.UpperLeft);
            SetPadding(ep2PointsTxt.rectTransform, 25f, 25f, 20f, 20f);
            ep2PointsTxt.lineSpacing = 1.25f;

            _ep2PlayBtn = CreateAceCornerButton(card2, "Ep2PlayBtn", "🎬 最初から（探偵＆法廷）", new Color(0.82f, 0.18f, 0.15f, 0.98f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(_ep2PlayBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 60f), new Vector2(360f, 64f));
            _ep2PlayBtn.onClick.AddListener(() =>
            {
                if (GetMaxClearedEpisode() < 1) return;
                _sound.PlayObjection();
                _episodeSelectModalPanel.SetActive(false);
                StartNewGameEpisode(2);
            });

            _ep2CourtBtn = CreateAceCornerButton(card2, "Ep2CourtBtn", "⚖️ 法廷パート直行", new Color(0.15f, 0.45f, 0.35f, 0.98f), Vector2.zero, Vector2.zero, 28);
            SetAnchor(_ep2CourtBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(190f, 60f), new Vector2(360f, 64f));
            _ep2CourtBtn.onClick.AddListener(() =>
            {
                if (GetMaxClearedEpisode() < 1) return;
                _sound.PlayClick();
                _episodeSelectModalPanel.SetActive(false);
                LoadEpisode(2);
                UnlockAllEvidences();
                StartCourtMode();
            });

            // 未解放時の半透明ロックオーバーレイ
            RectTransform lockMask = CreatePanel(card2, "Ep2LockedMask", new Color(0f, 0f, 0f, 0.82f));
            SetStretch(lockMask);
            _ep2LockedMask = lockMask.gameObject;

            Text lockTxt = CreateText(lockMask, "LockTxt", "🔒\n【 第1話（飛鳥時代）をクリアすると解放！ 】\nまずは第1話をクリアして歴史を進めよう！", 32, FontStyle.Bold, new Color(1f, 0.85f, 0.45f), TextAnchor.MiddleCenter);
            SetStretch(lockTxt.rectTransform);
            lockTxt.lineSpacing = 1.3f;

            modalBg.gameObject.SetActive(false);
            return modalBg.gameObject;
        }

        private void OpenEpisodeSelectModal()
        {
            int cleared = GetMaxClearedEpisode();
            bool ep2Unlocked = cleared >= 1;

            if (_ep2LockedMask != null) _ep2LockedMask.SetActive(!ep2Unlocked);
            if (_ep2StatusBadge != null)
            {
                _ep2StatusBadge.text = ep2Unlocked ? "🔓 解放済み（いつでも復習可能）" : "🔒 未解放（第1話クリアで解放！）";
                _ep2StatusBadge.color = ep2Unlocked ? new Color(0.35f, 0.95f, 0.45f) : new Color(0.95f, 0.55f, 0.45f);
            }
            if (_ep2PlayBtn != null) _ep2PlayBtn.interactable = ep2Unlocked;
            if (_ep2CourtBtn != null) _ep2CourtBtn.interactable = ep2Unlocked;

            _episodeSelectModalPanel.SetActive(true);
        }

        private void CloseEpisodeSelectModal()
        {
            _sound.PlayClick();
            if (_episodeSelectModalPanel != null)
            {
                _episodeSelectModalPanel.SetActive(false);
            }
        }

        // =====================================================================
        // 💥 参照画像2 & 4 完全再現！法廷シーン上に直接炸裂するギザギザ爆発吹き出し
        // =====================================================================
        private GameObject BuildCutInOverlay(Transform parent)
        {
            RectTransform overlay = CreatePanel(parent, "CutInOverlay", new Color(0f, 0f, 0f, 0f));
            SetStretch(overlay);
            overlay.GetComponent<Image>().raycastTarget = false;

            RectTransform burstRt = CreatePanel(overlay, "SpikyStarburstImg", Color.white);
            SetAnchor(burstRt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(180f, 60f), new Vector2(1060f, 795f));
            _cutInBurstRect = burstRt;
            _cutInBurstImage = burstRt.GetComponent<Image>();
            _cutInBurstImage.preserveAspect = true;
            _cutInBurstImage.raycastTarget = false;

            overlay.gameObject.SetActive(false);
            return overlay.gameObject;
        }

        // =====================================================================
        // フェーズ切り替え＆ゲーム進行ロジック
        // =====================================================================
        public void SwitchPhase(GamePhase nextPhase)
        {
            currentPhase = nextPhase;
            _titlePanel.SetActive(nextPhase == GamePhase.Title);
            _openingPanel.SetActive(nextPhase == GamePhase.Opening);
            _investigationPanel.SetActive(nextPhase == GamePhase.Investigation);
            _courtPanel.SetActive(nextPhase == GamePhase.Court);
            _endingPanel.SetActive(nextPhase == GamePhase.Ending);
            _evidenceModalPanel.SetActive(false);
            _cutInOverlayPanel.SetActive(false);

            switch (nextPhase)
            {
                case GamePhase.Title:
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.Title);
                    break;
                case GamePhase.Opening:
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.Opening);
                    break;
                case GamePhase.Investigation:
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.Investigation);
                    break;
                case GamePhase.Court:
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.CrossExamination);
                    break;
                case GamePhase.Ending:
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.Ending);
                    MarkEpisodeCleared(_currentEpisodeNumber);
                    if (_endingHeaderText != null && _currentEpisodeData != null)
                        _endingHeaderText.text = _currentEpisodeData.endingHeader;
                    if (_endingSummarySheetText != null && _currentEpisodeData != null)
                        _endingSummarySheetText.text = _currentEpisodeData.endingSummaryContent;
                    if (_endingNextEraBtn != null)
                    {
                        if (_currentEpisodeNumber == 1)
                        {
                            _endingNextEraBtn.gameObject.SetActive(true);
                            Text btnTxt = _endingNextEraBtn.GetComponentInChildren<Text>();
                            if (btnTxt != null) btnTxt.text = "⚡ 次の時代へタイムスリップ！（第2話・奈良時代へ）";
                            _endingNextEraBtn.onClick.RemoveAllListeners();
                            _endingNextEraBtn.onClick.AddListener(() =>
                            {
                                _sound.PlayObjection();
                                StartNewGameEpisode(2);
                            });
                        }
                        else
                        {
                            _endingNextEraBtn.gameObject.SetActive(true);
                            Text btnTxt = _endingNextEraBtn.GetComponentInChildren<Text>();
                            if (btnTxt != null) btnTxt.text = "🎉 全時代クリア！おめでとう！";
                            _endingNextEraBtn.onClick.RemoveAllListeners();
                            _endingNextEraBtn.onClick.AddListener(() =>
                            {
                                _sound.PlayClick();
                                SwitchPhase(GamePhase.Title);
                            });
                        }
                    }
                    break;
            }
        }

        private void StartNewGameFull()
        {
            StartNewGameEpisode(1);
        }

        public void StartNewGameEpisode(int epNum)
        {
            LoadEpisode(epNum);
            SwitchPhase(GamePhase.Opening);
            if (_opCoroutine != null) StopCoroutine(_opCoroutine);
            if (_opBgMotionCoroutine != null) StopCoroutine(_opBgMotionCoroutine);
            _opBgMotionCoroutine = StartCoroutine(AnimateOpeningBgCameraRoutine());
            _opCoroutine = StartCoroutine(PlayOpeningMovieRoutine());
        }

        private void StartCourtDirectly()
        {
            UnlockAllEvidences();
            StartCourtMode();
        }

        private void UnlockAllEvidences()
        {
            foreach (var ev in _allEvidences)
            {
                _collectedEvidenceIds.Add(ev.id);
            }
        }

        // =====================================================================
        // 🎬 アニメ調オープニング演出
        //    Scene 1: 現代の中学校・歴史教室で授業中にうとうと居眠り…💤
        //    Scene 2: まぶたが閉じ、光り輝く古文書とともに時空の渦へタイムスリップ！
        //    Scene 3: 目を覚ますと1400年前の『飛鳥時代（西暦604年）』！ミオと合流し飛鳥時代へ突入！
        // =====================================================================
        private IEnumerator AnimateOpeningBgCameraRoutine()
        {
            float t = 0f;
            while (currentPhase == GamePhase.Opening)
            {
                t += Time.deltaTime;
                if (_opIlluRect != null)
                {
                    float scale = 1.03f + Mathf.Sin(t * 0.45f) * 0.03f;
                    float panX = Mathf.Sin(t * 0.30f) * 28f;
                    float panY = Mathf.Cos(t * 0.25f) * 14f;
                    _opIlluRect.localScale = new Vector3(scale, scale, 1f);
                    _opIlluRect.anchoredPosition = new Vector2(panX, panY);
                }
                yield return null;
            }
        }

        private IEnumerator PlayOpeningCutRoutine(string speaker, string text, float autoWaitSeconds = 3.8f)
        {
            _opSpeakerText.text = FormatShortSpeakerName(speaker);
            _opAdvanceRequested = false;

            CharacterEmotion emotion = DetectEmotionFromText(speaker, text);
            string cleanText = StripEmotionTags(text);

            if (_opCharacterImage != null)
            {
                Sprite sp = null;
                if (speaker.Contains("レイ") || speaker.Contains("史堂"))
                {
                    sp = GetReiSprite(emotion);
                }
                else if (speaker.Contains("ミオ") || speaker.Contains("古文"))
                {
                    sp = GetMioSprite(emotion);
                }
                else if (speaker.Contains("仲麻呂") || speaker.Contains("藤原") || speaker.Contains("馬子") || speaker.Contains("蘇我"))
                {
                    sp = GetWitnessSprite(emotion);
                }

                if (sp != null)
                {
                    _opCharacterImage.sprite = sp;
                    _opCharacterImage.gameObject.SetActive(true);
                }
            }

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            yield return TypeTextRoutine(_opBodyText, cleanText);

            float timer = 0f;
            while (!_opAdvanceRequested && timer < autoWaitSeconds)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            _opAdvanceRequested = false;
        }

        private IEnumerator AnimateEyelidsRoutine(float targetHeight, float duration)
        {
            float startH = _opTopEyelidBar.sizeDelta.y;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float h = Mathf.SmoothStep(startH, targetHeight, elapsed / duration);
                _opTopEyelidBar.sizeDelta = new Vector2(0f, h);
                _opBottomEyelidBar.sizeDelta = new Vector2(0f, h);
                yield return null;
            }
            _opTopEyelidBar.sizeDelta = new Vector2(0f, targetHeight);
            _opBottomEyelidBar.sizeDelta = new Vector2(0f, targetHeight);
        }

        private IEnumerator PlayOpeningMovieRoutine()
        {
            if (_currentEpisodeNumber == 2)
            {
                yield return PlayOpeningMovieEpisode2Routine();
                yield break;
            }
            Sprite classroomSp = LoadIllustrationSprite("bg_op_classroom");
            Sprite timewarpSp = LoadIllustrationSprite("bg_op_timewarp");
            Sprite mioNormalSp = GetMioSprite(CharacterEmotion.Normal);
            Sprite mioSmileSp = GetMioSprite(CharacterEmotion.Smile);
            Sprite mioShockedSp = GetMioSprite(CharacterEmotion.Shocked);
            Sprite reiNormalSp = GetReiSprite(CharacterEmotion.Normal);
            Sprite reiSmileSp = GetReiSprite(CharacterEmotion.Smile);
            Sprite reiSweatSp = GetReiSprite(CharacterEmotion.Sweat);
            Sprite reiPointSp = GetReiSprite(CharacterEmotion.Pointing);
            Sprite umakoLaughSp = GetWitnessSprite(CharacterEmotion.Laugh);
            Sprite asukaSp = LoadIllustrationSprite("loc_asuka_palace");
            Sprite portSp = LoadIllustrationSprite("loc_naniwa_port");
            Sprite horyujiSp = LoadIllustrationSprite("loc_horyuji");
            Sprite itabukiSp = LoadIllustrationSprite("loc_itabuki");

            _opEpisodeTitleBanner.SetActive(false);
            _opTopEyelidBar.sizeDelta = new Vector2(0f, 36f);
            _opBottomEyelidBar.sizeDelta = new Vector2(0f, 36f);
            _opCharacterImage.gameObject.SetActive(true);
            _opCharacterImage.color = Color.white;
            _opCharRect.anchoredPosition = new Vector2(0f, 460f);
            _opCharRect.localRotation = Quaternion.identity;

            // -----------------------------------------------------------------
            // 【Scene 1】現代・中学校の歴史教室（ポカポカ陽気の授業中〜居眠り）
            // -----------------------------------------------------------------
            if (classroomSp != null) _opIllustrationImage.sprite = classroomSp;
            else if (asukaSp != null) _opIllustrationImage.sprite = asukaSp;
            _opIllustrationImage.color = Color.white;
            if (reiSmileSp != null) _opCharacterImage.sprite = reiSmileSp;

            _opEraYearText.text = "📍 現代（西暦2026年） 中学校・歴史教室";
            _opVisualArtText.text = "日本史の授業：『飛鳥時代（聖徳太子・遣隋使・大化の改新）』";

            yield return PlayOpeningCutRoutine(
                "レイ",
                "僕の名前は史堂 レイ！\n証拠や史料から真実を突き止めるのが大好きな中学生だ。"
            );

            // だんだん眠くなってまぶたが少し重くなる演出
            StartCoroutine(AnimateEyelidsRoutine(175f, 1.1f));
            _opVisualArtText.text = "窓から差し込むポカポカ陽気……だんだんまぶたが重くなっていく……";
            _opCharRect.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
            _opCharRect.anchoredPosition = new Vector2(0f, 430f);
            if (reiNormalSp != null) _opCharacterImage.sprite = reiNormalSp;

            yield return PlayOpeningCutRoutine(
                "レイ",
                "今は日本史の授業中。黒板には『飛鳥時代・聖徳太子・大化の改新』かぁ……\nふわぁぁ……なんだか猛烈に眠くなってきたぞ……"
            );

            // 机に突っ伏して居眠り＆まぶたが完全に閉じる演出
            _opVisualArtText.text = "机の上でうとうと居眠り中……💤";
            _opCharRect.localRotation = Quaternion.Euler(0f, 0f, -8f);
            _opCharRect.anchoredPosition = new Vector2(0f, 380f);
            if (reiSweatSp != null) _opCharacterImage.sprite = reiSweatSp;
            StartCoroutine(AnimateEyelidsRoutine(560f, 1.5f));

            yield return PlayOpeningCutRoutine(
                "レイ",
                "うとうと……あれ？ 机の上の日本史の資料集が\n急にまばゆい黄金色の光を放ち始めたような……スヤァ……💤",
                3.2f
            );

            // -----------------------------------------------------------------
            // 【Scene 2】まばゆい光と時空の渦（タイムスリップ演出！）
            // -----------------------------------------------------------------
            _sound.PlayEvidenceGet();
            StartCoroutine(FlashAndShakeRoutine(0.45f, 20f));
            if (timewarpSp != null) _opIllustrationImage.sprite = timewarpSp;
            _opCharacterImage.gameObject.SetActive(false);
            _opCharRect.localRotation = Quaternion.identity;
            _opCharRect.anchoredPosition = new Vector2(0f, 460f);

            // 時空の渦のなかで一度ハッと目を開ける
            yield return AnimateEyelidsRoutine(90f, 0.55f);

            string[] eras = new string[]
            {
                "🌀 西暦 2026年（令和・現代の教室）",
                "🌀 西暦 1868年（明治維新）",
                "🌀 西暦 1603年（江戸幕府成立）",
                "🌀 西暦 1185年（鎌倉幕府）",
                "🌀 西暦 794年（平安京）",
                "🌀 西暦 710年（平城京）",
                "⚡ 西暦 604年（飛鳥時代・推古天皇の世）"
            };

            for (int i = 0; i < eras.Length; i++)
            {
                _sound.PlayWarpTick();
                _opEraYearText.text = eras[i];
                _opVisualArtText.text = (i < eras.Length - 1)
                    ? "まばゆい光の渦に包まれ、時代がどんどん遡っていく……！！"
                    : "時空ゲートが1400年前の『飛鳥時代』へ到達！！";
                yield return new WaitForSeconds(0.24f);
            }

            // タイムスリップ中にミオのショック声！
            if (mioShockedSp != null) _opCharacterImage.sprite = mioShockedSp;
            _opCharacterImage.gameObject.SetActive(true);
            yield return PlayOpeningCutRoutine(
                "？？？",
                "――レイくん！ 起きて、レイくん！！\n大変よ、私たちがいた教室が時空の渦に飲み込まれていくわ！？",
                3.0f
            );
            _opCharacterImage.gameObject.SetActive(false);

            // まぶたが一度閉じてホワイトアウト
            yield return AnimateEyelidsRoutine(560f, 0.35f);

            // -----------------------------------------------------------------
            // 【Scene 3】目を覚ますと、そこは1400年前の『飛鳥時代』！
            // -----------------------------------------------------------------
            if (asukaSp != null) _opIllustrationImage.sprite = asukaSp;
            _opCharacterImage.gameObject.SetActive(true);
            if (reiSweatSp != null) _opCharacterImage.sprite = reiSweatSp;

            _opEraYearText.text = "📍 西暦604年・飛鳥の都（推古天皇の朝廷前）";
            _opVisualArtText.text = "教室で居眠りしていたはずが……目を覚ますと1400年前の飛鳥時代！？";

            // アニメの「ハッと目を覚ます（まぶたをパチパチ開ける）」演出
            yield return AnimateEyelidsRoutine(260f, 0.35f);
            yield return AnimateEyelidsRoutine(420f, 0.20f);
            StartCoroutine(FlashAndShakeRoutine(0.30f, 14f));
            yield return AnimateEyelidsRoutine(0f, 0.45f);

            yield return PlayOpeningCutRoutine(
                "レイ",
                "……ん？ ……あれっ！？ 教室の机じゃないぞ！？\n朱色の宮殿に、紫や青の冠をかぶった役人たち……ここはいったいどこだ！？"
            );

            // 助手・古文ミオが笑顔で駆け寄る！
            if (mioSmileSp != null) _opCharacterImage.sprite = mioSmileSp;
            _sound.PlayClick();
            _opVisualArtText.text = "古文歴史研究部のパートナー・古文ミオが駆け寄ってくる！";

            yield return PlayOpeningCutRoutine(
                "ミオ",
                "レイくん、やっと目が覚めたのね！！\n信じられないかもしれないけど、ここは大昔の日本――『飛鳥時代』の都よ！"
            );

            // 蘇我馬子が出現し、飛鳥時代の歴史改ざんを高笑い宣言！
            _sound.PlayObjection();
            StartCoroutine(FlashAndShakeRoutine(0.45f, 24f));
            if (itabukiSp != null) _opIllustrationImage.sprite = itabukiSp;
            if (umakoLaughSp != null) _opCharacterImage.sprite = umakoLaughSp;
            _opEraYearText.text = "⚠️ 歴史改ざん警報：飛鳥時代（593年〜645年）";
            _opVisualArtText.text = "飛鳥の大豪族・蘇我馬子が歴史を都合よく書き換えている！";

            yield return PlayOpeningCutRoutine(
                "馬子",
                "フハハハハ！聖徳太子の政治も、遣隋使も、645年の大化の改新も……\n飛鳥時代の歴史はすべて、この蘇我 馬子の一族の栄光に書き換えてくれるわ！"
            );

            // 飛鳥時代の舞台モンタージュ（難波津 ➔ 法隆寺 ➔ 飛鳥の朝廷）＋アニメ第1話タイトル表示！
            if (portSp != null) _opIllustrationImage.sprite = portSp;
            yield return new WaitForSeconds(0.28f);
            if (horyujiSp != null) _opIllustrationImage.sprite = horyujiSp;
            yield return new WaitForSeconds(0.28f);
            if (asukaSp != null) _opIllustrationImage.sprite = asukaSp;

            if (reiPointSp != null) _opCharacterImage.sprite = reiPointSp;
            _sound.PlayEvidenceGet();
            _opEraYearText.text = "📍 西暦604年・飛鳥の朝廷（推古天皇の宮）";
            _opVisualArtText.text = "まずは飛鳥の都を調査して『正しい歴史史料』を集めよう！";
            _opEpisodeTitleBanner.SetActive(true);

            yield return PlayOpeningCutRoutine(
                "レイ",
                "そんなデタラメな歴史改ざんは絶対に許さない！\nミオ、飛鳥の時代を調査して『歴史史料』を集め、法廷で馬子のウソを論破しよう！",
                4.2f
            );

            _opEpisodeTitleBanner.SetActive(false);
            FinishOpeningAndStartInvestigation();
        }

        private IEnumerator PlayOpeningMovieEpisode2Routine()
        {
            Sprite timewarpSp = LoadIllustrationSprite("bg_op_timewarp");
            Sprite palaceSp = LoadIllustrationSprite("loc_asuka_palace");
            Sprite daibutsuSiteSp = LoadIllustrationSprite("loc_horyuji");
            Sprite mioNormalSp = GetMioSprite(CharacterEmotion.Normal);
            Sprite mioSmileSp = GetMioSprite(CharacterEmotion.Smile);
            Sprite mioShockedSp = GetMioSprite(CharacterEmotion.Shocked);
            Sprite reiNormalSp = GetReiSprite(CharacterEmotion.Normal);
            Sprite reiSmileSp = GetReiSprite(CharacterEmotion.Smile);
            Sprite reiSweatSp = GetReiSprite(CharacterEmotion.Sweat);
            Sprite reiPointSp = GetReiSprite(CharacterEmotion.Pointing);
            Sprite nakamaroLaughSp = GetWitnessSprite(CharacterEmotion.Laugh);

            _opEpisodeTitleBanner.SetActive(false);
            _opTopEyelidBar.sizeDelta = new Vector2(0f, 36f);
            _opBottomEyelidBar.sizeDelta = new Vector2(0f, 36f);
            _opCharacterImage.gameObject.SetActive(true);
            _opCharacterImage.color = Color.white;
            _opCharRect.anchoredPosition = new Vector2(0f, 460f);
            _opCharRect.localRotation = Quaternion.identity;

            // Scene 1: 飛鳥時代から奈良時代へのタイムワープ
            if (timewarpSp != null) _opIllustrationImage.sprite = timewarpSp;
            _opIllustrationImage.color = Color.white;
            if (reiSweatSp != null) _opCharacterImage.sprite = reiSweatSp;
            _sound.PlayObjection();
            _opEraYearText.text = "🌀 時空の裂け目（タイムワープ発動！）";
            _opVisualArtText.text = "飛鳥時代を救った瞬間、歴史の古文書が再び激しく発光！新たな時代へ加速する！";

            yield return PlayOpeningCutRoutine(
                "レイ",
                "うわぁぁっ！？飛鳥時代の歴史を正したと思ったら、また古文書が光りだしたぞ！\n時空の渦に吸い込まれていく……！！"
            );

            if (mioShockedSp != null) _opCharacterImage.sprite = mioShockedSp;
            yield return PlayOpeningCutRoutine(
                "ミオ",
                "レイくん、しっかり掴まって！\n歴史の改ざんは飛鳥だけじゃなかったんだ……！次の時代へタイムスリップするよ〜っ！"
            );

            // Scene 2: 平城京到着（710年）
            StartCoroutine(FlashAndShakeRoutine(0.4f, 15f));
            if (palaceSp != null) _opIllustrationImage.sprite = palaceSp;
            if (mioSmileSp != null) _opCharacterImage.sprite = mioSmileSp;
            _sound.PlayClearFanfare();
            _opEraYearText.text = "📍 西暦710年 新都・平城京（朱雀大路）";
            _opVisualArtText.text = "元明天皇が唐の長安を手本に造営した壮大な条坊制の都！東西に市が栄えている！";

            yield return PlayOpeningCutRoutine(
                "ミオ",
                "着いたよ！ここは西暦710年、元明天皇によって造営された新都【平城京（へいじょうきょう）】！\n唐の長安を手本にした碁盤の目の大都市だよ！"
            );

            if (reiSmileSp != null) _opCharacterImage.sprite = reiSmileSp;
            yield return PlayOpeningCutRoutine(
                "レイ",
                "すごい……！朱雀大路が真っ直ぐ伸びて、市では【和同開珎】でお買い物をしている！\n【大宝律令（701年）】で国のかたちも整った時代だね！"
            );

            // Scene 3: 東大寺大仏殿と藤原仲麻呂の陰謀（743年）
            if (daibutsuSiteSp != null) _opIllustrationImage.sprite = daibutsuSiteSp;
            if (nakamaroLaughSp != null) _opCharacterImage.sprite = nakamaroLaughSp;
            _sound.PlayDamage();
            _opEraYearText.text = "📍 西暦743年 東大寺・大仏殿建立現場";
            _opVisualArtText.text = "聖武天皇が発注した盧舎那大仏の建設現場。しかし権勢貴族・藤原仲麻呂が暗躍…！";

            yield return PlayOpeningCutRoutine(
                "仲麻呂",
                "フハハハハ！怪僧・行基め！農民を唆して朝廷の税（租・庸・調）を拒否させ、大仏の資材を着服した罪で捕縛してやる！\n墾田永年私財法も大仏の金銅も、すべて我が藤原氏のものよ！"
            );

            if (mioShockedSp != null) _opCharacterImage.sprite = mioShockedSp;
            yield return PlayOpeningCutRoutine(
                "ミオ",
                "そんなのデタラメです！行基先生は民衆のためにため池や橋を造り、大仏建立に協力している仏教の指導者なのに……！"
            );

            // Scene 4: レイの反撃宣言
            if (reiPointSp != null) _opCharacterImage.sprite = reiPointSp;
            _sound.PlayEvidenceGet();
            _opEraYearText.text = "📍 西暦743年 奈良時代（平城京・天平文化）";
            _opVisualArtText.text = "平城京・東大寺・農村・唐招提寺を調査して、仲麻呂の野望を法廷で打ち砕け！";
            _opEpisodeTitleBanner.SetActive(true);

            yield return PlayOpeningCutRoutine(
                "レイ",
                "藤原仲麻呂……その横暴な言いがかり、法廷で必ず論破してみせる！\nミオ、奈良時代を調査して【大宝律令】【和同開珎】【租庸調】【墾田永年私財法】の真実の史料を集めよう！",
                4.2f
            );

            _opEpisodeTitleBanner.SetActive(false);
            FinishOpeningAndStartInvestigation();
        }

        private void FinishOpeningAndStartInvestigation()
        {
            if (_opCoroutine != null)
            {
                StopCoroutine(_opCoroutine);
                _opCoroutine = null;
            }
            if (_opBgMotionCoroutine != null)
            {
                StopCoroutine(_opBgMotionCoroutine);
                _opBgMotionCoroutine = null;
            }
            if (_opEpisodeTitleBanner != null) _opEpisodeTitleBanner.SetActive(false);
            if (_opTopEyelidBar != null) _opTopEyelidBar.sizeDelta = Vector2.zero;
            if (_opBottomEyelidBar != null) _opBottomEyelidBar.sizeDelta = Vector2.zero;

            SwitchPhase(GamePhase.Investigation);
            SelectInvestigationLocation(0);
        }

        // =====================================================================
        // 🔍 探偵パート制御
        // =====================================================================
        private void SelectInvestigationLocation(int locationIdx)
        {
            _currentLocationIndex = Mathf.Clamp(locationIdx, 0, _locations.Count - 1);
            LocationData loc = _locations[_currentLocationIndex];

            _invMoveOverlayPanel.SetActive(false);
            _invTalkOverlayPanel.SetActive(false);
            _invHotspotContainer.gameObject.SetActive(true);

            _invLocationTitleText.text = $"{loc.title}（{loc.eraYear}）";

            Sprite locSprite = LoadIllustrationSprite(loc.imageResourceName);
            if (locSprite != null)
            {
                _invFieldBackgroundImage.sprite = locSprite;
                _invFieldBackgroundImage.color = Color.white;
            }

            UpdateInvestigationProgressUI();

            for (int i = 0; i < _locationTabButtons.Count; i++)
            {
                Image btnImg = _locationTabButtons[i].transform.Find("InnerFill")?.GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = (i == _currentLocationIndex)
                        ? new Color(0.68f, 0.36f, 0.10f, 0.96f)
                        : new Color(0.18f, 0.24f, 0.46f, 0.96f);
                }
            }

            RebuildHotspotButtons();
            StartInvestigationDialogue("ミオ", new string[] { loc.introText }, null);
        }

        /// <summary>
        /// 探偵パートの『調べる』カードが互いに重なったり右側の立ち絵と被ったりしないよう、
        /// 上下に十分な間隔（155px差）をあけて左〜中央エリアへ整列配置する。
        /// </summary>
        private void RebuildHotspotButtons()
        {
            foreach (Transform child in _invHotspotContainer)
            {
                Destroy(child.gameObject);
            }

            LocationData loc = _locations[_currentLocationIndex];
            int spotCount = loc.spots.Count;

            for (int i = 0; i < spotCount; i++)
            {
                InvestigationSpot capturedSpot = loc.spots[i];
                bool isDone = _inspectedSpotIds.Contains(capturedSpot.spotId) ||
                              (!string.IsNullOrEmpty(capturedSpot.rewardEvidenceId) && _collectedEvidenceIds.Contains(capturedSpot.rewardEvidenceId));

                string btnLabel = isDone
                    ? $"✅ {capturedSpot.icon} 【調査済】{capturedSpot.label}"
                    : $"🔍 {capturedSpot.icon} 【調べる】{capturedSpot.label}";
                Color btnColor = isDone
                    ? new Color(0.10f, 0.38f, 0.26f, 0.95f)
                    : new Color(0.78f, 0.26f, 0.08f, 0.96f);

                // カード同士が絶対に重ならず、右側のミオの立ち絵とも被らない座標を計算
                Vector2 cardPos;
                if (spotCount == 1)
                {
                    cardPos = new Vector2(-260f, 150f);
                }
                else
                {
                    // 1枚目は左上寄り(y=225)、2枚目は中央下寄り(y=70)で縦に155px離す（カード高さ94pxなので61pxの隙間が空く）
                    float xPos = -330f + (i * 150f);
                    float yPos = 225f - (i * 155f);
                    cardPos = new Vector2(xPos, yPos);
                }

                Button spotBtn = CreateAceCornerButton(_invHotspotContainer, "Spot_" + capturedSpot.spotId, btnLabel, btnColor, cardPos, new Vector2(600f, 94f), 28);
                spotBtn.onClick.AddListener(() => OnClickInvestigationSpot(capturedSpot));
            }
        }

        private void ToggleTalkMenuOverlay()
        {
            if (_invTalkOverlayPanel.activeSelf)
            {
                _invTalkOverlayPanel.SetActive(false);
                _invHotspotContainer.gameObject.SetActive(true);
                return;
            }

            _invTalkOverlayPanel.SetActive(true);
            _invHotspotContainer.gameObject.SetActive(false);
            foreach (Transform child in _invTalkListContainer)
            {
                Destroy(child.gameObject);
            }

            LocationData loc = _locations[_currentLocationIndex];
            for (int i = 0; i < loc.talkTopics.Count; i++)
            {
                TalkTopic topic = loc.talkTopics[i];
                bool talked = _talkedTopicIds.Contains(topic.topicId);
                string label = (talked ? "☑️ " : "💬 ") + topic.title;
                Color bg = talked ? new Color(0.12f, 0.28f, 0.24f, 0.96f) : new Color(0.16f, 0.42f, 0.32f, 0.96f);

                Button tBtn = CreateAceCornerButton(_invTalkListContainer, "Topic_" + i, label, bg, new Vector2(0f, 55f - (i * 84f)), new Vector2(860f, 66f), 29);
                tBtn.onClick.AddListener(() =>
                {
                    _sound.PlayClick();
                    _talkedTopicIds.Add(topic.topicId);
                    _invTalkOverlayPanel.SetActive(false);
                    _invHotspotContainer.gameObject.SetActive(true);
                    StartInvestigationDialogue("ミオ", topic.dialogueLines, null);
                });
            }

            Button closeTalkBtn = CreateAceCornerButton(_invTalkListContainer, "CloseTalkBtn", "✖ 戻る", new Color(0.32f, 0.16f, 0.20f, 0.96f), new Vector2(0f, -120f), new Vector2(240f, 56f), 28);
            closeTalkBtn.onClick.AddListener(() =>
            {
                _sound.PlayClick();
                _invTalkOverlayPanel.SetActive(false);
                _invHotspotContainer.gameObject.SetActive(true);
            });
        }

        private void OnClickInvestigationSpot(InvestigationSpot spot)
        {
            _invMoveOverlayPanel.SetActive(false);
            _invTalkOverlayPanel.SetActive(false);
            _invHotspotContainer.gameObject.SetActive(true);
            _inspectedSpotIds.Add(spot.spotId);

            List<string> linesToPlay = new List<string>(spot.dialogueLines);

            // 会話の途中で別ボタンを押したり場所移動しても取りこぼさないよう、クリックした瞬間に史料を確実に入手！
            if (!string.IsNullOrEmpty(spot.rewardEvidenceId))
            {
                bool isNew = _collectedEvidenceIds.Add(spot.rewardEvidenceId);
                EvidenceItem item = GetEvidenceById(spot.rewardEvidenceId);
                if (item != null)
                {
                    if (isNew)
                    {
                        _sound.PlayEvidenceGet();
                    }
                    else
                    {
                        _sound.PlayClick();
                    }

                    string msg = isNew
                        ? $"法廷記録：「📁 法廷記録に【{item.iconSymbol} {item.title}】を追加した！（現在 {_collectedEvidenceIds.Count}/{_allEvidences.Count} 種）\n{item.examPoint}」"
                        : $"法廷記録：「📁 【{item.iconSymbol} {item.title}】は登録済みだ。（現在 {_collectedEvidenceIds.Count}/{_allEvidences.Count} 種）\n{item.examPoint}」";
                    linesToPlay.Add(msg);
                }
            }
            else
            {
                _sound.PlayClick();
            }

            UpdateInvestigationProgressUI();
            RebuildHotspotButtons();

            StartInvestigationDialogue(spot.speakerName, linesToPlay.ToArray(), null);
        }

        private void UpdateInvestigationProgressUI()
        {
            int count = _collectedEvidenceIds.Count;
            int total = _allEvidences.Count;
            _invProgressText.text = $"史料 {count}/{total}";

            Text courtBtnTxt = _invGoToCourtButton.GetComponentInChildren<Text>();
            if (count >= total)
            {
                courtBtnTxt.text = "⚖️ 全史料入手！法廷へ";
            }
            else
            {
                courtBtnTxt.text = $"⚖️ 法廷へ ({count}/{total})";
            }
        }

        private void StartInvestigationDialogue(string speaker, string[] lines, Action onComplete)
        {
            _invSpeakerText.text = FormatShortSpeakerName(speaker);
            _invDialogueQueue.Clear();
            foreach (string line in lines)
            {
                _invDialogueQueue.Enqueue(line);
            }
            _onInvDialogueFinished = onComplete;
            ShowNextInvestigationLine();
        }

        private void OnClickNextInvestigationDialogue()
        {
            if (_isTypingText)
            {
                _skipCurrentTyping = true;
                return;
            }
            _sound.PlayClick();
            ShowNextInvestigationLine();
        }

        private void ShowNextInvestigationLine()
        {
            if (_invDialogueQueue.Count > 0)
            {
                string rawLine = _invDialogueQueue.Dequeue();
                string speaker = _invSpeakerText.text;
                string body = rawLine;

                int colonIdx = rawLine.IndexOf("：「");
                if (colonIdx > 0)
                {
                    speaker = rawLine.Substring(0, colonIdx);
                    body = rawLine.Substring(colonIdx + 2);
                    if (body.EndsWith("」"))
                    {
                        body = body.Substring(0, body.Length - 1);
                    }
                }
                _invSpeakerText.text = FormatShortSpeakerName(speaker);

                CharacterEmotion emotion = DetectEmotionFromText(speaker, body);
                body = StripEmotionTags(body);

                if (_invStandingCharImage != null)
                {
                    Sprite sp = (speaker.Contains("レイ") || speaker.Contains("史堂"))
                        ? GetReiSprite(emotion)
                        : ((speaker.Contains("蘇我") || speaker.Contains("馬子") || speaker.Contains("仲麻呂") || speaker.Contains("藤原"))
                            ? GetWitnessSprite(emotion)
                            : GetMioSprite(emotion));
                    if (sp != null) _invStandingCharImage.sprite = sp;
                }

                if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                _typingCoroutine = StartCoroutine(TypeTextRoutine(_invDialogueText, body));
            }
            else
            {
                Action cb = _onInvDialogueFinished;
                _onInvDialogueFinished = null;
                cb?.Invoke();
            }
        }

        // =====================================================================
        // ⚖️ 法廷パート制御（参照画像1〜4 完全再現）
        // =====================================================================
        private void StartCourtMode()
        {
            SwitchPhase(GamePhase.Court);
            _playerTrustLife = MaxTrustLife;
            UpdateTrustLifeUI();
            StartCourtRound(0);
        }

        private void StartCourtRound(int roundIdx)
        {
            _currentRoundIndex = roundIdx;
            _currentStatementIndex = 0;
            CrossExaminationRound round = _courtRounds[_currentRoundIndex];

            _sound.PlayBgm(RetroSoundPlayer.BgmTrack.CrossExamination);
            SetWitnessExpression("normal");
            SwitchCourtCamera(CourtCameraAngle.WitnessStand);

            PlayCourtConversation(round.introDialogue, () =>
            {
                EnterCrossExaminationState();
            });
        }

        private void EnterCrossExaminationState()
        {
            _isInCourtConversation = false;
            _courtCrossExamArrows.SetActive(true);
            _topLeftPressBtn.gameObject.SetActive(true);
            _topRightPresentBtn.gameObject.SetActive(true);
            _sound.PlayBgm(RetroSoundPlayer.BgmTrack.CrossExamination);
            SetWitnessExpression("normal");
            SwitchCourtCamera(CourtCameraAngle.WitnessStand);
            DisplayCurrentStatement();
        }

        private void DisplayCurrentStatement()
        {
            CrossExaminationRound round = _courtRounds[_currentRoundIndex];
            _currentStatementIndex = Mathf.Clamp(_currentStatementIndex, 0, round.statements.Count - 1);
            TestimonyStatement stmt = round.statements[_currentStatementIndex];

            string dots = "";
            for (int i = 0; i < round.statements.Count; i++)
            {
                dots += (i == _currentStatementIndex) ? "● " : "○ ";
            }
            _statementIndexDotsText.text = $"尋問中 {dots.Trim()}";

            SwitchCourtCamera(CourtCameraAngle.WitnessStand);
            _courtSpeakerText.text = FormatShortSpeakerName(round.witnessName);

            // 証言内容のニュアンスに合わせた馬子の表情
            CharacterEmotion stmtEmotion = DetectEmotionFromText(round.witnessName, stmt.text);
            SetWitnessEmotion(stmtEmotion, withEffects: false);

            // 逆転裁判の証言テキストは鮮やかなグリーン！
            _courtDialogueText.color = new Color(0.28f, 1f, 0.42f, 1f);

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            _typingCoroutine = StartCoroutine(TypeTextRoutine(_courtDialogueText, stmt.text));
        }

        private void ChangeStatement(int delta)
        {
            if (_isInCourtConversation) return;
            _sound.PlayClick();
            CrossExaminationRound round = _courtRounds[_currentRoundIndex];
            _currentStatementIndex = (_currentStatementIndex + delta + round.statements.Count) % round.statements.Count;
            DisplayCurrentStatement();
        }

        private void OnClickPressStatement()
        {
            if (_isInCourtConversation) return;
            _sound.PlayObjection();

            CrossExaminationRound round = _courtRounds[_currentRoundIndex];
            TestimonyStatement stmt = round.statements[_currentStatementIndex];

            // 弁護席の史堂レイに切り替えて『待った！』のギザギザ爆発吹き出しを炸裂！
            SwitchCourtCamera(CourtCameraAngle.DefenseBench);
            SetReiEmotion(CharacterEmotion.Pointing);

            StartCoroutine(PlaySpikyCutInRoutine("burst_matta", new Vector2(220f, 80f), () =>
            {
                SetWitnessEmotion(CharacterEmotion.Sweat, withEffects: false);
                PlayCourtConversation(stmt.pressReactionLines, () =>
                {
                    EnterCrossExaminationState();
                });
            }));
        }

        private void OnConfirmPresentEvidence()
        {
            if (string.IsNullOrEmpty(_selectedEvidenceId)) return;

            _evidenceModalPanel.SetActive(false);
            _sound.StopBgm();
            _sound.PlayObjection();

            CrossExaminationRound round = _courtRounds[_currentRoundIndex];
            TestimonyStatement stmt = round.statements[_currentStatementIndex];
            EvidenceItem presentedItem = GetEvidenceById(_selectedEvidenceId);

            bool isCorrect = stmt.validEvidenceIds != null && stmt.validEvidenceIds.Contains(_selectedEvidenceId);

            // 参照画像2 & 4 完全再現：弁護席の史堂レイが指を突き出し、ギザギザの爆発吹き出し『異議あり！』が画面上にドカンと炸裂！
            SwitchCourtCamera(CourtCameraAngle.DefenseBench);
            SetReiEmotion(CharacterEmotion.Pointing);

            StartCoroutine(PlaySpikyCutInRoutine("burst_igiari", new Vector2(180f, 70f), () =>
            {
                if (isCorrect)
                {
                    _sound.PlayClearFanfare();
                    _sound.PlayBgm(RetroSoundPlayer.BgmTrack.Pursuit);

                    // つきつけ成功時：馬子は大ダメージで白目を剥き、ミオは笑顔で大喜び！
                    SetWitnessEmotion(CharacterEmotion.Damage, withEffects: true);
                    SetMioEmotion(CharacterEmotion.Smile);

                    List<string> lines = new List<string>(round.breakthroughDialogue);
                    lines.Add($"裁判長：「{round.summaryPoint}」");

                    PlayCourtConversation(lines.ToArray(), () =>
                    {
                        if (_currentRoundIndex + 1 < _courtRounds.Count)
                        {
                            StartCourtRound(_currentRoundIndex + 1);
                        }
                        else
                        {
                            _sound.PlayClearFanfare();
                            SwitchPhase(GamePhase.Ending);
                        }
                    });
                }
                else
                {
                    _sound.PlayDamage();
                    StartCoroutine(FlashAndShakeRoutine(0.35f, 18f));
                    _playerTrustLife = Mathf.Max(1, _playerTrustLife - 1);
                    UpdateTrustLifeUI();

                    // つきつけ失敗時：馬子は高笑い、レイは冷や汗、ミオはショック！
                    SetWitnessEmotion(CharacterEmotion.Laugh, withEffects: false);
                    SetReiEmotion(CharacterEmotion.Sweat);
                    SetMioEmotion(CharacterEmotion.Shocked);

                    string witnessWrongLine = _currentEpisodeNumber == 2
                        ? "藤原 仲麻呂：「[laugh]フッハハハ！甘いぞ小僧！我が藤原の前にそのような史料など無力よ！」"
                        : "蘇我 馬子：「[laugh]フッフッフ…甘いぞ小僧！今の私の証言と、その史料は直接矛盾しておらんわ！」";

                    string[] wrongLines = new string[]
                    {
                        $"史堂 レイ：「[sweat]この『{presentedItem.title}』を見れば、証言と食い違っています！」",
                        witnessWrongLine,
                        $"古文 ミオ：「[think]うーん、今の証言（{_currentStatementIndex + 1}番目）と『{presentedItem.title}』の組み合わせじゃなかったみたい…」",
                        round.partnerHint
                    };

                    PlayCourtConversation(wrongLines, () =>
                    {
                        EnterCrossExaminationState();
                    });
                }
            }));
        }

        private void OnClickPartnerHint()
        {
            _sound.PlayClick();
            CrossExaminationRound round = _courtRounds[_currentRoundIndex];
            PlayCourtConversation(new string[] { round.partnerHint }, () =>
            {
                EnterCrossExaminationState();
            });
        }

        private void PlayCourtConversation(string[] lines, Action onFinished)
        {
            _isInCourtConversation = true;
            _courtCrossExamArrows.SetActive(false);
            _topLeftPressBtn.gameObject.SetActive(false);
            _topRightPresentBtn.gameObject.SetActive(false);

            _courtDialogueQueue.Clear();
            foreach (string line in lines)
            {
                _courtDialogueQueue.Enqueue(line);
            }
            _onCourtDialogueFinished = onFinished;
            ShowNextCourtConversationLine();
        }

        private void OnClickNextCourtConversationLine()
        {
            if (_isTypingText)
            {
                _skipCurrentTyping = true;
                return;
            }
            _sound.PlayClick();
            ShowNextCourtConversationLine();
        }

        private void ShowNextCourtConversationLine()
        {
            if (_courtDialogueQueue.Count > 0)
            {
                string rawLine = _courtDialogueQueue.Dequeue();
                string speaker = "裁判長";
                string body = rawLine;

                int colonIdx = rawLine.IndexOf("：「");
                if (colonIdx > 0)
                {
                    speaker = rawLine.Substring(0, colonIdx);
                    body = rawLine.Substring(colonIdx + 2);
                    if (body.EndsWith("」"))
                    {
                        body = body.Substring(0, body.Length - 1);
                    }
                }

                _courtSpeakerText.text = FormatShortSpeakerName(speaker);
                _courtDialogueText.color = Color.white;

                CharacterEmotion emotion = DetectEmotionFromText(speaker, body);
                body = StripEmotionTags(body);

                if (speaker.Contains("史堂") || speaker.Contains("レイ"))
                {
                    SwitchCourtCamera(CourtCameraAngle.DefenseBench);
                    SetReiEmotion(emotion);
                }
                else if (speaker.Contains("古文") || speaker.Contains("ミオ"))
                {
                    SwitchCourtCamera(CourtCameraAngle.CoCounsel);
                    SetMioEmotion(emotion);
                }
                else if (speaker.Contains("蘇我") || speaker.Contains("馬子") || speaker.Contains("仲麻呂") || speaker.Contains("藤原"))
                {
                    SwitchCourtCamera(CourtCameraAngle.WitnessStand);
                    SetWitnessEmotion(emotion);
                }
                else
                {
                    SwitchCourtCamera(CourtCameraAngle.WitnessStand);
                }

                if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                _typingCoroutine = StartCoroutine(TypeTextRoutine(_courtDialogueText, body));
            }
            else
            {
                Action cb = _onCourtDialogueFinished;
                _onCourtDialogueFinished = null;
                cb?.Invoke();
            }
        }

        private void SwitchCourtCamera(CourtCameraAngle angle)
        {
            _witnessStageGroup.SetActive(angle == CourtCameraAngle.WitnessStand);
            _defenseStageGroup.SetActive(angle == CourtCameraAngle.DefenseBench);
            _counselStageGroup.SetActive(angle == CourtCameraAngle.CoCounsel);

            if (_courtBgRect != null)
            {
                switch (angle)
                {
                    case CourtCameraAngle.DefenseBench:
                        _courtBgRect.anchoredPosition = new Vector2(150f, 0f);
                        break;
                    case CourtCameraAngle.CoCounsel:
                        _courtBgRect.anchoredPosition = new Vector2(-150f, 0f);
                        break;
                    default:
                        _courtBgRect.anchoredPosition = Vector2.zero;
                        break;
                }
            }
        }

        // =====================================================================
        // 🎭 キャラクター感情・表情管理システム（逆転裁判完全再現）
        // =====================================================================
        public Sprite GetReiSprite(CharacterEmotion emotion)
        {
            string res = emotion switch
            {
                CharacterEmotion.Pointing => "char_rei_pointing",
                CharacterEmotion.Sweat or CharacterEmotion.Shocked => "char_rei_sweat",
                CharacterEmotion.Smile => "char_rei_smile",
                _ => "char_rei_normal"
            };
            Sprite sp = LoadIllustrationSprite(res);
            if (sp == null) sp = LoadIllustrationSprite("char_rei");
            return sp;
        }

        public Sprite GetMioSprite(CharacterEmotion emotion)
        {
            string res = emotion switch
            {
                CharacterEmotion.Smile => "char_mio_smile",
                CharacterEmotion.Shocked or CharacterEmotion.Damage => "char_mio_shocked",
                CharacterEmotion.Thinking => "char_mio_thinking",
                _ => "char_mio_normal"
            };
            Sprite sp = LoadIllustrationSprite(res);
            if (sp == null) sp = LoadIllustrationSprite("char_mio");
            return sp;
        }

        public Sprite GetWitnessSprite(CharacterEmotion emotion)
        {
            if (_currentEpisodeNumber == 2)
            {
                string resN = emotion switch
                {
                    CharacterEmotion.Sweat => "char_nakamaro_sweat",
                    CharacterEmotion.Damage or CharacterEmotion.Shocked => "char_nakamaro_damage",
                    CharacterEmotion.Laugh or CharacterEmotion.Smile => "char_nakamaro_laugh",
                    _ => "char_nakamaro_normal"
                };
                Sprite spN = LoadIllustrationSprite(resN);
                if (spN != null) return spN;
            }

            string res = emotion switch
            {
                CharacterEmotion.Sweat => "char_umako_sweat",
                CharacterEmotion.Damage or CharacterEmotion.Shocked => "char_umako_damage",
                CharacterEmotion.Laugh or CharacterEmotion.Smile => "char_umako_laugh",
                _ => "char_umako_normal"
            };
            Sprite sp = LoadIllustrationSprite(res);
            if (sp == null) sp = LoadIllustrationSprite("char_umako");
            return sp;
        }

        public void SetReiEmotion(CharacterEmotion emotion)
        {
            Sprite sp = GetReiSprite(emotion);
            if (_defensePortraitImage != null && sp != null)
            {
                _defensePortraitImage.sprite = sp;
            }
        }

        public void SetMioEmotion(CharacterEmotion emotion)
        {
            Sprite sp = GetMioSprite(emotion);
            if (_counselPortraitImage != null && sp != null)
            {
                _counselPortraitImage.sprite = sp;
            }
            if (_invStandingCharImage != null && sp != null)
            {
                _invStandingCharImage.sprite = sp;
            }
        }

        public void SetWitnessEmotion(CharacterEmotion emotion, bool withEffects = true)
        {
            Sprite sp = GetWitnessSprite(emotion);
            if (_witnessPortraitImage != null && sp != null)
            {
                _witnessPortraitImage.sprite = sp;
                RectTransform rt = _witnessPortraitImage.rectTransform;

                if (emotion == CharacterEmotion.Damage || emotion == CharacterEmotion.Shocked)
                {
                    _witnessPortraitImage.color = new Color(1f, 0.90f, 0.90f, 1f);
                    rt.localRotation = Quaternion.Euler(0, 0, -3.5f);
                    if (_witnessEffectEmojiText != null) _witnessEffectEmojiText.text = "💥💦";
                    if (withEffects) StartCoroutine(FlashAndShakeRoutine(0.35f, 20f));
                }
                else if (emotion == CharacterEmotion.Sweat)
                {
                    _witnessPortraitImage.color = new Color(0.95f, 0.95f, 1f, 1f);
                    rt.localRotation = Quaternion.Euler(0, 0, -1.5f);
                    if (_witnessEffectEmojiText != null) _witnessEffectEmojiText.text = "💦";
                }
                else if (emotion == CharacterEmotion.Laugh)
                {
                    _witnessPortraitImage.color = Color.white;
                    rt.localRotation = Quaternion.Euler(0, 0, 1.5f);
                    if (_witnessEffectEmojiText != null) _witnessEffectEmojiText.text = "✨";
                }
                else
                {
                    _witnessPortraitImage.color = Color.white;
                    rt.localRotation = Quaternion.identity;
                    if (_witnessEffectEmojiText != null) _witnessEffectEmojiText.text = "";
                }
            }
        }

        private void SetWitnessExpression(string state)
        {
            CharacterEmotion emotion = state switch
            {
                "shocked" or "damage" => CharacterEmotion.Damage,
                "sweat" or "flustered" => CharacterEmotion.Sweat,
                "smug" or "laugh" => CharacterEmotion.Laugh,
                _ => CharacterEmotion.Normal
            };
            SetWitnessEmotion(emotion, withEffects: false);
        }

        private CharacterEmotion DetectEmotionFromText(string speaker, string body)
        {
            if (string.IsNullOrEmpty(body)) return CharacterEmotion.Normal;

            // 1. タグによる明示的指定
            if (body.Contains("[smile]")) return CharacterEmotion.Smile;
            if (body.Contains("[sweat]")) return CharacterEmotion.Sweat;
            if (body.Contains("[shock]")) return CharacterEmotion.Shocked;
            if (body.Contains("[damage]")) return CharacterEmotion.Damage;
            if (body.Contains("[laugh]")) return CharacterEmotion.Laugh;
            if (body.Contains("[point]")) return CharacterEmotion.Pointing;
            if (body.Contains("[think]")) return CharacterEmotion.Thinking;
            if (body.Contains("[normal]")) return CharacterEmotion.Normal;

            // 2. 発話者ごとのセリフ文脈に応じた自動判定

            // 藤原 仲麻呂（第2話・奈良時代 証人/対立者）
            if (speaker.Contains("仲麻呂") || speaker.Contains("藤原"))
            {
                if (body.Contains("なにィ") || body.Contains("グワ") || body.Contains("ぐわ") ||
                    body.Contains("馬鹿な") || body.Contains("バカな") || body.Contains("ありえぬ") ||
                    body.Contains("オオオ") || body.Contains("なんということ") || body.Contains("ウソだ") ||
                    body.Contains("ぬわ") || body.Contains("敗北") || body.Contains("失脚") ||
                    body.Contains("うわぁ") || body.Contains("食い違っている") || body.Contains("暴かれる") ||
                    body.Contains("矛盾") || body.Contains("なんだと") || body.Contains("論破された"))
                    return CharacterEmotion.Damage;

                if (body.Contains("…っ") || body.Contains("ぬっ") || body.Contains("ぐぬぬ") ||
                    body.Contains("なに") || body.Contains("証拠") || body.Contains("根拠") ||
                    body.Contains("まさか") || body.Contains("何を言う") || body.Contains("言いがかり") ||
                    body.Contains("どこに") || body.Contains("違う") || body.Contains("弁解") ||
                    body.Contains("動揺") || body.Contains("百歩譲ろう") || body.Contains("ええい") ||
                    body.Contains("小癪な") || body.Contains("把握していたとは"))
                    return CharacterEmotion.Sweat;

                if (body.Contains("フッ") || body.Contains("フハハ") || body.Contains("ハッハッハ") ||
                    body.Contains("笑止") || body.Contains("たわけ") || body.Contains("愚か") ||
                    body.Contains("平民") || body.Contains("我が藤原") || body.Contains("死罪") ||
                    body.Contains("当然") || body.Contains("怠け者") || body.Contains("朝廷の威光") ||
                    body.Contains("捕縛") || body.Contains("私のもの") || body.Contains("栄華") ||
                    body.Contains("不要") || body.Contains("怪僧") || body.Contains("フン") ||
                    body.Contains("決まっておる") || body.Contains("無意味") || body.Contains("権力") ||
                    body.Contains("我ら貴族") || body.Contains("甘いぞ") || body.Contains("無力"))
                    return CharacterEmotion.Laugh;

                return CharacterEmotion.Normal;
            }

            // 蘇我 馬子（第1話・飛鳥時代 証人/対立者）
            if (speaker.Contains("馬子") || speaker.Contains("蘇我"))
            {
                if (body.Contains("なにィ") || body.Contains("ぐわ") || body.Contains("グワ") ||
                    body.Contains("うわぁ") || body.Contains("バカな") || body.Contains("ありえぬ") ||
                    body.Contains("ぐぬぬ") || body.Contains("そんなはず") || body.Contains("ぬわ") ||
                    body.Contains("敗北") || body.Contains("オオオ") || body.Contains("なんという") ||
                    body.Contains("論破された"))
                    return CharacterEmotion.Damage;

                if (body.Contains("フッ") || body.Contains("フハハ") || body.Contains("ハッハッハ") ||
                    body.Contains("笑止") || body.Contains("たわけ") || body.Contains("愚か") ||
                    body.Contains("小僧") || body.Contains("栄光") || body.Contains("書き換えて") ||
                    body.Contains("我が蘇我") || body.Contains("権勢") || body.Contains("不要") ||
                    body.Contains("当然") || body.Contains("フン") || body.Contains("甘いぞ"))
                    return CharacterEmotion.Laugh;

                if (body.Contains("…っ") || body.Contains("ぬっ") || body.Contains("根拠") ||
                    body.Contains("証拠") || body.Contains("まさか") || body.Contains("何を言う") ||
                    body.Contains("言いがかり") || body.Contains("どこに") || body.Contains("違う") ||
                    body.Contains("動揺") || body.Contains("ええい") || body.Contains("把握していたとは"))
                    return CharacterEmotion.Sweat;

                return CharacterEmotion.Normal;
            }

            // 史堂 レイ（主人公・弁護人）
            if (speaker.Contains("レイ") || speaker.Contains("史堂"))
            {
                if (body.Contains("異議あり") || body.Contains("くらえ") || body.Contains("食らえ") ||
                    body.Contains("つきつけ") || body.Contains("証拠です") || body.Contains("真実") ||
                    body.Contains("矛盾") || body.Contains("論破") || body.Contains("許さない") ||
                    body.Contains("暴く") || body.Contains("打ち砕") || body.Contains("解き明か") ||
                    body.Contains("証明") || body.Contains("間違い") || body.Contains("あなた自身だ") ||
                    body.Contains("待った") || body.Contains("食い違っています") || body.Contains("明らかにする") ||
                    body.Contains("つきつけるぞ"))
                    return CharacterEmotion.Pointing;

                if (body.Contains("しまっ") || body.Contains("うぐ") || body.Contains("くっ") ||
                    body.Contains("そんな…") || body.Contains("まずい") || body.Contains("どうすれば") ||
                    body.Contains("どこだ") || body.Contains("あれっ") || body.Contains("冷や汗") ||
                    body.Contains("ピンチ") || body.Contains("うわぁ") || body.Contains("ええっ") ||
                    body.Contains("吸い込まれて") || body.Contains("眠くなってきた") || body.Contains("スヤァ") ||
                    body.Contains("うとうと") || body.Contains("ふわぁ"))
                    return CharacterEmotion.Sweat;

                if (body.Contains("なるほど") || body.Contains("わかりました") || body.Contains("やった") ||
                    body.Contains("よし") || body.Contains("任せて") || body.Contains("僕の名前は") ||
                    body.Contains("すごい") || body.Contains("大好きな") || body.Contains("完成形だ") ||
                    body.Contains("行こう") || body.Contains("証明できるぞ") || body.Contains("お任せ") ||
                    body.Contains("勝訴") || body.Contains("ただいま"))
                    return CharacterEmotion.Smile;

                return CharacterEmotion.Normal;
            }

            // 古文 ミオ（ヒロイン・パートナー助手）
            if (speaker.Contains("ミオ") || speaker.Contains("古文"))
            {
                if (body.Contains("ええー") || body.Contains("えっ") || body.Contains("キャー") ||
                    body.Contains("きゃあ") || body.Contains("うそ") || body.Contains("大変") ||
                    body.Contains("危ない") || body.Contains("飲み込まれて") || body.Contains("信じられない") ||
                    body.Contains("ひどい") || body.Contains("まさか") || body.Contains("助けて") ||
                    body.Contains("タイムスリップするよ") || body.Contains("そんなのデタラメ") ||
                    body.Contains("大嘘") || body.Contains("ピンチ"))
                    return CharacterEmotion.Shocked;

                if (body.Contains("うーん") || body.Contains("たしか") || body.Contains("調べて") ||
                    body.Contains("記録による") || body.Contains("史料") || body.Contains("古代") ||
                    body.Contains("組み合わせ") || body.Contains("教科書") || body.Contains("歴史書") ||
                    body.Contains("条文") || body.Contains("木簡") || body.Contains("確かめ") ||
                    body.Contains("推理") || body.Contains("手本にした") || body.Contains("仕組み") ||
                    body.Contains("定められた") || body.Contains("造営された") || body.Contains("思想だね") ||
                    body.Contains("高僧だよ") || body.Contains("買い占めて") || body.Contains("荷札") ||
                    body.Contains("日誌") || body.Contains("戒本"))
                    return CharacterEmotion.Thinking;

                if (body.Contains("レイくん") || body.Contains("頑張って") || body.Contains("やった") ||
                    body.Contains("さすが") || body.Contains("すごい") || body.Contains("任せて") ||
                    body.Contains("目が覚めた") || body.Contains("見て") || body.Contains("着いたよ") ||
                    body.Contains("解明") || body.Contains("おめでとう") || body.Contains("つきつけるよ") ||
                    body.Contains("つきつけよう") || body.Contains("暴こう") || body.Contains("勝訴") ||
                    body.Contains("無実が証明"))
                    return CharacterEmotion.Smile;

                return CharacterEmotion.Normal;
            }

            return CharacterEmotion.Normal;
        }

        private string StripEmotionTags(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return Regex.Replace(text, @"\[(smile|sweat|shock|damage|laugh|point|think|normal)\]", "");
        }

        private void UpdateTrustLifeUI()
        {
            for (int i = 0; i < MaxTrustLife; i++)
            {
                if (_penaltyBarSegments[i] != null)
                {
                    _penaltyBarSegments[i].color = (i < _playerTrustLife)
                        ? new Color(0.20f, 0.95f, 0.38f, 1f)
                        : new Color(0.22f, 0.12f, 0.14f, 0.75f);
                }
            }
        }

        // =====================================================================
        // 📁 法廷記録モーダル制御
        // =====================================================================
        private void OpenEvidenceModal(bool isPresentMode)
        {
            _isSelectingEvidenceToPresent = isPresentMode;
            _evidenceModalPanel.SetActive(true);

            _evidenceModalHeaderText.text = isPresentMode
                ? "👉 【 法廷記録 】矛盾を暴く『歴史史料』を選んで【つきつける】！"
                : $"📁 【 法廷記録 】{(_currentEpisodeNumber == 1 ? "飛鳥時代" : "奈良時代")}の重要史料ファイル（全7種）";

            _presentEvidenceConfirmButton.gameObject.SetActive(isPresentMode);

            foreach (Transform child in _evidenceListContainer)
            {
                if (child.name == "InnerFill") continue;
                Destroy(child.gameObject);
            }

            string firstAvailableId = "";
            for (int i = 0; i < _allEvidences.Count; i++)
            {
                EvidenceItem ev = _allEvidences[i];
                bool isUnlocked = _collectedEvidenceIds.Contains(ev.id);
                if (isUnlocked && string.IsNullOrEmpty(firstAvailableId))
                {
                    firstAvailableId = ev.id;
                }

                string btnText = isUnlocked
                    ? $"{ev.iconSymbol} {ev.title}"
                    : "🔒 ？？？（探偵パートで入手）";
                Color btnColor = isUnlocked ? ev.badgeColor : new Color(0.20f, 0.20f, 0.26f);

                float yPos = 315f - (i * 104f);
                Button itemBtn = CreateAceCornerButton(_evidenceListContainer, "EvItem_" + ev.id, btnText, btnColor, new Vector2(0f, yPos), new Vector2(580f, 92f), 28);

                string capturedId = ev.id;
                itemBtn.interactable = isUnlocked;
                itemBtn.onClick.AddListener(() =>
                {
                    _sound.PlayClick();
                    SelectEvidenceInModal(capturedId);
                });
            }

            if (!string.IsNullOrEmpty(firstAvailableId))
            {
                SelectEvidenceInModal(firstAvailableId);
            }
            else
            {
                _selectedEvidenceId = "";
                _evidenceDetailIconText.text = "🔍";
                _evidenceDetailTitleText.text = "まだ史料を入手していません";
                _evidenceDetailSubText.text = "探偵パートで『🔍調べる』ボタンを押して史料を集めよう！";
                _evidenceDetailDescText.text = WrapTextForBox("飛鳥の朝廷・難波津・法隆寺・飛鳥板蓋宮の4箇所を調査すると、全部で7つの重要歴史史料が法廷記録に登録されます。", 26);
                _evidenceDetailExamText.text = WrapTextForBox("★ヒント：画面上のオレンジ色の『調べる』ボタンをタップしよう！", 28);
                _presentEvidenceConfirmButton.interactable = false;
            }
        }

        private void SelectEvidenceInModal(string evidenceId)
        {
            _selectedEvidenceId = evidenceId;
            EvidenceItem item = GetEvidenceById(evidenceId);
            if (item == null) return;

            Sprite previewSp = LoadIllustrationSprite(item.imageResourceName);
            if (previewSp != null && _evidenceDetailPreviewImage != null)
            {
                _evidenceDetailPreviewImage.sprite = previewSp;
                _evidenceDetailPreviewImage.color = Color.white;
            }

            _evidenceDetailIconText.text = item.iconSymbol;
            _evidenceDetailTitleText.text = item.title;
            _evidenceDetailSubText.text = item.subtitle;
            _evidenceDetailDescText.text = WrapTextForBox(item.description, 26);
            _evidenceDetailExamText.text = WrapTextForBox(item.examPoint, 28);
            _presentEvidenceConfirmButton.interactable = true;
        }

        private EvidenceItem GetEvidenceById(string id)
        {
            return _allEvidences.Find(e => e.id == id);
        }

        /// <summary>
        /// 文字が小さくなりすぎないよう、枠内の横幅（1行あたり maxCharsPerLine 文字目安）に合わせて
        /// 句読点や括弧の区切りで自然に改行（\n）を挿入するヘルパー。
        /// </summary>
        private static string WrapTextForBox(string rawText, int maxCharsPerLine = 34)
        {
            if (string.IsNullOrEmpty(rawText)) return "";

            string[] paragraphs = rawText.Split('\n');
            List<string> wrappedLines = new List<string>();
            const string breakAfterChars = "、。！？】』）… ";
            const string noStartChars = "、。！？】』）ーっッぁぃぅぇぉゃゅょ…";

            foreach (string para in paragraphs)
            {
                string remaining = para.TrimEnd('\r');
                while (remaining.Length > maxCharsPerLine)
                {
                    int splitPos = maxCharsPerLine;

                    // 22文字目〜maxCharsPerLine文字目の間に句読点・閉じ括弧があればそこで綺麗に改行する
                    int minSearch = Mathf.Max(18, maxCharsPerLine - 11);
                    for (int i = maxCharsPerLine - 1; i >= minSearch; i--)
                    {
                        if (breakAfterChars.IndexOf(remaining[i]) >= 0)
                        {
                            splitPos = i + 1;
                            break;
                        }
                    }

                    // 行頭禁則処理（句読点や閉じ括弧が次の行頭に来ないようにする）
                    while (splitPos < remaining.Length && noStartChars.IndexOf(remaining[splitPos]) >= 0)
                    {
                        splitPos++;
                    }

                    if (splitPos >= remaining.Length)
                    {
                        break;
                    }

                    wrappedLines.Add(remaining.Substring(0, splitPos));
                    remaining = remaining.Substring(splitPos);
                }

                if (remaining.Length > 0 || para.Length == 0)
                {
                    wrappedLines.Add(remaining);
                }
            }

            return string.Join("\n", wrappedLines);
        }

        // =====================================================================
        // 共通演出コルーチン（文字タイプ表示・ギザギザ爆発吹き出しカットイン・画面揺れ＆フラッシュ）
        // =====================================================================
        private IEnumerator TypeTextRoutine(Text targetText, string fullText)
        {
            string wrappedText = WrapTextForBox(fullText, 34);
            targetText.text = "";
            _isTypingText = true;
            _skipCurrentTyping = false;

            for (int i = 0; i < wrappedText.Length; i++)
            {
                if (_skipCurrentTyping)
                {
                    break;
                }
                targetText.text = wrappedText.Substring(0, i + 1);

                if (i % 2 == 0 && wrappedText[i] != '\n')
                {
                    _sound.PlayBlip();
                }
                yield return new WaitForSeconds(0.016f);
            }

            targetText.text = wrappedText;
            _isTypingText = false;
            _skipCurrentTyping = false;
        }

        private IEnumerator PlaySpikyCutInRoutine(string burstSpriteName, Vector2 burstOffset, Action onComplete)
        {
            Sprite burstSp = LoadIllustrationSprite(burstSpriteName);
            if (burstSp != null)
            {
                _cutInBurstImage.sprite = burstSp;
            }
            _cutInBurstRect.anchoredPosition = burstOffset;
            _cutInOverlayPanel.SetActive(true);

            StartCoroutine(FlashAndShakeRoutine(0.50f, 28f));

            float elapsed = 0f;
            float duration = 0.85f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = (t < 0.18f)
                    ? Mathf.Lerp(1.65f, 0.96f, t / 0.18f)
                    : Mathf.Lerp(0.96f, 1.04f, (t - 0.18f) / 0.82f);
                _cutInBurstRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            _cutInBurstRect.localScale = Vector3.one;
            _cutInOverlayPanel.SetActive(false);
            onComplete?.Invoke();
        }

        private IEnumerator FlashAndShakeRoutine(float duration, float magnitude)
        {
            _flashOverlay.color = new Color(1f, 1f, 1f, 0.88f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                _flashOverlay.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.88f, 0f, progress));

                float damper = 1f - progress;
                float offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude * damper;
                float offsetY = UnityEngine.Random.Range(-1f, 1f) * magnitude * damper;
                _shakeContainer.anchoredPosition = new Vector2(offsetX, offsetY);
                yield return null;
            }
            _flashOverlay.color = new Color(1f, 1f, 1f, 0f);
            _shakeContainer.anchoredPosition = Vector2.zero;
        }

        // =====================================================================
        // UIパーツ生成ヘルパー
        // =====================================================================
        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            Image img = obj.AddComponent<Image>();
            img.sprite = _whiteSprite;
            img.color = color;
            return rt;
        }

        private Text CreateText(Transform parent, string name, string content, int fontSize, FontStyle style, Color color, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.AddComponent<RectTransform>();
            SetStretch(rt);

            Text txt = obj.AddComponent<Text>();
            txt.font = _jpFont;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = alignment;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Truncate;
            txt.lineSpacing = 1.20f;
            txt.raycastTarget = false;
            return txt;
        }

        private static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        private static void SetPadding(RectTransform rt, float left, float right, float top, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        private static void SetAnchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
        }
    }
}
