#!/usr/bin/env python3
"""Build the reproducible PR3D source/export/Unity asset manifest."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]
OUTPUTS = (
    REPO / "Art/PR3D/PR3D_manifest.json",
    REPO / "Assets/_Projects/Art/PR3D/PR3D_manifest.json",
)

GROUPS = (
    {
        "family": "board",
        "task": "PR3D-003",
        "source": "Art/PR3D/Source/Board/PR3D_BoardKit.blend",
        "builder": "Art/PR3D/Source/Board/build_pr3d_003_board.py",
        "export_dir": "Art/PR3D/Exports/Board",
        "unity_dir": "Assets/_Projects/Art/PR3D/Board/Models",
        "source_audit": {
            "mesh_objects": 4,
            "mesh_datablocks": 4,
            "triangles": 1332,
            "materials": 4,
        },
    },
    {
        "family": "rails",
        "task": "PR3D-004",
        "source": "Art/PR3D/Source/Rails/PR3D_Rails_Master.blend",
        "builder": "Art/PR3D/Source/Rails/build_rails.py",
        "export_dir": "Art/PR3D/Exports/Rails",
        "unity_dir": "Assets/_Projects/Art/PR3D/Rails",
        "source_audit": {
            "mesh_objects": 37,
            "mesh_datablocks": 37,
            "triangles": 6000,
            "materials": 6,
        },
    },
    {
        "family": "gates",
        "task": "PR3D-005",
        "source": "Art/PR3D/Source/Gates/PR3D_Gates.blend",
        "builder": "Art/PR3D/Source/Gates/setup_gates.py",
        "export_dir": "Art/PR3D/Exports/Gates",
        "unity_dir": "Assets/_Projects/Art/PR3D/Gates/Models",
        "source_audit": {
            "mesh_objects": 11,
            "mesh_datablocks": 2,
            "triangles": 25668,
            "materials": 5,
            "shared_mesh_variants": 10,
        },
    },
    {
        "family": "pizza",
        "task": "PR3D-006",
        "source": "Art/PR3D/Source/Pizza/PR3D_006_PizzaVariants.blend",
        "builder": "Art/PR3D/Source/Pizza/build_pr3d_006_pizza.py",
        "export_dir": "Art/PR3D/Exports/Pizza",
        "unity_dir": "Assets/_Projects/Art/PR3D/Pizza",
        "source_audit": {
            "mesh_objects": 20,
            "mesh_datablocks": 11,
            "triangles": 20560,
            "materials": 5,
            "runtime_shared_meshes": 1,
            "material_variants": 10,
        },
    },
    {
        "family": "containers",
        "task": "PR3D-007",
        "source": "Art/PR3D/Source/Containers/PR3D_Containers_Master.blend",
        "builder": "Art/PR3D/Source/Containers/build_pr3d_007_containers.py",
        "export_dir": "Art/PR3D/Exports/Containers",
        "unity_dir": "Assets/_Projects/Art/PR3D/Containers",
        "source_audit": {
            "mesh_objects": 70,
            "mesh_datablocks": 35,
            "triangles": 15160,
            "materials": 5,
        },
    },
    {
        "family": "environment",
        "task": "PR3D-008",
        "source": "Art/PR3D/Source/Environment/PR3D_008_Environment.blend",
        "builder": "Art/PR3D/Source/Environment/build_pr3d_008_environment.py",
        "export_dir": "Art/PR3D/Exports/Environment",
        "unity_dir": "Assets/_Projects/Art/PR3D/Environment",
        "source_audit": {
            "mesh_objects": 43,
            "mesh_datablocks": 43,
            "triangles": 23756,
            "materials": 27,
            "module_families": 11,
        },
    },
)


def digest(path: Path) -> str:
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.hexdigest()


def record(path: Path) -> dict[str, object]:
    return {
        "path": path.relative_to(REPO).as_posix(),
        "sha256": digest(path),
        "bytes": path.stat().st_size,
    }


def assets(directory: str) -> list[dict[str, object]]:
    root = REPO / directory
    paths = sorted(
        path
        for path in root.iterdir()
        if path.is_file() and path.suffix.lower() in {".fbx", ".glb"}
    )
    return [record(path) for path in paths]


def build() -> dict[str, object]:
    groups = []
    for definition in GROUPS:
        group = dict(definition)
        group["source_asset"] = record(REPO / group.pop("source"))
        group["builder_asset"] = record(REPO / group.pop("builder"))
        group["exports"] = assets(group.pop("export_dir"))
        group["unity_assets"] = assets(group.pop("unity_dir"))
        groups.append(group)

    return {
        "schema_version": 3,
        "task": "PR3D-009",
        "status": "verified",
        "generated_on": "2026-07-26",
        "concept_reference": "docs/reference/pizza-factory-concept.png",
        "coordinate_contract": {
            "authoring": "Blender native Z-up",
            "unit_system": "METRIC",
            "meters_per_blender_unit": 1.0,
            "unity_result": "Y-up, +Z-forward",
            "fbx_axis_forward": "-Z",
            "fbx_axis_up": "Y",
            "unity_import_global_scale": 1.0,
            "unity_use_file_scale": True,
            "unity_bake_axis_conversion": False,
        },
        "optimization": {
            "all_source_mesh_scales_applied": True,
            "all_source_mesh_rotations_applied": True,
            "all_source_meshes_have_uvs": True,
            "solid_color_materials_use_shared_URP_materials": True,
            "texture_atlas": "not required: current slice uses solid-color/material parameters and no bitmap texture dependencies",
            "lod_policy": "LOD0-only: each runtime module is already low-poly and the complete authored family audit is below 100k triangles; re-evaluate after rollout composition profiling",
            "generated_backup_files_removed": True,
        },
        "asset_groups": groups,
        "runtime_integration": {
            "task": "PR3D-010",
            "status": "verified",
            "visual_prefab_count": 44,
            "prefab_root": "Assets/_Projects/Art/PR3D/Prefabs",
            "derived_mesh_root": "Assets/_Projects/Art/PR3D/Derived",
            "strategy": "reuse existing serialized gameplay renderers; add collider-free board/environment children",
            "concept_iteration_evidence": {
                "before": "Assets/_Projects/Art/PR3D/Evidence/PR3D_Phase4_Iteration01.png",
                "after": "Assets/_Projects/Art/PR3D/Evidence/PR3D_Phase4_FinalPortrait_GateRefined.png",
            },
            "gameplay_prefab_mono_behaviour_blocks_changed": 0,
            "gameplay_prefab_collider_blocks_changed": 0,
            "runtime_visual_colliders": 0,
            "runtime_visual_mono_behaviours": 0,
            "runtime_production_lines": 7,
            "runtime_productions": 128,
            "runtime_null_line_places": 0,
            "runtime_null_production_skins": 0,
        },
        "validation": {
            "blender_mcp_scene_count": 6,
            "blender_mcp_units_metric": True,
            "blender_mcp_transform_uv_audit": "pass",
            "unity_mcp_imported_asset_count": 25,
            "unity_mcp_scale_pivot_material_audit": "pass",
            "unity_visual_colliders": 0,
            "unity_visual_mono_behaviours": 0,
            "unity_pr3d_console_errors_after_refresh": 0,
            "known_external_package_errors_after_refresh": {
                "count": 2,
                "package": "com.draft.unitydraftutils",
                "message": "MonoBehaviourLifecycleCallbacks.cs has no meta file in immutable package folder",
            },
            "level_301_json_sha256": "7115923d4205df433c54d12e59c64ba5726db82f394bdc0001a89e64c5482faa",
            "gameplay_contract_changes": [],
        },
    }


def main() -> None:
    manifest = json.dumps(build(), indent=2, ensure_ascii=False) + "\n"
    for output in OUTPUTS:
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(manifest, encoding="utf-8")
    print(f"Wrote {len(OUTPUTS)} manifests")


if __name__ == "__main__":
    main()
