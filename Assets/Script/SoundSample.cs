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