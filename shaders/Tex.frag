#version 130
precision mediump float;

in vec4 vs_textureCoordinate;
in vec4 vs_color;

uniform sampler2D textureObj;
uniform vec4 proximityColor;
uniform vec4 objectColor;

out vec4 color;

void main(void)
{
    // Check whether a texture should be used
    if (vs_textureCoordinate.w >= 0)
        color = texture(textureObj, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y)) * vs_color;
    else
        color = vs_color;
    color *= proximityColor * objectColor;
    if (color.a == 0.0)
        discard;
}