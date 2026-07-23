"""Build the PR3D-003 board and tile kit in the open PR3D master scene.

Run this file from Blender 5.2, preferably through Blender MCP:

    exec(
        compile(open(<absolute path>, "rb").read(), <absolute path>, "exec"),
        {"__file__": <absolute path>, "__name__": "__main__"},
    )

The script is intentionally deterministic. It preserves the master-scene
coordinate contract, replaces only objects owned by PR3D-003, exports the two
art roots, renders a review preview, and saves a new master scene in this
worktree.
"""

from __future__ import annotations

import math
from pathlib import Path

import bpy


TASK_PREFIX = "PR3D003_"
CELL_PITCH_M = 1.0
GRID_SIZE = 7
WORKTREE_ROOT = Path(__file__).resolve().parents[3]
ART_ROOT = WORKTREE_ROOT / "Art" / "PR3D"
SOURCE_BLEND = ART_ROOT / "Source" / "PR3D_PizzaFactory_Master.blend"
EXPORT_ROOT = ART_ROOT / "Exports" / "Board"
UNITY_MODEL_ROOT = (
    WORKTREE_ROOT / "Assets" / "_Projects" / "Art" / "PR3D" / "Board" / "Models"
)
PREVIEW_PATH = ART_ROOT / "Previews" / "PR3D_003_BoardTile.png"


def ensure_directories() -> None:
    for path in (SOURCE_BLEND.parent, EXPORT_ROOT, UNITY_MODEL_ROOT, PREVIEW_PATH.parent):
        path.mkdir(parents=True, exist_ok=True)


def get_or_create_collection(name: str, parent: bpy.types.Collection | None = None):
    collection = bpy.data.collections.get(name)
    if collection is None:
        collection = bpy.data.collections.new(name)
        (parent or bpy.context.scene.collection).children.link(collection)
    return collection


def unlink_from_all_collections(obj: bpy.types.Object) -> None:
    for collection in tuple(obj.users_collection):
        collection.objects.unlink(obj)


def move_to_collection(obj: bpy.types.Object, collection: bpy.types.Collection) -> None:
    unlink_from_all_collections(obj)
    collection.objects.link(obj)


def remove_owned_objects() -> None:
    for obj in list(bpy.data.objects):
        if obj.name.startswith(TASK_PREFIX) or obj.name in {
            "PR3D_Board_7x7",
            "PR3D_Tile_Ceramic",
        }:
            bpy.data.objects.remove(obj, do_unlink=True)

    for collection in list(bpy.data.collections):
        if collection.name.startswith(TASK_PREFIX):
            bpy.data.collections.remove(collection)

    for material in list(bpy.data.materials):
        if material.name.startswith(TASK_PREFIX):
            bpy.data.materials.remove(material)


def make_material(
    name: str,
    color: tuple[float, float, float, float],
    metallic: float,
    roughness: float,
) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    principled = material.node_tree.nodes.get("Principled BSDF")
    principled.inputs["Base Color"].default_value = color
    principled.inputs["Metallic"].default_value = metallic
    principled.inputs["Roughness"].default_value = roughness
    return material


def apply_modifier(obj: bpy.types.Object, modifier_name: str) -> None:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier_name)
    obj.select_set(False)


def add_rounded_box(
    name: str,
    size: tuple[float, float, float],
    location: tuple[float, float, float],
    bevel: float,
    material: bpy.types.Material,
    collection: bpy.types.Collection,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel_modifier = obj.modifiers.new("EdgeSoftening", "BEVEL")
    bevel_modifier.width = bevel
    bevel_modifier.segments = 3
    bevel_modifier.limit_method = "ANGLE"
    apply_modifier(obj, bevel_modifier.name)
    obj.data.materials.append(material)
    move_to_collection(obj, collection)
    return obj


def rounded_rectangle_points(
    half_x: float,
    half_y: float,
    radius: float,
    segments_per_corner: int = 8,
) -> list[tuple[float, float]]:
    points: list[tuple[float, float]] = []
    corners = (
        (half_x - radius, half_y - radius, 0.0),
        (-half_x + radius, half_y - radius, 90.0),
        (-half_x + radius, -half_y + radius, 180.0),
        (half_x - radius, -half_y + radius, 270.0),
    )
    for center_x, center_y, start_degrees in corners:
        for index in range(segments_per_corner):
            angle = math.radians(start_degrees + index * 90.0 / segments_per_corner)
            points.append(
                (
                    center_x + radius * math.cos(angle),
                    center_y + radius * math.sin(angle),
                )
            )
    return points


def add_rounded_frame(
    name: str,
    outer_size: float,
    inner_size: float,
    bottom_z: float,
    top_z: float,
    material: bpy.types.Material,
    collection: bpy.types.Collection,
) -> bpy.types.Object:
    outer = rounded_rectangle_points(outer_size / 2.0, outer_size / 2.0, 0.42)
    inner = rounded_rectangle_points(inner_size / 2.0, inner_size / 2.0, 0.22)
    point_count = len(outer)
    vertices = (
        [(x, y, bottom_z) for x, y in outer]
        + [(x, y, top_z) for x, y in outer]
        + [(x, y, bottom_z) for x, y in inner]
        + [(x, y, top_z) for x, y in inner]
    )
    faces: list[tuple[int, int, int, int]] = []
    for index in range(point_count):
        next_index = (index + 1) % point_count
        outer_bottom = index
        outer_top = point_count + index
        inner_bottom = 2 * point_count + index
        inner_top = 3 * point_count + index
        next_outer_bottom = next_index
        next_outer_top = point_count + next_index
        next_inner_bottom = 2 * point_count + next_index
        next_inner_top = 3 * point_count + next_index
        faces.extend(
            (
                (outer_top, next_outer_top, next_inner_top, inner_top),
                (next_outer_bottom, outer_bottom, inner_bottom, next_inner_bottom),
                (outer_bottom, next_outer_bottom, next_outer_top, outer_top),
                (next_inner_bottom, inner_bottom, inner_top, next_inner_top),
            )
        )

    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    frame = bpy.data.objects.new(name, mesh)
    collection.objects.link(frame)
    frame.data.materials.append(material)

    bevel_modifier = frame.modifiers.new("EdgeSoftening", "BEVEL")
    bevel_modifier.width = 0.045
    bevel_modifier.segments = 2
    bevel_modifier.limit_method = "ANGLE"
    apply_modifier(frame, bevel_modifier.name)

    bpy.context.view_layer.objects.active = frame
    frame.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(66.0), island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    frame.select_set(False)
    return frame


def parent_keep_transform(child: bpy.types.Object, parent: bpy.types.Object) -> None:
    world_matrix = child.matrix_world.copy()
    child.parent = parent
    child.matrix_world = world_matrix


def make_asset_roots(gameplay_collection: bpy.types.Collection):
    board_root = bpy.data.objects.new("PR3D_Board_7x7", None)
    tile_root = bpy.data.objects.new("PR3D_Tile_Ceramic", None)
    gameplay_collection.objects.link(board_root)
    gameplay_collection.objects.link(tile_root)

    blue = make_material(
        f"{TASK_PREFIX}MAT_FrameBlue",
        (0.025, 0.19, 0.48, 1.0),
        metallic=0.22,
        roughness=0.25,
    )
    tray_blue = make_material(
        f"{TASK_PREFIX}MAT_TrayBlue",
        (0.018, 0.095, 0.24, 1.0),
        metallic=0.10,
        roughness=0.34,
    )
    ceramic = make_material(
        f"{TASK_PREFIX}MAT_CeramicCream",
        (1.0, 0.69, 0.43, 1.0),
        metallic=0.0,
        roughness=0.38,
    )
    ceramic_top = make_material(
        f"{TASK_PREFIX}MAT_CeramicHighlight",
        (1.0, 0.82, 0.62, 1.0),
        metallic=0.0,
        roughness=0.30,
    )

    tray = add_rounded_box(
        f"{TASK_PREFIX}Visual_TrayBase",
        (7.20, 7.20, 0.14),
        (0.0, 0.0, -0.09),
        0.18,
        tray_blue,
        gameplay_collection,
    )
    frame = add_rounded_frame(
        f"{TASK_PREFIX}Visual_Frame",
        outer_size=7.78,
        inner_size=7.10,
        bottom_z=-0.10,
        top_z=0.20,
        material=blue,
        collection=gameplay_collection,
    )
    tile_base = add_rounded_box(
        f"{TASK_PREFIX}Visual_TileBase",
        (0.90, 0.90, 0.10),
        (0.0, 0.0, 0.05),
        0.085,
        ceramic,
        gameplay_collection,
    )
    tile_inset = add_rounded_box(
        f"{TASK_PREFIX}Visual_TileInset",
        (0.76, 0.76, 0.025),
        (0.0, 0.0, 0.105),
        0.065,
        ceramic_top,
        gameplay_collection,
    )

    for child in (tray, frame):
        parent_keep_transform(child, board_root)
    for child in (tile_base, tile_inset):
        parent_keep_transform(child, tile_root)

    board_root["pr3d_task"] = "PR3D-003"
    board_root["grid_size"] = GRID_SIZE
    board_root["cell_pitch_m"] = CELL_PITCH_M
    board_root["unity_placement"] = "center at grid world (4.5, 0, 4.5)"
    tile_root["pr3d_task"] = "PR3D-003"
    tile_root["cell_footprint_m"] = [1.0, 1.0]
    tile_root["pivot_contract"] = "cell center at gameplay plane"
    return board_root, tile_root


def select_hierarchy(root: bpy.types.Object) -> list[bpy.types.Object]:
    selected = [root, *root.children_recursive]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in selected:
        obj.hide_set(False)
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    return selected


def export_fbx(root: bpy.types.Object, filepath: Path) -> None:
    select_hierarchy(root)
    bpy.ops.export_scene.fbx(
        filepath=str(filepath),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=False,
        add_leaf_bones=False,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        path_mode="AUTO",
    )


def export_glb(root: bpy.types.Object, filepath: Path) -> None:
    select_hierarchy(root)
    bpy.ops.export_scene.gltf(
        filepath=str(filepath),
        export_format="GLB",
        use_selection=True,
        export_yup=True,
        export_apply=True,
        export_materials="EXPORT",
    )


def add_preview_layout(
    board_root: bpy.types.Object,
    tile_root: bpy.types.Object,
    preview_collection: bpy.types.Collection,
) -> None:
    preview_root = bpy.data.objects.new(f"{TASK_PREFIX}PREVIEW_Root", None)
    preview_collection.objects.link(preview_root)

    tile_meshes = [obj for obj in tile_root.children_recursive if obj.type == "MESH"]
    for row in range(GRID_SIZE):
        for column in range(GRID_SIZE):
            x = (column - (GRID_SIZE - 1) / 2.0) * CELL_PITCH_M
            y = ((GRID_SIZE - 1) / 2.0 - row) * CELL_PITCH_M
            for source in tile_meshes:
                duplicate = bpy.data.objects.new(
                    f"{TASK_PREFIX}PREVIEW_Tile_{row}_{column}_{source.name}",
                    source.data,
                )
                preview_collection.objects.link(duplicate)
                duplicate.location = (x, y, source.location.z)
                duplicate.rotation_euler = source.rotation_euler
                duplicate.parent = preview_root

    backdrop = add_rounded_box(
        f"{TASK_PREFIX}PREVIEW_Backdrop",
        (13.0, 15.0, 0.12),
        (0.0, 0.0, -0.28),
        0.35,
        make_material(
            f"{TASK_PREFIX}MAT_PreviewWood",
            (0.18, 0.055, 0.022, 1.0),
            metallic=0.0,
            roughness=0.58,
        ),
        preview_collection,
    )
    backdrop.parent = preview_root

    for obj in (tile_root, *tile_root.children_recursive):
        obj.hide_render = True
    for obj in (board_root, *board_root.children_recursive):
        obj.hide_render = False
    import_probe = bpy.data.objects.get("PR3D_ImportProbe_Root")
    if import_probe is not None:
        for obj in (import_probe, *import_probe.children_recursive):
            obj.hide_render = True


def point_camera(camera: bpy.types.Object, target=(0.0, 0.0, 0.0)) -> None:
    direction = mathutils.Vector(target) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def render_preview() -> None:
    import mathutils

    scene = bpy.context.scene
    camera = bpy.data.objects.get("PR3D_CAM_Portrait")
    if camera is None or camera.type != "CAMERA":
        camera_data = bpy.data.cameras.new("PR3D_CAM_Portrait_Data")
        camera = bpy.data.objects.new("PR3D_CAM_Portrait", camera_data)
        bpy.context.scene.collection.objects.link(camera)

    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 11.0
    camera.location = (8.2, -10.5, 13.8)
    direction = mathutils.Vector((0.0, 0.0, 0.0)) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    scene.camera = camera

    key = bpy.data.objects.get("PR3D_LGT_Key")
    if key and key.type == "LIGHT":
        key.data.energy = 1300.0
        key.data.color = (1.0, 0.54, 0.32)
        key.data.shape = "DISK"
        key.data.size = 5.0
    fill = bpy.data.objects.get("PR3D_LGT_Fill")
    if fill and fill.type == "LIGHT":
        fill.data.energy = 900.0
        fill.data.color = (0.24, 0.48, 1.0)
        fill.data.size = 6.0

    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 900
    scene.render.resolution_y = 1200
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(PREVIEW_PATH)
    scene.render.film_transparent = False
    scene.world.color = (0.008, 0.015, 0.04)
    scene.render.image_settings.color_mode = "RGBA"
    bpy.ops.render.render(write_still=True)


def triangle_count(root: bpy.types.Object) -> int:
    count = 0
    for obj in root.children_recursive:
        if obj.type == "MESH":
            count += sum(max(0, len(polygon.vertices) - 2) for polygon in obj.data.polygons)
    return count


def validate_asset(root: bpy.types.Object) -> None:
    assert tuple(round(value, 6) for value in root.location) == (0.0, 0.0, 0.0)
    assert tuple(round(value, 6) for value in root.scale) == (1.0, 1.0, 1.0)
    for obj in root.children_recursive:
        if obj.type != "MESH":
            continue
        assert len(obj.data.uv_layers) > 0, f"{obj.name} is missing UVs"
        assert tuple(round(value, 6) for value in obj.scale) == (1.0, 1.0, 1.0)


def main() -> None:
    ensure_directories()
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    remove_owned_objects()

    gameplay = get_or_create_collection("10_GAMEPLAY_VISUALS")
    board_collection = get_or_create_collection("Board", gameplay)
    preview_collection = get_or_create_collection(f"{TASK_PREFIX}PREVIEW")
    board_root, tile_root = make_asset_roots(board_collection)

    validate_asset(board_root)
    validate_asset(tile_root)
    board_root["triangle_count"] = triangle_count(board_root)
    tile_root["triangle_count"] = triangle_count(tile_root)

    export_fbx(board_root, UNITY_MODEL_ROOT / "PR3D_Board_7x7.fbx")
    export_fbx(tile_root, UNITY_MODEL_ROOT / "PR3D_Tile_Ceramic.fbx")
    export_glb(board_root, EXPORT_ROOT / "PR3D_Board_7x7.glb")
    export_glb(tile_root, EXPORT_ROOT / "PR3D_Tile_Ceramic.glb")

    add_preview_layout(board_root, tile_root, preview_collection)
    render_preview()
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_BLEND))

    print(
        {
            "task": "PR3D-003",
            "master": str(SOURCE_BLEND),
            "board_fbx": str(UNITY_MODEL_ROOT / "PR3D_Board_7x7.fbx"),
            "tile_fbx": str(UNITY_MODEL_ROOT / "PR3D_Tile_Ceramic.fbx"),
            "preview": str(PREVIEW_PATH),
            "board_triangles": triangle_count(board_root),
            "tile_triangles": triangle_count(tile_root),
        }
    )


if __name__ == "__main__":
    main()
