#version 130
precision mediump float;

in vec4 vs_textureCoordinate;
in vec4 vs_color;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform sampler2D texture4;
uniform vec4 proximityColor;
uniform vec4 objectColor;

out vec4 color;

void main(void)
{
    color = vs_color;
    // Check whether a texture should be used
    if (vs_textureCoordinate.w >= 0)
    {
        if (vs_textureCoordinate.z < 0.5)
            color = texture(texture0, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y));
        else if (vs_textureCoordinate.z < 1.5)
            color = texture(texture1, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y));
        else if (vs_textureCoordinate.z < 2.5)
            color = texture(texture2, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y));
        else if (vs_textureCoordinate.z < 3.5)
            color = texture(texture3, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y));
        else if (vs_textureCoordinate.z < 4.5)
            color = texture(texture4, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y));
        color *= vs_color;
    }

    color *= proximityColor * objectColor;
    if (color.a == 0.0)
        discard;
}