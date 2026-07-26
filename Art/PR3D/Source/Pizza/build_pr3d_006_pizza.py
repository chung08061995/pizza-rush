import bpy
import json
import math
import os
from mathutils import Vector


ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../../.."))
SOURCE_DIR = os.path.join(ROOT, "Art/PR3D/Source/Pizza")
EXPORT_DIR = os.path.join(ROOT, "Art/PR3D/Exports/Pizza")
UNITY_DIR = os.path.join(ROOT, "Assets/_Projects/Art/PR3D/Pizza")
PREVIEW_DIR = os.path.join(UNITY_DIR, "Evidence")

for path in (SOURCE_DIR, EXPORT_DIR, UNITY_DIR, PREVIEW_DIR):
    os.makedirs(path, exist_ok=True)


def material(name, color, metallic=0.0, roughness=0.45):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    return mat


def add_tri_prism(vertices, faces, mat_ids, z0, z1, inset=0.0, material_index=0):
    # Keep the canonical slice inside one 1 m gameplay cell at object scale 1.
    # The back edge is intentionally broad and the point slightly truncated by
    # beveling so the silhouette survives the portrait gameplay camera.
    p = [(-0.42 + inset, 0.36 - inset), (0.42 - inset, 0.36 - inset), (0.0, -0.50 + inset)]
    start = len(vertices)
    vertices.extend([(x, y, z0) for x, y in p] + [(x, y, z1) for x, y in p])
    faces.extend([
        (start, start + 2, start + 1),
        (start + 3, start + 4, start + 5),
        (start, start + 1, start + 4, start + 3),
        (start + 1, start + 2, start + 5, start + 4),
        (start + 2, start, start + 3, start + 5),
    ])
    mat_ids.extend([material_index] * 5)


def add_box(vertices, faces, mat_ids, center, scale, material_index):
    cx, cy, cz = center
    sx, sy, sz = scale
    start = len(vertices)
    vertices.extend([
        (cx - sx, cy - sy, cz - sz), (cx + sx, cy - sy, cz - sz),
        (cx + sx, cy + sy, cz - sz), (cx - sx, cy + sy, cz - sz),
        (cx - sx, cy - sy, cz + sz), (cx + sx, cy - sy, cz + sz),
        (cx + sx, cy + sy, cz + sz), (cx - sx, cy + sy, cz + sz),
    ])
    faces.extend([
        (start, start + 3, start + 2, start + 1),
        (start + 4, start + 5, start + 6, start + 7),
        (start, start + 1, start + 5, start + 4),
        (start + 1, start + 2, start + 6, start + 5),
        (start + 2, start + 3, start + 7, start + 6),
        (start + 3, start, start + 4, start + 7),
    ])
    mat_ids.extend([material_index] * 6)


def add_rounded_rect_prism(vertices, faces, mat_ids, center, half_width, half_height,
                           radius, z0, z1, material_index, corner_segments=4):
    """Create the pizza's pill-shaped rear crust without a second mesh contract."""
    cx, cy = center
    outline = []
    for corner_x, corner_y, angle0 in (
        (cx + half_width - radius, cy + half_height - radius, 0.0),
        (cx - half_width + radius, cy + half_height - radius, math.pi / 2),
        (cx - half_width + radius, cy - half_height + radius, math.pi),
        (cx + half_width - radius, cy - half_height + radius, 3 * math.pi / 2),
    ):
        for step in range(corner_segments + 1):
            angle = angle0 + step * math.pi / (2 * corner_segments)
            outline.append((
                corner_x + radius * math.cos(angle),
                corner_y + radius * math.sin(angle),
            ))

    start = len(vertices)
    count = len(outline)
    vertices.extend([(x, y, z0) for x, y in outline])
    vertices.extend([(x, y, z1) for x, y in outline])
    faces.append(tuple(start + i for i in reversed(range(count))))
    faces.append(tuple(start + count + i for i in range(count)))
    for i in range(count):
        j = (i + 1) % count
        faces.append((start + i, start + j, start + count + j, start + count + i))
    mat_ids.extend([material_index] * (2 + count))


def add_cylinder(vertices, faces, mat_ids, center, radius, depth, material_index, sides=12):
    cx, cy, cz = center
    start = len(vertices)
    for z in (cz - depth / 2, cz + depth / 2):
        vertices.extend([
            (cx + radius * math.cos(2 * math.pi * i / sides),
             cy + radius * math.sin(2 * math.pi * i / sides), z)
            for i in range(sides)
        ])
    faces.append(tuple(start + i for i in reversed(range(sides))))
    faces.append(tuple(start + sides + i for i in range(sides)))
    for i in range(sides):
        j = (i + 1) % sides
        faces.append((start + i, start + j, start + sides + j, start + sides + i))
    mat_ids.extend([material_index] * (2 + sides))


def build_shared_mesh():
    vertices, faces, mat_ids = [], [], []
    # A chunky, rounded phone-readable silhouette modelled after the concept conveyor slices.
    add_tri_prism(vertices, faces, mat_ids, 0.00, 0.10, material_index=0)
    add_tri_prism(vertices, faces, mat_ids, 0.090, 0.150, inset=0.045, material_index=1)
    add_rounded_rect_prism(
        vertices, faces, mat_ids,
        center=(0.0, 0.325),
        half_width=0.43,
        half_height=0.105,
        radius=0.095,
        z0=0.070,
        z1=0.205,
        material_index=0,
    )

    # Three oversized topping coins stay readable after the slice is reduced to
    # roughly 30 px in the portrait gameplay camera.
    topping_centers = [
        (-0.17, 0.095, 0.180),
        (0.17, 0.095, 0.180),
        (0.00, -0.175, 0.180),
    ]
    for center in topping_centers:
        add_cylinder(vertices, faces, mat_ids, center, 0.115, 0.060, 2, sides=16)
        # Raised centre catches light and keeps variants legible when the slice is tiny.
        add_cylinder(
            vertices, faces, mat_ids,
            (center[0], center[1], center[2] + 0.036),
            0.043,
            0.028,
            3,
            sides=12,
        )

    mesh = bpy.data.meshes.new("PR3D_PizzaSlice_SharedMesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    mesh.materials.clear()
    for mat in (
        bpy.data.materials["PR3D_MAT_Crust"],
        bpy.data.materials["PR3D_MAT_Cheese"],
        bpy.data.materials["PR3D_MAT_Red_Tomato"],
        bpy.data.materials["PR3D_MAT_Red_Tomato_Garnish"],
    ):
        mesh.materials.append(mat)
    for poly, material_index in zip(mesh.polygons, mat_ids):
        poly.material_index = material_index
        poly.use_smooth = True

    # Apply one bevel to all disconnected pieces so the exported contract remains one mesh.
    temp = bpy.data.objects.new("PR3D_PizzaSlice_BevelSource", mesh)
    bpy.context.collection.objects.link(temp)
    bpy.context.view_layer.objects.active = temp
    temp.select_set(True)
    bevel = temp.modifiers.new("PR3D_PhoneReadable_RoundedEdges", "BEVEL")
    bevel.width = 0.022
    bevel.segments = 3
    bevel.limit_method = "ANGLE"
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(66.0), island_margin=0.025)
    bpy.ops.object.mode_set(mode="OBJECT")
    bpy.data.objects.remove(temp, do_unlink=True)
    mesh.name = "PR3D_PizzaSlice_SharedMesh"
    return mesh


def look_at(obj, point):
    obj.rotation_euler = (Vector(point) - obj.location).to_track_quat("-Z", "Y").to_euler()


# Remove hidden export helpers as well as visible objects so rerunning through Blender MCP
# stays deterministic and preserves the imported mesh name/file-ID contract.
for obj in list(bpy.data.objects):
    bpy.data.objects.remove(obj, do_unlink=True)
for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
    for block in list(datablocks):
        if block.users == 0:
            datablocks.remove(block)

scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 900
scene.render.resolution_y = 1600
scene.render.resolution_percentage = 60
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = False
scene.world.color = (0.018, 0.025, 0.045)

# A deeper baked-orange crust holds its contrast under the bright portrait key
# and matches the toasted borders in the supplied pizza-factory concept.
crust = material("PR3D_MAT_Crust", (0.46, 0.12, 0.025), roughness=0.56)
cheese = material("PR3D_MAT_Cheese", (1.0, 0.58, 0.07), roughness=0.38)

variants = [
    ("Red_Tomato", (0.92, 0.035, 0.025), (1.00, 0.70, 0.08)),
    ("Green_Basil", (0.03, 0.62, 0.10), (0.48, 0.95, 0.10)),
    ("Blue_Cheese", (0.035, 0.28, 0.95), (0.18, 0.78, 1.00)),
    ("White_Garlic", (0.95, 0.95, 0.88), (0.58, 0.62, 0.68)),
    ("Orange_Pepper", (1.00, 0.27, 0.015), (1.00, 0.82, 0.04)),
    ("Yellow_Corn", (1.00, 0.82, 0.015), (0.80, 0.25, 0.03)),
    ("Brown_Mushroom", (0.35, 0.13, 0.045), (0.78, 0.48, 0.16)),
    ("Cyan_Seafood", (0.00, 0.72, 0.82), (0.02, 0.18, 0.38)),
    ("DarkPurple_Olive", (0.16, 0.025, 0.30), (0.70, 0.20, 0.94)),
    ("Pink_Ham", (1.00, 0.20, 0.52), (0.55, 0.02, 0.18)),
]
for name, primary, accent in variants:
    material(f"PR3D_MAT_{name}", primary, roughness=0.38)
    material(f"PR3D_MAT_{name}_Garnish", accent, roughness=0.42)

shared_mesh = build_shared_mesh()

bpy.ops.object.empty_add(type="PLAIN_AXES", location=(0, 0, 0))
root = bpy.context.object
root.name = "PR3D_PizzaVariants_Root"
root["asset_contract"] = "One rounded shared triangular mesh; ten object-linked material variants"
root["concept_target"] = "Rounded colored cheese slice, visible rear crust, three topping motifs"
root["unity_scale_m"] = 1.0

variant_objects = []
spacing_x, spacing_y = 1.08, 1.25
for index, (name, _, _) in enumerate(variants):
    obj = bpy.data.objects.new(f"PR3D_Pizza_{index + 1:02d}_{name}", shared_mesh)
    bpy.context.collection.objects.link(obj)
    obj.parent = root
    obj.location = ((index % 2 - 0.5) * spacing_x, (index // 2 - 2.0) * spacing_y, 0.0)
    obj["variant_index"] = index
    obj["color_type"] = name.split("_", 1)[0]
    for slot in obj.material_slots:
        slot.link = "OBJECT"
    obj.material_slots[0].material = crust
    obj.material_slots[1].material = bpy.data.materials[f"PR3D_MAT_{name}"]
    obj.material_slots[2].material = bpy.data.materials[f"PR3D_MAT_{name}_Garnish"]
    obj.material_slots[3].material = cheese
    variant_objects.append(obj)

# Zero-origin canonical object used by the single-slice export contract.
export_single = bpy.data.objects.new("PR3D_PizzaSlice_Shared", shared_mesh)
bpy.context.collection.objects.link(export_single)
export_single.parent = root
for slot in export_single.material_slots:
    slot.link = "OBJECT"
export_single.material_slots[0].material = crust
export_single.material_slots[1].material = bpy.data.materials["PR3D_MAT_Red_Tomato"]
export_single.material_slots[2].material = bpy.data.materials["PR3D_MAT_Red_Tomato_Garnish"]
export_single.material_slots[3].material = cheese
export_single.hide_render = True
export_single.hide_set(True)

# Rounded display plinths make the ten variants legible in the portrait evidence render.
plinth_mat = material("PR3D_MAT_PreviewPlinth", (0.055, 0.075, 0.12), roughness=0.68)
for obj in variant_objects:
    bpy.ops.mesh.primitive_cube_add(location=(obj.location.x, obj.location.y, -0.10), scale=(0.52, 0.54, 0.055))
    plinth = bpy.context.object
    plinth.name = f"PREVIEW_{obj.name}_Plinth"
    plinth.data.materials.append(plinth_mat)
    bevel = plinth.modifiers.new("Preview_RoundedEdges", "BEVEL")
    bevel.width = 0.10
    bevel.segments = 3

# Camera portrait framing with enough margin to judge phone-size silhouette and color separation.
bpy.ops.object.camera_add(location=(0.0, -1.8, 14.7))
camera = bpy.context.object
camera.name = "PR3D_Pizza_PortraitCamera"
camera.data.type = "ORTHO"
camera.data.ortho_scale = 7.05
look_at(camera, (0.0, 0.0, 0.0))
scene.camera = camera

bpy.ops.object.light_add(type="AREA", location=(-4.0, -3.0, 10.0))
key = bpy.context.object
key.name = "PR3D_Pizza_Key"
key.data.energy = 1250
key.data.shape = "DISK"
key.data.size = 5.0
look_at(key, (0.0, 0.0, 0.0))
bpy.ops.object.light_add(type="AREA", location=(4.0, 2.0, 7.0))
fill = bpy.context.object
fill.name = "PR3D_Pizza_Fill"
fill.data.energy = 850
fill.data.size = 4.0
look_at(fill, (0.0, 0.0, 0.0))

blend_path = os.path.join(SOURCE_DIR, "PR3D_006_PizzaVariants.blend")
bpy.ops.wm.save_as_mainfile(filepath=blend_path)

# Export a single mesh contract to Unity. All ten named materials remain in the master source
# and can be assigned without cloning geometry during prefab integration.
bpy.ops.object.select_all(action="DESELECT")
export_single.hide_set(False)
export_single.select_set(True)
bpy.context.view_layer.objects.active = export_single
for target in (
    os.path.join(EXPORT_DIR, "PR3D_PizzaSlice_Shared.fbx"),
    os.path.join(UNITY_DIR, "PR3D_PizzaSlice_Shared.fbx"),
):
    bpy.ops.export_scene.fbx(
        filepath=target,
        use_selection=True,
        object_types={"MESH"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
for target in (
    os.path.join(EXPORT_DIR, "PR3D_PizzaSlice_Shared.glb"),
    os.path.join(UNITY_DIR, "PR3D_PizzaSlice_Shared.glb"),
):
    bpy.ops.export_scene.gltf(
        filepath=target,
        export_format="GLB",
        use_selection=True,
        export_yup=True,
        export_apply=True,
    )
export_single.hide_set(True)

# The ten variants deliberately remain material recipes rather than ten exported
# mesh objects. Unity can instantiate the one canonical mesh and assign these
# four-slot recipes without creating duplicate geometry.
variant_contract = {
    "task": "PR3D-006",
    "units": "meters",
    "concept_reference": "docs/reference/pizza-factory-concept.png",
    "source_asset": "Art/PR3D/Source/Pizza/PR3D_006_PizzaVariants.blend",
    "mesh_asset": "PR3D_PizzaSlice_Shared.fbx",
    "glb_asset": "PR3D_PizzaSlice_Shared.glb",
    "mesh_datablock": shared_mesh.name,
    "mesh_count": 1,
    "geometry": {
        "vertices": len(shared_mesh.vertices),
        "polygons": len(shared_mesh.polygons),
        "footprint_m": [0.86, 0.923],
        "height_m": 0.23,
        "cell_pitch_m": 1.0,
        "silhouette": "rounded triangular slice, chunky toasted rear crust, three oversized topping coins",
    },
    "pivot": "grounded at local origin; slice points toward local -Y in Blender / +Z after Unity FBX conversion",
    "transform_contract": {
        "location": [0.0, 0.0, 0.0],
        "rotation_degrees": [0.0, 0.0, 0.0],
        "scale": [1.0, 1.0, 1.0],
    },
    "unity_contract": {
        "visual_only": True,
        "colliders": 0,
        "scripts": 0,
        "mesh_reuse": "all ten variants must reference the canonical shared mesh",
    },
    "preview": "Evidence/PR3D_006_PizzaVariants.png",
    "material_slots": ["crust", "primary", "accent", "cheese"],
    "variants": [
        {
            "index": index,
            "name": name,
            "primary_rgb": list(primary),
            "accent_rgb": list(accent),
            "material_recipe": [
                "PR3D_MAT_Crust",
                f"PR3D_MAT_{name}",
                f"PR3D_MAT_{name}_Garnish",
                "PR3D_MAT_Cheese",
            ],
        }
        for index, (name, primary, accent) in enumerate(variants)
    ],
}
with open(os.path.join(UNITY_DIR, "PR3D_PizzaVariants.json"), "w", encoding="utf-8") as handle:
    json.dump(variant_contract, handle, ensure_ascii=False, indent=2)

# Restore and render the complete comparison board.
bpy.ops.object.select_all(action="DESELECT")
scene.render.filepath = os.path.join(PREVIEW_DIR, "PR3D_006_PizzaVariants.png")
if not globals().get("PR3D_SKIP_RENDER", False):
    bpy.ops.render.render(write_still=True)

print({
    "task": "PR3D-006",
    "shared_mesh": shared_mesh.name,
    "mesh_users": shared_mesh.users,
    "vertices": len(shared_mesh.vertices),
    "polygons": len(shared_mesh.polygons),
    "variants": [obj.name for obj in variant_objects],
    "blend": blend_path,
    "preview": scene.render.filepath,
})
