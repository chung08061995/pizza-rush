"""Build the PR3D-008 modular pizza-kitchen environment kit.

Run inside Blender, normally through Blender MCP. Blender authors in metres and
Z-up. FBX export converts to Unity Y-up with +Z forward. The script only writes
the PR3D Environment source/export/runtime folders assigned to PR3D-008.
"""

from __future__ import annotations

import math
import os
from pathlib import Path

import bpy
from mathutils import Vector


TASK = "PR3D-008"
FORWARD = "-Z"
UP = "Y"


def material(name, color, metallic=0.0, roughness=0.48, emission=None):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = color
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if emission is not None:
        bsdf.inputs["Emission Color"].default_value = emission
        bsdf.inputs["Emission Strength"].default_value = 3.0
    return mat


def move(obj, collection):
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
        for poly in obj.data.polygons:
            poly.use_smooth = False
    obj.select_set(False)


def finish(obj, name, mat, collection, parent=None, bevel=0.0, smooth=False):
    obj.name = name
    apply(obj)
    if bevel:
        mod = obj.modifiers.new("EdgeSoftening", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.modifier_apply(modifier=mod.name)
        obj.select_set(False)
    if smooth and obj.type == "MESH":
        for poly in obj.data.polygons:
            poly.use_smooth = True
    obj.data.materials.append(mat)
    obj.parent = parent
    move(obj, collection)
    return obj


def cube(name, loc, scale, mat, collection, parent=None, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    return finish(obj, name, mat, collection, parent, bevel)


def cyl(
    name,
    loc,
    radius,
    depth,
    mat,
    collection,
    parent=None,
    vertices=20,
    rotation=(0.0, 0.0, 0.0),
    bevel=0.0,
):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rotation
    )
    return finish(
        bpy.context.object, name, mat, collection, parent, bevel, smooth=True
    )


def sphere(name, loc, scale, mat, collection, parent=None):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=16, ring_count=8, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    return finish(obj, name, mat, collection, parent, 0.0, smooth=True)


def root(name, collection, display_location):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "CUBE"
    obj.empty_display_size = 0.28
    obj["task"] = TASK
    obj["units"] = "metres"
    obj["unity_axis"] = "Y-up, +Z-forward"
    obj["usage"] = "additive visual; preserve gameplay contracts"
    collection.objects.link(obj)
    obj.location = display_location
    return obj


def arch_mesh(name, collection, parent, mats):
    # Extruded half-ring with a flat hearth, seen from Blender -Y / Unity +Z.
    verts = []
    faces = []
    segments = 16
    outer = 1.10
    inner = 0.72
    zc = 0.86
    depth = 0.42
    profile = []
    for i in range(segments + 1):
        angle = math.pi - math.pi * i / segments
        profile.append((outer * math.cos(angle), zc + outer * math.sin(angle)))
    for i in range(segments, -1, -1):
        angle = math.pi - math.pi * i / segments
        profile.append((inner * math.cos(angle), zc + inner * math.sin(angle)))
    for y in (-depth / 2, depth / 2):
        for x, z in profile:
            verts.append((x, y, z))
    ring = len(profile)
    faces.append(tuple(range(ring)))
    faces.append(tuple(range(ring, ring * 2)))
    for i in range(ring):
        j = (i + 1) % ring
        faces.append((i, j, ring + j, ring + i))
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(verts, [], faces)
    mesh.materials.append(mats["brick"])
    obj = bpy.data.objects.new(name, mesh)
    collection.objects.link(obj)
    obj.parent = parent
    bevel = obj.modifiers.new("BrickArchSoftening", "BEVEL")
    bevel.width = 0.035
    bevel.segments = 2
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    apply(obj)
    return obj


def build_oven(collection, mats):
    r = root("PR3D_Environment_Oven_Root", collection, (-2.25, 3.1, 0.0))
    cube("Visual_OvenPlinth", (0, 0, 0.34), (1.52, 0.70, 0.34), mats["stone"], collection, r, 0.08)
    # Keep the inexpensive dome base, then layer a sparse brick pattern on the
    # camera-facing side. The orange brick rhythm is the concept's strongest
    # environment silhouette and must survive at phone scale.
    sphere("Visual_OvenDome", (0, 0.10, 1.30), (1.43, 0.62, 1.28), mats["brick_dark"], collection, r)
    brick_rows = (
        (0.72, 2.30, 6),
        (1.02, 2.55, 7),
        (1.32, 2.45, 7),
        (1.62, 2.12, 6),
        (1.92, 1.62, 5),
        (2.20, 0.92, 3),
    )
    for row, (z, width, count) in enumerate(brick_rows):
        brick_w = width / count
        offset = brick_w * 0.5 if row % 2 else 0.0
        for col in range(count):
            x = -width * 0.5 + brick_w * (col + 0.5) + offset
            if abs(x) > width * 0.5:
                continue
            # Leave the fire opening unobstructed.
            if z < 1.34 and abs(x) < 0.76:
                continue
            cube(
                f"Visual_OvenBrick_{row:02}_{col:02}",
                (x, -0.535, z),
                (brick_w * 0.45, 0.055, 0.115),
                mats["brick"] if (row + col) % 3 else mats["brick_light"],
                collection,
                r,
                0.025,
            )
    cube("Visual_OvenDomeTrim", (0, -0.50, 0.73), (1.53, 0.14, 0.12), mats["stone"], collection, r, 0.035)
    arch_mesh("Visual_OvenArch", collection, r, mats)
    cube("Visual_OvenOpening", (0, -0.235, 0.78), (0.68, 0.22, 0.55), mats["dark"], collection, r, 0.14)
    sphere("Visual_OvenFire", (0, -0.49, 0.55), (0.50, 0.10, 0.30), mats["fire"], collection, r)
    sphere("Visual_OvenFireCore", (0, -0.595, 0.55), (0.26, 0.055, 0.22), mats["fire_core"], collection, r)
    for x in (-0.26, 0.26):
        cyl(
            f"Visual_FireLog_{'L' if x < 0 else 'R'}",
            (x, -0.62, 0.45),
            0.09,
            0.72,
            mats["wood"],
            collection,
            r,
            12,
            rotation=(0, math.pi / 2, 0),
        )
    # Chimney is separable-looking but ships with the oven module.
    cyl("Visual_OvenChimney", (0, 0.10, 2.70), 0.34, 1.15, mats["metal"], collection, r, 20, bevel=0.025)
    cyl("Visual_OvenChimneyBand", (0, 0.10, 2.30), 0.40, 0.12, mats["copper"], collection, r, 20, bevel=0.02)
    return r


def build_wall(collection, mats):
    r = root("PR3D_Environment_WallBlue_Root", collection, (0.0, 4.25, 0.0))
    cube("Visual_WallBacking", (0, 0.18, 1.35), (2.20, 0.10, 1.35), mats["grout"], collection, r)
    tile_w, tile_h = 0.52, 0.42
    for row in range(6):
        for col in range(8):
            x = (col - 3.5) * (tile_w + 0.015)
            z = 0.20 + row * (tile_h + 0.015)
            cube(
                f"Visual_WallTile_{row:02}_{col:02}",
                (x, 0.04, z),
                (tile_w / 2, 0.035, tile_h / 2),
                mats["blue"],
                collection,
                r,
                0.035,
            )
    return r


def build_floor(collection, mats):
    r = root("PR3D_Environment_FloorTerracotta_Root", collection, (0.0, -2.7, 0.0))
    cube("Visual_FloorBacking", (0, 0, -0.05), (2.20, 1.45, 0.05), mats["darkgrout"], collection, r)
    size = 0.66
    for row in range(4):
        for col in range(6):
            x = (col - 2.5) * (size + 0.04)
            y = (row - 1.5) * (size + 0.04)
            cube(
                f"Visual_FloorTile_{row:02}_{col:02}",
                (x, y, 0.01),
                (size / 2, size / 2, 0.045),
                mats["terra_a"] if (row + col) % 2 else mats["terra_b"],
                collection,
                r,
                0.055,
            )
    return r


def build_counter(collection, mats):
    r = root("PR3D_Environment_Counter_Root", collection, (2.25, 3.0, 0.0))
    cube("Visual_CounterBody", (0, 0, 0.55), (1.85, 0.70, 0.55), mats["wood_dark"], collection, r, 0.055)
    cube("Visual_CounterTop", (0, -0.02, 1.16), (2.02, 0.82, 0.11), mats["wood"], collection, r, 0.055)
    for x in (-1.63, -0.55, 0.55, 1.63):
        cube("Visual_CounterPanel", (x, -0.715, 0.57), (0.48, 0.035, 0.46), mats["wood"], collection, r, 0.035)
    # Two pale preparation cloths echo the checked prep surfaces in the demo
    # without requiring a texture atlas or extra shader.
    for i, x in enumerate((-0.72, 0.72)):
        cube(
            f"Visual_CounterPrepCloth_{i}",
            (x, -0.84, 1.285),
            (0.56, 0.30, 0.025),
            mats["cloth_blue"] if i == 0 else mats["cloth_red"],
            collection,
            r,
            0.025,
        )
    return r


def build_shelf(collection, mats):
    r = root("PR3D_Environment_Shelf_Root", collection, (2.15, 1.25, 0.0))
    cube("Visual_ShelfBoard", (0, 0, 0.82), (1.32, 0.34, 0.10), mats["wood"], collection, r, 0.04)
    for x in (-1.05, 1.05):
        cube(f"Visual_ShelfBracket_{x:+.0f}", (x, 0.18, 0.42), (0.08, 0.10, 0.42), mats["metal"], collection, r, 0.025)
    cube("Visual_ShelfBackRail", (0, 0.29, 1.10), (1.36, 0.05, 0.05), mats["wood_dark"], collection, r, 0.02)
    return r


def build_light(collection, mats):
    r = root("PR3D_Environment_PendantLight_Root", collection, (-1.65, 4.35, 0.0))
    cyl("Visual_LightCord", (0, 0, 1.85), 0.025, 1.10, mats["dark"], collection, r, 10)
    bpy.ops.mesh.primitive_cone_add(vertices=24, radius1=0.48, radius2=0.16, depth=0.36, location=(0, 0, 1.18))
    finish(bpy.context.object, "Visual_LightShade", mats["metal"], collection, r, 0.025, True)
    sphere("Visual_LightBulb", (0, 0, 0.94), (0.18, 0.18, 0.22), mats["bulb"], collection, r)
    return r


def build_jar(collection, mats):
    r = root("PR3D_Environment_Jar_Root", collection, (1.65, 1.15, 0.92))
    for i, (x, fill) in enumerate(((-0.48, mats["tomato"]), (0, mats["basil"]), (0.48, mats["cheese"]))):
        cyl(f"Visual_JarGlass_{i}", (x, 0, 0.38), 0.22, 0.66, mats["glass"], collection, r, 20, bevel=0.025)
        cyl(f"Visual_JarFill_{i}", (x, 0, 0.34), 0.17, 0.44, fill, collection, r, 16)
        cyl(f"Visual_JarLid_{i}", (x, 0, 0.74), 0.235, 0.09, mats["copper"], collection, r, 20, bevel=0.015)
    return r


def build_bowl(collection, mats):
    r = root("PR3D_Environment_Bowl_Root", collection, (2.75, 1.10, 1.0))
    # Bowl silhouette: two nested squashed spheres and visible ingredients.
    sphere("Visual_Bowl", (0, 0, 0.28), (0.52, 0.52, 0.30), mats["ceramic"], collection, r)
    sphere("Visual_BowlInset", (0, -0.08, 0.39), (0.42, 0.42, 0.13), mats["dark"], collection, r)
    for i in range(7):
        angle = i * math.tau / 7
        sphere(
            f"Visual_BowlIngredient_{i}",
            (math.cos(angle) * 0.24, math.sin(angle) * 0.20 - 0.10, 0.49),
            (0.105, 0.105, 0.085),
            mats["mushroom"],
            collection,
            r,
        )
    return r


def leaf(name, loc, rot, scale, mat, collection, parent):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    obj.rotation_euler = rot
    return finish(obj, name, mat, collection, parent, 0.02, True)


def build_basil(collection, mats):
    r = root("PR3D_Environment_Basil_Root", collection, (-2.55, 1.05, 0.0))
    cyl("Visual_BasilPot", (0, 0, 0.28), 0.34, 0.52, mats["ceramic"], collection, r, 20, bevel=0.025)
    cyl("Visual_BasilSoil", (0, 0, 0.56), 0.29, 0.04, mats["soil"], collection, r, 20)
    for i in range(11):
        a = i * 2.4
        rad = 0.12 + 0.11 * (i % 3)
        x, y = math.cos(a) * rad, math.sin(a) * rad
        z = 0.66 + 0.08 * (i % 4)
        leaf(f"Visual_BasilLeaf_{i:02}", (x, y, z), (0.2, 0.45, a), (0.22, 0.09, 0.08), mats["basil"], collection, r)
    return r


def build_utensils(collection, mats):
    r = root("PR3D_Environment_Utensils_Root", collection, (-1.55, 1.00, 0.0))
    cube("Visual_UtensilRail", (0, 0.1, 1.08), (0.72, 0.05, 0.045), mats["metal"], collection, r, 0.02)
    for i, x in enumerate((-0.48, 0.0, 0.48)):
        cyl(f"Visual_UtensilHandle_{i}", (x, 0, 0.56), 0.045, 0.82, mats["wood"], collection, r, 12)
        if i == 0:
            cube("Visual_UtensilSpatula", (x, 0, 0.10), (0.18, 0.055, 0.20), mats["wood"], collection, r, 0.05)
        elif i == 1:
            cyl("Visual_UtensilLadle", (x, 0, 0.12), 0.19, 0.09, mats["copper"], collection, r, 20, rotation=(math.pi / 2, 0, 0))
        else:
            cube("Visual_UtensilTongs", (x, 0, 0.12), (0.12, 0.055, 0.22), mats["metal"], collection, r, 0.04)
    return r


def build_crate(collection, mats):
    r = root("PR3D_Environment_IngredientCrate_Root", collection, (2.10, -2.45, 0.0))
    for z in (0.12, 0.48, 0.84):
        for y in (-0.55, 0.55):
            cube(f"Visual_CrateSlat_{z:.2f}_{y:+.2f}", (0, y, z), (0.92, 0.07, 0.10), mats["wood"], collection, r, 0.025)
    for z in (0.12, 0.48, 0.84):
        for x in (-0.92, 0.92):
            cube(f"Visual_CrateSide_{z:.2f}_{x:+.2f}", (x, 0, z), (0.07, 0.55, 0.10), mats["wood"], collection, r, 0.025)
    for x in (-0.78, 0.0, 0.78):
        for y in (-0.35, 0.32):
            color = (
                mats["tomato"]
                if x < -0.25
                else mats["basil"]
                if x < 0.4
                else mats["pepper"]
            )
            sphere(f"Visual_CrateIngredient_{x:+.2f}_{y:+.2f}", (x, y, 0.86), (0.28, 0.28, 0.25), color, collection, r)
    for i, x in enumerate((-0.78, 0.0, 0.78)):
        leaf(
            f"Visual_CrateStem_{i}",
            (x, -0.02, 1.11),
            (0.15, 0.4, i * 1.7),
            (0.16, 0.065, 0.045),
            mats["basil"],
            collection,
            r,
        )
    return r


def descendants(r):
    result = []
    stack = [r]
    while stack:
        obj = stack.pop()
        result.append(obj)
        stack.extend(obj.children)
    return result


def batch_root_by_material(r):
    """Collapse repeated prop pieces into one renderer per shared material.

    The authored pieces remain procedural in this script, while exported FBXs
    stay suitable for a portrait mobile scene (tiles/bricks are not individual
    draw calls).
    """
    groups = {}
    for obj in descendants(r):
        if obj.type != "MESH" or not obj.data.materials:
            continue
        mat = obj.data.materials[0]
        groups.setdefault(mat, []).append(obj)
    for mat, objects in groups.items():
        if len(objects) < 2:
            continue
        bpy.ops.object.select_all(action="DESELECT")
        for obj in objects:
            obj.select_set(True)
        active = objects[0]
        bpy.context.view_layer.objects.active = active
        bpy.ops.object.join()
        active.name = f"Visual_Batched_{mat.name.replace('PR3D_MAT_Env_', '')}"
        # Joining one-material meshes can retain duplicate slots. Normalize
        # them so Unity imports one submesh/material for the batch.
        for poly in active.data.polygons:
            poly.material_index = 0
        active.data.materials.clear()
        active.data.materials.append(mat)
        active.parent = r
        apply(active)


def export_root(r, export_dir, runtime_dir):
    old_location = r.location.copy()
    r.location = (0, 0, 0)
    bpy.context.view_layer.update()
    bpy.ops.object.select_all(action="DESELECT")
    objects = descendants(r)
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = r
    filename = r.name.replace("_Root", "")
    kwargs = dict(
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        use_space_transform=True,
        bake_space_transform=False,
        axis_forward=FORWARD,
        axis_up=UP,
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
    )
    for folder in (export_dir, runtime_dir):
        bpy.ops.export_scene.fbx(filepath=str(folder / f"{filename}.fbx"), **kwargs)
    r.location = old_location
    bpy.ops.object.select_all(action="DESELECT")


def look_at(obj, target):
    direction = Vector(target) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def setup_render(scene, collection, preview_path):
    camera_data = bpy.data.cameras.new("PR3D_008_CAM_Portrait_Data")
    camera = bpy.data.objects.new("PR3D_008_CAM_Portrait", camera_data)
    collection.objects.link(camera)
    # Near-front portrait view keeps both side families visible while showing
    # enough floor depth to judge safe-area placement.
    camera.location = (0.0, -18.5, 12.8)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 12.2
    look_at(camera, (0, 1.0, 0.8))
    scene.camera = camera
    for name, loc, energy, size, color in (
        ("Key", (-5, -7, 12), 1500, 6.0, (1.0, 0.68, 0.42)),
        ("Fill", (7, -1, 9), 1100, 5.0, (0.35, 0.55, 1.0)),
        ("Rim", (0, 7, 10), 900, 4.0, (1.0, 0.30, 0.12)),
    ):
        data = bpy.data.lights.new(f"PR3D_008_LGT_{name}_Data", "AREA")
        data.energy, data.shape, data.size, data.color = energy, "DISK", size, color
        light = bpy.data.objects.new(f"PR3D_008_LGT_{name}", data)
        light.location = loc
        collection.objects.link(light)
        look_at(light, (0, 0, 0.8))
    world = scene.world or bpy.data.worlds.new("PR3D_008_World")
    scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.018, 0.025, 0.055, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.30
    # Blender 5.x reports the Eevee engine identifier as BLENDER_EEVEE.
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 1280
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(preview_path)
    scene.render.film_transparent = False


def build():
    repo_root = Path(os.environ["PR3D_REPO_ROOT"]).resolve()
    source_dir = repo_root / "Art/PR3D/Source/Environment"
    export_dir = repo_root / "Art/PR3D/Exports/Environment"
    runtime_dir = repo_root / "Assets/_Projects/Art/PR3D/Environment"
    # Keep every generated PR3D-008 artifact inside the task-owned folders.
    preview_dir = source_dir
    for folder in (source_dir, export_dir, runtime_dir, preview_dir):
        folder.mkdir(parents=True, exist_ok=True)

    # Remove all objects directly from the datablock, including objects that
    # were left outside the active view layer by a previous scripted rebuild.
    for obj in list(bpy.data.objects):
        bpy.data.objects.remove(obj, do_unlink=True)
    for block in bpy.data.collections:
        if block != bpy.context.scene.collection:
            bpy.data.collections.remove(block)
    scene = bpy.context.scene
    scene.name = "PR3D_008_Environment"
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    scene["task"] = TASK
    scene["portrait_safe_area"] = "central 7.0m x 7.0m board/HUD kept clear"
    scene["modularity"] = "each exported root can be instanced independently"

    kit = bpy.data.collections.new("20_ENVIRONMENT_KIT")
    scene.collection.children.link(kit)
    render_collection = bpy.data.collections.new("80_CAMERA_LIGHT")
    scene.collection.children.link(render_collection)

    mats = {
        "brick": material("PR3D_MAT_Env_Brick", (0.66, 0.13, 0.042, 1), 0, 0.54),
        "brick_dark": material("PR3D_MAT_Env_BrickDark", (0.38, 0.055, 0.018, 1), 0, 0.68),
        "brick_light": material("PR3D_MAT_Env_BrickLight", (0.86, 0.22, 0.065, 1), 0, 0.48),
        "stone": material("PR3D_MAT_Env_Stone", (0.25, 0.27, 0.30, 1), 0.2, 0.35),
        "metal": material("PR3D_MAT_Env_Metal", (0.22, 0.20, 0.20, 1), 0.75, 0.25),
        "copper": material("PR3D_MAT_Env_Copper", (0.55, 0.18, 0.06, 1), 0.75, 0.26),
        "dark": material("PR3D_MAT_Env_Dark", (0.018, 0.012, 0.010, 1), 0, 0.85),
        "fire": material("PR3D_MAT_Env_Fire", (1.0, 0.14, 0.01, 1), 0, 0.22, (1.0, 0.025, 0.001, 1)),
        "fire_core": material("PR3D_MAT_Env_FireCore", (1.0, 0.62, 0.05, 1), 0, 0.12, (1.0, 0.22, 0.005, 1)),
        "wood": material("PR3D_MAT_Env_Wood", (0.38, 0.13, 0.035, 1), 0, 0.54),
        "wood_dark": material("PR3D_MAT_Env_WoodDark", (0.16, 0.045, 0.018, 1), 0, 0.66),
        "blue": material("PR3D_MAT_Env_BlueTile", (0.018, 0.11, 0.38, 1), 0.05, 0.22),
        "grout": material("PR3D_MAT_Env_Grout", (0.035, 0.05, 0.08, 1), 0, 0.76),
        "darkgrout": material("PR3D_MAT_Env_DarkGrout", (0.08, 0.025, 0.015, 1), 0, 0.80),
        "terra_a": material("PR3D_MAT_Env_TerracottaA", (0.52, 0.10, 0.035, 1), 0, 0.65),
        "terra_b": material("PR3D_MAT_Env_TerracottaB", (0.33, 0.055, 0.025, 1), 0, 0.72),
        "bulb": material("PR3D_MAT_Env_Bulb", (1.0, 0.45, 0.08, 1), 0, 0.12, (1.0, 0.20, 0.02, 1)),
        "glass": material("PR3D_MAT_Env_Glass", (0.25, 0.42, 0.55, 0.38), 0.05, 0.10),
        "tomato": material("PR3D_MAT_Env_Tomato", (0.82, 0.035, 0.018, 1), 0, 0.36),
        "basil": material("PR3D_MAT_Env_Basil", (0.08, 0.48, 0.025, 1), 0, 0.40),
        "cheese": material("PR3D_MAT_Env_Cheese", (1.0, 0.58, 0.08, 1), 0, 0.46),
        "pepper": material("PR3D_MAT_Env_Pepper", (0.92, 0.32, 0.02, 1), 0, 0.37),
        "ceramic": material("PR3D_MAT_Env_Ceramic", (0.78, 0.52, 0.31, 1), 0, 0.28),
        "mushroom": material("PR3D_MAT_Env_Mushroom", (0.82, 0.70, 0.52, 1), 0, 0.58),
        "soil": material("PR3D_MAT_Env_Soil", (0.11, 0.025, 0.008, 1), 0, 0.95),
        "cloth_blue": material("PR3D_MAT_Env_ClothBlue", (0.12, 0.34, 0.70, 1), 0, 0.82),
        "cloth_red": material("PR3D_MAT_Env_ClothRed", (0.78, 0.12, 0.07, 1), 0, 0.82),
    }
    roots = [
        build_oven(kit, mats),
        build_wall(kit, mats),
        build_floor(kit, mats),
        build_counter(kit, mats),
        build_shelf(kit, mats),
        build_light(kit, mats),
        build_jar(kit, mats),
        build_bowl(kit, mats),
        build_basil(kit, mats),
        build_utensils(kit, mats),
        build_crate(kit, mats),
    ]
    for r in roots:
        batch_root_by_material(r)
    for r in roots:
        export_root(r, export_dir, runtime_dir)

    text = bpy.data.texts.new("PR3D_008_ENVIRONMENT_CONTRACT.md")
    text.write(
        "# PR3D-008 modular environment\n\n"
        "- Units: metres; Blender Z-up; Unity import Y-up/+Z-forward.\n"
        "- Every `PR3D_Environment_*_Root` is an independent instance unit.\n"
        "- Roots are at floor/contact pivots and have applied child transforms.\n"
        "- Keep the central 7x7 gameplay board and HUD/safe area clear.\n"
        "- Recommended placement: wall/oven/shelf above board; counters and props\n"
        "  outside left/right edges; floor below all gameplay visuals.\n"
        "- Add as visual children only; no gameplay roots, colliders, JSON, enums,\n"
        "  scenes, prefabs, or serialized references are modified by this kit.\n"
    )
    preview = preview_dir / "PR3D_008_EnvironmentKit.png"
    setup_render(scene, render_collection, preview)
    blend_path = source_dir / "PR3D_008_Environment.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    bpy.ops.render.render(write_still=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
    print(
        f"{TASK}: roots={len(roots)} meshes={sum(1 for o in bpy.data.objects if o.type == 'MESH')} "
        f"materials={len(mats)} blend={blend_path} preview={preview}"
    )


build()
