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
    /// 『逆転日本史』第1話「逆転の飛鳥時代」の全シナリオ＆学習データベース
    /// 登場人物：
    /// ・史堂 レイ（主人公・中学生弁護人）：「その証言、史料と食い違っています！」
    /// ・古文 ミオ（助手・古文歴史研究部）：古典と史料集を手にレイをサポート
    /// ・蘇我 馬子（証人・飛鳥の大豪族）：証言台から蘇我氏中心の歪んだ歴史を主張する
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
                    examPoint = "★重要：『家柄ではなく、個人の能力や功績で役人を登用した』がテスト頻出！"
                },
                new EvidenceItem
                {
                    id = "kenpo_17",
                    title = "十七条の憲法",
                    subtitle = "604年制定・日本初の役人の心得",
                    iconSymbol = "📜",
                    imageResourceName = "loc_asuka_palace",
                    badgeColor = new Color(0.82f, 0.55f, 0.15f),
                    description = "聖徳太子が定めた役人の心構え。第1条『和を以て貴しとなす（みんなで仲良く協力せよ）』、第2条『篤く三宝（仏教）を敬え』、第3条『詔（天皇の命令）を承りては必ず謹め』と記されている。",
                    examPoint = "★重要：豪族同士の争いを抑え、【天皇を中心とした国づくり】を目指した！"
                },
                new EvidenceItem
                {
                    id = "kenzuishi_letter",
                    title = "遣隋使の国書",
                    subtitle = "607年・小野妹子が隋の皇帝へ持参",
                    iconSymbol = "✉️",
                    imageResourceName = "loc_naniwa_port",
                    badgeColor = new Color(0.15f, 0.52f, 0.78f),
                    description = "小野妹子が中国の【隋（ずい）】の皇帝・煬帝（ようだい）に届けた手紙。『日出づる処の天子、書を日没する処の天子に致す、つつがなきや』と書かれ、家来としてではなく【対等な外交関係】を求めた。",
                    examPoint = "★重要：相手国は『唐』ではなく『隋（遣隋使）』！目的は『対等な外交』と『進んだ文化・制度の吸収』！"
                },
                new EvidenceItem
                {
                    id = "east_asia_timeline",
                    title = "飛鳥・中国王朝の年表",
                    subtitle = "6世紀末〜7世紀の歴史年表",
                    iconSymbol = "⏳",
                    imageResourceName = "loc_naniwa_port",
                    badgeColor = new Color(0.20f, 0.65f, 0.55f),
                    description = "【589年】中国を『隋（ずい）』が統一\n【593年】聖徳太子が推古天皇の摂政になる\n【607年】小野妹子を『遣隋使』として派遣・法隆寺建立\n【618年】隋が滅び、『唐（とう）』が建国される\n【645年】大化の改新が始まる",
                    examPoint = "★重要：607年の小野妹子派遣の時点では、中国はまだ『隋』の時代（唐は618年建国）！"
                },
                new EvidenceItem
                {
                    id = "horyuji_record",
                    title = "法隆寺と飛鳥文化の記録",
                    subtitle = "世界最古の木造建築・日本初の仏教文化",
                    iconSymbol = "🏯",
                    imageResourceName = "loc_horyuji",
                    badgeColor = new Color(0.75f, 0.30f, 0.25f),
                    description = "聖徳太子が斑鳩（いかるが・現在の奈良県）に建てた寺院。五重塔や金堂は【世界最古の木造建築】として現存する。中国や朝鮮半島の影響を受けた日本最初の仏教文化を【飛鳥文化】と呼ぶ。",
                    examPoint = "★重要：法隆寺＝世界最古の木造建築、飛鳥文化＝日本最初の仏教文化！"
                },
                new EvidenceItem
                {
                    id = "issbi_incident",
                    title = "乙巳の変（蘇我氏打倒の記録）",
                    subtitle = "645年・飛鳥板蓋宮での政変",
                    iconSymbol = "⚔️",
                    imageResourceName = "loc_itabuki",
                    badgeColor = new Color(0.80f, 0.20f, 0.25f),
                    description = "聖徳太子の死後、政治の実権を独占して天皇をしのぐ勢力となった【蘇我入鹿・蝦夷（そがのいるか・えみし）】を、【中大兄皇子（のちの天智天皇）】と【中臣鎌足（のちの藤原鎌足）】が協力して倒した事件。",
                    examPoint = "★重要：645年、中大兄皇子と中臣鎌足が『蘇我氏』を滅ぼし、大化の改新が始まった！"
                },
                new EvidenceItem
                {
                    id = "taika_edit",
                    title = "改新の詔（公地公民の制）",
                    subtitle = "大化の改新の根本方針",
                    iconSymbol = "⚖️",
                    imageResourceName = "loc_itabuki",
                    badgeColor = new Color(0.25f, 0.48f, 0.85f),
                    description = "大化の改新で出された新しい政治方針。それまで豪族たちが私有していた土地（私地）と人民（私民）をすべて廃止し、全国の土地と人民を国（天皇）が直接支配する【公地公民（こうちこうみん）】の制度を定めた。",
                    examPoint = "★重要：大化の改新のキーワードは『豪族の私有をやめさせ、公地公民にした』！"
                }
            };
        }

        public static List<LocationData> GetAllLocations()
        {
            return new List<LocationData>
            {
                // 場所①：飛鳥の朝廷
                new LocationData
                {
                    locationId = "asuka_palace",
                    title = "① 飛鳥の朝廷（推古天皇の宮）",
                    eraYear = "西暦604年頃・飛鳥の都",
                    imageResourceName = "loc_asuka_palace",
                    bgTopColor = new Color(0.18f, 0.14f, 0.28f),
                    bgBottomColor = new Color(0.32f, 0.22f, 0.42f),
                    sceneryArt = "🏯 飛鳥宮・朝廷の広場 👑",
                    introText = "古文 ミオ：「レイくん、見て！ここが推古天皇の宮殿前だよ。摂政の【聖徳太子（厩戸王）】が新しい国づくりを進めているみたい！現場を『調べる』で調査しよう！」",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_crown",
                            label = "紫や青の冠をかぶった役人",
                            icon = "👑",
                            normalizedPos = new Vector2(-340f, 220f),
                            speakerName = "若き朝廷の役人",
                            dialogueLines = new string[]
                            {
                                "若き役人：「おお、見慣れない学ランを着た少年だね！私がかぶっているこの冠が見えるかい？」",
                                "史堂 レイ：「色鮮やかな冠ですね！これはどういう制度なんですか？」",
                                "若き役人：「603年に聖徳太子様が定めた【冠位十二階（かんいじゅうにかい）】さ！昔は『生まれた家柄』が良い豪族しか高い役職につけなかったんだ…」",
                                "若き役人：「でも冠位十二階のおかげで、家柄に関係なく【個人の能力や功績】で役人に選ばれるようになったんだよ！」"
                            },
                            rewardEvidenceId = "kan_i_12"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_constitution",
                            label = "朝廷に掲げられた十七条の巻物",
                            icon = "📜",
                            normalizedPos = new Vector2(-180f, 65f),
                            speakerName = "古文 ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「レイくん、私の古文書と照らし合わせてみたよ！朝廷の柱に掲げられているのが604年の【十七条の憲法】だわ！」",
                                "史堂 レイ：「どれどれ…第1条『和を以て貴しとなす（みんなで争わず協力せよ）』、第2条『篤く三宝（仏教）を敬え』、第3条『天皇の命令には必ず従え』か！」",
                                "古文 ミオ：「うん！蘇我氏のような有力な豪族たちが勝手に争うのを抑えて、【天皇を中心とした国づくり】を進めるための役人の心得なんだね！」"
                            },
                            rewardEvidenceId = "kenpo_17"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "talk_asuka_1",
                            title = "聖徳太子と推古天皇について",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「593年、日本初の女性天皇である【推古天皇】が即位して、甥の【聖徳太子（厩戸王）】が【摂政（せっしょう）】になったの。」",
                                "史堂 レイ：「そして豪族の【蘇我馬子】と協力して政治を行ったんだったね。でも法廷では、その蘇我馬子が歴史を自分に都合よく書き換えようとしているらしいぞ！」"
                            }
                        },
                        new TalkTopic
                        {
                            topicId = "talk_asuka_2",
                            title = "冠位十二階と十七条の憲法の違い",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「テストでよく引っかかるポイントだよ！【冠位十二階】は『家柄ではなく能力・功績で役人を登用する制度』！」",
                                "古文 ミオ：「そして【十七条の憲法】は『役人の心構え（和を重んじ、仏教を敬い、天皇中心の国をつくる）』だよ！」"
                            }
                        }
                    }
                },
                // 場所②：難波津（港）
                new LocationData
                {
                    locationId = "naniwa_port",
                    title = "② 難波津（遣隋使の出航港）",
                    eraYear = "西暦607年・大阪湾の港",
                    imageResourceName = "loc_naniwa_port",
                    bgTopColor = new Color(0.10f, 0.24f, 0.38f),
                    bgBottomColor = new Color(0.18f, 0.42f, 0.58f),
                    sceneryArt = "⛵ 難波津の港・大海原へ発つ遣隋使船 🌊",
                    introText = "史堂 レイ：「潮風が吹く大きな港に出たぞ！西暦607年…中国へ向かう巨大な外交船が出航の準備をしている！」",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_imoko",
                            label = "外交使節・小野妹子",
                            icon = "✉️",
                            normalizedPos = new Vector2(-340f, 220f),
                            speakerName = "小野妹子（おののいもこ）",
                            dialogueLines = new string[]
                            {
                                "小野妹子：「私は【小野妹子】！聖徳太子様の命を受け、これより海を渡って中国の【隋（ずい）】という大国へ向かう【遣隋使】だ！」",
                                "史堂 レイ：「その手に持っている箱は、隋の皇帝に渡す手紙（国書）ですか？」",
                                "小野妹子：「いかにも！『日出づる処の天子、書を日没する処の天子に致す、つつがなきや』と記されている。」",
                                "小野妹子：「日本は決して中国の家来ではない。【対等な外交】を行い、進んだ文化や制度を学んでくるのだ！」"
                            },
                            rewardEvidenceId = "kenzuishi_letter"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_timeline",
                            label = "航海士の東アジア歴史年表",
                            icon = "⏳",
                            normalizedPos = new Vector2(-180f, 65f),
                            speakerName = "古文 ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「レイくん、この歴史年表を見て！589年に中国を統一したのが【隋（ずい）】という王朝よ。」",
                                "史堂 レイ：「なるほど！そして隋が滅びて【唐（とう）】に変わるのは11年後の【618年】なんだね。」",
                                "古文 ミオ：「つまり、ここ【607年】に小野妹子さんが向かう相手は『唐（遣唐使）』ではなく、間違いなく『隋（遣隋使）』！法廷で絶対役に立つ史料になるわ！」"
                            },
                            rewardEvidenceId = "east_asia_timeline"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "talk_port_1",
                            title = "遣隋使と遣唐使の見分け方",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「聖徳太子が607年に小野妹子を送ったのは、中国の『隋』だから【遣隋使（けんずいし）】！」",
                                "史堂 レイ：「『唐』に送った遣唐使と混ぜてひっかける問題が多いから、年表と国書をしっかり法廷記録に入れておこう！」"
                            }
                        }
                    }
                },
                // 場所③：斑鳩の里・法隆寺
                new LocationData
                {
                    locationId = "ikaruga_horyuji",
                    title = "③ 斑鳩の里・法隆寺",
                    eraYear = "西暦607年頃・斑鳩（奈良）",
                    imageResourceName = "loc_horyuji",
                    bgTopColor = new Color(0.28f, 0.16f, 0.14f),
                    bgBottomColor = new Color(0.45f, 0.28f, 0.20f),
                    sceneryArt = "⛩️ 斑鳩の里・そびえ立つ法隆寺五重塔 🔔",
                    introText = "古文 ミオ：「わぁ…美しい木造の五重塔と金堂が建ち並んでいるわ！ここが斑鳩（いかるが）の里だよ！」",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_horyuji",
                            label = "五重塔と仏像を彫る仏師",
                            icon = "🏯",
                            normalizedPos = new Vector2(-260f, 145f),
                            speakerName = "斑鳩の宮大工",
                            dialogueLines = new string[]
                            {
                                "斑鳩の宮大工：「ここは聖徳太子様が建てられた【法隆寺（ほうりゅうじ）】だ！未来の世まで1400年以上も残る【世界最古の木造建築】になる建物だぞ。」",
                                "古文 ミオ：「蘇我馬子が建てた飛鳥寺とともに、中国や朝鮮半島の影響を受けた日本最初の仏教文化――【飛鳥文化】が花開いているんですね！」"
                            },
                            rewardEvidenceId = "horyuji_record"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "talk_horyuji_1",
                            title = "飛鳥文化の特徴について",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「飛鳥文化は【日本最初の仏教文化】！聖徳太子の【法隆寺（世界最古の木造建築）】や釈迦三尊像が有名だよ。」"
                            }
                        }
                    }
                },
                // 場所④：飛鳥板蓋宮（大化の改新）
                new LocationData
                {
                    locationId = "itabuki_palace",
                    title = "④ 飛鳥板蓋宮（大化の改新の舞台）",
                    eraYear = "西暦645年・飛鳥の都",
                    imageResourceName = "loc_itabuki",
                    bgTopColor = new Color(0.26f, 0.10f, 0.16f),
                    bgBottomColor = new Color(0.46f, 0.18f, 0.24f),
                    sceneryArt = "⚔️ 飛鳥板蓋宮・645年 大改革の幕開け ⚖️",
                    introText = "史堂 レイ：「時代が約40年進んで【西暦645年】になったぞ！権力を独占していた蘇我氏に対し、若き皇子たちが立ち上がった瞬間だ！」",
                    spots = new List<InvestigationSpot>
                    {
                        new InvestigationSpot
                        {
                            spotId = "spot_nakanooe",
                            label = "中大兄皇子と中臣鎌足",
                            icon = "⚔️",
                            normalizedPos = new Vector2(-340f, 220f),
                            speakerName = "中大兄皇子 ＆ 中臣鎌足",
                            dialogueLines = new string[]
                            {
                                "中大兄皇子：「私、【中大兄皇子（なかのおおえのおうじ）】と【中臣鎌足（なかとみのかまたり）】は、ここ645年、朝廷の権力を独占していた蘇我入鹿・蝦夷を倒したぞ！（乙巳の変）」",
                                "中臣鎌足：「蘇我氏に降伏するどころか、蘇我氏の独裁を打ち破り、今こそ天皇中心の新しい政治改革【大化の改新】を始めるのです！」"
                            },
                            rewardEvidenceId = "issbi_incident"
                        },
                        new InvestigationSpot
                        {
                            spotId = "spot_kaishin",
                            label = "発布された『改新の詔』",
                            icon = "⚖️",
                            normalizedPos = new Vector2(-180f, 65f),
                            speakerName = "古文 ミオ",
                            dialogueLines = new string[]
                            {
                                "古文 ミオ：「レイくん、これが大化の改新の目玉となる方針書【改新の詔】よ！一番大事な四字熟語は【公地公民（こうちこうみん）】だわ！」",
                                "史堂 レイ：「これまで豪族たちが勝手に私有していた土地（私地）と人民（私民）をすべて廃止して、【全国の土地と人民を国（天皇）のものにする】と宣言されている！」",
                                "古文 ミオ：「これで大化の改新の決定的な史料も揃ったね！さあ、法廷で【蘇我馬子】のウソの証言を論破しよう！」"
                            },
                            rewardEvidenceId = "taika_edit"
                        }
                    },
                    talkTopics = new List<TalkTopic>
                    {
                        new TalkTopic
                        {
                            topicId = "talk_taika_1",
                            title = "645年・大化の改新の重要人物",
                            dialogueLines = new string[]
                            {
                                "史堂 レイ：「645年に【蘇我氏（蘇我入鹿・蝦夷）】を倒したのが、【中大兄皇子（のちの天智天皇）】と【中臣鎌足（のちの藤原鎌足）】の2人だ！」",
                                "古文 ミオ：「そして改革のスローガンが『豪族の私有をやめさせて【公地公民】にする』！これで法廷の準備はバッチリだよ！」"
                            }
                        }
                    }
                }
            };
        }

        public static List<CrossExaminationRound> GetCourtRounds()
        {
            return new List<CrossExaminationRound>
            {
                // 第1尋問：聖徳太子の国内政治
                new CrossExaminationRound
                {
                    roundNumber = 1,
                    themeTitle = "第1尋問：聖徳太子の国内政治について",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "飛鳥の朝廷を牛耳る大豪族",
                    introDialogue = new string[]
                    {
                        "裁判長：「これより、『逆転日本史』飛鳥時代の歴史法廷を開廷する！弁護側・史堂レイ君、準備はいいかね？」",
                        "史堂 レイ：「はい、裁判長！私たちが見つけてきた『正しい歴史史料』で、歪められた飛鳥の歴史をすべて論破してみせます！」",
                        "古文 ミオ：「レイくん、証言台に立つのは飛鳥時代の大豪族【蘇我 馬子（そが の うまこ）】だよ！蘇我氏に都合のいいウソの証言に気をつけて！」",
                        "蘇我 馬子：「フッフッフ…小僧どもめ。この蘇我馬子が語る『飛鳥時代の歴史』に矛盾などない！まずは聖徳太子の国内政治について証言してやろう！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 593年、推古天皇の摂政となった聖徳太子は、この私・蘇我馬子と協力して政治を行ったのだ。",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！推古天皇を助けた摂政が聖徳太子というのは本当ですか？」",
                                "蘇我 馬子：「うむ、女性初の天皇である推古天皇の摂政（せっしょう）となり、私と協力したのは紛れもない史実だ。ここにはウソはないぞ！」",
                                "古文 ミオ：「レイくん、この1つ目の証言は史実通りみたい。『次の証言 ▶』で2つ目・3つ目の証言を調べよう！」"
                            },
                            validEvidenceIds = new List<string>()
                        },
                        new TestimonyStatement
                        {
                            text = "② 『冠位十二階』という制度では、個人の能力など無視して【生まれた家柄が高い大豪族の息子だけ】に高い冠を授けたのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に『生まれた家柄だけ』で冠を授けたんですか！？」",
                                "蘇我 馬子：「当たり前だ！能力など関係ない！我が蘇我氏のような名門の家柄だけが栄える不公平な制度こそ『冠位十二階』なのだ！」",
                                "古文 ミオ：「レイくん、それって朝廷の役人さんから聞いた話と正反対だよ！『法廷記録』から【冠位十二階の制】をつきつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "kan_i_12" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ さらに『十七条の憲法』では、豪族たちが天皇の命令を無視して【自由にケンカや争いをすること】を奨励したのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！憲法なのに豪族同士の争いを奨励したなんて本当ですか！？」",
                                "蘇我 馬子：「もちろんだ！第1条には『力のある豪族はどんどん争って自分の利益を守れ』と書いてあったはずだぞ！」",
                                "古文 ミオ：「そんなわけないわ！『和を以て貴しとなす』と書かれた【十七条の憲法】を『つきつける』でぶつけよう！」"
                            },
                            validEvidenceIds = new List<string> { "kenpo_17" }
                        }
                    },
                    partnerHint = "古文 ミオ：「レイくん！2番目の証言（家柄だけで登用）には『冠位十二階の制』、3番目の証言（争いを奨励）には『十七条の憲法』を【つきつける】だよ！」",
                    breakthroughDialogue = new string[]
                    {
                        "史堂 レイ：「異議あり！！蘇我馬子さん、その証言、史料と食い違っています！」",
                        "蘇我 馬子：「な、なにィィッ！？その手に持っているのは『冠位十二階』と『十七条の憲法』の正本ではないか…！？」",
                        "史堂 レイ：「聖徳太子は、家柄にとらわれず【個人の能力や功績】で役人を登用するため『冠位十二階』を定めました！」",
                        "史堂 レイ：「さらに『十七条の憲法』では第1条に『和を以て貴しとなす』と説き、豪族の争いを抑えて【天皇を中心とした国づくり】を目指したのです！」",
                        "古文 ミオ：「やったあ、レイくん！見事な論破だよ！」",
                        "裁判長：「お見事！第1の証言の矛盾が見事に暴かれたぞ！」"
                    },
                    summaryPoint = "✅ 第1尋問クリア：聖徳太子は『冠位十二階（能力・功績重視）』と『十七条の憲法（和を重んじ天皇中心）』で新しい国づくりを進めた！"
                },

                // 第2尋問：遣隋使と外交
                new CrossExaminationRound
                {
                    roundNumber = 2,
                    themeTitle = "第2尋問：小野妹子と中国への使節について",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "動揺を隠せない飛鳥の大豪族",
                    introDialogue = new string[]
                    {
                        "蘇我 馬子：「くっ…国内政治の記録は見破られたか！だが、西暦607年の『中国への外交使節』についてお前たちに真実は暴けまい！」",
                        "古文 ミオ：「難波津の港で小野妹子本人から聞いた話を思い出して、レイくん！」"
                    },
                    statements = new List<TestimonyStatement>
                    {
                        new TestimonyStatement
                        {
                            text = "① 西暦607年、聖徳太子は小野妹子（おののいもこ）という人物を代表として中国へと派遣した。",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！607年に派遣された人物が小野妹子であることは間違いありませんね？」",
                                "蘇我 馬子：「うむ、小野妹子が海を渡ったのは事実だ。問題は『中国のどの王朝へ』『どんな立場で』行ったかだ！」",
                                "古文 ミオ：「次の2番目・3番目の証言に矛盾が隠れていそうだよ！」"
                            },
                            validEvidenceIds = new List<string>()
                        },
                        new TestimonyStatement
                        {
                            text = "② 607年当時の中国は『唐（とう）』という国だったため、小野妹子は【遣唐使】として派遣されたのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！607年当時の中国は本当に『唐』だったんですか！？」",
                                "蘇我 馬子：「そうに決まっておる！589年に中国を統一して607年に君臨していたのは『唐』だ！」",
                                "古文 ミオ：「レイくん、年表や国書を思い出して！607年当時の中国王朝は『唐』じゃなくて別の一文字の国だったはずよ！」"
                            },
                            validEvidenceIds = new List<string> { "kenzuishi_letter", "east_asia_timeline" }
                        },
                        new TestimonyStatement
                        {
                            text = "③ そして小野妹子が届けた国書には、【日本は中国皇帝の家来になります】と頭を下げる言葉が書かれていたのだ！",
                            pressReactionLines = new string[]
                            {
                                "史堂 レイ：「待った！本当に『家来になります』なんて手紙を送ったんですか！？」",
                                "蘇我 馬子：「うむ！中国の皇帝・煬帝（ようだい）がご機嫌になるほど、ペコペコへりくだった手紙を送ったのだ！」",
                                "古文 ミオ：「大ウソだわ！難波津で小野妹子さんから預かった【遣隋使の国書】をつきつけよう！」"
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

                // 第3尋問：大化の改新（645年）
                new CrossExaminationRound
                {
                    roundNumber = 3,
                    themeTitle = "第3尋問：645年・大化の改新について（最終尋問）",
                    witnessName = "蘇我 馬子",
                    witnessTitle = "絶체絶命の大豪族",
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
