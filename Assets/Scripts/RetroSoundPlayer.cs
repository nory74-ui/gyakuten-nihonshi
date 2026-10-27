using System.Collections.Generic;
using UnityEngine;

namespace AsukaHistoryCourt
{
    /// <summary>
    /// 外部音声ファイル不要で「逆転裁判」風のレトロ効果音＆5種類のBGM（タイトル・OP・現地調査・尋問・追求テーマ）を
    /// マルチトラック波形合成（メロディ＋ベース＋和音＋ドラム）で鳴らすサウンド＆BGMプレイヤー
    /// </summary>
    public class RetroSoundPlayer : MonoBehaviour
    {
        public enum BgmTrack
        {
            None,
            Title,
            Opening,
            Investigation,
            CrossExamination,
            Pursuit,
            Ending
        }

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;

        private AudioClip _blipClip;
        private AudioClip _clickClip;
        private AudioClip _objectionClip;
        private AudioClip _holdItClip;
        private AudioClip _evidenceGetClip;
        private AudioClip _damageClip;
        private AudioClip _warpClip;
        private AudioClip _clearFanfareClip;

        private Dictionary<BgmTrack, AudioClip> _bgmClips = new Dictionary<BgmTrack, AudioClip>();
        private BgmTrack _currentTrack = BgmTrack.None;
        private bool _isBgmMuted = false;

        public bool IsBgmMuted => _isBgmMuted;

        private AudioClip LoadAudioClip(string audioName)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + audioName);
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(audioName);
            }
            return clip;
        }

        public void Initialize()
        {
            AudioSource[] sources = gameObject.GetComponents<AudioSource>();
            if (sources.Length > 0)
            {
                _sfxSource = sources[0];
            }
            else
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
            }
            _sfxSource.playOnAwake = false;

            if (sources.Length > 1)
            {
                _bgmSource = sources[1];
            }
            else
            {
                _bgmSource = gameObject.AddComponent<AudioSource>();
            }
            _bgmSource.playOnAwake = false;
            _bgmSource.loop = true;
            _bgmSource.volume = 0.40f;

            // 高品質効果音のロード（Resources/Audioから読み込み、なければシンセフォールバック）
            _blipClip = LoadAudioClip("se_blip") ?? CreateToneClip("Blip", 0.035f, 880f, 920f, false, 0.16f);
            _clickClip = LoadAudioClip("se_click") ?? CreateToneClip("Click", 0.07f, 520f, 1040f, false, 0.28f);
            _objectionClip = LoadAudioClip("se_objection") ?? CreateObjectionImpactClip();
            _holdItClip = LoadAudioClip("se_holdit") ?? CreateObjectionImpactClip();
            _evidenceGetClip = LoadAudioClip("se_evidence_get") ?? CreateArpeggioClip("EvidenceGet", new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.11f, 0.35f);
            _damageClip = LoadAudioClip("se_damage") ?? CreateToneClip("Damage", 0.32f, 220f, 95f, true, 0.45f);
            _warpClip = LoadAudioClip("se_warp") ?? CreateToneClip("Warp", 0.18f, 340f, 1280f, false, 0.25f);
            _clearFanfareClip = LoadAudioClip("se_clear_fanfare") ?? CreateArpeggioClip("ClearFanfare", new float[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f, 1567.98f }, 0.13f, 0.40f);

            // 全BGMトラックの事前ロード＆合成
            BuildAllBgmClips();
        }

        public void PlayBgm(BgmTrack track)
        {
            if (_bgmSource == null) return;

            if (track == BgmTrack.None)
            {
                _currentTrack = BgmTrack.None;
                _bgmSource.Stop();
                return;
            }

            if (_currentTrack == track && _bgmSource.isPlaying)
            {
                return;
            }

            _currentTrack = track;
            if (_bgmClips.TryGetValue(track, out AudioClip clip) && clip != null)
            {
                _bgmSource.clip = clip;
                _bgmSource.mute = _isBgmMuted;
                // 追求テーマは少し音量を上げて大迫力に
                _bgmSource.volume = (track == BgmTrack.Pursuit) ? 0.48f : 0.40f;
                _bgmSource.Play();
            }
        }

        public void StopBgm()
        {
            PlayBgm(BgmTrack.None);
        }

        public bool ToggleBgmMute()
        {
            _isBgmMuted = !_isBgmMuted;
            if (_bgmSource != null)
            {
                _bgmSource.mute = _isBgmMuted;
            }
            return _isBgmMuted;
        }

        public void PlayBlip()
        {
            if (_sfxSource != null && _blipClip != null)
            {
                _sfxSource.PlayOneShot(_blipClip, 0.45f);
            }
        }

        public void PlayClick()
        {
            if (_sfxSource != null && _clickClip != null)
            {
                _sfxSource.PlayOneShot(_clickClip, 0.8f);
            }
        }

        public void PlayObjection()
        {
            if (_sfxSource != null && _objectionClip != null)
            {
                _sfxSource.PlayOneShot(_objectionClip, 1.0f);
            }
        }

        public void PlayHoldIt()
        {
            if (_sfxSource != null && _holdItClip != null)
            {
                _sfxSource.PlayOneShot(_holdItClip, 1.0f);
            }
        }

        public void PlayEvidenceGet()
        {
            if (_sfxSource != null && _evidenceGetClip != null)
            {
                _sfxSource.PlayOneShot(_evidenceGetClip, 0.9f);
            }
        }

        public void PlayDamage()
        {
            if (_sfxSource != null && _damageClip != null)
            {
                _sfxSource.PlayOneShot(_damageClip, 0.9f);
            }
        }

        public void PlayWarpTick()
        {
            if (_sfxSource != null && _warpClip != null)
            {
                _sfxSource.PlayOneShot(_warpClip, 0.6f);
            }
        }

        public void PlayClearFanfare()
        {
            if (_sfxSource != null && _clearFanfareClip != null)
            {
                _sfxSource.PlayOneShot(_clearFanfareClip, 1.0f);
            }
        }

        // =====================================================================
        // 🎵 和風オーケストラ＆逆転法廷BGMプレイヤー（高品質オーディオロード ＋ フォールバック合成）
        // =====================================================================
        private void BuildAllBgmClips()
        {
            // ① タイトル曲：堂々とした和風・歴史法廷テーマ (BPM 112)
            _bgmClips[BgmTrack.Title] = LoadAudioClip("bgm_title");
            if (_bgmClips[BgmTrack.Title] == null)
            {
                int[] titleMelody = new int[]
                {
                    62, 62, 65, 69,  72, 69, 65, 62,  60, 60, 64, 67,  69, 67, 64, 60,
                    62, 65, 69, 74,  72, 69, 65, 69,  71, 69, 67, 64,  62,  0, 62,  0
                };
                int[] titleBass = new int[]
                {
                    38, 38, 45, 45,  38, 38, 45, 45,  36, 36, 43, 43,  45, 45, 40, 40,
                    38, 38, 45, 45,  41, 41, 48, 48,  43, 43, 36, 36,  38, 45, 38, 38
                };
                _bgmClips[BgmTrack.Title] = SynthesizeLoopBgm("BGM_Title", 116f, titleMelody, titleBass, true, false);
            }

            // ② オープニング曲：時空の渦・ミステリアスな導入 (BPM 128)
            _bgmClips[BgmTrack.Opening] = LoadAudioClip("bgm_opening");
            if (_bgmClips[BgmTrack.Opening] == null)
            {
                int[] opMelody = new int[]
                {
                    69,  0, 72, 69,  76,  0, 72,  0,  67,  0, 71, 67,  74,  0, 71,  0,
                    65, 69, 72, 76,  74, 71, 67, 64,  69,  0, 64, 69,  72, 71, 69,  0
                };
                int[] opBass = new int[]
                {
                    45, 57, 45, 57,  45, 57, 45, 57,  43, 55, 43, 55,  43, 55, 43, 55,
                    41, 53, 41, 53,  40, 52, 40, 52,  45, 52, 45, 57,  45, 52, 45,  0
                };
                _bgmClips[BgmTrack.Opening] = SynthesizeLoopBgm("BGM_Opening", 128f, opMelody, opBass, true, true);
            }

            // ③ 探偵パート（現地調査）：飛鳥の都・和風ヨナ抜き音階テーマ (BPM 96)
            _bgmClips[BgmTrack.Investigation] = LoadAudioClip("bgm_investigation");
            if (_bgmClips[BgmTrack.Investigation] == null)
            {
                int[] invMelody = new int[]
                {
                    62, 64, 67, 69,  71, 69, 67, 64,  62, 64, 67, 71,  74,  0, 71,  0,
                    74, 71, 69, 67,  64, 67, 69, 67,  64, 62, 59, 62,  64, 67, 62,  0
                };
                int[] invBass = new int[]
                {
                    38,  0, 45,  0,  38,  0, 45,  0,  43,  0, 50,  0,  38,  0, 45,  0,
                    43,  0, 50,  0,  40,  0, 47,  0,  38,  0, 43,  0,  38, 45, 38,  0
                };
                _bgmClips[BgmTrack.Investigation] = SynthesizeLoopBgm("BGM_Investigation", 108f, invMelody, invBass, false, false);
            }

            // ④ 法廷パート（尋問〜モデラート〜）：張り詰めた法廷推理テーマ (BPM 116)
            _bgmClips[BgmTrack.CrossExamination] = LoadAudioClip("bgm_crossexam");
            if (_bgmClips[BgmTrack.CrossExamination] == null)
            {
                int[] courtMelody = new int[]
                {
                    60,  0, 63, 60,  67,  0, 65, 63,  62,  0, 65, 62,  67,  0, 62,  0,
                    60,  0, 63, 67,  72, 70, 68, 67,  65, 63, 62, 63,  60,  0, 55,  0
                };
                int[] courtBass = new int[]
                {
                    36, 48, 36, 48,  36, 48, 36, 48,  35, 47, 35, 47,  35, 47, 35, 47,
                    36, 48, 36, 48,  32, 44, 32, 44,  34, 46, 35, 47,  36, 43, 36,  0
                };
                _bgmClips[BgmTrack.CrossExamination] = SynthesizeLoopBgm("BGM_CrossExam", 114f, courtMelody, courtBass, true, false);
            }

            // ⑤ 追求（論破！）テーマ：『異議あり！』正解時に鳴り響く疾走逆転BGM (BPM 156)
            _bgmClips[BgmTrack.Pursuit] = LoadAudioClip("bgm_pursuit");
            if (_bgmClips[BgmTrack.Pursuit] == null)
            {
                int[] pursuitMelody = new int[]
                {
                    64, 64, 67, 71,  76, 74, 71, 67,  69, 69, 72, 76,  74, 71, 69, 67,
                    64, 67, 71, 76,  79, 76, 74, 71,  72, 71, 69, 67,  71, 74, 76,  0
                };
                int[] pursuitBass = new int[]
                {
                    40, 52, 40, 52,  40, 52, 40, 52,  45, 57, 45, 57,  43, 55, 42, 54,
                    40, 52, 40, 52,  43, 55, 43, 55,  45, 57, 47, 59,  40, 52, 40, 52
                };
                _bgmClips[BgmTrack.Pursuit] = SynthesizeLoopBgm("BGM_Pursuit", 154f, pursuitMelody, pursuitBass, true, true);
            }

            // ⑥ エンディング（勝訴・現代帰還）：明るく晴れやかな凱旋テーマ (BPM 120)
            _bgmClips[BgmTrack.Ending] = LoadAudioClip("bgm_ending");
            if (_bgmClips[BgmTrack.Ending] == null)
            {
                int[] endingMelody = new int[]
                {
                    60, 64, 67, 72,  76, 74, 72, 67,  69, 72, 76, 77,  76, 74, 72,  0,
                    67, 71, 74, 79,  76, 72, 69, 65,  67, 72, 71, 74,  72,  0, 72,  0
                };
                int[] endingBass = new int[]
                {
                    36, 48, 43, 48,  36, 48, 43, 48,  41, 53, 45, 53,  43, 55, 36, 48,
                    43, 55, 47, 55,  45, 57, 41, 53,  43, 55, 43, 47,  36, 43, 36,  0
                };
                _bgmClips[BgmTrack.Ending] = SynthesizeLoopBgm("BGM_Ending", 124f, endingMelody, endingBass, true, false);
            }
        }

        private AudioClip SynthesizeLoopBgm(string clipName, float bpm, int[] melodyMidi, int[] bassMidi, bool hasDrums, bool intenseStyle)
        {
            int sampleRate = 22050; // 軽量かつレトロな質感に最適なサンプルレート
            float stepDuration = (60f / bpm) * 0.5f; // 8分音符1ステップ
            int steps = melodyMidi.Length;
            int samplesPerStep = Mathf.Max(1, Mathf.FloorToInt(sampleRate * stepDuration));
            int totalSamples = samplesPerStep * steps;

            float[] samples = new float[totalSamples];
            float melPhase = 0f;
            float subPhase = 0f;
            float bassPhase = 0f;
            System.Random rng = new System.Random(clipName.GetHashCode());

            for (int s = 0; s < steps; s++)
            {
                int melNote = melodyMidi[s];
                int bassNote = bassMidi[s % bassMidi.Length];

                float melFreq = (melNote > 0) ? MidiToFreq(melNote) : 0f;
                float subFreq = (melNote > 0) ? MidiToFreq(melNote - 12) : 0f;
                float bassFreq = (bassNote > 0) ? MidiToFreq(bassNote) : 0f;

                for (int i = 0; i < samplesPerStep; i++)
                {
                    int idx = s * samplesPerStep + i;
                    float localT = (float)i / samplesPerStep;

                    // ① メロディ（温かみのある倍音サイン＋矩形ブレンド）
                    float melSignal = 0f;
                    if (melFreq > 0f)
                    {
                        melPhase += melFreq / sampleRate;
                        subPhase += subFreq / sampleRate;
                        float sinWave = Mathf.Sin(melPhase * Mathf.PI * 2f);
                        float sqWave = sinWave >= 0f ? 0.5f : -0.5f;
                        float subWave = Mathf.Sin(subPhase * Mathf.PI * 2f) * 0.25f;
                        float env = Mathf.Clamp01((1f - localT * 0.65f) * 1.1f);
                        melSignal = (sinWave * 0.65f + sqWave * (intenseStyle ? 0.45f : 0.22f) + subWave) * env * 0.36f;
                    }

                    // ② ベースライン（太い三角波＋サイン波）
                    float bassSignal = 0f;
                    if (bassFreq > 0f)
                    {
                        bassPhase += bassFreq / sampleRate;
                        float bSin = Mathf.Sin(bassPhase * Mathf.PI * 2f);
                        float bEnv = 1f - localT * 0.55f;
                        bassSignal = bSin * bEnv * 0.34f;
                    }

                    // ③ ドラム・パーカッション（キック＆ハイハット）
                    float drumSignal = 0f;
                    if (hasDrums)
                    {
                        // 偶数ステップでキック、奇数ステップでハイハット
                        if (s % 2 == 0 && localT < 0.35f)
                        {
                            float kickFreq = Mathf.Lerp(130f, 42f, localT / 0.35f);
                            float kickEnv = 1f - (localT / 0.35f);
                            drumSignal += Mathf.Sin(localT * kickFreq * Mathf.PI * 2f) * kickEnv * (intenseStyle ? 0.28f : 0.18f);
                        }
                        if ((s % 2 == 1 || intenseStyle) && localT < 0.20f)
                        {
                            float noise = (float)rng.NextDouble() * 2f - 1f;
                            float hatEnv = 1f - (localT / 0.20f);
                            drumSignal += noise * hatEnv * (intenseStyle ? 0.12f : 0.07f);
                        }
                    }

                    samples[idx] = Mathf.Clamp(melSignal + bassSignal + drumSignal, -0.95f, 0.95f);
                }
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float MidiToFreq(int midiNote)
        {
            return 440f * Mathf.Pow(2f, (midiNote - 69f) / 12f);
        }

        // =====================================================================
        // 効果音合成ヘルパー
        // =====================================================================
        private AudioClip CreateToneClip(string clipName, float duration, float startFreq, float endFreq, bool addNoise, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.FloorToInt(sampleRate * duration));
            float[] samples = new float[sampleCount];
            float phase = 0f;
            System.Random rng = new System.Random(12345);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += freq / sampleRate;
                float wave = Mathf.Sin(phase * Mathf.PI * 2f) >= 0f ? 0.7f : -0.7f;
                if (addNoise)
                {
                    float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.5f;
                    wave = Mathf.Lerp(wave, noise, 0.45f);
                }
                float envelope = 1f - t;
                samples[i] = wave * envelope * volume;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateArpeggioClip(string clipName, float[] notes, float noteDuration, float volume)
        {
            int sampleRate = 44100;
            int samplesPerNote = Mathf.FloorToInt(sampleRate * noteDuration);
            int totalSamples = samplesPerNote * notes.Length;
            float[] samples = new float[totalSamples];
            float phase = 0f;

            for (int n = 0; n < notes.Length; n++)
            {
                float freq = notes[n];
                for (int i = 0; i < samplesPerNote; i++)
                {
                    int idx = n * samplesPerNote + i;
                    float localT = (float)i / samplesPerNote;
                    phase += freq / sampleRate;
                    float wave = Mathf.Sin(phase * Mathf.PI * 2f) * 0.7f + (Mathf.Sin(phase * Mathf.PI * 4f) * 0.3f);
                    float env = Mathf.Sin((1f - localT * 0.7f) * Mathf.PI * 0.5f);
                    samples[idx] = wave * env * volume;
                }
            }

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip CreateObjectionImpactClip()
        {
            int sampleRate = 44100;
            float duration = 0.42f;
            int sampleCount = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            System.Random rng = new System.Random(999);
            float phase1 = 0f;
            float phase2 = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 14f);
                phase1 += 440f / sampleRate;
                phase2 += 880f / sampleRate;
                float chord = (Mathf.Sin(phase1 * Mathf.PI * 2f) + Mathf.Sin(phase2 * Mathf.PI * 2f)) * 0.5f;
                float env = Mathf.Exp(-t * 4.5f);
                samples[i] = Mathf.Clamp((noise * 0.65f + chord * env * 0.65f) * 0.6f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("ObjectionImpact", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
