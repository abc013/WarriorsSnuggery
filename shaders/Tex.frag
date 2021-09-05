#version 130
precision mediump float;

in vec2 vs_textureCoordinate;
flat in int vs_texture;
in vec4 vs_color;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform vec4 proximityColor;
uniform vec4 objectColor;

out vec4 color;

void main(void)
{
    if (vs_texture == 0)
        color = texture(texture0, vs_textureCoordinate);
    else if (vs_texture == 1)
        color = texture(texture1, vs_textureCoordinate);
    else if (vs_texture == 2)
        color = texture(texture2, vs_textureCoordinate);
    else if (vs_texture == 3)
        color = texture(texture3, vs_textureCoordinate);
    else
        color = vec4(1);

    color *= vs_color * proximityColor * objectColor;

    if (color.a == 0.0)
        discard;
}