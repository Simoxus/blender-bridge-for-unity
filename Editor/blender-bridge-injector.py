import bpy
import os
import sys
import json

bpy.context.preferences.view.show_splash = False

def load_settings():
    """Loads settings written out by Unity."""
    if "--" not in sys.argv:
        raise RuntimeError("No arguments passed to Blender")

    argv = sys.argv[sys.argv.index("--") + 1:]
    if len(argv) < 2:
        raise RuntimeError("Expected model path and settings path arguments")

    settings_path = argv[1]
    with open(settings_path, "r") as f:
        return json.load(f)


SETTINGS = load_settings()

class UnityModelExporter:
    def __init__(self, model_path):
        self.model_path = model_path
        self.filename = os.path.basename(model_path)
        self.extension = os.path.splitext(model_path)[1].lower()

    def load_model(self):
        # Delete everything in startup scene
        bpy.ops.object.select_all(action='SELECT')
        bpy.ops.object.delete(use_global=False)

        if self.extension == '.fbx':
            bpy.ops.import_scene.fbx(filepath=self.model_path)
        elif self.extension == '.obj':
            bpy.ops.wm.obj_import(filepath=self.model_path)
        elif self.extension == '.dae':
            bpy.ops.wm.collada_import(filepath=self.model_path)
        else:
            print(f"Unsupported format '{self.extension}'")
            return

        bpy.context.scene['unity_model_path'] = self.model_path
        bpy.context.scene['unity_model_format'] = self.extension

        bpy.context.tool_settings.mesh_select_mode = (False, False, True)
        bpy.ops.object.select_all(action='SELECT')

        if SETTINGS["loadTextures"]:
            self.apply_textures()

        # Set shading to solid and also zoom in
        for area in bpy.context.screen.areas:
            if area.type == 'VIEW_3D':
                for space in area.spaces:
                    if space.type == 'VIEW_3D':
                        space.shading.type = 'SOLID'
                        if SETTINGS["loadTextures"]:
                            space.shading.color_type = 'TEXTURE'
                override = {'area': area, 'region': area.regions[-1]}
                with bpy.context.temp_override(**override):
                    bpy.ops.view3d.view_selected()

        bpy.ops.object.select_all(action='DESELECT')
        bpy.ops.ed.undo_history_clear()

        print(f"Loaded '{self.filename}'")

    def apply_textures(self):
        search_dir = os.path.normpath(SETTINGS["texturePath"])

        if not os.path.exists(search_dir):
            print(f"Texture path not found: {search_dir}")
            return

        for obj in bpy.context.selected_objects:
            if obj.type == 'MESH':
                for slot in obj.material_slots:
                    if slot.material:
                        self.setup_material_node(slot.material, search_dir)

    def setup_material_node(self, mat, search_dir):
        mat.use_nodes = True
        nodes = mat.node_tree.nodes
        links = mat.node_tree.links

        principled = next((n for n in nodes if n.type == 'BSDF_PRINCIPLED'), None)
        if not principled:
            return

        # Set roughness to 1
        if 'Roughness' in principled.inputs:
            principled.inputs['Roughness'].default_value = 1.0

        for ext in SETTINGS["textureExtensions"]:
            image_name = f"{mat.name}{ext}"
            full_image_path = os.path.join(search_dir, image_name)

            if os.path.exists(full_image_path):
                try:
                    img = bpy.data.images.load(full_image_path)
                    tex_node = next((n for n in nodes if n.type == 'TEX_IMAGE'), None)

                    if not tex_node:
                        tex_node = nodes.new('ShaderNodeTexImage')
                        tex_node.location = (-300, 300)

                    tex_node.image = img

                    if 'Base Color' in principled.inputs:
                        links.new(tex_node.outputs['Color'], principled.inputs['Base Color'])

                    print(f"Applied texture '{image_name}'")
                    break
                except Exception as e:
                    print(f"Failed to load '{image_name}': {e}")

    @staticmethod
    def export_to_unity(filepath, file_format):
        """Export scene back to Unity in the original format"""
        if file_format == '.fbx':
            object_types = {'ARMATURE', 'MESH', 'EMPTY'}
            if SETTINGS.get("includeCameras"):
                object_types.add('CAMERA')
            if SETTINGS.get("includeLights"):
                object_types.add('LIGHT')
            if SETTINGS.get("includeOther"):
                object_types.add('OTHER')

            bpy.ops.export_scene.fbx(
                filepath=filepath,
                global_scale=1.0,
                apply_unit_scale=True,
                apply_scale_options='FBX_SCALE_UNITS',
                object_types=object_types,
                add_leaf_bones=False,
                primary_bone_axis='Y',
                secondary_bone_axis='X',
                armature_nodetype='NULL',
                bake_anim=SETTINGS.get("bakeAnimation", True),
                axis_forward='-Z',
                axis_up='Y'
            )
        elif file_format == '.obj':
            bpy.ops.wm.obj_export(
                filepath=filepath,
                export_animation=False,
                apply_modifiers=True,
                forward_axis='NEGATIVE_Z',
                up_axis='Y'
            )
        elif file_format == '.dae':
            bpy.ops.wm.collada_export(
                filepath=filepath,
                apply_modifiers=True
            )
        else:
            raise ValueError(f"Unsupported export format '{file_format}'")

class WM_OT_save_unity_model(bpy.types.Operator):
    bl_idname = "wm.save_unity_model"
    bl_label = "Save Unity Model"
    bl_description = "Export back to Unity"
    bl_options = {'REGISTER'}

    from_shortcut: bpy.props.BoolProperty(default=False)  # type: ignore

    def execute(self, context):
        if 'unity_model_path' not in context.scene:
            bpy.ops.wm.save_mainfile('INVOKE_DEFAULT')
            return {'FINISHED'}

        path = context.scene['unity_model_path']
        file_format = context.scene.get('unity_model_format', '.fbx')

        try:
            UnityModelExporter.export_to_unity(path, file_format)
            self.report({'INFO'}, f"Saved '{os.path.basename(path)}' to Unity")

            if self.from_shortcut and SETTINGS["closeAfterQuickSave"]:
                bpy.ops.wm.quit_blender()
            elif not self.from_shortcut and SETTINGS["closeAfterManualSave"]:
                bpy.ops.wm.quit_blender()

        except Exception as e:
            self.report({'ERROR'}, f"Save failed '{str(e)}'")
            return {'CANCELLED'}

        return {'FINISHED'}


def menu_func_export(self, context):
    if 'unity_model_path' in context.scene:
        file_format = context.scene.get('unity_model_format', '.fbx').upper()
        self.layout.operator(
            WM_OT_save_unity_model.bl_idname,
            text=f"{file_format[1:]} (back to original Unity asset)",
            icon='EXPORT'
        )


addon_keymaps = []


def register():
    bpy.utils.register_class(WM_OT_save_unity_model)
    bpy.types.TOPBAR_MT_file_export.append(menu_func_export)

    # Add keybind
    wm = bpy.context.window_manager
    kc = wm.keyconfigs.addon
    if kc:
        km = kc.keymaps.new(name='Window', space_type='EMPTY')
        kmi = km.keymap_items.new(WM_OT_save_unity_model.bl_idname, 'S', 'PRESS', ctrl=True)
        kmi.properties.from_shortcut = True
        addon_keymaps.append((km, kmi))


def unregister():
    # Remove keybind
    for km, kmi in addon_keymaps:
        km.keymap_items.remove(kmi)
    addon_keymaps.clear()

    bpy.types.TOPBAR_MT_file_export.remove(menu_func_export)
    bpy.utils.unregister_class(WM_OT_save_unity_model)


if __name__ == "__main__":
    register()

    if "--" in sys.argv:
        argv = sys.argv[sys.argv.index("--") + 1:]
        if argv:
            model_path = argv[0]
            exporter = UnityModelExporter(model_path)
            exporter.load_model()