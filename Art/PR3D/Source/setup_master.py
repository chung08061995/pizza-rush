"""Build the PR3D Pizza Rush Blender master scene.

Run this script from Blender or Blender MCP with PR3D_REPO_ROOT set to the
repository root. Blender authors in its native Z-up space. The FBX preset
converts exports to Unity's Y-up, +Z-forward coordinate system.
"""

from __future__ import annotations

import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


TASK_ID = "PR3D-002"
MASTER_NAME = "PR3D_PizzaFactory_Master"
# Blender's FBX exporter uses the conventional negative forward-axis token.
# Unity imports this preset as +Z-forward after axis conversion.
EXPORT_AXIS_FORWARD = "-Z"
EXPORT_AXIS_UP = "Y"


def _link_only(obj: bpy.types.Object, collection: bpy.types.Collection) -> None:
    for owner in list(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)


def _collection(parent: bpy.types.Collection, name: str) -> bpy.types.Collection:
    collection = bpy.data.collections.new(name)
    parent.children.link(collection)
    return collection


def _material(
    name: str,
    base_color: tuple[float, float, float, float],
    metallic: float = 0.0,
    roughness: float = 0.45,
) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.diffuse_color = base_color
    material.use_nodes = True
    principled = material.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Base Color"].default_value = base_color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    return material


def _look_at(obj: bpy.types.Object, target: tuple[float, float, float]) -> None:
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def _apply_transform(obj: bpy.types.Object) -> None:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    # Static render children are baked around the shared gameplay/root pivot.
    # This prevents FBX axis conversion from leaving Blender-space offsets on
    # child transforms while rotating only the mesh vertex data.
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    obj.select_set(False)


def _write_contract_text() -> None:
    text = bpy.data.texts.new("PR3D_NAMING_AND_EXPORT.md")
    text.write(
        "# PR3D Blender master scene\n\n"
        "- Units: meters (1 Blender unit = 1 meter).\n"
        "- Authoring: Blender-native Z-up.\n"
        "- Unity result: Y-up, +Z-forward, scale 1.0, apply unit scale.\n"
        "- Root names: `PR3D_<Family>_<Variant>`.\n"
        "- Render children: `Visual`, `GateGlow`, `Topping`.\n"
        "- Collections: 00 reference, 10 gameplay visuals, 20 environment, "
        "80 camera/light, 90 export, 99 guides.\n"
        "- Export only validated objects below `90_EXPORT/PR3D_Export_Root`.\n"
        "- Preserve gameplay roots, colliders, anchors, JSON, enums, and "
        "serialized names.\n"
    )

    preset = bpy.data.texts.new("PR3D_Unity_FBX_Export_Preset.py")
    preset.write(
        "bpy.ops.export_scene.fbx(\n"
        "    filepath=target_path,\n"
        "    use_selection=True,\n"
        "    object_types={'EMPTY', 'MESH'},\n"
        "    global_scale=1.0,\n"
        "    apply_unit_scale=True,\n"
        "    apply_scale_options='FBX_SCALE_UNITS',\n"
        "    use_space_transform=True,\n"
        "    bake_space_transform=False,\n"
        "    axis_forward='-Z',\n"
        "    axis_up='Y',\n"
        "    use_mesh_modifiers=True,\n"
        "    mesh_smooth_type='FACE',\n"
        "    add_leaf_bones=False,\n"
        "    bake_anim=False,\n"
        "    path_mode='AUTO',\n"
        ")\n"
    )


def _create_import_probe(
    export_collection: bpy.types.Collection,
) -> tuple[bpy.types.Object, list[bpy.types.Object]]:
    root = bpy.data.objects.new("PR3D_ImportProbe_Root", None)
    root.empty_display_type = "ARROWS"
    root.empty_display_size = 0.35
    export_collection.objects.link(root)
    root["purpose"] = "Unity scale, pivot, Y-up and +Z-forward validation"

    blue = _material("PR3D_MAT_ProbeBlue", (0.05, 0.25, 0.8, 1.0), 0.15, 0.3)
    orange = _material(
        "PR3D_MAT_ForwardOrange", (1.0, 0.22, 0.025, 1.0), 0.0, 0.32
    )

    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0.0, 0.0, 0.5))
    meter_cube = bpy.context.object
    meter_cube.name = "Visual_MeterCube_1m"
    meter_cube.data.name = "PR3D_MESH_MeterCube_1m"
    meter_cube.data.materials.append(blue)
    meter_cube.parent = root
    _apply_transform(meter_cube)
    _link_only(meter_cube, export_collection)

    # Blender's native -Y direction is converted to Unity +Z by the preset.
    bpy.ops.mesh.primitive_cone_add(
        vertices=3,
        radius1=0.22,
        radius2=0.0,
        depth=0.55,
        location=(0.0, -0.92, 0.22),
        rotation=(math.pi / 2.0, 0.0, 0.0),
    )
    forward_marker = bpy.context.object
    forward_marker.name = "Visual_ForwardZ"
    forward_marker.data.name = "PR3D_MESH_ForwardZ"
    forward_marker.data.materials.append(orange)
    forward_marker.parent = root
    _apply_transform(forward_marker)
    _link_only(forward_marker, export_collection)

    for mesh_obj in (meter_cube, forward_marker):
        bpy.context.view_layer.objects.active = mesh_obj
        mesh_obj.select_set(True)
        if not mesh_obj.data.uv_layers:
            mesh_obj.data.uv_layers.new(name="UVMap")
        mesh_obj.select_set(False)

    return root, [meter_cube, forward_marker]


def _create_board_guide(guides: bpy.types.Collection) -> None:
    guide_material = _material("PR3D_MAT_Guide", (0.05, 0.6, 0.7, 1.0), 0.0, 0.8)
    for index in range(8):
        offset = index - 3.5
        for start, end in (
            ((offset, -3.5, 0.0), (offset, 3.5, 0.0)),
            ((-3.5, offset, 0.0), (3.5, offset, 0.0)),
        ):
            curve_data = bpy.data.curves.new(
                f"PR3D_GUIDE_Grid_{index}_{len(guides.objects)}", "CURVE"
            )
            curve_data.dimensions = "3D"
            curve_data.bevel_depth = 0.008 if index not in (0, 7) else 0.018
            curve_data.materials.append(guide_material)
            spline = curve_data.splines.new("POLY")
            spline.points.add(1)
            spline.points[0].co = (*start, 1.0)
            spline.points[1].co = (*end, 1.0)
            line = bpy.data.objects.new(curve_data.name, curve_data)
            guides.objects.link(line)

    origin = bpy.data.objects.new("PR3D_Guide_Origin", None)
    origin.empty_display_type = "PLAIN_AXES"
    origin.empty_display_size = 0.6
    guides.objects.link(origin)


def _create_reference(
    repo_root: Path, reference_collection: bpy.types.Collection
) -> None:
    concept_path = repo_root / "docs/reference/pizza-factory-concept.png"
    image = bpy.data.images.load(str(concept_path), check_existing=True)
    image.filepath = "//../../../docs/reference/pizza-factory-concept.png"
    reference = bpy.data.objects.new("PR3D_REF_PizzaFactoryConcept", None)
    reference.empty_display_type = "IMAGE"
    reference.data = image
    reference.empty_display_size = 5.5
    reference.color[3] = 0.8
    reference.location = (7.5, 0.0, 0.02)
    reference_collection.objects.link(reference)


def _create_camera_and_lights(
    camera_collection: bpy.types.Collection,
) -> bpy.types.Object:
    camera_data = bpy.data.cameras.new("PR3D_CAM_Portrait_Data")
    camera = bpy.data.objects.new("PR3D_CAM_Portrait", camera_data)
    camera_collection.objects.link(camera)
    camera.location = (8.6, -11.8, 13.5)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 12.5
    camera.data.lens = 50
    camera.data.dof.use_dof = False
    _look_at(camera, (1.7, 0.0, 0.0))
    bpy.context.scene.camera = camera

    area_data = bpy.data.lights.new("PR3D_LGT_Key_Data", "AREA")
    area_data.energy = 1050.0
    area_data.shape = "DISK"
    area_data.size = 5.0
    area = bpy.data.objects.new("PR3D_LGT_Key", area_data)
    area.location = (-4.0, -4.0, 9.0)
    camera_collection.objects.link(area)
    _look_at(area, (0.0, 0.0, 0.0))

    fill_data = bpy.data.lights.new("PR3D_LGT_Fill_Data", "AREA")
    fill_data.energy = 650.0
    fill_data.color = (0.35, 0.55, 1.0)
    fill_data.size = 4.0
    fill = bpy.data.objects.new("PR3D_LGT_Fill", fill_data)
    fill.location = (5.0, 3.0, 7.0)
    camera_collection.objects.link(fill)
    _look_at(fill, (0.0, 0.0, 0.0))
    return camera


def _export_probe(
    root: bpy.types.Object,
    children: list[bpy.types.Object],
    output_path: Path,
) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in children:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=str(output_path),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
        bake_space_transform=False,
        axis_forward=EXPORT_AXIS_FORWARD,
        axis_up=EXPORT_AXIS_UP,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    bpy.ops.object.select_all(action="DESELECT")


def build_master_scene(repo_root: str | os.PathLike[str]) -> dict[str, str]:
    repo = Path(repo_root).resolve()
    source_dir = repo / "Art/PR3D/Source"
    export_dir = repo / "Art/PR3D/Exports"
    preview_dir = repo / "Art/PR3D/Previews"
    for directory in (source_dir, export_dir, preview_dir):
        directory.mkdir(parents=True, exist_ok=True)

    scene = bpy.context.scene
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    for text in list(bpy.data.texts):
        bpy.data.texts.remove(text)

    scene.name = MASTER_NAME
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.unit_settings.length_unit = "METERS"
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1080
    scene.render.resolution_y = 1920
    scene.render.resolution_percentage = 50
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.012, 0.02, 0.055)
    scene["pr3d_task"] = TASK_ID
    scene["coordinate_contract"] = (
        "Blender native Z-up; FBX preset converts to Unity Y-up, +Z-forward"
    )
    scene["unit_contract"] = "1 Blender unit = 1 meter"
    scene["naming_contract"] = (
        "PR3D_<Family>_<Variant>; children use Visual, GateGlow, Topping"
    )
    scene["export_contract"] = (
        "FBX scale 1.0, apply units, axis_forward=-Z, axis_up=Y, leaf bones off"
    )
    scene["concept_reference"] = "//../../../docs/reference/pizza-factory-concept.png"

    top = scene.collection
    reference = _collection(top, "00_REFERENCE")
    gameplay = _collection(top, "10_GAMEPLAY_VISUALS")
    environment = _collection(top, "20_ENVIRONMENT")
    camera_lights = _collection(top, "80_CAMERAS_LIGHTS")
    export = _collection(top, "90_EXPORT")
    guides = _collection(top, "99_GUIDES")
    for name in ("Board", "Rails", "Gates", "Pizza", "Containers", "Ice"):
        _collection(gameplay, name)
    for name in ("Architecture", "Props"):
        _collection(environment, name)

    _write_contract_text()
    _create_reference(repo, reference)
    _create_board_guide(guides)
    _create_camera_and_lights(camera_lights)
    export_root = bpy.data.objects.new("PR3D_Export_Root", None)
    export_root.empty_display_type = "CUBE"
    export_root.empty_display_size = 0.25
    export.objects.link(export_root)
    probe_root, probe_children = _create_import_probe(export)
    probe_root.parent = export_root

    blend_path = source_dir / f"{MASTER_NAME}.blend"
    export_path = export_dir / "PR3D_ImportProbe.fbx"
    preview_path = preview_dir / "PR3D_MasterScene.png"
    _export_probe(probe_root, probe_children, export_path)

    scene.render.filepath = str(preview_path)
    file_preferences = bpy.context.preferences.filepaths
    previous_save_versions = file_preferences.save_version
    file_preferences.save_version = 0
    try:
        bpy.ops.wm.save_as_mainfile(filepath=str(blend_path), relative_remap=False)
        bpy.ops.render.render(write_still=True)
        bpy.data.images["pizza-factory-concept.png"].filepath = (
            "//../../../docs/reference/pizza-factory-concept.png"
        )
        bpy.ops.wm.save_as_mainfile(filepath=str(blend_path), relative_remap=False)
    finally:
        file_preferences.save_version = previous_save_versions

    return {
        "blend": str(blend_path),
        "fbx": str(export_path),
        "preview": str(preview_path),
        "scene": scene.name,
    }


if __name__ == "__main__":
    root = os.environ.get("PR3D_REPO_ROOT")
    if not root:
        raise RuntimeError("Set PR3D_REPO_ROOT before running setup_master.py")
    print(build_master_scene(root))
