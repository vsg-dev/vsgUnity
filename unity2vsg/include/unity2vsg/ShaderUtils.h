#pragma once

#include <unity2vsg/Export.h>

#include <vsg/all.h>

namespace unity2vsg
{
    enum GeometryAttributes : uint32_t
    {
        VERTEX = 1,
        NORMAL = 2,
        NORMAL_OVERALL = 4,
        TANGENT = 8,
        TANGENT_OVERALL = 16,
        COLOR = 32,
        COLOR_OVERALL = 64,
        TEXCOORD0 = 128,
        TEXCOORD1 = 256,
        TEXCOORD2 = 512,
        STANDARD_ATTS = VERTEX | NORMAL | TANGENT | COLOR | TEXCOORD0,
        ALL_ATTS = VERTEX | NORMAL | NORMAL_OVERALL | TANGENT | TANGENT_OVERALL | COLOR | COLOR_OVERALL | TEXCOORD0 | TEXCOORD1 | TEXCOORD2
    };

    enum ShaderModeMask : uint32_t
    {
        NONE = 0,
        LIGHTING = 1,
        MATERIAL = 2,
        BLEND = 4,
        BILLBOARD = 8,
        DIFFUSE_MAP = 16,
        OPACITY_MAP = 32,
        AMBIENT_MAP = 64,
        NORMAL_MAP = 128,
        SPECULAR_MAP = 256,
        ALL_SHADER_MODE_MASK = LIGHTING | MATERIAL | BLEND | BILLBOARD | DIFFUSE_MAP | OPACITY_MAP | AMBIENT_MAP | NORMAL_MAP | SPECULAR_MAP
    };

    // taken from osg fbx plugin
    enum TextureUnit : uint32_t
    {
        DIFFUSE_TEXTURE_UNIT = 0,
        OPACITY_TEXTURE_UNIT,
        REFLECTION_TEXTURE_UNIT,
        EMISSIVE_TEXTURE_UNIT,
        AMBIENT_TEXTURE_UNIT,
        NORMAL_TEXTURE_UNIT,
        SPECULAR_TEXTURE_UNIT,
        SHININESS_TEXTURE_UNIT
    };

    // read a glsl file and inject defines based on shadermodemask and geometryatts
    extern std::string readGLSLShader(const std::string& filename, const uint32_t& shaderModeMask, const uint32_t& geometryAttrbutes);

    // create shader source using statemask to determine type of shader to build and geometryattributes to determine attribute binding locations (specific to fbx files)
    extern std::string createFbxVertexSource(const uint32_t& shaderModeMask, const uint32_t& geometryAttrbutes);
    extern std::string createFbxFragmentSource(const uint32_t& shaderModeMask, const uint32_t& geometryAttrbutes);

    class ShaderCompiler : public vsg::Object
    {
    public:
        ShaderCompiler(vsg::Allocator* allocator=nullptr);
        virtual ~ShaderCompiler();

        bool compile(vsg::ShaderModule* shader);
        bool compile(vsg::ShaderModules& shaders);
    };
}
