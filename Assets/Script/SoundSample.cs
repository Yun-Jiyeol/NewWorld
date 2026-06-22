using UnityEngine;

public static class ScreenTypingDotSoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 선택된 Tape Typing 계열 5개
    //
    // Dot1 = 기존 03 CleanReadDot
    // Dot2 = 기존 04 OldTapeGrain
    // Dot3 = 기존 05 LowRecordPrint
    // Dot4 = 기존 07 ReadThenGlow
    // Dot5 = 기존 09 DryRecordTick
    //
    // 각 DotPitch 값으로 내부 음 높이 조절 가능
    // =========================================================

    [Header("Pitch")]
    public static float Dot1Pitch = 1f;
    public static float Dot2Pitch = 1f;
    public static float Dot3Pitch = 1f;
    public static float Dot4Pitch = 1f;
    public static float Dot5Pitch = 1f;

    // =========================================================
    // dot1
    // 짧게 읽고 바로 출력되는 깔끔한 느낌
    // =========================================================

    public static AudioClip CreateDot1()
    {
        string name = "dot1";
        float duration = 0.052f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float pitch = Mathf.Max(0.01f, Dot1Pitch);

        float readNoise = 0f;
        float phase = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float readEnv = Mathf.Exp(-Mathf.Pow(t - 0.20f, 2f) * 90f);
            float dotEnv = SharpEnvelope(t, 0.08f, 9.5f);

            float raw = Random.Range(-1f, 1f);
            readNoise = Mathf.Lerp(readNoise, raw, 0.065f);

            float read = readNoise * 0.055f * readEnv;

            float dot =
                Mathf.Sin(2f * Mathf.PI * 690f * pitch * time + phase) *
                0.12f *
                dotEnv;

            samples[i] = read + dot;
        }

        Finish(samples, 0.115f, 18);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // dot2
    // 낡은 테이프 노이즈가 살짝 섞인 출력
    // =========================================================

    public static AudioClip CreateDot2()
    {
        string name = "dot2";
        float duration = 0.086f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float pitch = Mathf.Max(0.01f, Dot2Pitch);

        float grainA = 0f;
        float grainB = 0f;
        float phase = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float grainEnv =
                Mathf.Sin(t * Mathf.PI) *
                Mathf.Pow(1f - t, 2.2f);

            float printEnv = SharpEnvelope(t, 0.13f, 6.4f);

            float rawA = Random.Range(-1f, 1f);
            float rawB = Random.Range(-1f, 1f);

            grainA = Mathf.Lerp(grainA, rawA, 0.045f);
            grainB = Mathf.Lerp(grainB, rawB, 0.18f);

            float oldGrain =
                grainA * 0.075f * grainEnv +
                grainB * 0.022f * Mathf.Pow(1f - t, 5f);

            float print =
                Mathf.Sin(2f * Mathf.PI * 575f * pitch * time + phase) *
                0.125f *
                printEnv;

            samples[i] = oldGrain + print;
        }

        Finish(samples, 0.13f, 30);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // dot3
    // 기록 장치가 짧게 읽고 낮게 찍는 느낌
    // =========================================================

    public static AudioClip CreateDot3()
    {
        string name = "dot3";
        float duration = 0.082f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float pitch = Mathf.Max(0.01f, Dot3Pitch);

        float read = 0f;
        float phaseA = Random.Range(0f, 1000f);
        float phaseB = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float readEnv =
                Mathf.Exp(-Mathf.Pow(t - 0.26f, 2f) * 42f) *
                Mathf.Pow(1f - t, 1.7f);

            float bodyEnv = SharpEnvelope(t, 0.12f, 5.8f);

            float raw = Random.Range(-1f, 1f);
            read = Mathf.Lerp(read, raw, 0.055f);

            float readPart = read * 0.080f * readEnv;

            float body =
                Mathf.Sin(2f * Mathf.PI * 420f * pitch * time + phaseA) *
                0.105f *
                bodyEnv;

            float print =
                Mathf.Sin(2f * Mathf.PI * 710f * pitch * time + phaseB) *
                0.050f *
                Mathf.Pow(1f - t, 8f);

            samples[i] = readPart + body + print;
        }

        Finish(samples, 0.125f, 28);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // dot4
    // 테이프 읽힘 뒤에 아주 작은 화면 점등이 붙는 느낌
    // =========================================================

    public static AudioClip CreateDot4()
    {
        string name = "dot4";
        float duration = 0.090f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float pitch = Mathf.Max(0.01f, Dot4Pitch);

        float readNoise = 0f;
        float phaseA = Random.Range(0f, 1000f);
        float phaseB = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float readT = Mathf.Clamp01(t / 0.62f);

            float readEnv =
                Mathf.Sin(readT * Mathf.PI) *
                Mathf.Pow(1f - t, 2.1f);

            float glowEnv =
                Mathf.Exp(-Mathf.Pow(t - 0.56f, 2f) * 90f) *
                Mathf.Pow(1f - t, 1.5f);

            float raw = Random.Range(-1f, 1f);
            readNoise = Mathf.Lerp(readNoise, raw, 0.070f);

            float read = readNoise * 0.080f * readEnv;

            float print =
                Mathf.Sin(2f * Mathf.PI * 610f * pitch * time + phaseA) *
                0.095f *
                SharpEnvelope(t, 0.10f, 7.0f);

            float glow =
                Mathf.Sin(2f * Mathf.PI * 1150f * pitch * time + phaseB) *
                0.040f *
                glowEnv;

            samples[i] = read + print + glow;
        }

        Finish(samples, 0.125f, 30);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // dot5
    // 조금 더 건조한 기록 출력음
    // =========================================================

    public static AudioClip CreateDot5()
    {
        string name = "dot5";
        float duration = 0.048f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float pitch = Mathf.Max(0.01f, Dot5Pitch);

        float grain = 0f;
        float phaseA = Random.Range(0f, 1000f);
        float phaseB = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float raw = Random.Range(-1f, 1f);
            grain = Mathf.Lerp(grain, raw, 0.14f);

            float grainEnv = Mathf.Pow(1f - t, 5.5f);
            float tickEnv = SharpEnvelope(t, 0.08f, 9.0f);

            float dryGrain = grain * 0.045f * grainEnv;

            float body =
                Mathf.Sin(2f * Mathf.PI * 530f * pitch * time + phaseA) *
                0.10f *
                tickEnv;

            float edge =
                Mathf.Sin(2f * Mathf.PI * 920f * pitch * time + phaseB) *
                0.035f *
                Mathf.Pow(1f - t, 11f);

            samples[i] = dryGrain + body + edge;
        }

        Finish(samples, 0.115f, 16);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // 공용 유틸
    // =========================================================

    private static float SharpEnvelope(float t, float attackRatio, float decayPower)
    {
        if (t < attackRatio)
        {
            float attackT = t / attackRatio;
            return Mathf.SmoothStep(0f, 1f, attackT);
        }

        float decayT = (t - attackRatio) / (1f - attackRatio);
        return Mathf.Pow(1f - decayT, decayPower);
    }

    private static void Finish(float[] samples, float targetPeak, int fadeSamples)
    {
        RemoveClick(samples, fadeSamples);
        NormalizeSamples(samples, targetPeak);
    }

    private static void RemoveClick(float[] samples, int maxFadeSampleCount)
    {
        int fadeSampleCount = Mathf.Min(maxFadeSampleCount, samples.Length / 6);

        for (int i = 0; i < fadeSampleCount; i++)
        {
            float t = i / (float)fadeSampleCount;

            samples[i] *= t;
            samples[samples.Length - 1 - i] *= t;
        }
    }

    private static void NormalizeSamples(float[] samples, float targetPeak)
    {
        float max = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Mathf.Abs(samples[i]);

            if (abs > max)
                max = abs;
        }

        if (max <= 0f)
            return;

        float multiplier = targetPeak / max;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= multiplier;
        }
    }
}

public static class DrawerSoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 공통 수치
    // =========================================================

    public static float StartDelay = 0.02f;
    public static float SustainTime = 0.62f;
    public static float EndDelay = 0.03f;

    public static float PitchScale = 1.2f;
    public static float TargetPeak = 0.34f;

    // =========================================================
    // 열림 수치
    // =========================================================

    public static float OpenSlideNoiseAmount = 0.28f;

    public static float OpenLowBodyFreq = 52f;
    public static float OpenLowBodyAmount = 0.38f;

    public static float OpenMidBodyFreq = 105f;
    public static float OpenMidBodyAmount = 0.20f;

    public static float OpenRailFreq = 210f;
    public static float OpenRailAmount = 0.045f;

    public static float OpenRattleAmount = 0.025f;

    public static float OpenStartBumpAmount = 0.23f;
    public static float OpenEndBumpAmount = 0.11f;

    public static float OpenRoughness = 0.025f;

    public static float OpenStartBumpDecay = 36f;
    public static float OpenEndBumpDecay = 48f;

    // =========================================================
    // 닫힘 수치
    // 마지막 둔탁한 충격음 제거 버전
    // =========================================================

    public static float CloseSlideNoiseAmount = 0.20f;

    public static float CloseLowBodyFreq = 58f;
    public static float CloseLowBodyAmount = 0.34f;

    public static float CloseMidBodyFreq = 118f;
    public static float CloseMidBodyAmount = 0.18f;

    public static float CloseRailFreq = 230f;
    public static float CloseRailAmount = 0.05f;

    public static float CloseRattleAmount = 0.03f;

    public static float CloseStartBumpAmount = 0.08f;

    public static float CloseRoughness = 0.03f;

    public static float CloseStartBumpDecay = 28f;

    // 닫힘 끝부분을 부드럽게 사라지게 하는 값
    public static float CloseFadeOutPower = 2.4f;

    // =========================================================
    // 생성 함수
    // =========================================================

    public static AudioClip CreateDrawerOpen()
    {
        return CreateDrawerMoveSound(
            name: "Drawer_Open_Heavy_Low_Pull",
            isClose: false,
            startDelay: StartDelay,
            sustainTime: SustainTime,
            endDelay: EndDelay,
            pitchScale: PitchScale,
            targetPeak: TargetPeak,

            slideNoiseAmount: OpenSlideNoiseAmount,

            lowBodyFreq: OpenLowBodyFreq,
            lowBodyAmount: OpenLowBodyAmount,

            midBodyFreq: OpenMidBodyFreq,
            midBodyAmount: OpenMidBodyAmount,

            railFreq: OpenRailFreq,
            railAmount: OpenRailAmount,

            rattleAmount: OpenRattleAmount,

            startBumpAmount: OpenStartBumpAmount,
            endBumpAmount: OpenEndBumpAmount,

            roughness: OpenRoughness,

            startBumpDecay: OpenStartBumpDecay,
            endBumpDecay: OpenEndBumpDecay
        );
    }

    public static AudioClip CreateDrawerClose()
    {
        return CreateDrawerMoveSound(
            name: "Drawer_Close_No_End_Bump",
            isClose: true,
            startDelay: StartDelay,
            sustainTime: SustainTime * 0.82f,
            endDelay: EndDelay,
            pitchScale: PitchScale,
            targetPeak: TargetPeak,

            slideNoiseAmount: CloseSlideNoiseAmount,

            lowBodyFreq: CloseLowBodyFreq,
            lowBodyAmount: CloseLowBodyAmount,

            midBodyFreq: CloseMidBodyFreq,
            midBodyAmount: CloseMidBodyAmount,

            railFreq: CloseRailFreq,
            railAmount: CloseRailAmount,

            rattleAmount: CloseRattleAmount,

            startBumpAmount: CloseStartBumpAmount,

            // 닫힘의 마지막 둔탁한 소리를 제거
            endBumpAmount: 0f,

            roughness: CloseRoughness,

            startBumpDecay: CloseStartBumpDecay,
            endBumpDecay: 1f
        );
    }

    // =========================================================
    // 내부 생성 함수
    // =========================================================

    private static AudioClip CreateDrawerMoveSound(
        string name,
        bool isClose,
        float startDelay,
        float sustainTime,
        float endDelay,
        float pitchScale,
        float targetPeak,

        float slideNoiseAmount,

        float lowBodyFreq,
        float lowBodyAmount,

        float midBodyFreq,
        float midBodyAmount,

        float railFreq,
        float railAmount,

        float rattleAmount,

        float startBumpAmount,
        float endBumpAmount,

        float roughness,

        float startBumpDecay,
        float endBumpDecay
    )
    {
        float totalDuration = startDelay + sustainTime + endDelay;
        int totalSampleCount = Mathf.CeilToInt(SampleRate * totalDuration);

        float[] samples = new float[totalSampleCount];

        int startDelaySamples = Mathf.CeilToInt(SampleRate * startDelay);
        int sustainSamples = Mathf.CeilToInt(SampleRate * sustainTime);

        float smoothNoise = 0f;
        float lowFilteredNoise = 0f;
        float rattleNoise = 0f;
        float phaseOffset = Random.Range(0f, 1000f);

        for (int i = 0; i < sustainSamples; i++)
        {
            int sampleIndex = startDelaySamples + i;

            if (sampleIndex < 0 || sampleIndex >= samples.Length)
                continue;

            float t = i / (float)sustainSamples;
            float time = i / (float)SampleRate;

            float slideEnvelope = Mathf.Sin(t * Mathf.PI);
            slideEnvelope = Mathf.Pow(slideEnvelope, isClose ? 0.58f : 0.72f);

            if (isClose)
            {
                float closePush = Mathf.Lerp(0.75f, 1.05f, t);
                slideEnvelope *= closePush;

                // 닫힘 끝부분에서 충격 없이 자연스럽게 줄어들게 함
                float closeFade = Mathf.Pow(1f - t, CloseFadeOutPower);
                slideEnvelope *= Mathf.Lerp(1f, closeFade, t);
            }

            // 시작 충격
            float startBumpEnvelope = Mathf.Exp(-t * startBumpDecay);

            // 끝 충격
            float endT = 1f - t;
            float endBumpEnvelope = Mathf.Exp(-endT * endBumpDecay);

            float rawNoise = Random.Range(-1f, 1f);
            smoothNoise = Mathf.Lerp(smoothNoise, rawNoise, 0.035f + roughness);
            lowFilteredNoise = Mathf.Lerp(lowFilteredNoise, smoothNoise, 0.045f);

            float slideNoise =
                lowFilteredNoise *
                slideNoiseAmount *
                slideEnvelope;

            float lowBody =
                Mathf.Sin(2f * Mathf.PI * lowBodyFreq * pitchScale * time) *
                lowBodyAmount *
                slideEnvelope;

            float midBody =
                Mathf.Sin(2f * Mathf.PI * midBodyFreq * pitchScale * time + phaseOffset) *
                midBodyAmount *
                slideEnvelope;

            float railTone =
                Mathf.Sin(2f * Mathf.PI * railFreq * pitchScale * time + phaseOffset) *
                railAmount *
                slideEnvelope;

            float rawRattle = Random.Range(-1f, 1f);
            rattleNoise = Mathf.Lerp(rattleNoise, rawRattle, 0.09f);

            float rattleGate = 0f;
            float rattleRate = isClose ? 11f : 9f;
            float rattleWave = Mathf.Sin(2f * Mathf.PI * rattleRate * time + phaseOffset);

            if (rattleWave > 0.80f)
            {
                rattleGate = 1f;
            }

            // 닫힘 끝부분에서 덜걱거림도 사라지게 함
            if (isClose && t > 0.75f)
            {
                rattleGate = 0f;
            }

            float rattle =
                rattleNoise *
                rattleAmount *
                rattleGate *
                slideEnvelope;

            float startBump =
                Mathf.Sin(2f * Mathf.PI * 95f * pitchScale * time) *
                startBumpAmount *
                startBumpEnvelope;

            // 열림에는 끝 충격 유지
            // 닫힘에는 endBumpAmount가 0이라서 마지막 둔탁한 소리 없음
            float endBump =
                Mathf.Sin(2f * Mathf.PI * 76f * pitchScale * time) *
                endBumpAmount *
                endBumpEnvelope;

            float value =
                slideNoise +
                lowBody +
                midBody +
                railTone +
                rattle +
                startBump +
                endBump;

            samples[sampleIndex] = value;
        }

        RemoveClick(samples);
        NormalizeSamples(samples, targetPeak);

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    // =========================================================
    // 유틸
    // =========================================================

    private static void RemoveClick(float[] samples)
    {
        int fadeSampleCount = Mathf.Min(160, samples.Length / 8);

        for (int i = 0; i < fadeSampleCount; i++)
        {
            float t = i / (float)fadeSampleCount;

            samples[i] *= t;
            samples[samples.Length - 1 - i] *= t;
        }
    }

    private static void NormalizeSamples(float[] samples, float targetPeak)
    {
        float max = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Mathf.Abs(samples[i]);

            if (abs > max)
            {
                max = abs;
            }
        }

        if (max <= 0f)
            return;

        float multiplier = targetPeak / max;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= multiplier;
        }
    }
}

public static class SoundChoose
{
    public static AudioClip CallAudio(MouseOnSound _MouseSound)
    {
        switch (_MouseSound)
        {
            case MouseOnSound.DigitalSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            case MouseOnSound.ResetSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            case MouseOnSound.NameSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            case MouseOnSound.IconSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            default: return null;
        }
    }

    public static AudioClip CallAudio(ClickSound _ClickSound)
    {
        switch (_ClickSound)
        {
            case ClickSound.DigitalSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            case ClickSound.NameSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            case ClickSound.IconSound:
                return ScreenTypingDotSoundFactory.CreateDot2();

            default: return null;
        }
    }
}

public static class CassetteLiftClackSoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 5번 샘플 기반
    // 깔끔한 카세트 딸깍 + 순간 피치 상승 조절 가능
    // =========================================================

    public static AudioClip CreateCassetteLiftClack_05_ClearCassetteClack()
    {
        string name = "CassetteLiftClack_05_Clear_Cassette_Clack";
        float duration = 0.076f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        AddSinglePlasticClack(
            samples,
            startTime: 0.010f,
            length: 0.038f,

            bodyFreq: 135f,
            bodyAmount: 0.16f,

            clackFreq: 500f,
            clackAmount: 0.26f,

            edgeFreq: 690f,
            edgeAmount: 0.040f,

            bodyDecayPower: 3.9f,
            clackDecayPower: 9.0f,

            pitchRiseStart: 0.1f,
            pitchRiseEnd: 0.25f,
            pitchRiseAmount: 0.16f
        );

        Finish(samples, 0.20f, 22);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // 조절용 버전
    // 필요하면 다른 코드에서 직접 수치 넣어서 만들 수 있음
    // =========================================================

    public static AudioClip CreateCassetteLiftClack_05_CustomPitchRise(
        float pitchRiseStart,
        float pitchRiseEnd,
        float pitchRiseAmount
    )
    {
        string name = "CassetteLiftClack_05_Custom_Pitch_Rise";
        float duration = 0.076f;
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        AddSinglePlasticClack(
            samples,
            startTime: 0.010f,
            length: 0.038f,

            bodyFreq: 135f,
            bodyAmount: 0.16f,

            clackFreq: 500f,
            clackAmount: 0.26f,

            edgeFreq: 690f,
            edgeAmount: 0.040f,

            bodyDecayPower: 3.9f,
            clackDecayPower: 9.0f,

            pitchRiseStart: pitchRiseStart,
            pitchRiseEnd: pitchRiseEnd,
            pitchRiseAmount: pitchRiseAmount
        );

        Finish(samples, 0.20f, 22);

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    // =========================================================
    // 한 번만 나는 플라스틱 카세트 딸깍
    // =========================================================

    private static void AddSinglePlasticClack(
        float[] samples,
        float startTime,
        float length,

        float bodyFreq,
        float bodyAmount,

        float clackFreq,
        float clackAmount,

        float edgeFreq,
        float edgeAmount,

        float bodyDecayPower,
        float clackDecayPower,

        float pitchRiseStart,
        float pitchRiseEnd,
        float pitchRiseAmount
    )
    {
        int startIndex = Mathf.FloorToInt(startTime * SampleRate);
        int count = Mathf.CeilToInt(length * SampleRate);

        float bodyPhase = 0.41f;
        float clackPhase = 2.15f;
        float edgePhase = 0.88f;

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;

            if (index < 0 || index >= samples.Length)
                continue;

            float t = i / (float)count;
            float time = i / (float)SampleRate;

            float pitchMultiplier = GetPitchRiseMultiplier(
                t,
                pitchRiseStart,
                pitchRiseEnd,
                pitchRiseAmount
            );

            float attack = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.055f));

            float bodyEnvelope =
                attack *
                Mathf.Pow(1f - t, bodyDecayPower);

            float clackEnvelope =
                attack *
                Mathf.Pow(1f - t, clackDecayPower);

            float body =
                Mathf.Sin(
                    2f * Mathf.PI * bodyFreq * pitchMultiplier * time + bodyPhase
                ) *
                bodyAmount *
                bodyEnvelope;

            float clack =
                Mathf.Sin(
                    2f * Mathf.PI * clackFreq * pitchMultiplier * time + clackPhase
                ) *
                clackAmount *
                clackEnvelope;

            float edge =
                Mathf.Sin(
                    2f * Mathf.PI * edgeFreq * pitchMultiplier * time + edgePhase
                ) *
                edgeAmount *
                Mathf.Pow(1f - t, clackDecayPower + 3.5f);

            samples[index] += body + clack + edge;
        }
    }

    // =========================================================
    // 순간 피치 상승 조절 함수
    // =========================================================

    private static float GetPitchRiseMultiplier(
        float t,
        float riseStart,
        float riseEnd,
        float riseAmount
    )
    {
        if (riseAmount <= 0f)
            return 1f;

        if (riseEnd <= riseStart)
            return 1f;

        if (t < riseStart || t > riseEnd)
            return 1f;

        float riseT = Mathf.InverseLerp(riseStart, riseEnd, t);

        // 가운데에서 가장 높아지고 다시 내려오는 형태
        float curve = Mathf.Sin(riseT * Mathf.PI);

        return 1f + curve * riseAmount;
    }

    private static void Finish(float[] samples, float targetPeak, int fadeSamples)
    {
        RemoveClick(samples, fadeSamples);
        NormalizeSamples(samples, targetPeak);
    }

    private static void RemoveClick(float[] samples, int maxFadeSampleCount)
    {
        int fadeSampleCount = Mathf.Min(maxFadeSampleCount, samples.Length / 10);

        for (int i = 0; i < fadeSampleCount; i++)
        {
            float t = i / (float)fadeSampleCount;

            samples[i] *= t;
            samples[samples.Length - 1 - i] *= t;
        }
    }

    private static void NormalizeSamples(float[] samples, float targetPeak)
    {
        float max = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            float abs = Mathf.Abs(samples[i]);

            if (abs > max)
                max = abs;
        }

        if (max <= 0f)
            return;

        float multiplier = targetPeak / max;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] *= multiplier;
        }
    }
}