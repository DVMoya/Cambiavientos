void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color,
	out float DistanceAtten, out float ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(0.5f, 0.5f, 0.25f));
    Color = float3(1.0f, 1.0f, 1.0f);
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
 
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void MainLight_half(half3 WorldPos, out half3 Direction, out half3 Color,
	out half DistanceAtten, out half ShadowAtten)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(half3(0.5f, 0.5f, 0.25f));
    Color = half3(1.0f, 1.0f, 1.0f);
    DistanceAtten = 1.0f;
    ShadowAtten = 1.0f;
#else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);
 
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void AdditionalLight_float(float3 WorldPos, int Index, out float3 Direction,
	out float3 Color, out float DistanceAtten, out float ShadowAtten)
{
    Direction = normalize(float3(0.5f, 0.5f, 0.25f));
    Color = float3(0.0f, 0.0f, 0.0f);
    DistanceAtten = 0.0f;
    ShadowAtten = 0.0f;

#ifndef SHADERGRAPH_PREVIEW
    int pixelLightCount = GetAdditionalLightsCount();
    if (Index < pixelLightCount)
    {
        Light light = GetAdditionalLight(Index, WorldPos);
    
        Direction = light.direction;
        Color = light.color;
        DistanceAtten = light.distanceAttenuation;
        ShadowAtten = light.shadowAttenuation;
    }
#endif
}

void AdditionalLight_half(half3 WorldPos, int Index, out half3 Direction,
	out half3 Color, out half DistanceAtten, out half ShadowAtten)
{
    Direction = normalize(half3(0.5f, 0.5f, 0.25f));
    Color = half3(0.0f, 0.0f, 0.0f);
    DistanceAtten = 0.0f;
    ShadowAtten = 0.0f;

#ifndef SHADERGRAPH_PREVIEW
    int pixelLightCount = GetAdditionalLightsCount();
    if (Index < pixelLightCount)
    {
        Light light = GetAdditionalLight(Index, WorldPos);

        Direction = light.direction;
        Color = light.color;
        DistanceAtten = light.distanceAttenuation;
        ShadowAtten = light.shadowAttenuation;
    }
#endif
}

void MultipleLightsDiffuse_float(float3 WorldPos, float3 WorldNormal, float2 CutoffThresholds, out float3 LightColor)
{
    LightColor = 0.0f;
#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();

    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos);

        float3 color = dot(WorldNormal, light.direction);
        color = saturate(color);    // saturate evita los valores negativos
        color = smoothstep(CutoffThresholds.x, CutoffThresholds.y, color);
        color *= light.color;
        float atten = light.distanceAttenuation * light.shadowAttenuation;
        color *= atten;

        LightColor += color;
    }
#endif
}

void MultipleLightsDiffuse_half(half3 WorldPos, half3 WorldNormal, half2 CutoffThresholds, out half3 LightColor)
{
    LightColor = 0.0f;
#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();

    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos);

        half3 color = dot(WorldNormal, light.direction);
        color = saturate(color);    // saturate evita los valores negativos
        color = smoothstep(CutoffThresholds.x, CutoffThresholds.y, color);
        color *= light.color;
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        color *= atten;

        LightColor += color;
    }
#endif
}

void MultipleLightsSpecular_float(float3 WorldPos, float3 WorldNormal, float2 CutoffThresholds, float Smoothness, out float3 LightColor)
{
    LightColor = 0.0f;
#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();

    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos);

        float3 viewDir = normalize(_WorldSpaceCameraPos - WorldPos);
        float3 direction = normalize(viewDir + light.direction);
        float3 color = dot(WorldNormal, direction);
        color = saturate(color);
        float3 NdotL = dot(WorldNormal, light.direction);
        NdotL = step(0.5f, NdotL);
        color *= NdotL;
        color = pow(color, Smoothness);
        color *= light.color;
        float atten = light.distanceAttenuation * light.shadowAttenuation;
        color *= atten;

        LightColor += color;
    }
#endif
}

void MultipleLightsSpecular_half(half3 WorldPos, half3 WorldNormal, half2 CutoffThresholds, half Smoothness, out half3 LightColor)
{
    LightColor = 0.0f;
#ifndef SHADERGRAPH_PREVIEW
    int lightCount = GetAdditionalLightsCount();

    for (int i = 0; i < lightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos);

        half3 viewDir = normalize(_WorldSpaceCameraPos - WorldPos);
        half3 direction = normalize(viewDir + light.direction);
        half3 color = dot(WorldNormal, direction);
        color = saturate(color);
        half3 NdotL = dot(WorldNormal, light.direction);
        NdotL = step(0.5f, NdotL);
        color *= NdotL;
        color = pow(color, Smoothness);
        color *= light.color;
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        color *= atten;

        LightColor += color;
    }
#endif
}