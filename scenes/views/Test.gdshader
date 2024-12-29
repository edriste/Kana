shader_type spatial;

// The label's texture
uniform sampler2D text_texture;
uniform sampler2D metallic_texture;

void fragment() {
    vec4 text_color = texture(text_texture, UV);
    vec4 metallic_color = texture(metallic_texture, UV);

    float mask = text_color.a;

    ALBEDO = text_color.rgb + (metallic_color.rgb * mask);
    METALLIC = mask;
}