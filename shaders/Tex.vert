#version 130
precision mediump float;

// layout (location = 0)
in vec4 position;
// layout (location = 1)  
in vec4 textureCoordinate;

uniform mat4 projection;
uniform mat4 modelView;

out vec4 vs_textureCoordinate;

void main(void)
{
    gl_Position = projection * modelView * position;
    vs_textureCoordinate = textureCoordinate;
}