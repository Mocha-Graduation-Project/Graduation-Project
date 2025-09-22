void raymarch_float(float3 rayOrigin, float3 rayDirection, float numSteps,float stepSize, float densityScale, UnityTexture3D volumeTex, UnitySamplerState volumeSampler, float3 offset,float numLightSteps, float LightStepSize,float3 lightDir, float lightAbsorb, float darknessThreshold, float transmittance,out float3 result)
{
    float density = 0;
    float transmission = 0;
    float lightAccumulation = 0;
    float finalLight = 0;
    

    for (int i = 0; i < numSteps; i++)
    {
        rayOrigin += (rayDirection * stepSize);
        
        float3 samplePos = rayOrigin + offset;
        float sampledDensity = SAMPLE_TEXTURE3D(volumeTex, volumeSampler, samplePos).r;
        density += sampledDensity*densityScale;
        
        //light loop
        float3 lightRayOrigin = samplePos;
        
        for (int j = 0; j < numLightSteps; j++)
        {
            //The red dot position
            lightRayOrigin += -lightDir * LightStepSize;
            float lightDensity = SAMPLE_TEXTURE3D(volumeTex, volumeSampler, lightRayOrigin).r;
            
            lightAccumulation += lightDensity;
        }
        
        float lightTransmission = exp(-lightAccumulation);
        
        float shadow = darknessThreshold + lightTransmission * (1.0 - darknessThreshold);
        
        finalLight += density * transmittance * shadow;
        
        transmittance *= exp(-density * lightAbsorb);

    }

    transmission = exp(-density);
    
    result = float3(finalLight, transmission, transmittance);
}

