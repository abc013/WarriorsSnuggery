#version 100
precision mediump float;

attribute vec4 position;
attribute vec4 color;

uniform mat4 projection;
uniform mat4 modelView;
uniform vec4 proximityColor;

varying vec4 vs_color;

void main(void)
{
	gl_Position = projection * modelView * position;
	vs_color = color * proximityColor;
}