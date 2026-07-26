"""Deterministically build the PR3D-003 modular board kit.

This family-local source recreates the verified lightweight asset contract from
archived attempt d87eeef without modifying the shared master scene. Run it in
Blender 5.2 through Blender MCP. Blender authoring is metric/Z-up; FBX export is
converted to Unity Y-up/+Z-forward.
"""

from __future__ import annotations

import math
from pathlib import Path

import bpy


TASK = "PR3D-003"
PREFIX = "PR3D003_"
GRID_SIZE = 7
CELL_PITCH_M = 1.0
WORKTREE = Path(__file__).resolve().parents[4]
SOURCE_DIR = WORKTREE / "Art" / "PR3D" / "Source" / "Board"
EXPORT_DIR = WORKTREE / "Art" / "PR3D" / "Exports" / "Board"
UNITY_DIR = WORKTREE / "Assets" / "_Projects" / "Art" / "PR3D" / "Board" / "Models"
BLEND_PATH = SOURCE_DIR / "PR3D_BoardKit.blend"


def material(name: str, color: tuple[float, float, float, float], metallic=0.0, roughness=0.4):
    mat = bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    return mat


def apply_modifier(obj, modifier):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


def rounded_box(name, size, location, bevel, mat, collection):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.active_object
    obj.name = name
    obj.dimensions = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    modifier = obj.modifiers.new("EdgeSoftening", "BEVEL")
    modifier.width = bevel
    modifier.segments = 3
    modifier.limit_method = "ANGLE"
    apply_modifier(obj, modifier)
    obj.data.materials.append(mat)
    for owner in tuple(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)
    return obj


def rounded_loop(half_x, half_y, radius, segments=8):
    points = []
    for center_x, center_y, start in (
        (half_x - radius, half_y - radius, 0.0),
        (-half_x + radius, half_y - radius, 90.0),
        (-half_x + radius, -half_y + radius, 180.0),
        (half_x - radius, -half_y + radius, 270.0),
    ):
        for index in range(segments):
            angle = math.radians(start + index * 90.0 / segments)
            points.append(
                (center_x + radius * math.cos(angle), center_y + radius * math.sin(angle))
            )
    return points


def rounded_frame(name, outer_size, inner_size, bottom_z, top_z, mat, collection):
    outer = rounded_loop(outer_size / 2.0, outer_size / 2.0, 0.42)
    inner = rounded_loop(inner_size / 2.0, inner_size / 2.0, 0.22)
    count = len(outer)
    vertices = (
        [(x, y, bottom_z) for x, y in outer]
        + [(x, y, top_z) for x, y in outer]
        + [(x, y, bottom_z) for x, y in inner]
        + [(x, y, top_z) for x, y in inner]
    )
    faces = []
    for index in range(count):
        nxt = (index + 1) % count
        ob, ot = index, count + index
        ib, it = 2 * count + index, 3 * count + index
        nob, not_ = nxt, count + nxt
        nib, nit = 2 * count + nxt, 3 * count + nxt
        faces.extend(
            (
                (ot, not_, nit, it),
                (nob, ob, ib, nib),
                (ob, nob, not_, ot),
                (nib, ib, it, nit),
            )
        )
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.data.materials.append(mat)
    modifier = obj.modifiers.new("EdgeSoftening", "BEVEL")
    modifier.width = 0.045
    modifier.segments = 2
    modifier.limit_method = "ANGLE"
    apply_modifier(obj, modifier)
    return obj


def smart_uv(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=math.radians(66.0), island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    obj.select_set(False)


def parent_keep_transform(child, parent):
    matrix = child.matrix_world.copy()
    child.parent = parent
    child.matrix_world = matrix


def duplicate_at(source, name, location, collection):
    """Duplicate evaluated tile geometry without rebuilding bevel modifiers."""
    obj = source.copy()
    obj.data = source.data.copy()
    obj.name = name
    obj.location = location
    obj.parent = None
    collection.objects.link(obj)
    return obj


def join_meshes(objects, name):
    """Collapse repeated tiles into one renderer-friendly mesh per material."""
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.object.join()
    joined = bpy.context.object
    joined.name = name
    joined.data.name = f"{name}_Mesh"
    joined.select_set(False)
    return joined


def hierarchy(root):
    return [root, *root.children_recursive]


def select_hierarchy(root):
    bpy.ops.object.select_all(action="DESELECT")
    for obj in hierarchy(root):
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root


def export_fbx(root, path):
    select_hierarchy(root)
    bpy.ops.export_scene.fbx(
        filepath=str(path),
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


def export_glb(root, path):
    select_hierarchy(root)
    bpy.ops.export_scene.gltf(
        filepath=str(path),
        export_format="GLB",
        use_selection=True,
        export_yup=True,
        export_apply=True,
        export_materials="EXPORT",
    )


def bounds(root):
    from mathutils import Vector

    corners = [
        obj.matrix_world @ Vector(corner)
        for obj in hierarchy(root)
        if obj.type == "MESH"
        for corner in obj.bound_box
    ]
    minimum = [min(point[axis] for point in corners) for axis in range(3)]
    maximum = [max(point[axis] for point in corners) for axis in range(3)]
    return [round(maximum[axis] - minimum[axis], 4) for axis in range(3)]


def triangles(root):
    return sum(
        max(0, len(poly.vertices) - 2)
        for obj in hierarchy(root)
        if obj.type == "MESH"
        for poly in obj.data.polygons
    )


def validate(root):
    assert tuple(round(v, 6) for v in root.location) == (0.0, 0.0, 0.0)
    assert tuple(round(v, 6) for v in root.rotation_euler) == (0.0, 0.0, 0.0)
    assert tuple(round(v, 6) for v in root.scale) == (1.0, 1.0, 1.0)
    meshes = [obj for obj in hierarchy(root) if obj.type == "MESH"]
    assert meshes
    assert all(obj.data.uv_layers for obj in meshes)
    assert all(tuple(round(v, 6) for v in obj.scale) == (1.0, 1.0, 1.0) for obj in meshes)


def main():
    SOURCE_DIR.mkdir(parents=True, exist_ok=True)
    EXPORT_DIR.mkdir(parents=True, exist_ok=True)
    UNITY_DIR.mkdir(parents=True, exist_ok=True)

    # Keep the MCP add-on and its socket alive: clear scene data explicitly
    # instead of loading factory settings.
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for collection in list(bpy.data.collections):
        bpy.data.collections.remove(collection)
    for mat in list(bpy.data.materials):
        bpy.data.materials.remove(mat)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    collection = bpy.data.collections.new("Board")
    scene.collection.children.link(collection)

    frame_blue = material(f"{PREFIX}MAT_FrameBlue", (0.025, 0.12, 0.42, 1.0), 0.28, 0.22)
    tray_blue = material(f"{PREFIX}MAT_TrayBlue", (0.012, 0.045, 0.16, 1.0), 0.14, 0.32)
    ceramic = material(f"{PREFIX}MAT_CeramicCream", (0.88, 0.68, 0.52, 1.0), 0.0, 0.46)
    highlight = material(f"{PREFIX}MAT_CeramicHighlight", (1.0, 0.86, 0.72, 1.0), 0.0, 0.34)

    board = bpy.data.objects.new("PR3D_Board_7x7", None)
    tile = bpy.data.objects.new("PR3D_Tile_Ceramic", None)
    collection.objects.link(board)
    collection.objects.link(tile)
    tray = rounded_box(
        f"{PREFIX}Visual_TrayBase", (7.20, 7.20, 0.14), (0.0, 0.0, -0.09), 0.18, tray_blue, collection
    )
    frame = rounded_frame(
        f"{PREFIX}Visual_Frame", 7.78, 7.10, -0.10, 0.40, frame_blue, collection
    )
    tile_base = rounded_box(
        f"{PREFIX}Visual_TileBase", (0.90, 0.90, 0.10), (0.0, 0.0, 0.05), 0.085, ceramic, collection
    )
    tile_inset = rounded_box(
        f"{PREFIX}Visual_TileInset", (0.76, 0.76, 0.025), (0.0, 0.0, 0.105), 0.065, highlight, collection
    )
    for obj in (tray, frame, tile_base, tile_inset):
        smart_uv(obj)
    for obj in (tray, frame):
        parent_keep_transform(obj, board)
    for obj in (tile_base, tile_inset):
        parent_keep_transform(obj, tile)

    # The archived board only exported its tray/frame and left the lavender
    # legacy floor visible. Build the actual 7x7 ivory ceramic surface inside
    # the additive board visual. Two joined meshes keep renderer/material cost
    # bounded while preserving the exact 1 m gameplay pitch and cell centres.
    board_tile_bases = []
    board_tile_highlights = []
    for row in range(GRID_SIZE):
        for column in range(GRID_SIZE):
            x = column - (GRID_SIZE - 1) / 2.0
            y = row - (GRID_SIZE - 1) / 2.0
            index = row * GRID_SIZE + column
            board_tile_bases.append(
                duplicate_at(
                    tile_base,
                    f"{PREFIX}BoardTileBase_{index:02d}",
                    (x, y, 0.245),
                    collection,
                )
            )
            board_tile_highlights.append(
                duplicate_at(
                    tile_inset,
                    f"{PREFIX}BoardTileHighlight_{index:02d}",
                    (x, y, 0.2975),
                    collection,
                )
            )
    board_tiles = join_meshes(board_tile_bases, f"{PREFIX}Visual_CeramicTiles")
    board_highlights = join_meshes(
        board_tile_highlights, f"{PREFIX}Visual_CeramicHighlights"
    )
    smart_uv(board_tiles)
    smart_uv(board_highlights)
    parent_keep_transform(board_tiles, board)
    parent_keep_transform(board_highlights, board)

    board["pr3d_task"] = TASK
    board["grid_size"] = GRID_SIZE
    board["cell_pitch_m"] = CELL_PITCH_M
    board["unity_placement"] = "center at grid world (4.5, 0, 4.5)"
    tile["pr3d_task"] = TASK
    tile["cell_footprint_m"] = [1.0, 1.0]
    tile["pivot_contract"] = "cell center at gameplay plane"

    for root in (board, tile):
        validate(root)
    export_fbx(board, UNITY_DIR / "PR3D_Board_7x7.fbx")
    export_fbx(tile, UNITY_DIR / "PR3D_Tile_Ceramic.fbx")
    export_glb(board, EXPORT_DIR / "PR3D_Board_7x7.glb")
    export_glb(tile, EXPORT_DIR / "PR3D_Tile_Ceramic.glb")
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_PATH))

    print(
        {
            "task": TASK,
            "blend": str(BLEND_PATH),
            "board_bounds_blender_xyz_m": bounds(board),
            "tile_bounds_blender_xyz_m": bounds(tile),
            "board_triangles": triangles(board),
            "tile_triangles": triangles(tile),
            "cell_pitch_m": CELL_PITCH_M,
        }
    )


if __name__ == "__main__":
    main()
