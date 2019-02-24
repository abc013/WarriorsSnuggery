#version 130
precision mediump float;

in vec2 vs_textureOffset;
in vec4 vs_color;

uniform sampler2D textureObject;
uniform vec4 proximityColor;
uniform vec4 objectColor;

out vec4 color;

void main(void)
{
	ivec2 isize = textureSize(textureObject, 0);
	vec2 size = vec2(isize.x, isize.y);
	vec2 offset = vec2(vs_textureOffset.x / size.x, vs_textureOffset.y / size.y);
	vec4 alpha = texture(textureObject, offset);//texelFetch(textureObject, ivec2(vs_textureOffset.x, vs_textureOffset.y), 0);
	color = vs_color * proximityColor * objectColor;
	color.a = vs_color.w * alpha.r;
}