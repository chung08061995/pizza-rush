"""Read-only validation helpers for the PR3D Blender master scene."""

from __future__ import annotations

import bpy
from mathutils import Vector


def validate_board() -> dict[str, object]:
    root = bpy.data.objects["PR3D_Board_7x7_Root"]
    meshes = [obj for obj in root.children_recursive if obj.type == "MESH"]
    corners = [
        obj.matrix_world @ Vector(corner)
        for obj in meshes
        for corner in obj.bound_box
    ]
    bounds_min = [min(value[index] for value in corners) for index in range(3)]
    bounds_max = [max(value[index] for value in corners) for index in range(3)]
    tiles = [obj for obj in meshes if obj.name.startswith("Visual_Tile_r")]
    triangles = sum(
        sum(max(0, len(polygon.vertices) - 2) for polygon in obj.data.polygons)
        for obj in meshes
    )

    center_visual = bpy.data.objects["Visual_Center"]
    return {
        "root_location": tuple(round(value, 4) for value in root.location),
        "mesh_count": len(meshes),
        "tile_count": len(tiles),
        "bounds_min": [round(value, 4) for value in bounds_min],
        "bounds_max": [round(value, 4) for value in bounds_max],
        "bounds_size": [
            round(bounds_max[index] - bounds_min[index], 4)
            for index in range(3)
        ],
        "triangles": triangles,
        "all_scale_one": all(
            all(abs(value - 1.0) < 1e-6 for value in obj.scale)
            for obj in meshes
        ),
        "all_uv": all(bool(obj.data.uv_layers) for obj in meshes),
        "tile_center_dimensions": tuple(
            round(value, 4) for value in center_visual.dimensions
        ),
        "tile_center_location": tuple(
            round(value, 4) for value in center_visual.location
        ),
    }


if __name__ == "__main__":
    print(validate_board())
