#version 450 core
#pragma import_defines ( VSG_NORMAL, VSG_COLOR, VSG_TEXCOORD0, VSG_LIGHTING)
#extension GL_ARB_separate_shader_objects : enable

layout(set = 0, binding = 0) uniform sampler2D diffuseTextureArray[12];
layout(set = 0, binding = 1) uniform sampler2D splatMask1;
layout(set = 0, binding = 2) uniform sampler2D splatMask2;


layout(set = 0, binding = 3) uniform TextureIndex1 { float index; } diffuseTextureIndex1;
layout(set = 0, binding = 4) uniform TextureIndex2 { float index; } diffuseTextureIndex2;
layout(set = 0, binding = 5) uniform TextureIndex3 { float index; } diffuseTextureIndex3;
layout(set = 0, binding = 6) uniform TextureIndex4 { float index; } diffuseTextureIndex4;
layout(set = 0, binding = 7) uniform TextureIndex5 { float index; } diffuseTextureIndex5;
layout(set = 0, binding = 8) uniform TextureIndex6 { float index; } diffuseTextureIndex6;


layout(set = 0, binding = 9) uniform TextureScale1 { float scale; } diffuseTextureScale1;
layout(set = 0, binding = 10) uniform TextureScale2 { float scale; } diffuseTextureScale2;
layout(set = 0, binding = 11) uniform TextureScale3 { float scale; } diffuseTextureScale3;
layout(set = 0, binding = 12) uniform TextureScale4 { float scale; } diffuseTextureScale4;
layout(set = 0, binding = 13) uniform TextureScale5 { float scale; } diffuseTextureScale5;
layout(set = 0, binding = 14) uniform TextureScale6 { float scale; } diffuseTextureScale6;


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
	vec4 mask = texture(splatMask1, texCoord0.st);
	
	// tex 1
	vec4 diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex1.index)], (texCoord0.st) * diffuseTextureScale1.scale);
	base = mix(base, diffuse, mask[0]);
	
	// tex 2
	diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex2.index)], (texCoord0.st) * diffuseTextureScale2.scale);
	base = mix(base, diffuse, mask[1]);
	
	// tex 3
	diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex3.index)], (texCoord0.st) * diffuseTextureScale3.scale);
	base = mix(base, diffuse, mask[2]);
	
	// tex 4
	diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex4.index)], (texCoord0.st) * diffuseTextureScale4.scale);
	base = mix(base, diffuse, mask[3]);

	// new mask
	 mask = texture(splatMask2, texCoord0.st);

	// tex 5
	diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex5.index)], (texCoord0.st) * diffuseTextureScale5.scale);
	base = mix(base, diffuse, mask[0]);
	
	// tex 6
	diffuse = texture(diffuseTextureArray[int(diffuseTextureIndex6.index)], (texCoord0.st) * diffuseTextureScale6.scale);
	base = mix(base, diffuse, mask[1]);


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

