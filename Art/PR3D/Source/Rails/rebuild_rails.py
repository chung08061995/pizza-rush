import bpy, math, os
from mathutils import Vector

OUT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../../Exports/Rails"))
os.makedirs(OUT, exist_ok=True)

def clear():
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    for d in (bpy.data.meshes, bpy.data.curves, bpy.data.materials):
        pass

def mat(name, color, metallic=0.0, rough=.45):
    m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.diffuse_color=(*color,1); m.metallic=metallic; m.roughness=rough
    return m
CHAR=mat("PR3D_Rail_Charcoal",(0.055,.07,.09),.15,.3)
STEEL=mat("PR3D_Rail_Steel",(.25,.3,.34),.8,.25)
ARROW=mat("PR3D_Rail_Arrow",(.16,.42,.48),.3,.28)
RUBBER=mat("PR3D_Rail_Rubber",(.025,.03,.035),.1,.55)

def cube(name, loc, scale, material, bevel=.06, rot=0):
    bpy.ops.mesh.primitive_cube_add(location=loc, rotation=(0,0,rot))
    o=bpy.context.object; o.name=name; o.scale=scale; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        b=o.modifiers.new("Edge bevel","BEVEL"); b.width=bevel; b.segments=3
        bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=b.name)
    o.data.materials.append(material); return o

def export(path):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, apply_unit_scale=True, axis_forward='-Z', axis_up='Y', object_types={'MESH'})

def straight(path):
    clear()
    cube("Rail_Straight_Belt",(0,.16,0),(2.64,.10,.42),CHAR,.08)
    cube("Rail_Straight_LeftHousing",(0,.32,-.5),(2.64,.11,.08),STEEL,.04)
    cube("Rail_Straight_RightHousing",(0,.32,.5),(2.64,.11,.08),STEEL,.04)
    for x in [-2.2,-1.6,-1,-.4,.2,.8,1.4,2.0]:
        cube("Belt_Slat_%02d"%int((x+2.2)*10),(x,.275,0),(.045,.018,.34),RUBBER,.015)
    for x in [-1.65,-.55,.55,1.65]:
        cube("Arrow_%02d"%int(x*10),(x,.285,0),(.15,.018,.035),ARROW,.02)
        cube("ArrowHead_%02d"%int(x*10),(x+.15,.285,0),(.04,.018,.09),ARROW,.02,rot=math.pi/4)
    export(path)

def curve(path, side=1):
    clear()
    # quarter turn centered at origin; segments form smooth continuous belt
    r=1.7
    for i in range(10):
        a=(i/9)*math.pi/2
        x=side*r*math.sin(a); z=side*r*(1-math.cos(a))
        cube("Rail_Curve_Belt_%02d"%i,(x,.16,z),(.26,.10,.42),CHAR,.07,rot=side*a)
        cube("Rail_Curve_Housing_%02d"%i,(x,.32,z+side*.5),(.26,.11,.08),STEEL,.04,rot=side*a)
    cube("Curve_Arrow",(side*.9,.285,side*.35),(.18,.018,.04),ARROW,.02,rot=side*math.pi/4)
    export(path)

def connector(path):
    clear()
    cube("Rail_Connector_Body",(0,.18,0),(.30,.16,.50),STEEL,.06)
    cube("Rail_Connector_DarkBelt",(0,.29,0),(.18,.025,.34),CHAR,.02)
    export(path)

straight(os.path.join(OUT,"PR3D_Rail_Straight_5p28.fbx"))
curve(os.path.join(OUT,"PR3D_Rail_Curve90_Left.fbx"),1)
curve(os.path.join(OUT,"PR3D_Rail_Curve90_Right.fbx"),-1)
connector(os.path.join(OUT,"PR3D_Rail_Connector.fbx"))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(os.path.dirname(__file__),"PR3D_Rails_Master.blend"))
