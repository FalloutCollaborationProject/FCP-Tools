#version 450

// V.A.T.S. zoom lens, vertex stage. Port of Unity's Unlit/ZoomShader (GrabPass magnifier).
//
// One quad per lens. Besides the usual transform this stage finds where the quad's
// model origin lands on screen, so the fragment stage can magnify the scene grab
// around that point (Unity: ComputeGrabScreenPos(UnityObjectToClipPos(0,0,0,1))).

layout(location = 0) in vec3 inPosition; // @vertex Position required
layout(location = 3) in vec4 inUv0; // @vertex TexCoord0 optional @default = vec4(0.0, 0.0, 0.0, 0.0)

layout(push_constant) uniform Push { mat4 modelMatrix; } push;

layout(location = 0) out vec2 vUv;
layout(location = 1) flat out vec2 vCenterUv; // lens centre in scene-grab UV space

layout(set = 0, binding = 0) uniform VertexUniforms
{
    mat4 ViewProj;
} uVertex;

void main()
{
    gl_Position = uVertex.ViewProj * push.modelMatrix * vec4(inPosition, 1.0);
    vUv = inUv0.xy;

    // Model origin -> clip -> NDC -> [0,1] screen UV.
    //
    // Waylith's ViewProj already targets Vulkan clip space: NDC (-1,-1) is the
    // top-left corner and +y points down, the same orientation as gl_FragCoord
    // (origin top-left) and as the scene grab, which is addressed with
    // gl_FragCoord.xy / textureSize(grab, 0). So ndc * 0.5 + 0.5 is directly
    // comparable with the fragment's grab UV and must NOT be y-flipped (Unity's
    // ComputeGrabScreenPos flip has no equivalent here). Core's TerrainWater.frag
    // does the same: (vClip.xy / vClip.w) * 0.5 + 0.5, "vulkan y down".
    //
    // The value is identical for all vertices of the quad, hence flat.
    vec4 centerClip = uVertex.ViewProj * push.modelMatrix * vec4(0.0, 0.0, 0.0, 1.0);
    vCenterUv = (centerClip.xy / centerClip.w) * 0.5 + 0.5;
}
