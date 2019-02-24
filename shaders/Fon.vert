#version 130
precision mediump float;

in vec4 position;
in vec4 textureCoordinate;
in vec4 color;
in vec4 textureOffset;

uniform mat4 projection;
uniform mat4 modelView;

out vec2 vs_textureOffset;
out vec4 vs_color;

void main(void)
{
	gl_Position = projection * modelView * position;
	vs_textureOffset = vec2(textureCoordinate.x,textureCoordinate.y);
	vs_color = color;
	vs_textureOffset.x += textureOffset.x;
}