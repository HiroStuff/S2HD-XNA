sampler2D InputTexture : register(s0);
sampler2D InputOverlayTexture : register(s1);

float4x4 ModelViewMatrix : register(c0);
float4x4 ProjectionMatrix : register(c4);
float4 InputColour : register(c8);
int InputOverlayEnabled : register(c9);

struct VertexShaderInput
{
    float2 VertexPosition : POSITION0;
    float4 VertexColour : COLOR0;
    float2 VertexTextureMapping : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 Position : POSITION0;
    float4 FragmentColour : COLOR0;
    float2 FragmentTextureMapping : TEXCOORD0;
};

PixelShaderInput VertexShaderFunction(VertexShaderInput input)
{
    PixelShaderInput output;
    
    output.FragmentColour = input.VertexColour;
    output.FragmentTextureMapping = input.VertexTextureMapping;

    float4 modelViewPosition = mul(float4(input.VertexPosition, 0.0, 1.0), ModelViewMatrix);
    output.Position = mul(modelViewPosition, ProjectionMatrix);
    
    return output;
}

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float4 mainTexel = tex2D(InputTexture, input.FragmentTextureMapping);    
    float4 result = InputColour * mainTexel;    
    if (InputOverlayEnabled > 0)
    {
        float4 overlayTexel = tex2D(InputOverlayTexture, input.FragmentTextureMapping);       
        result = result * overlayTexel;
    }
    return result;
}

technique FontOverlayTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}