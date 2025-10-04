sampler2D InputTexture : register(s0);
sampler2D InputTextureMask : register(s1);

float4 InputColour : register(c0);
int MaskInput : register(c1);
int MaskColorMultiply : register(c2);

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

PixelShaderInput VertexShaderFunction(VertexShaderInput input)
{
    PixelShaderInput output;
    
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 PixelShaderFunction(PixelShaderInput input) : COLOR0
{
    float4 texel = tex2D(InputTexture, input.TexCoord);
    float4 texelColor = float4(1.0, 1.0, 1.0, 1.0);

    if (MaskInput > 0)
    {
        float4 texelMask = tex2D(InputTextureMask, input.TexCoord);
        texelColor = InputColour * texel;
        
        if (MaskColorMultiply > 0) 
        {
            texelColor = texelColor * texelMask;
        }
        else 
        {
            texelColor.w = texelColor.w * texelMask.w;
        }
    }
    else
    {
        texelColor = InputColour * texel;
    }

    return texelColor;
}

technique MaskTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}