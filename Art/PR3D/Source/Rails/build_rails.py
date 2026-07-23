"""Build the PR3D-004 modular production-line rail kit.

Run inside Blender (including through Blender MCP). Blender authors in metres
and Z-up; the FBX exporter converts to Unity Y-up with +Z forward.
"""

from __future__ import annotations

import json
import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


TASK = "PR3D-004"
MODULE = 0.48
TRACK_WIDTH = 0.82
DECK_Z = 0.16
RAIL_Z = 0.30
STRAIGHT_LENGTH = MODULE * 11
CURVE_RADIUS = 1.89


def mat(name, color, metallic=0.0, roughness=0.45, emission=None):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color = color
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission:
        bsdf.inputs["Emission Color"].default_value = emission
        bsdf.inputs["Emission Strength"].default_value = 2.2
    return m


def move_to_collection(obj, collection):
    for owner in list(obj.users_collection):
        owner.objects.unlink(obj)
    collection.objects.link(obj)


def apply(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    if obj.type == "MESH":
        if not obj.data.uv_layers:
            obj.data.uv_layers.new(name="UVMap")
        for polygon in obj.data.polygons:
            polygon.use_smooth = False
    obj.select_set(False)


def cube(name, location, scale, material, collection, bevel=0.0, parent=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    apply(obj)
    if bevel:
        mod = obj.modifiers.new("EdgeSoftening", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.modifier_apply(modifier=mod.name)
    obj.data.materials.append(material)
    obj.parent = parent
    move_to_collection(obj, collection)
    return obj


def cylinder(name, location, radius, depth, material, collection, parent=None):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=20, radius=radius, depth=depth, location=location
    )
    obj = bpy.context.object
    obj.name = name
    apply(obj)
    obj.data.materials.append(material)
    obj.parent = parent
    move_to_collection(obj, collection)
    return obj


def arrow_mesh(name, center, angle, material, collection, parent):
    # Local arrow points towards -Y, which imports as Unity +Z.
    verts = [
        (-0.13, 0.20, 0.0),
        (0.13, 0.20, 0.0),
        (0.13, -0.02, 0.0),
        (0.24, -0.02, 0.0),
        (0.0, -0.30, 0.0),
        (-0.24, -0.02, 0.0),
        (-0.13, -0.02, 0.0),
    ]
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(verts, [], [tuple(range(7))])
    mesh.materials.append(material)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.location = center
    obj.rotation_euler.z = angle
    obj.parent = parent
    solid = obj.modifiers.new("ArrowThickness", "SOLIDIFY")
    solid.thickness = 0.025
    bevel = obj.modifiers.new("ArrowSoftening", "BEVEL")
    bevel.width = 0.018
    bevel.segments = 2
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=solid.name)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    apply(obj)
    return obj


def ribbon(name, points, half_width, z, material, collection, parent):
    verts = []
    for i, point in enumerate(points):
        if i == 0:
            tangent = Vector(points[1]) - Vector(points[0])
        elif i == len(points) - 1:
            tangent = Vector(points[-1]) - Vector(points[-2])
        else:
            tangent = Vector(points[i + 1]) - Vector(points[i - 1])
        tangent.normalize()
        normal = Vector((-tangent.y, tangent.x, 0.0))
        p = Vector(point)
        verts.extend(
            [
                (p.x + normal.x * half_width, p.y + normal.y * half_width, z),
                (p.x - normal.x * half_width, p.y - normal.y * half_width, z),
            ]
        )
    faces = []
    for i in range(len(points) - 1):
        a = i * 2
        faces.append((a, a + 1, a + 3, a + 2))
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(material)
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.parent = parent
    solid = obj.modifiers.new("DeckThickness", "SOLIDIFY")
    solid.thickness = 0.12
    bevel = obj.modifiers.new("DeckSoftening", "BEVEL")
    bevel.width = 0.025
    bevel.segments = 2
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=solid.name)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    apply(obj)
    return obj


def tube(name, points, radius, material, collection, parent):
    curve_data = bpy.data.curves.new(name + "_Curve", "CURVE")
    curve_data.dimensions = "3D"
    curve_data.resolution_u = 2
    curve_data.bevel_depth = radius
    curve_data.bevel_resolution = 2
    curve_data.resolution_u = 3
    spline = curve_data.splines.new("POLY")
    spline.points.add(len(points) - 1)
    for dst, src in zip(spline.points, points):
        dst.co = (*src, 1.0)
    curve_data.materials.append(material)
    obj = bpy.data.objects.new(name, curve_data)
    collection.objects.link(obj)
    obj.parent = parent
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.convert(target="MESH")
    apply(obj)
    return obj


def empty(name, collection, parent=None, location=(0, 0, 0)):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "ARROWS"
    obj.empty_display_size = 0.18
    obj.location = location
    obj.parent = parent
    collection.objects.link(obj)
    return obj


def straight(collection, materials, origin):
    root = empty("PR3D_Rail_Straight_Root", collection, location=origin)
    root["module_spacing_m"] = MODULE
    root["entry_local"] = [0.0, 0.0, 0.0]
    root["exit_local"] = [0.0, -STRAIGHT_LENGTH, 0.0]
    # Children are authored in root-local space so the exported root is the
    # entry pivot; ``origin`` only lays out the source/preview scene.
    x, y = 0.0, 0.0
    center_y = y - STRAIGHT_LENGTH / 2
    cube(
        "Visual_StraightDeck",
        (x, center_y, DECK_Z),
        (TRACK_WIDTH / 2, STRAIGHT_LENGTH / 2, 0.06),
        materials["deck"],
        collection,
        0.04,
        root,
    )
    for side in (-1, 1):
        cube(
            f"Visual_StraightRail_{'L' if side < 0 else 'R'}",
            (x + side * TRACK_WIDTH / 2, center_y, RAIL_Z),
            (0.055, STRAIGHT_LENGTH / 2, 0.09),
            materials["metal"],
            collection,
            0.025,
            root,
        )
    for i in range(12):
        py = y - i * MODULE
        cube(
            f"Visual_Slat_{i:02}",
            (x, py, DECK_Z + 0.075),
            (TRACK_WIDTH * 0.43, 0.035, 0.018),
            materials["slat"],
            collection,
            0.012,
            root,
        )
    for i in (2, 5, 8):
        arrow_mesh(
            f"Arrow_Straight_{i:02}",
            (x, y - i * MODULE, DECK_Z + 0.12),
            0.0,
            materials["arrow"],
            collection,
            root,
        )
    empty("Anchor_Entry", collection, root, (x, y, 0.0))
    empty("Anchor_Exit", collection, root, (x, y - STRAIGHT_LENGTH, 0.0))
    for i in range(11):
        empty(f"Anchor_Place_{i + 1:02}", collection, root, (x, y - (i + 0.5) * MODULE, 0.0))
    return root


def curve90(collection, materials, origin, mirror=False):
    handed = "Left" if mirror else "Right"
    root = empty(f"PR3D_Rail_Curve90_{handed}_Root", collection, location=origin)
    sign = 1.0 if mirror else -1.0
    x0, y0 = 0.0, 0.0
    points = []
    # Entry tangent is -Y. Exit tangent is +/-X.
    for i in range(17):
        t = (math.pi / 2) * i / 16
        x = x0 + sign * CURVE_RADIUS * (1.0 - math.cos(t))
        y = y0 - CURVE_RADIUS * math.sin(t)
        points.append((x, y, 0.0))
    ribbon(
        f"Visual_CurveDeck_{handed}",
        points,
        TRACK_WIDTH / 2,
        DECK_Z,
        materials["deck"],
        collection,
        root,
    )
    for side in (-1, 1):
        offset_points = []
        for i, p in enumerate(points):
            if i == 0:
                tangent = Vector(points[1]) - Vector(points[0])
            elif i == len(points) - 1:
                tangent = Vector(points[-1]) - Vector(points[-2])
            else:
                tangent = Vector(points[i + 1]) - Vector(points[i - 1])
            tangent.normalize()
            normal = Vector((-tangent.y, tangent.x, 0))
            offset_points.append(
                (
                    p[0] + normal.x * side * TRACK_WIDTH / 2,
                    p[1] + normal.y * side * TRACK_WIDTH / 2,
                    RAIL_Z,
                )
            )
        tube(
            f"Visual_CurveRail_{handed}_{side:+d}",
            offset_points,
            0.055,
            materials["metal"],
            collection,
            root,
        )
    # Direction markers remain easy to read at phone scale.
    for index in (4, 8, 12):
        p = points[index]
        p2 = points[index + 1]
        tangent = math.atan2(p2[1] - p[1], p2[0] - p[0])
        arrow_mesh(
            f"Arrow_Curve_{handed}_{index:02}",
            (p[0], p[1], DECK_Z + 0.12),
            tangent + math.pi / 2,
            materials["arrow"],
            collection,
            root,
        )
    empty("Anchor_Entry", collection, root, points[0])
    empty("Anchor_Exit", collection, root, points[-1])
    root["mirrorable"] = True
    root["rotation_step_degrees"] = 90
    root["curve_radius_m"] = CURVE_RADIUS
    return root


def connector(collection, materials, origin):
    root = empty("PR3D_Rail_Connector_Root", collection, location=origin)
    x, y = 0.0, 0.0
    cube(
        "Visual_ConnectorDeck",
        (x, y - MODULE / 4, DECK_Z),
        (TRACK_WIDTH / 2, MODULE / 4, 0.06),
        materials["deck"],
        collection,
        0.035,
        root,
    )
    for side in (-1, 1):
        cube(
            f"Visual_ConnectorLatch_{side:+d}",
            (x + side * TRACK_WIDTH / 2, y - MODULE / 4, RAIL_Z),
            (0.075, MODULE / 4, 0.12),
            materials["accent"],
            collection,
            0.025,
            root,
        )
    empty("Anchor_In", collection, root, (x, y, 0))
    empty("Anchor_Out", collection, root, (x, y - MODULE / 2, 0))
    root["connector_length_m"] = MODULE / 2
    return root


def support(collection, materials, origin):
    root = empty("PR3D_Rail_Support_Root", collection, location=origin)
    x, y = 0.0, 0.0
    cube(
        "Visual_SupportCrossbar",
        (x, y, 0.08),
        (TRACK_WIDTH * 0.58, 0.12, 0.07),
        materials["metal"],
        collection,
        0.035,
        root,
    )
    for side in (-1, 1):
        cylinder(
            f"Visual_SupportFoot_{side:+d}",
            (x + side * TRACK_WIDTH * 0.42, y, -0.08),
            0.10,
            0.25,
            materials["accent"],
            collection,
            root,
        )
    empty("Anchor_TrackCenter", collection, root, (x, y, 0))
    return root


def export_root(root, output):
    preview_location = root.location.copy()
    root.location = (0.0, 0.0, 0.0)
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for child in root.children_recursive:
        child.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.export_scene.fbx(
        filepath=str(output),
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
    root.location = preview_location
    bpy.context.view_layer.update()


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def build(repo_root):
    repo = Path(repo_root).resolve()
    source_dir = repo / "Art/PR3D/Source/Rails"
    export_dir = repo / "Art/PR3D/Exports/Rails"
    unity_dir = repo / "Assets/_Projects/Art/PR3D/Rails"
    preview_dir = repo / "Art/PR3D/Previews"
    for d in (source_dir, export_dir, unity_dir, preview_dir):
        d.mkdir(parents=True, exist_ok=True)

    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for c in list(bpy.data.collections):
        bpy.data.collections.remove(c)
    for m in list(bpy.data.materials):
        bpy.data.materials.remove(m)

    scene = bpy.context.scene
    scene.name = "PR3D_Rails_Master"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene.unit_settings.length_unit = "METERS"
    scene["task"] = TASK
    scene["unity_contract"] = "FBX Y-up +Z-forward, root pivot at entry anchor"
    scene["module_spacing_m"] = MODULE
    scene["source_place_evidence"] = (
        "ProductionLine_Belt Place spacing sampled 0.388-0.484m; kit nominal 0.48m"
    )
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1080
    scene.render.resolution_y = 1920
    scene.render.resolution_percentage = 50
    scene.render.image_settings.file_format = "PNG"
    if scene.world is None:
        scene.world = bpy.data.worlds.new("PR3D_Rails_World")
    scene.world.color = (0.012, 0.018, 0.035)

    kit = bpy.data.collections.new("Rails")
    scene.collection.children.link(kit)
    materials = {
        "deck": mat("PR3D_MAT_RailDeck", (0.08, 0.12, 0.16, 1), 0.15, 0.35),
        "metal": mat("PR3D_MAT_RailSteel", (0.48, 0.58, 0.64, 1), 0.78, 0.24),
        "slat": mat("PR3D_MAT_RailSlat", (0.18, 0.24, 0.27, 1), 0.45, 0.32),
        "accent": mat("PR3D_MAT_RailAccent", (0.95, 0.34, 0.06, 1), 0.15, 0.3),
        "arrow": mat(
            "PR3D_MAT_RailArrow",
            (1.0, 0.65, 0.06, 1),
            0.0,
            0.28,
            (1.0, 0.22, 0.01, 1),
        ),
    }

    roots = [
        straight(kit, materials, (-2.4, 2.6, 0)),
        curve90(kit, materials, (0.0, 2.6, 0), False),
        curve90(kit, materials, (2.8, 2.6, 0), True),
        connector(kit, materials, (-1.0, -3.7, 0)),
        support(kit, materials, (1.0, -3.7, 0)),
    ]

    for root in roots:
        root["task"] = TASK
        root["units"] = "metres"
        root["axis_export"] = "Unity Y-up +Z-forward"

    # Preview floor, excluded from exports.
    preview = bpy.data.collections.new("Preview_Only")
    scene.collection.children.link(preview)
    floor_mat = mat("PR3D_MAT_PreviewFloor", (0.025, 0.045, 0.055, 1), 0, 0.72)
    cube("Preview_Floor", (0, -0.8, -0.24), (5.0, 4.8, 0.12), floor_mat, preview, 0.08)

    cam_data = bpy.data.cameras.new("PR3D_CAM_RailsPortrait_Data")
    cam = bpy.data.objects.new("PR3D_CAM_RailsPortrait", cam_data)
    preview.objects.link(cam)
    cam.location = (8.8, -10.8, 12.8)
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = 10.0
    look_at(cam, (0, -0.6, 0))
    scene.camera = cam
    for name, loc, energy, size, color in (
        ("Key", (-4, -4, 10), 1150, 5.0, (1.0, 0.72, 0.48)),
        ("Fill", (5, 2, 7), 850, 4.0, (0.3, 0.55, 1.0)),
    ):
        data = bpy.data.lights.new("PR3D_LGT_" + name + "_Data", "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        data.color = color
        light = bpy.data.objects.new("PR3D_LGT_" + name, data)
        light.location = loc
        preview.objects.link(light)
        look_at(light, (0, -0.8, 0))

    names = {
        roots[0]: "PR3D_Rail_Straight_5p28",
        roots[1]: "PR3D_Rail_Curve90_Right",
        roots[2]: "PR3D_Rail_Curve90_Left",
        roots[3]: "PR3D_Rail_Connector",
        roots[4]: "PR3D_Rail_Support",
    }
    for root, filename in names.items():
        export_root(root, export_dir / f"{filename}.fbx")
        export_root(root, unity_dir / f"{filename}.fbx")

    scene.render.filepath = str(preview_dir / "PR3D_004_RailKit.png")
    bpy.ops.render.render(write_still=True)
    blend_path = source_dir / "PR3D_Rails_Master.blend"
    prefs = bpy.context.preferences.filepaths
    old_versions = prefs.save_version
    prefs.save_version = 0
    try:
        bpy.ops.wm.save_as_mainfile(filepath=str(blend_path), relative_remap=False)
    finally:
        prefs.save_version = old_versions

    contract = {
        "task": TASK,
        "units": "metres",
        "blender_axis": "Z-up",
        "unity_axis": "Y-up, +Z-forward",
        "module_spacing_m": MODULE,
        "track_width_m": TRACK_WIDTH,
        "straight_length_m": STRAIGHT_LENGTH,
        "curve_radius_m": CURVE_RADIUS,
        "variants": list(names.values()),
        "reuse": {
            "curve": "Right/Left variants share dimensions; rotate in 90-degree steps",
            "connector": "0.24m half-module bridge between modular pieces",
            "support": "instance at module joints or every 0.96m",
        },
        "gameplay_contract": "visual-only; no prefab, scene, collider, JSON, enum, or Place edits",
    }
    (unity_dir / "PR3D_Rails_Contract.json").write_text(
        json.dumps(contract, indent=2) + "\n", encoding="utf-8"
    )
    return {
        "blend": str(blend_path),
        "preview": scene.render.filepath,
        "exports": [str(export_dir / f"{n}.fbx") for n in names.values()],
        "unity": str(unity_dir),
    }


if __name__ == "__main__":
    root = os.environ.get("PR3D_REPO_ROOT")
    if not root:
        raise RuntimeError("Set PR3D_REPO_ROOT")
    print(json.dumps(build(root), indent=2))
