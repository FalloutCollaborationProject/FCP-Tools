#version 450

// V.A.T.S. zoom lens, fragment stage. Port of Unity's Unlit/ZoomShader (GrabPass magnifier).
//
// The scene grab is declared in FCP_Shaders.xml:
//   <sceneGrab><name>FCP_VATSZoom</name><queue>3174</queue></sceneGrab>
// which binds the frame colour, as it was just before queue 3174, to
// _GrabFCP_VATSZoom. Grabs are addressed in gl_FragCoord space (origin top-left,
// +y down): uv = gl_FragCoord.xy / textureSize(grab, 0).
//
// _Mask channels (unchanged from the Unity shader):
//   A  lens coverage; below 0.1 the pixel is outside the lens and left untouched
//   B  1 = show the magnified scene, 0 = show the mask's own colour (scope rim /
//      reticle); values in between blend the two
//
// The ShaderDef uses blendMode Alpha, so alpha is always written as 1.0: the lens
// replaces the frame (Unity: Blend One Zero) and pixels outside it are discarded,
// which is equivalent to Unity re-emitting the unmodified background there.

layout(location = 0) in vec2 vUv;
layout(location = 1) flat in vec2 vCenterUv; // lens centre in scene-grab UV space

layout(location = 0) out vec4 outColor;

// std140: two vec4 (offsets 0, 16) followed by the float (offset 32). Keep the
// float last so no padding is introduced between members.
layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _UVCenterOffset; // @default = vec4(0.0, 0.0, 0.0, 0.0) // xy added to the lens centre, in grab UV (+y down)
    vec4 _Mask_ST;        // @default = vec4(0.0, 0.0, 1.0, 1.0) // xy offset, zw scale
    float _Magnification; // @default = 3.0
} u;

layout(set = 2, binding = 1) uniform sampler2D _Mask;
layout(set = 2, binding = 2) uniform sampler2D _GrabFCP_VATSZoom;

// Quad UV has v = 0 at the bottom (Unity convention) while textures are stored
// top row first, and the ShaderDef sets flipTextureST = false, so the flip is done
// here, as Core's shaders do for their own textures.
vec2 flipV(vec2 c) { return vec2(c.x, 1.0 - c.y); }

void main()
{
    vec4 mask = texture(_Mask, flipV(vUv * u._Mask_ST.zw + u._Mask_ST.xy));

    // Outside the lens: keep the frame as it is.
    if (mask.a < 0.1)
    {
        discard;
    }

    vec2 grabSize = max(vec2(textureSize(_GrabFCP_VATSZoom, 0)), vec2(1.0));
    vec2 screenUv = gl_FragCoord.xy / grabSize;

    // Pull the sample point towards the lens centre: a fragment at distance d from
    // the centre shows the scene at distance d / magnification. Both UVs are in
    // the same top-left, +y down space (see the vertex stage), so no flip is needed.
    vec2 centerUv = vCenterUv + u._UVCenterOffset.xy;
    vec2 magnifiedUv = centerUv + (screenUv - centerUv) / max(u._Magnification, 0.0001);
    magnifiedUv = clamp(magnifiedUv, vec2(0.0), vec2(1.0));

    vec3 albedo = texture(_GrabFCP_VATSZoom, magnifiedUv).rgb;

    // mask.b = 1 -> magnified scene, mask.b = 0 -> mask colour.
    outColor = vec4(mix(mask.rgb, albedo, mask.b), 1.0);
}
