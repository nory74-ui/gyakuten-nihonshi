using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsukaHistoryCourt
{
    /// <summary>
    /// 歴史史料（法廷記録・証拠品）データ
    /// </summary>
    [Serializable]
    public class EvidenceItem
    {
        public string id;
        public string title;
        public string subtitle;
        public string iconSymbol;
        public string imageResourceName; // Resources/Images/ 内の画像名
        public Color badgeColor;
        [TextArea(3, 6)]
        public string description;
        public string examPoint; // テスト対策の重要ポイント
    }

    /// <summary>
    /// 探偵パートの調査対象（ホットスポット）
    /// </summary>
    [Serializable]
    public class InvestigationSpot
    {
        public string spotId;
        public string label;
        public string icon;
        public Vector2 normalizedPos; // 画面内の配置座標
        public string speakerName;
        [TextArea(3, 6)]
        public string[] dialogueLines;
        public string rewardEvidenceId; // 入手できる史料ID（なければ空）
    }

    /// <summary>
    /// 探偵パートの「話す」コマンド用トピック
    /// </summary>
    [Serializable]
    public class TalkTopic
    {
        public string topicId;
        public string title;
        public string[] dialogueLines;
    }

    /// <summary>
    /// 探偵パートのロケーション（場所）
    /// </summary>
    [Serializable]
    public class LocationData
    {
        public string locationId;
        public string title;
        public string eraYear;
        public string imageResourceName; // Resources/Images/ 内の背景画像名
        public Color bgTopColor;
        public Color bgBottomColor;
        public string sceneryArt;
        public string introText;
        public List<InvestigationSpot> spots = new List<InvestigationSpot>();
        public List<TalkTopic> talkTopics = new List<TalkTopic>();
    }

    /// <summary>
    /// 法廷パートの1つの証言（Statement）
    /// </summary>
    [Serializable]
    public class TestimonyStatement
    {
        public string text;
        [TextArea(2, 5)]
        public string[] pressReactionLines; // 「ゆさぶる」したときの会話
        public string addedStatementAfterPress;
        public List<string> validEvidenceIds = new List<string>(); // この証言に「つきつける」と正解になる史料IDリスト
    }

    /// <summary>
    /// 法廷パートの1ラウンド分の尋問データ
    /// </summary>
    [Serializable]
    public class CrossExaminationRound
    {
        public int roundNumber;
        public string themeTitle;
        public string witnessName;
        public string witnessTitle;
        public string[] introDialogue;
        public List<TestimonyStatement> statements = new List<TestimonyStatement>();
        public string partnerHint; // 古文ミオからのヒント
        public string[] breakthroughDialogue; // 論破成功時の会話
        public string summaryPoint; // 論破後の歴史まとめポイント
    }

    /// <summary>
    /// 1つのエピソード（時代）の全データパッケージ
    /// </summary>
    [Serializable]
    public class EpisodeData
    {
        public int episodeNumber;
        public string episodeId;
        public string title;
        public string subtitle;
        public string eraName;
        public string witnessName;
        public string witnessTitle;
        public string endingHeader;
        [TextArea(4, 10)]
        public string endingSummaryContent;
        public List<EvidenceItem> evidences = new List<EvidenceItem>();
        public List<LocationData> locations = new List<LocationData>();
        public List<CrossExaminationRound> courtRounds = new List<CrossExaminationRound>();
    }

    /// <summary>
    /// 全エピソード統合データベース（第1話：飛鳥時代 / 第2話：奈良時代）
    /// </summary>
    public static class HistoryDatabase
    {
        public static EpisodeData GetEpisode(int episodeNumber)
        {
            switch (episodeNumber)
            {
                case 1:
                    return GetEpisode1Asuka();
                case 2:
                    return GetEpisode2Nara();
                default:
                    return GetEpisode1Asuka();
            }
        }

        // =====================================================================
        // 📜 第1話：逆転の飛鳥時代（604年〜645年頃）
        // =====================================================================
        public static EpisodeData GetEpisode1Asuka()
        {
            EpisodeData ep = new EpisodeData
            {
                episodeNumber = 1,
                episodeId = "ep1_asuka",
                title = "第1話：逆転の飛鳥時代",
                subtitle = "〜聖徳太子の政治と大化の改新〜",
                eraName = "飛鳥時代（西暦604年〜645年頃）",
                witnessName = "蘇我 馬子",
                witnessTitle = "飛鳥の大豪族・大臣",
                endingHeader = "🎊 完全勝訴！『逆転日本史』第1話・飛鳥時代クリア！ 🎊",
                endingSummaryContent =
                    "📝【史堂レイ＆古文ミオの飛鳥時代・最重要まとめノート】\n" +
                    "① 聖徳太子（厩戸王）の政治（593年〜）\n" +
                    "  ・推古天皇の【摂政（せっしょう）】となり、豪族の蘇我馬子と協力して政治を行う。\n" +
                    "  ・【冠位十二階（603年）】：家柄ではなく、【個人の能力や功績】で役人を登用した制度。\n" +
                    "  ・【十七条の憲法（604年）】：『和を以て貴しとなす』。豪族の争いを抑え、【天皇中心の国づくり】を目指した。\n" +
                    "② 外交と飛鳥文化（607年〜）\n" +
                    "  ・【遣隋使（607年）】：【小野妹子】を中国の【隋（ずい）】へ派遣。対等な外交と先進文化・制度の吸収を図った。\n" +
                    "  ・【法隆寺】と【飛鳥文化】：斑鳩に建てられた世界最古の木造建築。日本最初の仏教文化が栄えた。\n" +
                    "③ 大化の改新（645年〜）\n" +
                    "  ・645年（乙巳の変）、【中大兄皇子（のちの天智天皇）】と【中臣鎌足（のちの藤原鎌足）】が【蘇我氏】を倒す。\n" +
                    "  ・豪族が私有していた土地と人民を廃止し、すべて国（天皇）のものにする【公地公民（こうちこうみん）】を打ち出した！",
                evidences = AsukaEpisodeDatabase.GetAllEvidences(),
                locations = AsukaEpisodeDatabase.GetAllLocations(),
                courtRounds = AsukaEpisodeDatabase.GetCourtRounds()
            };
            return ep;
        }

        // =====================================================================
        // 🌸 第2話：咲くやこの花、逆転の奈良時代（710年〜752年頃）
        // =====================================================================
        public static EpisodeData GetEpisode2Nara()
        {
            EpisodeData ep = new EpisodeData
            {
                episodeNumber = 2,
                episodeId = "ep2_nara",
                title = "第2話：咲くやこの花、逆転の奈良時代",
                subtitle = "〜大仏建立の危機と天平の謎〜",
                eraName = "奈良時代（西暦710年〜752年頃）",
                witnessName = "藤原 仲麻呂",
                witnessTitle = "奈良の権勢貴族・大納言",
                endingHeader = "🎊 完全勝訴！『逆転日本史』第2話・奈良時代クリア！ 🎊",
                endingSummaryContent =
                    "📝【史堂レイ＆古文ミオの奈良時代・最重要まとめノート】\n" +
                    "① 平城京と律令国家（710年〜）\n" +
                    "  ・【平城京（710年・元明天皇）】：唐の都【長安】を手本にした碁盤の目の都。\n" +
                    "  ・【大宝律令（701年）】に基づき、中央に二官八省、全国を国・郡・里に区分。\n" +
                    "  ・【和同開珎（708年）】：唐の開元通宝を手本にした貨幣で、都の市で流通。\n" +
                    "② 重税と農民の苦しみ（租・庸・調と防人）\n" +
                    "  ・【租（そ）】：収穫の約3%の稲。\n" +
                    "  ・【庸（よう）】：都での労役の代わりの布。\n" +
                    "  ・【調（ちょう）】：地方の特産物（絹や海産物）。自力で都まで運ぶ重労働！\n" +
                    "  ・【雑徭（年間60日の労役）】や【防人（北九州の警備3年）】の負担で、多くの農民が口分田を捨てて逃亡した。\n" +
                    "③ 土地制度の変化と墾田永年私財法（743年）\n" +
                    "  ・農民の逃亡と口分田不足を補うため、743年に【墾田永年私財法】を発布。\n" +
                    "  ・開墾した土地の【永久私有】を認めたため、貴族や寺院が大土地を私有化。\n" +
                    "  ・これがのちの【荘園（しょうえん）】となり、【公地公民】が崩れる原因となった！\n" +
                    "④ 天平文化と聖武天皇の鎮護国家\n" +
                    "  ・天然痘（疫病）の大流行や反乱などの社会不安を仏教で鎮めるため【国分寺・国分尼寺】を各国に建立。\n" +
                    "  ・都に【東大寺】を建て【大仏（盧舎那仏）】を造立。\n" +
                    "  ・民衆を指導して社会事業を行い大仏に協力した【行基】と、唐から失明を乗り越えて戒律を伝えた【鑑真（唐招提寺）】！",
                evidences = GetNaraEvidences(),
                locations = GetNaraLocations(),
                courtRounds = GetNaraCourtRounds()
            };
            return ep;
        }

        private static List<EvidenceItem> GetNaraEvidences()
        {
            return new List<EvidenceItem>
            {
                new EvidenceItem
                {
                    id = "ev_taiho",
                    title = "大宝律令の木簡",
                    subtitle = "701年制定・律令国家の法典",
                    iconSymbol = "📜",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.35f, 0.45f, 0.85f),
                    description = "701年に制定された法典。刑罰の決まり（律）と政治の決まり（令）を整備。中央に二官八省を置き、全国を国・郡・里に分けて国司を派遣した。",
                    examPoint = "【大宝律令（701年）】と【二官八省】。唐の律令にならい、天皇を中心とする律令国家の支配体制が完成した。"
                },
                new EvidenceItem
                {
                    id = "ev_wado",
                    title = "和同開珎（わどうかいちん）",
                    subtitle = "708年発行・日本初の流通貨幣",
                    iconSymbol = "🪙",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.85f, 0.65f, 0.15f),
                    description = "708年に発行された銀銭・銅銭。唐の『開元通宝』を手本に鋳造され、平城京の市での取引や、工事の給料としても支払われた。",
                    examPoint = "708年発行の【和同開珎】。平城京の『東西の市』で流通し、都の商業を発展させた。"
                },
                new EvidenceItem
                {
                    id = "ev_soyotyo",
                    title = "租・庸・調の荷札（木簡）",
                    subtitle = "農民に課された三大税の過酷な記録",
                    iconSymbol = "🌾",
                    imageResourceName = "loc_naniwa_port",
                    badgeColor = new Color(0.75f, 0.35f, 0.15f),
                    description = "稲を納める【租】（収穫の約3%）、都の労役の代わりの布を納める【庸】、特産物を都まで運ぶ【調】。年間60日の【雑徭】や北九州警備の【防人】も記されている。",
                    examPoint = "【租】＝稲、【庸】＝布、【調】＝特産物。【防人】＝北九州警備。負担の重さから多くの農民が口分田を捨てて逃亡した。"
                },
                new EvidenceItem
                {
                    id = "ev_daibutsu",
                    title = "大仏造立の詔（みことのり）",
                    subtitle = "743年・聖武天皇の鎮護国家の勅令",
                    iconSymbol = "📜",
                    imageResourceName = "loc_horyuji",
                    badgeColor = new Color(0.85f, 0.75f, 0.25f),
                    description = "743年、聖武天皇が出した勅令。『一枝の草、一握りの土を持ち寄り、国の平安を祈ろう』と記され、天然痘の疫病や飢饉を仏教の力で救うことを願った。",
                    examPoint = "聖武天皇の【鎮護国家（ちんごこっか）】。社会不安を鎮めるため、都に【東大寺・大仏】、各国に【国分寺・国分尼寺】を建てた。"
                },
                new EvidenceItem
                {
                    id = "ev_konden",
                    title = "墾田永年私財法の布告",
                    subtitle = "743年・新開墾地の永久私有を認めた法令",
                    iconSymbol = "🗺️",
                    imageResourceName = "loc_itabuki",
                    badgeColor = new Color(0.25f, 0.65f, 0.35f),
                    description = "743年に出された法令。新しく開墾した土地は永久に私有してよいと定めた。逃亡農民を私有地に囲い込む貴族が続出した。",
                    examPoint = "743年【墾田永年私財法】。開墾地の【永久私有】を認めたことで公地公民が崩れ、貴族や寺院の【荘園（しょうえん）】が生まれた。"
                },
                new EvidenceItem
                {
                    id = "ev_gyoki",
                    title = "行基のため池日誌",
                    subtitle = "民衆の英雄・行基の社会事業記録",
                    iconSymbol = "📘",
                    imageResourceName = "loc_naniwa_port",
                    badgeColor = new Color(0.20f, 0.55f, 0.75f),
                    description = "行基が民衆と造ったため池や橋、布施屋の記録。大仏の資材を着服した記録は一切なく、むしろ民の喜捨で正しく運営されていた。",
                    examPoint = "【行基（ぎょうき）】。弾圧を乗り越えて民衆に仏教と社会事業（池・橋）を広め、のちに聖武天皇に招かれて【大僧正】となり大仏造立に尽力。"
                },
                new EvidenceItem
                {
                    id = "ev_ganjin",
                    title = "鑑真の戒本（かいほん）",
                    subtitle = "唐の高僧・鑑真が伝えた正統な戒律",
                    iconSymbol = "📿",
                    imageResourceName = "loc_horyuji",
                    badgeColor = new Color(0.65f, 0.25f, 0.65f),
                    description = "唐から5度の渡航失敗と失明を乗り越えて来日した鑑真の教え。東大寺に戒壇を設け、行基や聖武天皇の志を讃えた証言が記されている。",
                    examPoint = "【鑑真（がんじん）】。唐から【失明】しながらも来日し、日本の僧に正しい【戒律】を伝えた。のちに【唐招提寺】を建立。"
                }
            };
        }

        private static List<LocationData> GetNaraLocations()
        {
            return new List<LocationData>
            {
                // ロケーション①：平城京 朱雀大路（710年）
                new LocationData
                {
                    locationId = "loc_nara_heijokyo",
                    title = "① 平城京 朱雀大路（710年〜）",
                    eraYear = "西暦710年（元明天皇の平城京遷都）",
                    imageResourceName = "loc_asuka_palace",
                    bgTopColor = new Color(0.08f, 0.12f, 0.25f),
                    bgBottomColor = new Color(0.35f, 0.22f, 0.15f),
                    sceneryArt = "唐の都・長安を手本にした碁盤の目の大都市！中央を貫く朱雀大路と賑わう東市・西市",
                    introText = "【平城京（へいじょうきょう）】：710年に奈良に造営された都。唐の長安を手本とした条坊制の壮大な大路が広がっている！",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_heijo_market",
                            label = "大路の市場（和同開珎の取引）",
                            icon = "🪙",
                            normalizedPos = new Vector2(0.25f, 0.45f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「見てレイくん！市場で丸くて四角い穴のあいたお金が使われているよ！」",
                                "史堂 レイ：「これは……【和同開珎（わどうかいちん）】だね！708年に発行された日本最初の流通貨幣だ！」",
                                "古文 ミオ：「唐の『開元通宝』を手本にしたんだよね。都の市で米や布の売買に使われているんだ！」"
                            },
                            rewardEvidenceId = "ev_wado"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_heijo_board",
                            label = "朱雀門前の高札（律令の布告）",
                            icon = "📜",
                            normalizedPos = new Vector2(0.65f, 0.55f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「門の前に国の決まりがびっしり書かれた板が立っているぞ。」",
                                "古文 ミオ：「これは【大宝律令（701年）】の条文だよ！二官八省の仕組みや、全国を国・郡・里に分けるきまりが定められたんだ。」",
                                "史堂 レイ：「刑罰の『律』と政治の『令』。天皇を中心とする律令国家の土台になった超重要法典だね！」"
                            },
                            rewardEvidenceId = "ev_taiho"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "topic_nara_heijo",
                            title = "平城京の都市計画について",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「平城京は本当に広いね！道路が碁盤の目のように整然と並んでいるよ。」",
                                "古文 ミオ：「710年、元明天皇の時に奈良に造営された新都だよ！唐の都【長安】をお手本にしたんだ。」",
                                "史堂 レイ：「中央に幅80メートル以上の朱雀大路があって、東市・西市の市では【和同開珎】でお買い物が行われているね！」"
                            }
                        },
                        new TalkTopic
                        {
                            topicId = "topic_nara_taiho",
                            title = "大宝律令と律令国家のしくみ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「701年に制定された【大宝律令】によって、天皇中心の国の形が完成したんだね。」",
                                "古文 ミオ：「中央には【二官八省】が置かれ、全国を国・郡・里に区分して、都から【国司】が派遣されたんだよ！」",
                                "史堂 レイ：「刑罰の『律』と政治の『令』。まさに法に基づいた国づくり（律令国家）の完成形だ！」"
                            }
                        }
                    }
                },

                // ロケーション②：東大寺 大仏殿建設現場（743年）
                new LocationData
                {
                    locationId = "loc_nara_daibutsu",
                    title = "② 東大寺 大仏殿建設現場（743年〜）",
                    eraYear = "西暦743年（聖武天皇の大仏造立の詔）",
                    imageResourceName = "loc_horyuji",
                    bgTopColor = new Color(0.12f, 0.10f, 0.22f),
                    bgBottomColor = new Color(0.45f, 0.35f, 0.18f),
                    sceneryArt = "巨大な大仏の鋳造工事とそびえ立つ木組み足場！都中の人々が息を呑んで見上げる聖域",
                    introText = "【東大寺 大仏殿】：聖武天皇の命により、銅の盧舎那仏（大仏）が造られている現場。疫病や社会不安を鎮める祈りが込められている。",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_daibutsu_edict",
                            label = "大仏造立の御触書",
                            icon = "📜",
                            normalizedPos = new Vector2(0.40f, 0.60f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「『一枝の草、一握りの土を持ち寄り、仏の力で国の平安を祈ろう』……」",
                                "古文 ミオ：「聖武天皇が出した【大仏造立の詔（743年）】だよ！当時、天然痘の流行や反乱で国中が大混乱していたんだ。」",
                                "史堂 レイ：「仏教の力で国を安定させようとした【鎮護国家（ちんごこっか）】の思想だね。各国には国分寺・国分尼寺も建てられたんだ！」"
                            },
                            rewardEvidenceId = "ev_daibutsu"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "topic_nara_shomu",
                            title = "聖武天皇の『鎮護国家』と大仏造立",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「これほど巨大な仏像を造るなんて、想像を絶する国家事業だね。」",
                                "古文 ミオ：「聖武天皇は、当時大流行した天然痘（疫病）や飢饉などの社会不安を仏教の力で鎮めようとしたんだ。」",
                                "古文 ミオ：「それを【鎮護国家（ちんごこっか）】というの。都に【東大寺】と【大仏】、各国に【国分寺・国分尼寺】を建てたんだよ！」"
                            }
                        },
                        new TalkTopic
                        {
                            topicId = "topic_nara_gyoki_talk",
                            title = "民衆の英雄・行基の協力",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「行基先生は、弾圧を受けながらも民衆のためにため池や橋を造る社会事業を行った高僧だよ！」",
                                "史堂 レイ：「聖武天皇も行基先生の人望を認めて最高位の『大僧正』に任命し、大仏建立の協力を求めたんだね。」",
                                "古文 ミオ：「行基先生が大仏の銅を盗むはずがないよ！真実を必ず明らかにしよう！」"
                            }
                        }
                    }
                },

                // ロケーション③：行基のため池・開墾地
                new LocationData
                {
                    locationId = "loc_nara_field",
                    title = "③ 奈良郊外の農村と新開墾地（743年〜）",
                    eraYear = "西暦743年（墾田永年私財法と農民の逃亡）",
                    imageResourceName = "loc_naniwa_port",
                    bgTopColor = new Color(0.15f, 0.20f, 0.12f),
                    bgBottomColor = new Color(0.42f, 0.38f, 0.22f),
                    sceneryArt = "行基と民衆が築いたため池と広大な水田。しかし重税に喘ぎ逃げ出す農民の姿も…",
                    introText = "【農村と開墾地】：行基が民衆と開墾した土地。朝廷の重税（租・庸・調）に苦しむ農民が集まっている。",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_field_farmer",
                            label = "ため池のほとりで休む逃亡農民",
                            icon = "🌾",
                            normalizedPos = new Vector2(0.25f, 0.40f),
                            speakerName = "農民",
                            dialogueLines = new string[]
                            {
                                "農民：「稲の【租】だけならまだしも……布を納める【庸】や、絹や魚を都へ運ぶ【調】が重すぎるんだ！」",
                                "農民：「何十日もかけて自力で運ばされ、年間60日もタダ働きさせられ、しまいには九州の防人にまで取られる……もう口分田にはいられねえ！」",
                                "史堂 レイ：「農民が逃げ出したのは、この過酷すぎる【租・庸・調】と【防人・雑徭】のせいだったんだ！」"
                            },
                            rewardEvidenceId = "ev_soyotyo"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_field_konden",
                            label = "墾田永年私財法の布告板",
                            icon = "🗺️",
                            normalizedPos = new Vector2(0.55f, 0.50f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「【墾田永年私財法（743年）】……新しく開墾した土地は永久に自分のものにしてよい！？」",
                                "古文 ミオ：「口分田不足を解決するための法律だけど、お金や力のある貴族や大寺院が農民を使って大土地を買い占めているの。」",
                                "史堂 レイ：「これがのちの【荘園（しょうえん）】になって、【公地公民】が崩れる大転換点になったんだね！」"
                            },
                            rewardEvidenceId = "ev_konden"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_field_gyoki_diary",
                            label = "行基の小屋の工事記録",
                            icon = "📘",
                            normalizedPos = new Vector2(0.80f, 0.35f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「行基先生が書き残した【ため池日誌】があるわ！橋やため池をどこに造ったか克明に記されている！」",
                                "史堂 レイ：「大仏の銅や金を横領した記録なんてどこにもない！資材はすべて民衆の真心と正しい献金で賄われているぞ！」"
                            },
                            rewardEvidenceId = "ev_gyoki"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "topic_nara_tax",
                            title = "農民を苦しめた三大税（租・庸・調）と防人",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「班田収授法で口分田がもらえたのに、どうして農民は逃げ出したのかな？」",
                                "古文 ミオ：「税があまりにも過酷だったからだよ！収穫の約3%の稲を納める【租】！」",
                                "古文 ミオ：「都での労役の代わりの布を納める【庸】、地方の特産物を都まで運ぶ【調】！」",
                                "史堂 レイ：「さらに年間60日の労役【雑徭】や、3年間の北九州警備【防人】まであって、耐えきれず逃亡したんだね。」"
                            }
                        },
                        new TalkTopic
                        {
                            topicId = "topic_nara_konden",
                            title = "墾田永年私財法（743年）と荘園の始まり",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「農民の逃亡で田んぼが荒れたため、743年に【墾田永年私財法】が出されたよ。」",
                                "史堂 レイ：「新しく開墾した土地は永久に私有してよいという法令だね。」",
                                "古文 ミオ：「でも、貴族や大寺院が貧しい農民を使って大土地を開墾し、私有地【荘園（しょうえん）】を広げていったんだ。」",
                                "史堂 レイ：「これによって、土地と人民はすべて国のものという【公地公民】が崩れていったんだね！」"
                            }
                        }
                    }
                },

                // ロケーション④：唐招提寺・鑑真の庵
                new LocationData
                {
                    locationId = "loc_nara_toshodaiji",
                    title = "④ 唐招提寺・鑑真の庵（754年〜）",
                    eraYear = "西暦754年（鑑真来日・唐招提寺）",
                    imageResourceName = "loc_itabuki",
                    bgTopColor = new Color(0.18f, 0.12f, 0.22f),
                    bgBottomColor = new Color(0.30f, 0.25f, 0.35f),
                    sceneryArt = "静寂に包まれた天平の寺院。唐から命がけで渡海した高僧・鑑真が静かに経典を説く場所",
                    introText = "【唐招提寺（とうしょうだいじ）】：唐の高僧・鑑真が創建した寺院。失明しながらも日本の仏教に正しい戒律を授けた。",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_ganjin_temple",
                            label = "鑑真が授けた戒律の書",
                            icon = "📿",
                            normalizedPos = new Vector2(0.45f, 0.50f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「鑑真和上は5回も海難事故に遭い、失明しながらも日本へ来てくださった唐の高僧なの。」",
                                "鑑真の弟子：「鑑真和上は東大寺に戒壇を設け、聖武天皇にも戒律を授けられました。そして行基大僧正の慈悲の心こそ仏の教えそのものであると仰せです。」",
                                "史堂 レイ：「この【鑑真の戒本】があれば、行基先生が正統な大仏建立の指導者であることを証明できるぞ！」"
                            },
                            rewardEvidenceId = "ev_ganjin"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "topic_nara_ganjin",
                            title = "鑑真和上の不屈の渡日と戒律",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「鑑真和上は、正しい仏教の戒律を授けるため唐から招かれた高僧だよ。」",
                                "史堂 レイ：「5回も航海が難破して失明しながら、6回目に命がけで日本へ渡ってきてくれたんだ！」",
                                "古文 ミオ：「奈良に【唐招提寺】を建てて、多くの人々に仏教の正しい教えを広めたんだよ。」"
                            }
                        },
                        new TalkTopic
                        {
                            topicId = "topic_nara_tenpyo",
                            title = "天平文化とシルクロードの国際色",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「奈良時代の文化は、聖武天皇のころの年号をとって【天平文化（てんぴょうぶんか）】と呼ばれるね。」",
                                "古文 ミオ：「【遣唐使】が唐の先進文化をもたらし、仏教を中心とした国際色豊かな文化が花開いたんだ！」",
                                "史堂 レイ：「東大寺の【正倉院】には、シルクロードを通って伝わった宝物が大切に保存されているんだよ。」"
                            }
                        }
                    }
                }
            };
        }

        private static List<CrossExaminationRound> GetNaraCourtRounds()
        {
            return new List<CrossExaminationRound>
            {
                // 第1尋問：重税と農民逃亡の真相
                new CrossExaminationRound
                {
                    roundNumber = 1,
                    themeTitle = "第1尋問：農民逃亡の真相について",
                    witnessName = "藤原 仲麻呂",
                    witnessTitle = "奈良の権勢貴族・大納言",
                    introDialogue = new string[]
                    {
                        "藤原 仲麻呂：「フッ……朝廷の許可なく勝手に民を集める怪僧・行基め！農民たちをたぶらかし、口分田を捨てて逃亡させた罪は死罪に値する！」",
                        "史堂 レイ：「行基先生が農民を逃亡させたなんて嘘だ！農民たちが土地を捨てざるを得なかった真実を暴くぞ！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 朝廷は【班田収授法】により、6歳以上の男女に口分田を与えて豊かな生活を保障している。",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に農民の生活は豊かだったのですか！？」",
                                "藤原 仲麻呂：「当然だ！口分田を与えているのだから、朝廷に感謝して一生懸命耕せば誰も苦しむはずがない！」"
                            },
                            validEvidenceIds = new List<string>()
                        },
                        new TestimonyStatement
                        {
                            text = "② それなのに農民たちが逃げ出したのは、行基が『税など払うな』と民を唆したからに他ならない！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！行基先生が税を払うなと唆した証拠はあるのですか！？」",
                                "藤原 仲麻呂：「証拠など不要！現に行基の元に多くの農民が集まり、朝廷に税を納めなくなっているではないか！」"
                            },
                            validEvidenceIds = new List<string> { "ev_soyotyo" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ 農民に課された税は収穫の3%の稲（租）だけであり、逃げ出す理由などどこにもないのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！税は本当に稲の『租』だけだったのですか！？」",
                                "藤原 仲麻呂：「フハハ！稲の3%など微々たる負担だ！それでも逃げるなど怠け者の証拠よ！」",
                                "古文 ミオ：「レイくん、稲の【租】だけじゃないよ！【租・庸・調の荷札】をつきつけて本当の重税を暴こう！」"
                            },
                            validEvidenceIds = new List<string> { "ev_soyotyo" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！税は稲（租）だけじゃないよ！都へ運ぶ特産物の【調】や布の【庸】、年間60日の【雑徭】、九州の【防人】が書かれた『租・庸・調の荷札』をつきつけるんだ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！藤原仲麻呂さん、その証言は史実と食い違っています！」",
                        "藤原 仲麻呂：「な、なにィ！？食い違っているだと！？」",
                        "史堂 レイ：「農民に課されていたのは稲の【租】だけではありません！都まで自力で運ぶ【調（特産物）】や【庸（布）】、年間60日の【雑徭】、命がけで北九州を守る【防人】という、過酷極まりない負担があったのです！」",
                        "史堂 レイ：「農民が土地を捨てたのは行基先生のせいではなく、その過酷な重税に生活が耐えられなかったからです！行基先生は行き場を失った彼らに生きる術を与えたのです！」",
                        "藤原 仲麻呂：「ぐぬぬぬ……！農民の税の負担まで細かく把握していたとは……！」"
                    },
                    summaryPoint = "✅ 第1尋問クリア：農民は『租（稲）』だけでなく、『庸（布）』『調（特産物）』『雑徭』『防人』の過酷な負担に苦しんで口分田から逃亡した！"
                },

                // 第2尋問：消えた金銅と墾田永年私財法の罠
                new CrossExaminationRound
                {
                    roundNumber = 2,
                    themeTitle = "第2尋問：消えた大仏資材の真相",
                    witnessName = "藤原 仲麻呂",
                    witnessTitle = "奈良の権勢貴族・大納言",
                    introDialogue = new string[]
                    {
                        "藤原 仲麻呂：「フン、税のことは百歩譲ろう。だが大仏建立に使われるはずだった大量の金と銅が行基の周辺から消えた事実は動かぬ！奴が私腹を肥やすために着服したのだ！」",
                        "史堂 レイ：「行基先生が金銅を横領したなんてあり得ない！資材が本当はどこへ消えたのか突き止めるぞ！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 大仏造立のために集められた銅と黄金は、すべて厳重に朝廷の倉で管理されていた。",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に朝廷の倉だけで厳重に管理されていたのですか？」",
                                "藤原 仲麻呂：「当然だ！我ら貴族が責任を持って管理していたのだ！」"
                            },
                            validEvidenceIds = new List<string>()
                        },
                        new TestimonyStatement
                        {
                            text = "② 行基のため池工事には莫大な資材が使われており、消えた金銅が行基の私腹に消えたのは明白だ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！ため池工事に使われた資材の記録を確認しましたか？」",
                                "藤原 仲麻呂：「確認など不要！怪僧の工事に金がかかっているのだから、大仏の資材を盗んだに決まっておる！」"
                            },
                            validEvidenceIds = new List<string> { "ev_gyoki" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ 当時の日本には私有地など存在せず、貴族である私が資材を着服する動機などあるはずがない！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に当時の日本に私有地は存在しなかったのですか！？」",
                                "藤原 仲麻呂：「そうだとも！土地はすべて国のもの（公地公民）なのだから、土地を私有して肥え太る動機などない！」",
                                "古文 ミオ：「大嘘だよレイくん！743年に出された【墾田永年私財法】をつきつけて、貴族の私有地（荘園）の企みを暴こう！」"
                            },
                            validEvidenceIds = new List<string> { "ev_konden" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！743年に新しく開墾した土地の永久私有を認めた【墾田永年私財法の布告】、または行基先生の潔白を示す【ため池日誌】をつきつけるんだ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！藤原仲麻呂さん、その証言、明白な矛盾があります！」",
                        "藤原 仲麻呂：「ぐわぁぁっ！？どこに矛盾があると言うのだ！？」",
                        "史堂 レイ：「743年、朝廷は【墾田永年私財法】を出しました！新しく開墾した土地は永久に自分のものにして良いと定めたのです！」",
                        "史堂 レイ：「そして行基先生の日誌を見れば、銅や金を横領した記録など一切なく、すべて民の真心で造られています！大仏の金銅と逃亡農民を私有地に囲い込み、私腹を肥やして【荘園】を広げようとしていた真犯人は……あなた自身だ！！」",
                        "藤原 仲麻呂：「バ、バカなァァァッ！？なぜその企みまで暴かれるのだァァッ！！」"
                    },
                    summaryPoint = "✅ 第2尋問クリア：743年の『墾田永年私財法』により土地の永久私有が認められ、貴族や寺院が私有地（荘園）を拡大して公地公民が崩れていった！"
                },

                // 第3尋問：国家鎮護と天平の真実
                new CrossExaminationRound
                {
                    roundNumber = 3,
                    themeTitle = "第3尋問：大仏建立と聖武天皇の願い（最終尋問）",
                    witnessName = "藤原 仲麻呂",
                    witnessTitle = "絶体絶命の権勢貴族",
                    introDialogue = new string[]
                    {
                        "藤原 仲麻呂：「ええい、小癪な弁護人め！だが仏教の権威を私物化しようとした行基が罪人であることに変わりはない！国家の仏教を導く資格があるのは、我ら貴族の高僧だけなのだ！」",
                        "古文 ミオ：「レイくん、これが最後の尋問だよ！聖武天皇が大仏を造った真の目的と、行基先生の真実をつきつけよう！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 聖武天皇が大仏を造立したのは、我ら貴族の富と権力を誇示するためである！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！貴族の権力を誇示するためだったのですか！？」",
                                "藤原 仲麻呂：「そうだ！大仏とは国家の権力そのもの！貧しい民のことなど関係ないのだ！」",
                                "古文 ミオ：「違うよ！【大仏造立の詔】をつきつけて、聖武天皇の真の願いを示そう！」"
                            },
                            validEvidenceIds = new List<string> { "ev_daibutsu" }
                        },
                        new TestimonyStatement
                        {
                            text = "② 行基のような民衆の僧など、唐の正しい仏教界からも認められるはずがないのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に認められていないのですか！？」",
                                "藤原 仲麻呂：「唐の高僧・鑑真和上からも、戒律なき怪僧として軽蔑されているに決まっておる！」",
                                "古文 ミオ：「唐から失明しながら来日した【鑑真の戒本】をつきつけて、鑑真和上の真意を示そう！」"
                            },
                            validEvidenceIds = new List<string> { "ev_ganjin" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ ゆえに行基は大仏造立に関わる資格などなく、大僧正に任命されるなど絶対にあり得ないのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！行基先生には民衆の絶大な支持がありました！」",
                                "藤原 仲麻呂：「民の支持など無意味！朝廷の認めた貴族の法こそがすべてなのだ！」"
                            },
                            validEvidenceIds = new List<string> { "ev_daibutsu", "ev_ganjin" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！聖武天皇が社会不安を鎮めるために仏教の力を借りようとした【大仏造立の詔（鎮護国家）】、または唐の高僧が認めた【鑑真の戒本】をつきつけるよ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！藤原仲麻呂さん、その証言こそ、聖武天皇の願いを冒涜するものです！」",
                        "藤原 仲麻呂：「グワァァァァッ！！な、なんだとォォッ！？」",
                        "史堂 レイ：「聖武天皇が大仏を造立したのは、貴族の権力誇示ではありません！当時大流行した天然痘（疫病）や飢饉、乱で苦しむすべての民を救うための【鎮護国家（ちんごこっか）】の祈りだったのです！」",
                        "史堂 レイ：「そして唐から失明を乗り越えて来日した鑑真和上も、行基先生の慈悲の心こそ真の仏教であると認めています！だからこそ聖武天皇は行基先生を最高位の【大僧正】に任命し、共に大仏を造ることを決断したのです！」",
                        "藤原 仲麻呂：「う、うわぁぁぁぁっ！！すべて論破されたァァ……！我が不正の帳簿が露見して失脚するぅぅ〜〜っ！！（完全敗北）」",
                        "古文 ミオ：「やったねレイくん！行基先生の無実が証明されて、大仏開眼供養への道が開かれたよ！」",
                        "裁判長：「静粛に！これにて判決を言い渡す！被告人・行基を無罪とし、大僧正として大仏建立の最高指導者に任ずる！弁護人・史堂レイ、完全勝訴！」"
                    },
                    summaryPoint = "✅ 第3尋問クリア：聖武天皇は疫病や飢饉の社会不安を鎮めるため『鎮護国家』を掲げて東大寺大仏を建立し、民衆の絶大な支持を集める行基を『大僧正』に任命して大仏造立に尽力させた！"
                }
            };
        }
    }

    /// <summary>
    /// 既存コード互換用ラッパー
    /// </summary>
    public static class AsukaEpisodeDatabase
    {
        public static List<EvidenceItem> GetAllEvidences()
        {
            return new List<EvidenceItem>
            {
                new EvidenceItem
                {
                    id = "kan_i_12",
                    title = "冠位十二階の制",
                    subtitle = "603年制定・能力重視の役人登用",
                    iconSymbol = "👑",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.55f, 0.25f, 0.78f),
                    description = "聖徳太子（厩戸王）が定めた制度。これまでの「生まれた家柄（氏姓制度）」にとらわれず、【個人の能力や功績】に応じて12段階の冠（最高位は紫）を授け、優秀な人物を役人に登用した。",
                    examPoint = "【冠位十二階（603年）】：家柄ではなく『個人の才能・功績』で登用。聖徳太子が推古天皇の摂政として制定。"
                },
                new EvidenceItem
                {
                    id = "kenpo_17",
                    title = "十七条の憲法",
                    subtitle = "604年制定・役人の道徳と心得",
                    iconSymbol = "📜",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.85f, 0.35f, 0.15f),
                    description = "聖徳太子が制定した、役人や貴族が守るべき政治の心得。第1条「和を以て貴しとなす」、第2条「篤く三宝（仏・法・僧）を敬え」、第3条「天皇の命には必ず従え」など、仏教を重んじ天皇中心の政治を目指した。",
                    examPoint = "【十七条の憲法（604年）】：第1条『和を以て貴しとなす』。役人の心得を定め、天皇を中心とする国づくりを目指した。"
                },
                new EvidenceItem
                {
                    id = "kenzuishi_letter",
                    title = "遣隋使の国書（小野妹子）",
                    subtitle = "607年・隋の皇帝への対等な親書",
                    iconSymbol = "✉️",
                    imageResourceName = "loc_naniwa_port",
                    badgeColor = new Color(0.20f, 0.55f, 0.85f),
                    description = "607年、聖徳太子が小野妹子に持たせて中国の【隋（ずい）】の煬帝へ送った親書。「日出づる処の天子、書を日没する処の天子に致す、恙無きや」と記し、中国の臣下にならず【対等な外交】を求めて先進文化を導入した。",
                    examPoint = "【遣隋使（607年・小野妹子）】：当時の中国は『隋』。『対等な立場』で国交を結び、進んだ制度・文化を学んだ。"
                },
                new EvidenceItem
                {
                    id = "horyuji_blueprint",
                    title = "法隆寺の建立図面",
                    subtitle = "世界最古の木造建築・飛鳥文化",
                    iconSymbol = "🏯",
                    imageResourceName = "loc_horyuji",
                    badgeColor = new Color(0.25f, 0.65f, 0.40f),
                    description = "聖徳太子が奈良の斑鳩（いかるが）に建立した寺院の図面。現存する【世界最古の木造建築群】であり、日本最初の仏教文化である【飛鳥文化】の最高傑作。釈迦三尊像など大陸やギリシャ・ペルシャの影響を残す。",
                    examPoint = "【法隆寺】：聖徳太子が建立した世界最古の木造建築。【飛鳥文化】は仏教中心の国際色豊かな文化。"
                },
                new EvidenceItem
                {
                    id = "issbi_incident",
                    title = "乙巳の変（いっしのへん）の記録",
                    subtitle = "645年・蘇我氏打倒のクーデター",
                    iconSymbol = "⚔️",
                    imageResourceName = "loc_itabuki",
                    badgeColor = new Color(0.85f, 0.18f, 0.22f),
                    description = "645年、飛鳥板蓋宮（あすかいたぶきのみや）で【中大兄皇子】と【中臣鎌足】が独裁を行っていた【蘇我入鹿】を倒し、父の蘇我蝦夷を自害に追い込んだ事件。これに続く政治改革を【大化の改新】と呼ぶ。",
                    examPoint = "【645年 乙巳の変（いっしのへん）】：中大兄皇子と中臣鎌足が蘇我氏を倒した事件。大化の改新の出発点。"
                },
                new EvidenceItem
                {
                    id = "taika_edit",
                    title = "改新の詔（かいしんのみことのり）",
                    subtitle = "646年発布・公地公民の原則",
                    iconSymbol = "📜",
                    imageResourceName = "loc_itabuki",
                    badgeColor = new Color(0.70f, 0.20f, 0.50f),
                    description = "大化の改新の方針を示した詔勅。豪族が私有していた土地（私地）と人民（私民）を廃止し、すべて国のものとする【公地公民（こうちこうみん）】を宣言。全国を国・郡に分け、戸籍と口分田（班田収授）の基礎を定めた。",
                    examPoint = "【公地公民（こうちこうみん）】：豪族の私有を禁じ、土地と人民をすべて天皇（国）のものとした大原則。"
                },
                new EvidenceItem
                {
                    id = "asuka_dynasty_chart",
                    title = "飛鳥・中国王朝の年表",
                    subtitle = "隋（581-618）と唐（618-907）の対比",
                    iconSymbol = "📊",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.40f, 0.40f, 0.45f),
                    description = "中国王朝の推移表。聖徳太子の時代（607年）の中国は【隋（ずい）】であり、【唐（とう）】が建国されたのは618年である。遣唐使が始まったのは630年（犬上御田鍬）から。",
                    examPoint = "小野妹子は『遣隋使（607年）』。中大兄皇子らの大化の改新（645年）の時代はすでに中国は『唐』。"
                }
            };
        }

        public static List<LocationData> GetAllLocations()
        {
            return new List<LocationData>
            {
                new LocationData
                {
                    locationId = "loc_palace",
                    title = "① 飛鳥の朝廷（小墾田宮）",
                    eraYear = "西暦604年頃（推古天皇の御代）",
                    imageResourceName = "loc_asuka_palace",
                    bgTopColor = new Color(0.10f, 0.12f, 0.22f),
                    bgBottomColor = new Color(0.38f, 0.18f, 0.14f),
                    sceneryArt = "朱色の大柱と玉座が並ぶ飛鳥の宮殿！冠を戴いた役人たちが行き交う政治の中心地",
                    introText = "【飛鳥の朝廷（あすかのちょうてい）】：推古天皇のもと、聖徳太子と蘇我馬子が政治を行った宮殿。新しい国づくりの法がここで発布された！",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_palace_court_hat",
                            label = "十二色の冠（冠位の展示）",
                            icon = "👑",
                            normalizedPos = new Vector2(0.24f, 0.46f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「見てミオ！紫や青、赤や黄色の色とりどりの冠が並んでいるよ！」",
                                "古文 ミオ：「これは聖徳太子が定めた【冠位十二階（603年）】の冠だよ！家柄じゃなくて能力で昇進できる画期的な制度なんだ！」",
                                "史堂 レイ：「テストにも超頻出だね！『家柄にとらわれず能力で登用』というキーワードを覚えておこう！」"
                            },
                            rewardEvidenceId = "kan_i_12"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_palace_constitution_board",
                            label = "憲法第1条の扁額（木札）",
                            icon = "📜",
                            normalizedPos = new Vector2(0.68f, 0.58f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「壁に立派な墨文字で『和を以て貴しとなす』と掲げられているわ！」",
                                "史堂 レイ：「これは聖徳太子が604年に定めた【十七条の憲法】の第1条だね！」",
                                "古文 ミオ：「豪族たちの争いをやめさせ、天皇を中心とする国づくりの道徳を教えたんだよ！」"
                            },
                            rewardEvidenceId = "kenpo_17"
                        }
                    }
                },
                new LocationData
                {
                    locationId = "loc_port",
                    title = "② 難波津の港（遣隋使の出航地）",
                    eraYear = "西暦607年（遣隋使出航の年）",
                    imageResourceName = "loc_naniwa_port",
                    bgTopColor = new Color(0.12f, 0.22f, 0.38f),
                    bgBottomColor = new Color(0.18f, 0.32f, 0.24f),
                    sceneryArt = "大阪湾から大陸を目指す大型帆船！風になびく日の丸と大勢の見送り人",
                    introText = "【難波津（なにわづ）の港】：瀬戸内海から中国大陸へ船出する外交の玄関口。小野妹子を乗せた遣隋使船がここに停泊している！",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_port_ship_letter",
                            label = "外交使節の荷物箱",
                            icon = "✉️",
                            normalizedPos = new Vector2(0.35f, 0.38f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「荷物箱の中に、中国の皇帝へ宛てた大切な手紙の控えがあるぞ！」",
                                "古文 ミオ：「『日出づる処の天子、書を日没する処の天子に致す』……これ、聖徳太子が小野妹子に託した国書よ！」",
                                "史堂 レイ：「607年の【遣隋使】だね！家来としてではなく【対等な立場】で隋の皇帝・煬帝と国交を結ぼうとしたんだ！」"
                            },
                            rewardEvidenceId = "kenzuishi_letter"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_port_dynasty_scroll",
                            label = "航海士の中国王朝巻物",
                            icon = "📊",
                            normalizedPos = new Vector2(0.72f, 0.48f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「船員さんが持っている中国の年表巻物を見せてもらったわ！」",
                                "古文 ミオ：「聖徳太子の時代の中国は【隋（581-618）】で、次の【唐（618-907）】が建国されたのはもっと後なのね！」",
                                "史堂 レイ：「テストで『遣隋使』と『遣唐使』をごちゃ混ぜにする引っかけ問題によく出るから要注意だ！」"
                            },
                            rewardEvidenceId = "asuka_dynasty_chart"
                        }
                    }
                },
                new LocationData
                {
                    locationId = "loc_temple",
                    title = "③ 斑鳩の法隆寺（飛鳥文化の中心）",
                    eraYear = "西暦607年創建（現存世界最古の木造建築）",
                    imageResourceName = "loc_horyuji",
                    bgTopColor = new Color(0.15f, 0.12f, 0.24f),
                    bgBottomColor = new Color(0.28f, 0.22f, 0.16f),
                    sceneryArt = "青空にそびえる五重塔と金堂の美！日本仏教の夜明けを告げる荘厳な伽藍",
                    introText = "【斑鳩（いかるが）の法隆寺】：聖徳太子が建立した寺院。世界最古の木造建築群であり、飛鳥文化を象徴する国宝の宝庫！",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_temple_blueprint",
                            label = "金堂・五重塔の建築設計図",
                            icon = "🏯",
                            normalizedPos = new Vector2(0.48f, 0.52f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「五重塔と金堂の組み木の図面が丁寧に描かれているよ！」",
                                "古文 ミオ：「法隆寺は【現存する世界最古の木造建築】なのよ！釈迦三尊像など大陸文化の影響を受けた【飛鳥文化】の最高傑作ね！」",
                                "史堂 レイ：「仏教を厚く敬った聖徳太子の情熱が今も生きているんだね！」"
                            },
                            rewardEvidenceId = "horyuji_blueprint"
                        }
                    }
                },
                new LocationData
                {
                    locationId = "loc_itabuki_scene",
                    title = "④ 飛鳥板蓋宮（大化の改新の舞台）",
                    eraYear = "西暦645年（大化の改新・乙巳の変）",
                    imageResourceName = "loc_itabuki",
                    bgTopColor = new Color(0.20f, 0.08f, 0.10f),
                    bgBottomColor = new Color(0.32f, 0.16f, 0.12f),
                    sceneryArt = "板葺きの屋根が連なる宮殿の大広間！歴史を揺るがす政変が起きた緊迫の現場",
                    introText = "【飛鳥板蓋宮（あすかいたぶきのみや）】：645年、中大兄皇子と中臣鎌足が蘇我氏を倒し、大化の改新をスタートさせた歴史の転換点！",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_itabuki_sword_scroll",
                            label = "皇子の密書（政変の記録）",
                            icon = "⚔️",
                            normalizedPos = new Vector2(0.30f, 0.42f),
                            speakerName = "ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「柱の陰から、中大兄皇子と中臣鎌足が交わした計画書が見つかったわ！」",
                                "古文 ミオ：「645年、専横を極めていた蘇我入鹿を板蓋宮で討ち、蘇我氏を滅ぼした【乙巳の変（いっしのへん）】の記録よ！」",
                                "史堂 レイ：「ここから日本の政治が豪族の支配から天皇中心の国づくりへと大きく舵を切ったんだ！」"
                            },
                            rewardEvidenceId = "issbi_incident"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_itabuki_decree_roll",
                            label = "大化の改新・改新の詔の木簡",
                            icon = "📜",
                            normalizedPos = new Vector2(0.68f, 0.48f),
                            speakerName = "レイ",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「『豪族の私有地・私民を廃止し、すべて公の土地・人民とする』……！」",
                                "古文 ミオ：「これこそ大化の改新の基本方針である【公地公民（こうちこうみん）】の詔（みことのり）よ！」",
                                "史堂 レイ：「豪族たちの勝手な土地支配を禁じ、国が直接人民を治めて口分田を与える仕組みの始まりだ！」"
                            },
                            rewardEvidenceId = "taika_edit"
                        }
                    }
                }
            };
        }

        public static List<CrossExaminationRound> GetCourtRounds()
        {
            return new List<CrossExaminationRound>
            {
                new CrossExaminationRound
                {
                    roundNumber = 1,
                    themeTitle = "第1尋問：聖徳太子の政治改革について",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "飛鳥の大豪族・大臣",
                    introDialogue = new string[]
                    {
                        "蘇我 馬子：「フン！小僧ども、よくぞ飛鳥の法廷までやってきたな！だが飛鳥時代の歴史はすべて、我が蘇我氏の独裁と権力の歴史なのだ！」",
                        "史堂 レイ：「それは絶対に違います！聖徳太子の政治改革の真実を、法廷で暴いてみせます！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 603年に制定された【冠位十二階】は、有力豪族の家柄を永遠に保障するための制度であった！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！家柄を保障するためだったのですか！？」",
                                "蘇我 馬子：「そうだとも！蘇我氏のような名門貴族だけが高い位を独占するための制度よ！」",
                                "古文 ミオ：「違うよレイくん！冠位十二階の目的は真逆のはず！『冠位十二階の制』をつきつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "kan_i_12" }
                        },
                        new TestimonyStatement
                        {
                            text = "② 604年の【十七条の憲法】第1条には『豪族同士、争って勝ち残れ』と定めたのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！憲法に争えなんて書いてあるはずがありません！」",
                                "蘇我 馬子：「ハッハッハ！強い豪族が生き残る弱肉強食こそ飛鳥の掟！何か文句でもあるか！？」",
                                "古文 ミオ：「『和を以て貴しとなす』の言葉を思い出して！『十七条の憲法』をつきつけるよ！」"
                            },
                            validEvidenceIds = new List<string> { "kenpo_17" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ 聖徳太子は仏教など信じておらず、寺などただの1つも建てておらんわ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！聖徳太子がお寺を建てていないなんてありえません！」",
                                "蘇我 馬子：「寺など作って何の役に立つ！飛鳥に仏教など広まってはおらんのだ！」",
                                "古文 ミオ：「世界最古の木造建築の図面があるでしょ！『法隆寺の建立図面』をつきつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "horyuji_blueprint" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！1番目の証言には『冠位十二階（能力重視）』、2番目には『十七条の憲法（和を以て貴しとなす）』、3番目には『法隆寺の建立図面』を【つきつける】よ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！蘇我馬子さん、その証言は真っ赤なウソです！」",
                        "蘇我 馬子：「なにィィィッ！？ウソだとォッ！？」",
                        "史堂 レイ：「【冠位十二階】は家柄ではなく【個人の能力や功績】で役人を登用した制度です！そして【十七条の憲法】は『和を以て貴しとなす』と説き、争いを戒め天皇中心の国を目指したのです！」",
                        "蘇我 馬子：「ぬわぁぁぁぁっ！？なぜ1400年後の未来人がそんなことまで知っているのだァァッ！？」"
                    },
                    summaryPoint = "✅ 第1尋問クリア：聖徳太子は『冠位十二階（能力重視）』と『十七条の憲法（和の精神）』を定め、天皇中心の国づくりを進めた！"
                },
                new CrossExaminationRound
                {
                    roundNumber = 2,
                    themeTitle = "第2尋問：外交と遣隋使について",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "飛鳥の大豪族・大臣",
                    introDialogue = new string[]
                    {
                        "蘇我 馬子：「ぐぬぬ……だが外交はどうだ！日本は偉大な大帝国である【唐】の皇帝にひれ伏し、朝貢の使者を送ったのだ！」",
                        "史堂 レイ：「小野妹子が派遣された当時の中国は『唐』だったのか……？史料をよく確認しよう！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 607年、聖徳太子は小野妹子を【遣唐使】として中国の【唐】へ派遣した！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！607年に派遣されたのは本当に『遣唐使』なのですか！？」",
                                "蘇我 馬子：「当然だ！小野妹子は中国の唐へ赴き、唐の皇帝に仕えたのだ！」",
                                "古文 ミオ：「年代がおかしいよ！607年の中国の王朝を『年表』や『国書』で確認してみて！」"
                            },
                            validEvidenceIds = new List<string> { "kenzuishi_letter", "asuka_dynasty_chart" }
                        },
                        new TestimonyStatement
                        {
                            text = "② 小野妹子が持参した国書には『日本は唐の忠実な家来になります』と記されていたのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！家来になりますなんて書いたのですか！？」",
                                "蘇我 馬子：「中国皇帝の家来になることこそ外交！対等な関係など求めてはおらんわ！」",
                                "古文 ミオ：「『日出づる処の天子…』の文面を思い出して！『遣隋使の国書』をつきつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "kenzuishi_letter" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！607年当時の中国は『唐』ではなく『隋（ずい）』だよ！2番目か3番目の証言に『遣隋使の国書』または『飛鳥・中国王朝の年表』を【つきつける】だ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！その証言、史料と食い違っています！」",
                        "蘇我 馬子：「ぐわぁぁっ！？そ、その国書と年表はどこで手に入れたァァッ！？」",
                        "史堂 レイ：「607年当時の中国は『唐（618年建国）』ではなく【隋（ずい）】です！つまり小野妹子は【遣隋使（けんずいし）】として派遣されました！」",
                        "史堂 レイ：「さらに国書には『日出づる処の天子、書を日没する処の天子に致す』と記され、家来としてではなく【対等な外交】を求めたのです！」",
                        "蘇我 馬子：「ぐぬぬぬ…！遣唐使ではなく遣隋使で、しかも対等な外交だったことまで見抜かれるとは…！」"
                    },
                    summaryPoint = "✅ 第2尋問クリア：607年、小野妹子は『遣隋使』として中国の『隋』へ渡り、対等な立場で外交を行い先進文化を吸収した！"
                },
                new CrossExaminationRound
                {
                    roundNumber = 3,
                    themeTitle = "第3尋問：645年・大化の改新について（最終尋問）",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "絶体絶命の大豪族",
                    introDialogue = new string[]
                    {
                        "蘇我 馬子：「ま、まだだ！私と聖徳太子が世を去ったあと、我が息子・蘇我蝦夷と孫・蘇我入鹿の時代（645年）こそ、蘇我氏の栄光の頂点なのだ！」",
                        "古文 ミオ：「レイくん、これが最後の尋問だよ！645年の【大化の改新】の真実をつきつければ、私たちの完全勝訴だよ！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 聖徳太子の死後、朝廷では我が一族である蘇我蝦夷・蘇我入鹿（そがのいるか）親子が権力を握った。",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！蘇我氏が権力を握っていたのは事実ですね？」",
                                "蘇我 馬子：「うむ、ここまでは本当の史実だ。問題は645年に飛鳥板蓋宮で起きた出来事の結末よ…フハハハ！」"
                            },
                            validEvidenceIds = new List<string>()
                        },
                        new TestimonyStatement
                        {
                            text = "② 645年、【中大兄皇子】と【中臣鎌足】は蘇我入鹿の前にひれ伏し、蘇我氏を永遠に日本の王にすると誓ったのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！中大兄皇子と中臣鎌足が蘇我氏に降伏したなんて本当ですか！？」",
                                "蘇我 馬子：「もちろんだ！飛鳥板蓋宮で蘇我入鹿に降伏し、蘇我氏の独裁がその後1000年続いたのだ！」",
                                "古文 ミオ：「真逆だよレイくん！645年の【乙巳の変（蘇我氏打倒の記録）】をつきつけて論破しよう！」"
                            },
                            validEvidenceIds = new List<string> { "issbi_incident" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ そして始まった『大化の改新』により、全国の土地と人民はすべて【豪族たちの私有物（私地私民）】にすることが決まったのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！大化の改新で土地や人民を『豪族の私有物』にしたんですか！？」",
                                "蘇我 馬子：「そうだとも！全国の土地も人民もすべて豪族が自由に私有してよいという改革こそ『大化の改新』なのだ！」",
                                "古文 ミオ：「大化の改新で一番大事な四字熟語を思い出して！【改新の詔（公地公民の制）】をつきつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "taika_edit", "issbi_incident" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！2番目の証言（蘇我氏に降伏）には『乙巳の変』、3番目の証言（豪族の私有物にした）には『改新の詔（公地公民の制）』を【つきつける】よ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！蘇我馬子さん、その証言、史料と完全に食い違っています！！」",
                        "蘇我 馬子：「グワァァァァッ！！ば、馬鹿なァァッ！その史料は645年の『乙巳の変』と『改新の詔』ではないかァァッ！！」",
                        "史堂 レイ：「645年、【中大兄皇子（のちの天智天皇）】と【中臣鎌足（のちの藤原鎌足）】は、横暴を極めていた蘇我入鹿・蝦夷を倒しました！」",
                        "史堂 レイ：「そして始まった【大化の改新】では、豪族の土地・人民の私有をやめさせ、すべて国（天皇）が直接治める【公地公民（こうちこうみん）】を定めたのです！」",
                        "蘇我 馬子：「う、うわぁぁぁっ！完璧に論破されたァァ…！飛鳥時代の歴史が正しい姿へ戻っていくぅぅ〜！！（敗北）」",
                        "古文 ミオ：「やったねレイくん！すべての矛盾を論破して、飛鳥時代の歴史が完全に元通りになったよ！」",
                        "裁判長：「これにて閉廷！弁護人・史堂レイ、ならびに助手・古文ミオの完全勝訴とする！現代への帰還ゲートを開く！」"
                    },
                    summaryPoint = "✅ 第3尋問クリア：645年、中大兄皇子と中臣鎌足が蘇我氏を倒して『大化の改新』を始め、『公地公民』の方針を打ち出した！"
                }
            };
        }
    }
}
