#version 130
precision mediump float;

in vec4 vs_textureCoordinate;

uniform sampler2D textureObj;
uniform vec4 proximityColor;
uniform vec4 objectColor;
uniform float height;

out vec4 color;

void main(void)
{
    color = texture(textureObj, vec2(vs_textureCoordinate.x, vs_textureCoordinate.y)) * proximityColor * objectColor;
    if (color.a < 0.5)
        discard;
	color = vec4(0,0,0,0.2);
}