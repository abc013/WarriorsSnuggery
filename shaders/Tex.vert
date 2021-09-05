#version 130
precision mediump float;

// layout (location = 0)
in vec4 position;
// layout (location = 1)
in vec2 textureCoordinate;
// layout (location = 2)
in int texture;
// layout (location = 3)
in vec4 color;

uniform mat4 projection;
uniform mat4 modelView;

out vec2 vs_textureCoordinate;
flat out int vs_texture;
out vec4 vs_color;

void main(void)
{
    gl_Position = projection * modelView * position;
    vs_textureCoordinate = textureCoordinate;
    vs_texture = texture;
    vs_color = color;
}