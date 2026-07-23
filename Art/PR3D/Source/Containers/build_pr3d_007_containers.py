"""Build the PR3D-007 additive container visual kit through Blender MCP.

The exported roots are visual-only children intended for the integration phase.
They contain no collider or gameplay component and preserve the one-metre grid
pitch used by the existing Level 301 board.
"""

from pathlib import Path
import math

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[4]
SOURCE_DIR = ROOT / "Art/PR3D/Source/Containers"
EXPORT_DIR = ROOT / "Art/PR3D/Exports/Containers"
UNITY_DIR = ROOT / "Assets/_Projects/Art/PR3D/Containers"
PREVIEW = ROOT / "Art/PR3D/Previews/PR3D_007_ContainerKit.png"
BLEND = SOURCE_DIR / "PR3D_Containers_Master.blend"

CELL_PITCH = 1.0
CELL_FOOTPRINT = 0.86
BASE_HEIGHT = 0.22
RIM_HEIGHT = 0.12

SHAPES = {
    "1x1": [(0, 0)],
    "1x2": [(0, 0), (0, 1)],
    "1x3": [(0, 0), (0, 1), (0, 2)],
    "T": [(0, 0), (-1, 1), (0, 1), (1, 1)],
}


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.collections, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)


def material(name, color, metallic=0.0, roughness=0.45, transmission=0.0, alpha=1.0):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = (*color, alpha)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*color, 1.0)
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if "Transmission Weight" in bsdf.inputs:
        bsdf.inputs["Transmission Weight"].default_value = transmission
    if alpha < 1.0:
        bsdf.inputs["Alpha"].default_value = alpha
        mat.surface_render_method = "DITHERED"
    return mat


def rounded_cube(name, location, scale, mat, bevel=0.08, collection=None):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = (scale[0] * 0.5, scale[1] * 0.5, scale[2] * 0.5)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    modifier = obj.modifiers.new("Soft_Edges", "BEVEL")
    modifier.width = bevel
    modifier.segments = 3
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.data.materials.append(mat)
    for polygon in obj.data.polygons:
        polygon.use_smooth = False
    if collection is not None:
        for owner in tuple(obj.users_collection):
            owner.objects.unlink(obj)
        collection.objects.link(obj)
    return obj


def torus(name, location, major_radius, minor_radius, mat, collection):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius,
        minor_radius=minor_radius,
        major_segments=24,
        minor_segments=6,
        location=location,
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    for owner in tuple(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)
    return obj


def add_root(name, collection, contract):
    root = bpy.data.objects.new(name, None)
    root.empty_display_type = "PLAIN_AXES"
    root.empty_display_size = 0.25
    root["visual_only"] = True
    root["cell_pitch_m"] = CELL_PITCH
    root["pivot_contract"] = "first occupied cell center; Unity local origin"
    root["occupied_cells"] = str(contract)
    collection.objects.link(root)
    return root


def parent_keep_local(obj, root):
    obj.parent = root


def build_shape(asset_name, cells, body_mat, rim_mat, inset_mat):
    collection = bpy.data.collections.new(f"ASSET_{asset_name}")
    bpy.context.scene.collection.children.link(collection)
    root = add_root(f"PR3D_Container_{asset_name}_Root", collection, cells)

    for index, (x, y) in enumerate(cells):
        loc_x, loc_y = x * CELL_PITCH, y * CELL_PITCH
        base = rounded_cube(
            f"Visual_{asset_name}_Base_{index:02d}",
            (loc_x, loc_y, BASE_HEIGHT * 0.5),
            (CELL_FOOTPRINT, CELL_FOOTPRINT, BASE_HEIGHT),
            body_mat,
            0.11,
            collection,
        )
        inset = rounded_cube(
            f"Visual_{asset_name}_Inset_{index:02d}",
            (loc_x, loc_y, BASE_HEIGHT + 0.035),
            (0.62, 0.62, 0.055),
            inset_mat,
            0.12,
            collection,
        )
        rim = torus(
            f"Visual_{asset_name}_Rim_{index:02d}",
            (loc_x, loc_y, BASE_HEIGHT + 0.075),
            0.365,
            0.045,
            rim_mat,
            collection,
        )
        for obj in (base, inset, rim):
            parent_keep_local(obj, root)
    return collection, root


def build_ice(ice_mat, frost_mat):
    cells = SHAPES["1x1"]
    collection = bpy.data.collections.new("ASSET_Ice")
    bpy.context.scene.collection.children.link(collection)
    root = add_root("PR3D_Container_Ice_Root", collection, cells)

    shell = rounded_cube(
        "Visual_Ice_ClearShell",
        (0, 0, 0.31),
        (0.92, 0.92, 0.58),
        ice_mat,
        0.16,
        collection,
    )
    core = rounded_cube(
        "Visual_Ice_FrostedCore",
        (0, 0, 0.25),
        (0.68, 0.68, 0.38),
        frost_mat,
        0.12,
        collection,
    )
    parent_keep_local(shell, root)
    parent_keep_local(core, root)

    for index, angle in enumerate((25, 145, 265)):
        radians = math.radians(angle)
        shard = rounded_cube(
            f"Visual_Ice_Highlight_{index:02d}",
            (math.cos(radians) * 0.31, math.sin(radians) * 0.31, 0.48),
            (0.08, 0.19, 0.10),
            frost_mat,
            0.035,
            collection,
        )
        shard.rotation_euler.z = radians
        bpy.context.view_layer.objects.active = shard
        shard.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        shard.select_set(False)
        parent_keep_local(shard, root)
    return collection, root


def select_collection(collection):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in collection.all_objects:
        obj.hide_set(False)
        obj.select_set(True)
    roots = [obj for obj in collection.objects if obj.parent is None]
    if roots:
        bpy.context.view_layer.objects.active = roots[0]


def export_asset(label, collection):
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    UNITY_DIR.mkdir(parents=True, exist_ok=True)
    select_collection(collection)
    for target in (
        EXPORT_DIR / f"PR3D_Container_{label}.fbx",
        UNITY_DIR / f"PR3D_Container_{label}.fbx",
    ):
        bpy.ops.export_scene.fbx(
            filepath=str(target),
            use_selection=True,
            object_types={"EMPTY", "MESH"},
            apply_unit_scale=True,
            apply_scale_options="FBX_SCALE_UNITS",
            axis_forward="-Z",
            axis_up="Y",
            add_leaf_bones=False,
            bake_anim=False,
        )
    bpy.ops.export_scene.gltf(
        filepath=str(EXPORT_DIR / f"PR3D_Container_{label}.glb"),
        export_format="GLB",
        use_selection=True,
        export_yup=True,
        export_apply=True,
    )


def bounds(collection):
    points = [
        obj.matrix_world @ Vector(corner)
        for obj in collection.all_objects
        if obj.type == "MESH"
        for corner in obj.bound_box
    ]
    minimum = [min(point[i] for point in points) for i in range(3)]
    maximum = [max(point[i] for point in points) for i in range(3)]
    return {
        "min": [round(v, 4) for v in minimum],
        "max": [round(v, 4) for v in maximum],
        "size": [round(maximum[i] - minimum[i], 4) for i in range(3)],
    }


reset_scene()
scene = bpy.context.scene
scene.name = "PR3D_Containers_Master"
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene["pr3d_task"] = "PR3D-007"
scene["integration"] = "additive visual child only; no gameplay contract"

body = material("PR3D_MAT_Container_Body_Default", (0.89, 0.17, 0.09), roughness=0.30)
rim = material("PR3D_MAT_Container_Rim", (1.0, 0.72, 0.22), metallic=0.15, roughness=0.23)
inset = material("PR3D_MAT_Container_Interior", (0.28, 0.035, 0.025), roughness=0.42)
ice = material("PR3D_MAT_Ice_Clear", (0.34, 0.82, 1.0), roughness=0.10, transmission=0.25, alpha=0.62)
frost = material("PR3D_MAT_Ice_Frost", (0.80, 0.96, 1.0), roughness=0.28, alpha=0.88)

assets = {}
for label, cells in SHAPES.items():
    assets[label] = build_shape(label, cells, body, rim, inset)
assets["Ice"] = build_ice(ice, frost)

for label, (collection, _) in assets.items():
    export_asset(label, collection)
    collection.hide_viewport = True
    collection.hide_render = True

# Arrange linked preview copies away from asset origins without changing exports.
preview_collection = bpy.data.collections.new("PREVIEW_PR3D007")
scene.collection.children.link(preview_collection)
placements = {
    "1x1": (-3.2, 1.6, 0),
    "1x2": (-1.7, 1.1, 0),
    "1x3": (0.0, 0.6, 0),
    "T": (2.2, 0.8, 0),
    "Ice": (4.1, 1.6, 0),
}
for label, (collection, _) in assets.items():
    preview_root = bpy.data.objects.new(f"PREVIEW_{label}", None)
    preview_root.location = placements[label]
    preview_collection.objects.link(preview_root)
    for source in collection.all_objects:
        if source.type != "MESH":
            continue
        duplicate = source.copy()
        duplicate.data = source.data
        duplicate.name = f"PREVIEW_{source.name}"
        duplicate.parent = preview_root
        preview_collection.objects.link(duplicate)

bpy.ops.object.camera_add(location=(0.5, -10.8, 8.2), rotation=(math.radians(55), 0, 0))
camera = bpy.context.object
camera.name = "PR3D007_PREVIEW_Camera"
scene.camera = camera

def point_camera(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()

point_camera(camera, (0.5, 1.25, 0.25))
camera.data.type = "ORTHO"
camera.data.ortho_scale = 8.7

bpy.ops.object.light_add(type="AREA", location=(-3.5, -2.5, 7.0))
key = bpy.context.object
key.name = "PR3D007_PREVIEW_Key"
key.data.energy = 1100
key.data.shape = "DISK"
key.data.size = 5.0
point_camera(key, (0.5, 1.2, 0))
bpy.ops.object.light_add(type="AREA", location=(4.8, 1.0, 5.0))
fill = bpy.context.object
fill.name = "PR3D007_PREVIEW_Fill"
fill.data.energy = 850
fill.data.size = 4.0
point_camera(fill, (1.0, 1.2, 0.2))

world = scene.world or bpy.data.worlds.new("PR3D007_World")
scene.world = world
world.color = (0.025, 0.035, 0.055)
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 1200
scene.render.resolution_y = 700
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.filepath = str(PREVIEW)
scene.render.film_transparent = False

SOURCE_DIR.mkdir(parents=True, exist_ok=True)
PREVIEW.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
bpy.ops.render.render(write_still=True)

print(
    {
        "task": "PR3D-007",
        "source": str(BLEND),
        "preview": str(PREVIEW),
        "unit_scale_m": scene.unit_settings.scale_length,
        "asset_bounds_m": {label: bounds(data[0]) for label, data in assets.items()},
        "roots": {label: data[1].name for label, data in assets.items()},
        "exports": sorted(path.name for path in EXPORT_DIR.glob("PR3D_Container_*")),
    }
)
