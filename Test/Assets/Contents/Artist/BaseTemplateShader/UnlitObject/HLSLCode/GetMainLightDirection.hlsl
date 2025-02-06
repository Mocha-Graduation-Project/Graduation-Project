#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED


void GetLighting_float(out float3 LightColor, out float3 Direction, out float LightStrength)
{
#ifdef SHADERGRAPH_PREVIEW

Direction = half3(0.5,0.5,0);
LightColor = half3(0.5,0.5,0);
LightStrength = 1.0f;

#else 
    Light light = GetMainLight(0);
    Direction = light.direction;
    LightColor = light.color;
    LightStrength = length(light.color);

#endif
}

#endif