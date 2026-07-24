import bpy, os
OUT=os.path.abspath(os.path.join(os.path.dirname(__file__),"../../Exports/Gates")); os.makedirs(OUT,exist_ok=True)
def clear(): bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
def mat(name,c,metal=0,rough=.3):
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name); m.diffuse_color=(*c,1); m.metallic=metal; m.roughness=rough; return m
FRAME=mat("PR3D_Gate_Frame",(.08,.1,.12),.7,.24); INNER=mat("PR3D_Gate_Inner",(.16,.2,.23),.4,.22)
def cube(name,loc,scale,ma,bev=.06):
 bpy.ops.mesh.primitive_cube_add(location=loc); o=bpy.context.object; o.name=name; o.scale=scale; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 b=o.modifiers.new("Soft corners","BEVEL"); b.width=bev; b.segments=3; bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=b.name); o.data.materials.append(ma)
def export():
 bpy.ops.object.select_all(action='SELECT'); bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,"PR3D_Gate_Shared.fbx"),use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH'})
clear()
# chunky tunnel: two square pillars + rounded top beam, sized around existing anchors
cube("Gate_LeftPost",(-.62,.55,0),(.16,.55,.24),FRAME,.09)
cube("Gate_RightPost",(.62,.55,0),(.16,.55,.24),FRAME,.09)
cube("Gate_TopBeam",(0,1.05,0),(.78,.16,.24),FRAME,.09)
cube("Gate_InnerAccent",(0,.52,.245),(.48,.42,.025),INNER,.025)
export()
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(os.path.dirname(__file__),"PR3D_Gates_Master.blend"))
