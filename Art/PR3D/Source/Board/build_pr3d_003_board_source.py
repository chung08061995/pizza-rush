"""Rebuild the isolated PR3D-003 Board source scene from verified exports.

This script intentionally writes only PR3D-003-owned paths. Run it through the
Blender MCP bridge in the open Blender instance. Gameplay coordinates use a
1 m cell pitch; the visual board's asset pivot remains at the origin.
"""

from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[4]
BOARD_GLB = ROOT / "Art/PR3D/Exports/Board/PR3D_Board_7x7.glb"
TILE_GLB = ROOT / "Art/PR3D/Exports/Board/PR3D_Tile_Ceramic.glb"
SOURCE_BLEND = ROOT / "Art/PR3D/Source/Board/PR3D_003_BoardTile.blend"
GRID_SIZE = 7
CELL_PITCH_M = 1.0
FIRST_CELL_CENTER_M = 1.5
BOARD_CENTER_M = (4.5, 4.5, 0.0)


def import_asset(path: Path, collection_name: str):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.gltf(filepath=str(path))
    imported = [obj for obj in bpy.data.objects if obj not in before]
    collection = bpy.data.collections.new(collection_name)
    bpy.context.scene.collection.children.link(collection)
    for obj in imported:
        for owner in tuple(obj.users_collection):
            owner.objects.unlink(obj)
        collection.objects.link(obj)
    return imported, collection


def linked_display_copy(source, name, location, collection):
    duplicate = source.copy()
    duplicate.data = source.data
    duplicate.name = name
    duplicate.location = location
    collection.objects.link(duplicate)
    return duplicate


for obj in list(bpy.data.objects):
    bpy.data.objects.remove(obj, do_unlink=True)
for collection in list(bpy.data.collections):
    bpy.data.collections.remove(collection)
for material in list(bpy.data.materials):
    bpy.data.materials.remove(material)
scene = bpy.context.scene
scene.unit_settings.system = "METRIC"
scene.unit_settings.scale_length = 1.0
scene["pr3d_task"] = "PR3D-003"
scene["grid_shape"] = "7x7"
scene["cell_pitch_m"] = CELL_PITCH_M
scene["board_center_unity"] = BOARD_CENTER_M

board_objects, board_collection = import_asset(
    BOARD_GLB, "PR3D003_ASSET_Board_7x7"
)
tile_objects, tile_collection = import_asset(
    TILE_GLB, "PR3D003_ASSET_Tile_Ceramic"
)

for collection in (board_collection, tile_collection):
    collection.hide_viewport = True
    collection.hide_render = True

display = bpy.data.collections.new("PR3D003_PREVIEW_Level301_Grid")
scene.collection.children.link(display)

board_meshes = [obj for obj in board_objects if obj.type == "MESH"]
tile_meshes = [obj for obj in tile_objects if obj.type == "MESH"]
for index, obj in enumerate(board_meshes):
    linked_display_copy(
        obj,
        f"PR3D003_PREVIEW_Board_{index:02d}",
        (BOARD_CENTER_M[0], BOARD_CENTER_M[1], obj.location.z),
        display,
    )

for row in range(GRID_SIZE):
    for column in range(GRID_SIZE):
        center_x = FIRST_CELL_CENTER_M + column * CELL_PITCH_M
        center_y = FIRST_CELL_CENTER_M + row * CELL_PITCH_M
        for index, obj in enumerate(tile_meshes):
            linked_display_copy(
                obj,
                f"PR3D003_PREVIEW_Tile_R{row + 1}C{column + 1}_{index:02d}",
                (center_x, center_y, obj.location.z),
                display,
            )

SOURCE_BLEND.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE_BLEND))

def world_bounds(objects):
    points = [
        obj.matrix_world @ Vector(corner)
        for obj in objects
        if obj.type == "MESH"
        for corner in obj.bound_box
    ]
    return tuple(
        round(max(point[axis] for point in points) - min(point[axis] for point in points), 4)
        for axis in range(3)
    )

print(
    {
        "task": "PR3D-003",
        "source": str(SOURCE_BLEND),
        "board_meshes": len(board_meshes),
        "tile_meshes": len(tile_meshes),
        "preview_cells": GRID_SIZE * GRID_SIZE,
        "cell_pitch_m": CELL_PITCH_M,
        "board_asset_bounds_m": world_bounds(board_meshes),
        "tile_asset_bounds_m": world_bounds(tile_meshes),
    }
)
