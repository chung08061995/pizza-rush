"""Build the PR3D shared color gate and Unity-ready exports.

Run from Blender (normally through Blender MCP). The authored scene uses
Blender's native Z-up coordinates. FBX export converts to Unity Y-up, +Z
forward using the project master preset.
"""

from __future__ import annotations

import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


REPO_ROOT = Path(os.environ["PR3D_REPO_ROOT"]).resolve()
SOURCE_DIR = REPO_ROOT / "Art/PR3D/Source/Gates"
EXPORT_DIR = REPO_ROOT / "Art/PR3D/Exports/Gates"
UNITY_MODEL_DIR = REPO_ROOT / "Assets/_Projects/Art/PR3D/Gates/Models"
PREVIEW_DIR = REPO_ROOT / "Art/PR3D/Previews"

SOURCE_BLEND = SOURCE_DIR / "PR3D_Gates.blend"
EXPORT_FBX = EXPORT_DIR / "PR3D_Gate_Shared.fbx"
EXPORT_GLB = EXPORT_DIR / "PR3D_Gate_Shared.glb"
UNITY_FBX = UNITY_MODEL_DIR / "PR3D_Gate_Shared.fbx"
PREVIEW = PREVIEW_DIR / "PR3D_005_GateVariants.png"

COLORS = {
    "Red": (1.00, 0.055, 0.035, 1.0),
    "Green": (0.12, 0.78, 0.18, 1.0),
    "Blue": (0.035, 0.30, 1.00, 1.0),
    "White": (0.92, 0.96, 1.00, 1.0),
    "Orange": (1.00, 0.27, 0.025, 1.0),
    "Yellow": (1.00, 0.72, 0.025, 1.0),
    "Brown": (0.38, 0.095, 0.025, 1.0),
    "Cyan": (0.015, 0.78, 1.00, 1.0),
    "DarkPurple": (0.26, 0.025, 0.58, 1.0),
    "Pink": (1.00, 0.055, 0.48, 1.0),
}


def material(
    name: str,
    color: tuple[float, float, float, float],
    metallic: float,
    roughness: float,
    emission_strength: float = 0.0,
) -> bpy.types.Material:
    result = bpy.data.materials.new(name)
    result.diffuse_color = color
    result.use_nodes = True
    principled = result.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    if emission_strength > 0.0:
        principled.inputs["Emission Color"].default_value = color
        principled.inputs["Emission Strength"].default_value = emission_strength
    return result


def look_at(obj: bpy.types.Object, target: tuple[float, float, float]) -> None:
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def move_to_collection(
    obj: bpy.types.Object, collection: bpy.types.Collection
) -> None:
    for owner in list(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)


def apply_transform(obj: bpy.types.Object) -> None:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.select_set(False)


def add_box(
    name: str,
    location: tuple[float, float, float],
    scale: tuple[float, float, float],
    mat: bpy.types.Material,
    collection: bpy.types.Collection,
    bevel: float = 0.025,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply_transform(obj)
    obj.data.materials.append(mat)
    modifier = obj.modifiers.new("PR3D_Bevel", "BEVEL")
    modifier.width = bevel
    modifier.segments = 3
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    move_to_collection(obj, collection)
    return obj


def add_bolt(
    name: str,
    location: tuple[float, float, float],
    mat: bpy.types.Material,
    collection: bpy.types.Collection,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=12, ring_count=6, radius=0.032, location=location
    )
    obj = bpy.context.object
    obj.name = name
    obj.scale = (1.0, 0.55, 1.0)
    apply_transform(obj)
    obj.data.materials.append(mat)
    move_to_collection(obj, collection)
    return obj


def build_shared_mesh(
    collection: bpy.types.Collection,
    metal: bpy.types.Material,
    dark: bpy.types.Material,
    accent: bpy.types.Material,
    glow: bpy.types.Material,
) -> bpy.types.Object:
    pieces: list[bpy.types.Object] = []
    # A compact arch that fits the existing one-cell production-line width.
    pieces.extend(
        [
            add_box(
                "Gate_LeftPillar",
                (-0.47, 0.0, 0.34),
                (0.14, 0.17, 0.68),
                metal,
                collection,
            ),
            add_box(
                "Gate_RightPillar",
                (0.47, 0.0, 0.34),
                (0.14, 0.17, 0.68),
                metal,
                collection,
            ),
            add_box(
                "Gate_Top",
                (0.0, 0.0, 0.70),
                (0.82, 0.17, 0.18),
                metal,
                collection,
                0.04,
            ),
            add_box(
                "Gate_LeftInset",
                (-0.47, -0.176, 0.34),
                (0.075, 0.025, 0.48),
                dark,
                collection,
                0.016,
            ),
            add_box(
                "Gate_RightInset",
                (0.47, -0.176, 0.34),
                (0.075, 0.025, 0.48),
                dark,
                collection,
                0.016,
            ),
            add_box(
                "Gate_TopInset",
                (0.0, -0.176, 0.70),
                (0.60, 0.025, 0.09),
                dark,
                collection,
                0.016,
            ),
            add_box(
                "Gate_AccentLeft",
                (-0.47, -0.205, 0.34),
                (0.045, 0.018, 0.39),
                accent,
                collection,
                0.012,
            ),
            add_box(
                "Gate_AccentRight",
                (0.47, -0.205, 0.34),
                (0.045, 0.018, 0.39),
                accent,
                collection,
                0.012,
            ),
            add_box(
                "Gate_GlowTop",
                (0.0, -0.207, 0.70),
                (0.52, 0.017, 0.045),
                glow,
                collection,
                0.012,
            ),
            add_box(
                "Gate_GlowLeft",
                (-0.47, -0.225, 0.34),
                (0.020, 0.012, 0.31),
                glow,
                collection,
                0.008,
            ),
            add_box(
                "Gate_GlowRight",
                (0.47, -0.225, 0.34),
                (0.020, 0.012, 0.31),
                glow,
                collection,
                0.008,
            ),
        ]
    )
    for x in (-0.52, 0.52):
        for z in (0.12, 0.57):
            pieces.append(
                add_bolt(
                    f"Gate_Bolt_{x:+.2f}_{z:.2f}",
                    (x, -0.185, z),
                    metal,
                    collection,
                )
            )

    bpy.ops.object.select_all(action="DESELECT")
    for piece in pieces:
        piece.select_set(True)
    bpy.context.view_layer.objects.active = pieces[0]
    bpy.ops.object.join()
    shared = bpy.context.object
    shared.name = "Visual_Gate"
    shared.data.name = "PR3D_MESH_Gate_Shared"
    shared["pr3d_contract"] = "visual-only; root/entry/exit gameplay transforms unchanged"
    shared["unity_bounds_m"] = "1.10 x 0.45 x 0.79"
    if not shared.data.uv_layers:
        shared.data.uv_layers.new(name="UVMap")
    for polygon in shared.data.polygons:
        polygon.use_smooth = True
    shared.select_set(False)
    return shared


def set_object_materials(
    obj: bpy.types.Object,
    metal: bpy.types.Material,
    dark: bpy.types.Material,
    accent: bpy.types.Material,
    glow: bpy.types.Material,
) -> None:
    replacements = {
        "PR3D_MAT_GateMetal": metal,
        "PR3D_MAT_GateDark": dark,
        "PR3D_MAT_GateAccent_Default": accent,
        "PR3D_MAT_GateGlow_Default": glow,
    }
    for slot in obj.material_slots:
        source_name = slot.material.name if slot.material else ""
        slot.link = "OBJECT"
        if source_name in replacements:
            slot.material = replacements[source_name]


def add_contract_root(
    collection: bpy.types.Collection, shared: bpy.types.Object
) -> tuple[bpy.types.Object, list[bpy.types.Object]]:
    root = bpy.data.objects.new("PR3D_Gate_Root", None)
    root.empty_display_type = "ARROWS"
    root.empty_display_size = 0.18
    root["pivot"] = "Unity local (0,0,0); ground-centered gate visual"
    root["forward"] = "Unity +Z after FBX conversion"
    collection.objects.link(root)
    shared.parent = root

    # Blender +Y becomes Unity -Z, Blender -Y becomes Unity +Z.
    entry = bpy.data.objects.new("Entry", None)
    entry.empty_display_type = "CONE"
    entry.empty_display_size = 0.10
    entry.location = (0.0, 0.25, 0.0)
    entry.parent = root
    collection.objects.link(entry)

    exit_anchor = bpy.data.objects.new("Exit", None)
    exit_anchor.empty_display_type = "CONE"
    exit_anchor.empty_display_size = 0.10
    exit_anchor.location = (0.0, -0.25, 0.0)
    exit_anchor.rotation_euler.z = math.pi
    exit_anchor.parent = root
    collection.objects.link(exit_anchor)
    return root, [shared, entry, exit_anchor]


def export_selection(
    root: bpy.types.Object,
    children: list[bpy.types.Object],
) -> None:
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    UNITY_MODEL_DIR.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in children:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=str(EXPORT_FBX),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
        bake_space_transform=False,
        axis_forward="-Z",
        axis_up="Y",
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    bpy.ops.export_scene.gltf(
        filepath=str(EXPORT_GLB),
        export_format="GLB",
        use_selection=True,
        export_yup=True,
        export_apply=True,
        export_materials="EXPORT",
    )
    UNITY_FBX.write_bytes(EXPORT_FBX.read_bytes())
    bpy.ops.object.select_all(action="DESELECT")


def build_preview(
    collection: bpy.types.Collection,
    shared: bpy.types.Object,
    metal: bpy.types.Material,
    dark: bpy.types.Material,
) -> None:
    preview_collection = bpy.data.collections.new("PR3D_005_PREVIEW")
    bpy.context.scene.collection.children.link(preview_collection)
    shared.hide_render = True

    for index, (name, color) in enumerate(COLORS.items()):
        accent = material(
            f"PR3D_MAT_GateAccent_{name}", color, metallic=0.08, roughness=0.28
        )
        glow = material(
            f"PR3D_MAT_GateGlow_{name}",
            color,
            metallic=0.0,
            roughness=0.18,
            emission_strength=5.0,
        )
        instance = bpy.data.objects.new(
            f"PR3D_Gate_{name}", shared.data
        )
        instance.location = ((index % 5 - 2) * 1.42, (index // 5 - 0.5) * 1.55, 0)
        preview_collection.objects.link(instance)
        set_object_materials(instance, metal, dark, accent, glow)
        instance["variant"] = name
        instance["shared_mesh"] = shared.data.name

        text_curve = bpy.data.curves.new(f"Label_{name}", "FONT")
        text_curve.body = name
        text_curve.align_x = "CENTER"
        text_curve.size = 0.18
        label = bpy.data.objects.new(f"Label_{name}", text_curve)
        label.location = (instance.location.x, instance.location.y - 0.52, 0.03)
        label.rotation_euler = (0.0, 0.0, 0.0)
        preview_collection.objects.link(label)

    floor_mat = material("PR3D_MAT_PreviewFloor", (0.035, 0.045, 0.07, 1), 0.1, 0.34)
    add_box(
        "PR3D_PreviewFloor",
        (0.0, 0.0, -0.08),
        (7.5, 4.4, 0.12),
        floor_mat,
        preview_collection,
        0.06,
    )

    camera_data = bpy.data.cameras.new("PR3D_CAM_Gates_Data")
    camera = bpy.data.objects.new("PR3D_CAM_Gates", camera_data)
    preview_collection.objects.link(camera)
    camera.location = (0.0, -9.0, 7.2)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 7.8
    look_at(camera, (0.0, 0.0, 0.35))
    bpy.context.scene.camera = camera

    key_data = bpy.data.lights.new("PR3D_LGT_Gates_Key_Data", "AREA")
    key_data.energy = 1150.0
    key_data.shape = "RECTANGLE"
    key_data.size = 5.0
    key = bpy.data.objects.new("PR3D_LGT_Gates_Key", key_data)
    key.location = (-3.0, -4.0, 7.5)
    preview_collection.objects.link(key)
    look_at(key, (0.0, 0.0, 0.0))

    rim_data = bpy.data.lights.new("PR3D_LGT_Gates_Rim_Data", "AREA")
    rim_data.energy = 850.0
    rim_data.color = (0.25, 0.45, 1.0)
    rim_data.size = 4.0
    rim = bpy.data.objects.new("PR3D_LGT_Gates_Rim", rim_data)
    rim.location = (3.5, 3.0, 5.5)
    preview_collection.objects.link(rim)
    look_at(rim, (0.0, 0.0, 0.3))

    scene = bpy.context.scene
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1000
    scene.render.resolution_y = 720
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.008, 0.012, 0.025)
    scene.render.filepath = str(PREVIEW)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_BLEND))
    bpy.ops.render.render(write_still=True)


def main() -> None:
    for directory in (SOURCE_DIR, EXPORT_DIR, UNITY_MODEL_DIR, PREVIEW_DIR):
        directory.mkdir(parents=True, exist_ok=True)
    # Do not call read_factory_settings from Blender MCP: it also unloads the
    # add-on that owns the active socket. Clear data in place instead.
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    for datablocks in (
        bpy.data.meshes,
        bpy.data.curves,
        bpy.data.materials,
        bpy.data.cameras,
        bpy.data.lights,
    ):
        for datablock in list(datablocks):
            if datablock.users == 0:
                datablocks.remove(datablock)
    scene = bpy.context.scene
    scene.name = "PR3D_Gates"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.unit_settings.length_unit = "METERS"

    export_collection = bpy.data.collections.new("90_EXPORT_GATES")
    scene.collection.children.link(export_collection)
    metal = material("PR3D_MAT_GateMetal", (0.34, 0.38, 0.44, 1), 0.72, 0.24)
    dark = material("PR3D_MAT_GateDark", (0.018, 0.025, 0.045, 1), 0.42, 0.30)
    accent = material(
        "PR3D_MAT_GateAccent_Default", COLORS["Red"], 0.08, 0.28
    )
    glow = material(
        "PR3D_MAT_GateGlow_Default", COLORS["Red"], 0.0, 0.18, 5.0
    )
    shared = build_shared_mesh(export_collection, metal, dark, accent, glow)
    root, children = add_contract_root(export_collection, shared)
    export_selection(root, children)
    build_preview(export_collection, shared, metal, dark)
    print(
        {
            "task": "PR3D-005",
            "mesh": shared.data.name,
            "vertices": len(shared.data.vertices),
            "polygons": len(shared.data.polygons),
            "variants": list(COLORS),
            "fbx": str(EXPORT_FBX),
            "glb": str(EXPORT_GLB),
            "preview": str(PREVIEW),
            "source": str(SOURCE_BLEND),
        }
    )


if __name__ == "__main__":
    main()
