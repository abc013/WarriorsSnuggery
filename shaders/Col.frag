#version 100
precision mediump float;

varying vec4 vs_color;

uniform vec4 proximityColor;
uniform vec4 objectColor;

void main(void)
{
	gl_FragColor = vs_color * proximityColor * objectColor;
}