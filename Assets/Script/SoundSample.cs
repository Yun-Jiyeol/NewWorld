using UnityEngine;

public static class ScreenTextFinalSoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 최종 글자 생성 사운드 - 깔끔한 단타 버전
    //
    // 느낌:
    // - 뒤에 끄는 느낌 제거
    // - 글자가 화면에 딱 찍히는 짧은 전기 점등음
    // - 글자 1개마다 재생해도 부담 적게
    // =========================================================

    public static AudioClip CreateFinalScreenTextAppear()
    {
        string name = "Final_Screen_Text_Appear_Clean_Click";

        float duration = 0.007f;
        float targetPeak = 0.105f;

        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            // 거의 즉시 켜지고 바로 꺼지는 형태
            float envelope = CreateCleanClickEnvelope(t);

            // 화면에 글자가 딱 점등되는 중심음
            float phosphorClick = Mathf.Sin(2f * Mathf.PI * 1650f * time) * 0.34f;

            // 작고 건조한 전기 점
            float electricDot = Mathf.Sin(2f * Mathf.PI * 5200f * time) * 0.11f;

            // 약간 각진 디지털 클릭감
            float digitalEdge = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 2800f * time)) * 0.055f;

            // 아주 짧은 입자감만 남김
            float grain = Random.Range(-1f, 1f) * 0.025f;

            float value =
                phosphorClick +
                electricDot +
                digitalEdge +
                grain;

            samples[i] = value * envelope;
        }

        RemoveClick(samples);
        NormalizeSamples(samples, targetPeak);

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private static float CreateCleanClickEnvelope(float t)
    {
        float attackRatio = 0.08f;

        if (t < attackRatio)
        {
            float attackT = t / attackRatio;
            return Mathf.SmoothStep(0f, 1f, attackT);
        }

        float decayT = (t - attackRatio) / (1f - attackRatio);

        // 숫자가 클수록 뒤가 더 빨리 사라짐
        return Mathf.Pow(1f - decayT, 12f);
    }

    private static void RemoveClick(float[] samples)
    {
        int fadeSampleCount = Mathf.Min(12, samples.Length / 5);

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

public static class WireConnect03SoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 3번 샘플
    // 낡은 단자가 맞물리는 소리
    // 약간 낮은 금속 접점 + 전기 스파크
    // =========================================================
    public static AudioClip CreateWireConnect03()
    {
        string name = "WireConnect_03_Old_Terminal_Contact";

        float duration = 0.034f;
        float contactFreq = 1250f;
        float sparkFreq = 4100f;
        float bodyFreq = 280f;

        float contactAmount = 0.30f;
        float sparkAmount = 0.17f;
        float bodyAmount = 0.16f;
        float noiseAmount = 0.12f;
        float crackleAmount = 0.06f;

        float attackRatio = 0.06f;
        float decayPower = 7.2f;
        float targetPeak = 0.17f;

        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float lastNoise = 0f;
        float sparkPhase = Random.Range(0f, 1000f);
        float contactPhase = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float envelope = CreateContactEnvelope(t, attackRatio, decayPower);

            // 단자가 닿는 중심 접점음
            float contactTone = Mathf.Sin(
                2f * Mathf.PI * contactFreq * time + contactPhase
            ) * contactAmount;

            // 전기가 붙는 순간의 고주파 스파크
            float sparkTone = Mathf.Sin(
                2f * Mathf.PI * sparkFreq * time + sparkPhase
            ) * sparkAmount;

            // 살짝 각진 전기 접촉감
            float squareSpark = Mathf.Sign(
                Mathf.Sin(2f * Mathf.PI * sparkFreq * 0.5f * time)
            ) * sparkAmount * 0.35f;

            // 낮은 단자 몸통 울림
            float bodyTone = Mathf.Sin(2f * Mathf.PI * bodyFreq * time) * bodyAmount;

            // 부드러운 전기 노이즈
            float rawNoise = Random.Range(-1f, 1f);
            lastNoise = Mathf.Lerp(lastNoise, rawNoise, 0.32f);
            float electricNoise = lastNoise * noiseAmount;

            // 초반에만 아주 약한 스파크 입자
            float earlySparkGate = t < 0.28f ? 1f : 0f;
            float randomSpark = 0f;

            if (Random.value > 0.88f)
            {
                randomSpark = Random.Range(-1f, 1f) * crackleAmount * earlySparkGate;
            }

            // 뒤로 갈수록 고주파를 빨리 죽여서 질질 끌리지 않게
            float sparkEnvelope = Mathf.Pow(1f - t, decayPower + 3f);
            float bodyEnvelope = Mathf.Pow(1f - t, decayPower * 0.7f);

            float value =
                contactTone * envelope +
                sparkTone * sparkEnvelope +
                squareSpark * sparkEnvelope +
                bodyTone * bodyEnvelope +
                electricNoise * envelope +
                randomSpark;

            samples[i] = value;
        }

        RemoveClick(samples);
        NormalizeSamples(samples, targetPeak);

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private static float CreateContactEnvelope(float t, float attackRatio, float decayPower)
    {
        if (t < attackRatio)
        {
            float attackT = t / attackRatio;
            return Mathf.SmoothStep(0f, 1f, attackT);
        }

        float decayT = (t - attackRatio) / (1f - attackRatio);
        return Mathf.Pow(1f - decayT, decayPower);
    }

    private static void RemoveClick(float[] samples)
    {
        int fadeSampleCount = Mathf.Min(16, samples.Length / 5);

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
                return DigitalWindowSoundFactory.CreateButtonClick_03_LowPanelTap();

            case MouseOnSound.ResetSound:
                return DigitalWindowSoundFactory.CreateButtonClick_01_SoftDataTap();

            case MouseOnSound.NameSound:
                return DigitalWindowSoundFactory.CreateWindowOn_01_ShortSystemPop();

            case MouseOnSound.IconSound:
                return DigitalWindowSoundFactory.CreateButtonClick_05_OldWindowSelect();

            default: return null;
        }
    }

    public static AudioClip CallAudio(ClickSound _ClickSound)
    {
        switch (_ClickSound)
        {
            case ClickSound.DigitalSound:
                return DigitalWindowSoundFactory.CreateButtonClick_05_OldWindowSelect();

            case ClickSound.NameSound:
                return DigitalWindowSoundFactory.CreateWindowOn_02_SoftWindowEnable();

            case ClickSound.IconSound:
                return DigitalWindowSoundFactory.CreateWindowOn_05_SelectOpenPop();

            default: return null;
        }
    }
}

public static class DigitalWindowSoundFactory
{
    private const int SampleRate = 44100;

    // =========================================================
    // 버튼 클릭 사운드 5개
    // SoftScreenTap + DataSelectClick 느낌 기반
    // =========================================================

    // 1. 부드러운 디지털 터치
    public static AudioClip CreateButtonClick_01_SoftDataTap()
    {
        return CreateDigitalButtonClick(
            name: "ButtonClick_01_Soft_Data_Tap",
            duration: 0.050f,
            mainFreq: 980f,
            highFreq: 3000f,
            lowFreq: 240f,
            mainAmount: 0.23f,
            highAmount: 0.045f,
            lowAmount: 0.055f,
            noiseAmount: 0.022f,
            squareAmount: 0.045f,
            decayPower: 6.4f,
            targetPeak: 0.16f
        );
    }

    // 3. 낮고 부드러운 패널 클릭
    public static AudioClip CreateButtonClick_03_LowPanelTap()
    {
        return CreateDigitalButtonClick(
            name: "ButtonClick_03_Low_Panel_Tap",
            duration: 0.058f,
            mainFreq: 780f,
            highFreq: 2500f,
            lowFreq: 210f,
            mainAmount: 0.22f,
            highAmount: 0.035f,
            lowAmount: 0.075f,
            noiseAmount: 0.026f,
            squareAmount: 0.035f,
            decayPower: 5.9f,
            targetPeak: 0.16f
        );
    }

    // 5. 구식 창 선택 클릭
    public static AudioClip CreateButtonClick_05_OldWindowSelect()
    {
        return CreateDigitalButtonClick(
            name: "ButtonClick_05_Old_Window_Select",
            duration: 0.064f,
            mainFreq: 860f,
            highFreq: 2900f,
            lowFreq: 230f,
            mainAmount: 0.24f,
            highAmount: 0.050f,
            lowAmount: 0.070f,
            noiseAmount: 0.038f,
            squareAmount: 0.055f,
            decayPower: 5.6f,
            targetPeak: 0.17f
        );
    }

    // =========================================================
    // 창 켜짐 사운드 5개
    // SystemPopOn 느낌 기반
    // =========================================================

    // 1. 짧은 시스템 팝업
    public static AudioClip CreateWindowOn_01_ShortSystemPop()
    {
        return CreateDigitalWindowOn(
            name: "WindowOn_01_Short_System_Pop",
            duration: 0.080f,
            startFreq: 820f,
            endFreq: 1500f,
            highFreq: 4200f,
            lowFreq: 260f,
            bodyAmount: 0.23f,
            highAmount: 0.065f,
            lowAmount: 0.045f,
            noiseAmount: 0.022f,
            squareAmount: 0.050f,
            secondPulseDelay: 0.40f,
            secondPulseAmount: 0.20f,
            decayPower: 4.9f,
            targetPeak: 0.17f
        );
    }

    // 2. 부드러운 창 활성화
    public static AudioClip CreateWindowOn_02_SoftWindowEnable()
    {
        return CreateDigitalWindowOn(
            name: "WindowOn_02_Soft_Window_Enable",
            duration: 0.095f,
            startFreq: 720f,
            endFreq: 1320f,
            highFreq: 3600f,
            lowFreq: 240f,
            bodyAmount: 0.235f,
            highAmount: 0.055f,
            lowAmount: 0.055f,
            noiseAmount: 0.025f,
            squareAmount: 0.040f,
            secondPulseDelay: 0.44f,
            secondPulseAmount: 0.18f,
            decayPower: 4.3f,
            targetPeak: 0.17f
        );
    }

    // 3. 데이터 창 점등
    public static AudioClip CreateWindowOn_03_DataWindowBlink()
    {
        return CreateDigitalWindowOn(
            name: "WindowOn_03_Data_Window_Blink",
            duration: 0.072f,
            startFreq: 1020f,
            endFreq: 1750f,
            highFreq: 4700f,
            lowFreq: 280f,
            bodyAmount: 0.225f,
            highAmount: 0.075f,
            lowAmount: 0.035f,
            noiseAmount: 0.018f,
            squareAmount: 0.070f,
            secondPulseDelay: 0.36f,
            secondPulseAmount: 0.24f,
            decayPower: 5.2f,
            targetPeak: 0.165f
        );
    }

    // 4. 구식 시스템 창 켜짐
    public static AudioClip CreateWindowOn_04_OldSystemWindow()
    {
        return CreateDigitalWindowOn(
            name: "WindowOn_04_Old_System_Window",
            duration: 0.110f,
            startFreq: 620f,
            endFreq: 1180f,
            highFreq: 3100f,
            lowFreq: 210f,
            bodyAmount: 0.25f,
            highAmount: 0.050f,
            lowAmount: 0.075f,
            noiseAmount: 0.040f,
            squareAmount: 0.040f,
            secondPulseDelay: 0.46f,
            secondPulseAmount: 0.17f,
            decayPower: 3.9f,
            targetPeak: 0.18f
        );
    }

    // 5. 선택 후 창 열림
    public static AudioClip CreateWindowOn_05_SelectOpenPop()
    {
        return CreateDigitalWindowOn(
            name: "WindowOn_05_Select_Open_Pop",
            duration: 0.088f,
            startFreq: 900f,
            endFreq: 1600f,
            highFreq: 4400f,
            lowFreq: 260f,
            bodyAmount: 0.235f,
            highAmount: 0.070f,
            lowAmount: 0.045f,
            noiseAmount: 0.022f,
            squareAmount: 0.060f,
            secondPulseDelay: 0.32f,
            secondPulseAmount: 0.26f,
            decayPower: 4.7f,
            targetPeak: 0.17f
        );
    }

    // =========================================================
    // 내부 생성 함수 - 버튼 클릭
    // =========================================================

    private static AudioClip CreateDigitalButtonClick(
        string name,
        float duration,
        float mainFreq,
        float highFreq,
        float lowFreq,
        float mainAmount,
        float highAmount,
        float lowAmount,
        float noiseAmount,
        float squareAmount,
        float decayPower,
        float targetPeak
    )
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float lastNoise = 0f;
        float phaseOffset = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float envelope = CreateClickEnvelope(t, decayPower);

            float mainTone =
                Mathf.Sin(2f * Mathf.PI * mainFreq * time + phaseOffset) *
                mainAmount;

            float highTone =
                Mathf.Sin(2f * Mathf.PI * highFreq * time) *
                highAmount;

            float lowTone = 0f;

            if (lowFreq > 0f)
            {
                lowTone =
                    Mathf.Sin(2f * Mathf.PI * lowFreq * time) *
                    lowAmount;
            }

            float square =
                Mathf.Sign(Mathf.Sin(2f * Mathf.PI * highFreq * 0.5f * time)) *
                squareAmount;

            float rawNoise = Random.Range(-1f, 1f);
            lastNoise = Mathf.Lerp(lastNoise, rawNoise, 0.20f);
            float noise = lastNoise * noiseAmount;

            float value =
                mainTone +
                highTone +
                lowTone +
                square +
                noise;

            samples[i] = value * envelope;
        }

        RemoveClick(samples, 32);
        NormalizeSamples(samples, targetPeak);

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    // =========================================================
    // 내부 생성 함수 - 창 켜짐
    // =========================================================

    private static AudioClip CreateDigitalWindowOn(
        string name,
        float duration,
        float startFreq,
        float endFreq,
        float highFreq,
        float lowFreq,
        float bodyAmount,
        float highAmount,
        float lowAmount,
        float noiseAmount,
        float squareAmount,
        float secondPulseDelay,
        float secondPulseAmount,
        float decayPower,
        float targetPeak
    )
    {
        int sampleCount = Mathf.CeilToInt(SampleRate * duration);
        float[] samples = new float[sampleCount];

        float lastNoise = 0f;
        float phaseOffset = Random.Range(0f, 1000f);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float time = i / (float)SampleRate;

            float envelope = CreateWindowOnEnvelope(t, decayPower);

            float freq = Mathf.Lerp(
                startFreq,
                endFreq,
                Mathf.SmoothStep(0f, 1f, t)
            );

            float bodyTone =
                Mathf.Sin(2f * Mathf.PI * freq * time + phaseOffset) *
                bodyAmount;

            float highTone =
                Mathf.Sin(2f * Mathf.PI * highFreq * time) *
                highAmount;

            float lowTone = 0f;

            if (lowFreq > 0f)
            {
                lowTone =
                    Mathf.Sin(2f * Mathf.PI * lowFreq * time) *
                    lowAmount;
            }

            float square =
                Mathf.Sign(Mathf.Sin(2f * Mathf.PI * highFreq * 0.5f * time)) *
                squareAmount;

            float rawNoise = Random.Range(-1f, 1f);
            lastNoise = Mathf.Lerp(lastNoise, rawNoise, 0.22f);
            float noise = lastNoise * noiseAmount;

            float secondPulse = CreateSecondPulse(t, secondPulseDelay) * secondPulseAmount;

            float value =
                bodyTone +
                highTone +
                lowTone +
                square +
                noise;

            samples[i] = value * envelope + value * secondPulse;
        }

        RemoveClick(samples, 48);
        NormalizeSamples(samples, targetPeak);

        AudioClip clip = AudioClip.Create(name, samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    private static float CreateClickEnvelope(float t, float decayPower)
    {
        float attackRatio = 0.08f;

        if (t < attackRatio)
        {
            float attackT = t / attackRatio;
            return Mathf.SmoothStep(0f, 1f, attackT);
        }

        float decayT = (t - attackRatio) / (1f - attackRatio);
        return Mathf.Pow(1f - decayT, decayPower);
    }

    private static float CreateWindowOnEnvelope(float t, float decayPower)
    {
        float attackRatio = 0.10f;

        if (t < attackRatio)
        {
            float attackT = t / attackRatio;
            return Mathf.SmoothStep(0f, 1f, attackT);
        }

        float decayT = (t - attackRatio) / (1f - attackRatio);
        return Mathf.Pow(1f - decayT, decayPower);
    }

    private static float CreateSecondPulse(float t, float delay)
    {
        float width = 0.16f;

        if (t < delay)
            return 0f;

        float pulseT = (t - delay) / width;

        if (pulseT < 0f || pulseT > 1f)
            return 0f;

        return Mathf.Sin(pulseT * Mathf.PI) * Mathf.Pow(1f - pulseT, 2f);
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