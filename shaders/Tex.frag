#version 130
precision mediump float;

in vec2 vs_textureCoordinate;
flat in int vs_texture;
flat in int vs_textureFlags;
in vec4 vs_color;
in vec3 vs_hideDistance;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform vec4 proximityColor;

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

    color *= vs_color;

	// only consider ambience if enabled
	if (vs_textureFlags != 1 && vs_textureFlags != 3)
		color *= proximityColor;

	// check whether we have a shadow and whether hiding is enabled
	if (!(color.r == 0.0 && color.g == 0.0 && color.b == 0.0 && color.a < 1.0) && (vs_textureFlags == 2 || vs_textureFlags == 3))
	{
		float hideDiff = length(vec2(vs_hideDistance.x, vs_hideDistance.y * 2));
		color.a *= min(1, (hideDiff * hideDiff) / (4 * 4) + 0.3);
	}

    if (color.a == 0.0)
        discard;
}