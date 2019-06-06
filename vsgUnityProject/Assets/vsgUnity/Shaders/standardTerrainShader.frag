#version 450 core
#pragma import_defines ( VSG_NORMAL, VSG_COLOR, VSG_TEXCOORD0, VSG_LIGHTING, VSG_TERRAIN_LAYERS)
#extension GL_ARB_separate_shader_objects : enable


#ifdef VSG_TERRAIN_LAYERS
layout (constant_id = 0) const uint SPLAT_LAYER_COUNT = 1;
layout (constant_id = 1) const uint SPLAT_MASK_COUNT = 1;
layout(set = 0, binding = 0) uniform sampler2D layerDiffuseTextures[SPLAT_LAYER_COUNT];
layout(set = 0, binding = 1) uniform sampler2D layerMaskTextures[SPLAT_MASK_COUNT];
#endif

#ifdef VSG_NORMAL
layout(location = 1) in vec3 normalDir;
#endif

#ifdef VSG_TEXCOORD0
layout(location = 4) in vec2 texCoord0;
#endif

#ifdef VSG_LIGHTING
layout(location = 5) in vec3 viewDir;
layout(location = 6) in vec3 lightDir;
#endif

layout(location = 0) out vec4 outColor;

void main()
{
    vec4 base = vec4(0.0,0.0,0.0,0.0);
		
#ifdef VSG_TERRAIN_LAYERS
	vec4 mask = texture(layerMaskTextures[0], texCoord0.st);
	for(int i = 0; i < SPLAT_LAYER_COUNT; i++)
	{
		vec4 splat = texture(layerDiffuseTextures[i], texCoord0.st);
		base = mix(base, splat, mask[i]);
	}
#endif

#ifdef VSG_COLOR
    base = base * vertColor;
#endif
    float ambientOcclusion = 1.0;
    vec3 specularColor = vec3(0.2,0.2,0.2);
#ifdef VSG_LIGHTING
    vec3 nDir = normalDir;
    vec3 nd = normalize(nDir);
    vec3 ld = normalize(lightDir);
    vec3 vd = normalize(viewDir);
    vec4 color = vec4(0.01, 0.01, 0.01, 1.0);
    color += /*osg_Material.ambient*/ vec4(0.1, 0.1, 0.1, 0.0);
    float diff = max(dot(ld, nd), 0.0);
    color += /*osg_Material.diffuse*/ vec4(0.8, 0.8, 0.8, 0.0) * diff;
    color *= ambientOcclusion;
    color *= base;
    if (diff > 0.0)
    {
        vec3 halfDir = normalize(ld + vd);
        color.rgb += base.a * specularColor *
            pow(max(dot(halfDir, nd), 0.0), 16.0/*osg_Material.shine*/);
    }
#else
    vec4 color = base;
#endif
    outColor = color;
    if (outColor.a==0.0) discard;
}

