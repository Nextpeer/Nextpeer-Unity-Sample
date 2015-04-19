#!/usr/bin/python
import os
import sys
import shutil
import json
from distutils.version import LooseVersion

from mod_pbxproj import XcodeProject


NP_DID_RUN_FILE = ".npdidrun"
CONFIGURATION_FILE_NAME = "config.json"


def main():
    script_path = os.path.abspath(os.path.dirname(__file__))
    ios_path = os.path.abspath(sys.argv[1])
    unity_assets_path = os.path.abspath(sys.argv[2])
    np_resources_bundle = "NPResources.bundle"
    ios_sdk_version = sys.argv[3]
    unity_version = sys.argv[4]
    np_xcode_path = os.path.normpath(os.path.join(script_path, "XcodeFiles"))
    
    print 'Nextpeer path: ' + script_path
    print 'Xcode project path: ' + ios_path
    print 'Unity Assets path: ' + unity_assets_path
    print "NP Resources Bundle: " + np_resources_bundle
    print "iOS SDK Version: " + ios_sdk_version
    print "Unity version: " + unity_version
    print "Nextpeer XcodeFiles path: " + np_xcode_path
    
    # Have we already patched this build? If yes, do nothing, for fear of creating
    # duplicate changes that will bite us in the behind.
    if os.path.exists(os.path.join(ios_path, NP_DID_RUN_FILE)):
        print "Build was already patched, doing nothing. Enjoy Nextpeer!"
        return

    # Load configuration (for future use):
    config = load_config(os.path.join(unity_assets_path, "Editor", "Nextpeer", CONFIGURATION_FILE_NAME))

    # Extract Nextpeer SDK into the Xcode project:
    np_sdk_zip = os.path.join(script_path, "NextpeerSDK.zip")
    if not os.path.exists(np_sdk_zip):
        raise Exception("NextpeerSDK zip did not exist - unable to add Nextpeer to Xcode project (expected %s)" % np_sdk_zip)
    cmd = 'unzip -o -qq "' + os.path.join(np_sdk_zip) + '" -d "' + ios_path + '"'
    os.system(cmd)
    
    # Verify the path of the resources bundle to use:
    selected_bundle_path = os.path.join(ios_path, "NextpeerSDK", np_resources_bundle)
    if not os.path.exists(selected_bundle_path):
        raise Exception("Requested bundle was not found - unable to add Nextpeer to Xcode project (expected %s)" % selected_bundle_path)
    
    # Open the Xcode project for editing:
    xcode_project_path = os.path.join(ios_path, 'Unity-iPhone.xcodeproj/project.pbxproj')
    print 'Opening: ' + xcode_project_path
    project = XcodeProject.Load(xcode_project_path)

    # Create a group for Nextpeer:
    print "Creating Nextpeer group"
    parent = project.get_or_create_group("Nextpeer")
    
    # Add the Nextpeer framework and resources bundle:
    print "Adding Nextpeer framwork and resources bundle"
    project.add_folder(os.path.join(ios_path, "NextpeerSDK/Nextpeer.framework"), parent=parent)
    project.add_folder(selected_bundle_path, parent=parent)
    
    # Add system frameworks:
    print "Adding system frameworks"
    system_frameworks_path = 'System/Library/Frameworks'
    frameworks_parent = project.get_or_create_group("Frameworks", parent=parent)
    
    # Required
    for f in ("CoreText", "OpenGLES", "Security", "MobileCoreServices", "StoreKit", "SystemConfiguration", "CFNetwork", "MessageUI", "QuartzCore", "UIKit", "CoreGraphics", "Foundation", "AVFoundation", "AssetsLibrary", "ImageIO", "Social"):
        project.add_file(os.path.join(system_frameworks_path, f+'.framework'), parent=frameworks_parent, tree='SDKROOT', weak=False)

    # Optional
    for f in ("AdSupport", ):
        project.add_file(os.path.join(system_frameworks_path, f+'.framework'), parent=frameworks_parent, tree='SDKROOT', weak=True)    
    
    # Add all source files from Classes directory:
    print "Adding Nextpeer sources"
    for f in os.listdir(os.path.join(np_xcode_path, "Classes")):
        if f.endswith((".h", ".mm")):
            file_refs = project.add_file(os.path.join(np_xcode_path, "Classes", f), parent=parent, tree='SOURCE_ROOT')
            # The first file ref is the added file and the rest are refs added to the build phases.
            for build_file_ref in file_refs[1:]:
                build_file_ref.add_compiler_flag("-fobjc-arc")
    
    # Add -ObjC as a linker flag:
    print "Adding -ObjC linker flag"
    project.add_other_ldflags('-ObjC')
    
    print 'Saving: ' + xcode_project_path
    project.saveFormat3_2()
    
    # Populate NPDynamicDefines.h:
    # Unity 4.2 changed the name and file of their app delegate class from AppController to UnityAppController.
    # We chose to handle this by defining a special macro, with our own app delegate using the appropriate
    # class depending on this macro.
    dynamic_defines_path = os.path.join(np_xcode_path, "Classes", "NPDynamicDefines.h")
    if LooseVersion(unity_version) >= LooseVersion("4.2"):
        print "Populating NPDynamicDefines.h at path: " + dynamic_defines_path
        with open(dynamic_defines_path, "wb") as dd_out:
            dd_out.writelines( ("#define UNITY_4_2_APP_CONTORLLER_STYLE",
                                "\n") )

    # Edit main.mm file to use our own AppController:
    main_file_path = os.path.join(ios_path, "Classes/main.mm")
    print "Patching main.mm located at: " + main_file_path
    main_file_text = None
    with open(main_file_path, "rb") as main_file_reader:
        main_file_text = main_file_reader.read()
        
        if LooseVersion(unity_version) >= LooseVersion("4.2"):
            main_file_text = main_file_text.replace(r'const char* AppControllerClassName = "UnityAppController";',
                                                     r'const char* AppControllerClassName = "NextpeerAppController";')
        else:
            main_file_text = main_file_text.replace(r'UIApplicationMain(argc, argv, nil, @"AppController");',
                                                    r'UIApplicationMain(argc, argv, nil, @"NextpeerAppController");')
        
    if main_file_text is not None:
        with open(main_file_path, "wb") as main_file_writer:
            main_file_writer.write(main_file_text)

    # Fix for crash on iOS 7: http://forum.unity3d.com/threads/203506-Workaround-ios7-DisplayLink-Scrolling-results-in-crash
    unity_app_ctrlr_path = os.path.join(ios_path, "Classes/UnityAppController.mm")
    old_unity_app_ctrlr_path = os.path.join(ios_path, "Classes/AppController.mm")
    if os.path.exists(unity_app_ctrlr_path):
        print "Patching UnityAppController.mm"
        unity_app_ctrlr_patched = None
        with open(unity_app_ctrlr_path, "rb") as unity_app_ctrlr_reader:
            lines = unity_app_ctrlr_reader.read().split("\n")
            start = _find_in_list(lambda s: "[_displayLink setPaused: YES];" in s, lines)
            end = _find_in_list(lambda s: "[_displayLink setPaused: NO];" in s, lines)
            if start != -1 and end != -1 and "/*" not in lines[start] and "*/" not in lines[end]:
                for line_ix in range(start, end+1):
                    lines[line_ix] = "//" + lines[line_ix]
                unity_app_ctrlr_patched = "\n".join(lines)

        if unity_app_ctrlr_patched == None:
            print "WARNING: UnityAppController.mm wasn't patched"
        else:
            with open(unity_app_ctrlr_path, "wb") as unity_app_ctrlr_writer:
                unity_app_ctrlr_writer.write(unity_app_ctrlr_patched)
    elif os.path.exists(old_unity_app_ctrlr_path):
        print "Patching AppController.mm"
        with open(old_unity_app_ctrlr_path, "rb") as old_app_ctrlr_reader:
            updated_code = old_app_ctrlr_reader.read().replace("#define USE_DISPLAY_LINK_IF_AVAILABLE 1", "#define USE_DISPLAY_LINK_IF_AVAILABLE 0")
        with open(old_unity_app_ctrlr_path, "wb") as old_app_ctrlr_writer:
            old_app_ctrlr_writer.write(updated_code)


    # If we're compiling for the simulator, patch RegisterMonoModules.cpp so we can run in the simulator.
    # RegisterMonoModules has a fucntion that registers our native functions (that are used in P/Invoke)
    # with the Unity runtime. The registration calls look like this:
    # mono_dl_register_symbol("_NPReleaseVersionString", (void*)&_NPReleaseVersionString);
    # However, for an unknown reason, if the target platform is the simulator, those calls won't be registered, 
    # as they are inside an '#if !(TARGET_IPHONE_SIMULATOR)' directive. To run in the simulaor, then, we must
    # patch that file to always register our functions.
    # Currently, we'll always perform this patch, because the dev may switch the SDK type and perform an append
    # (in which case our Python post-build script won't run).
    if False: #ios_sdk_version == "SimulatorSDK":
        # We assume the format of RegisterMonoModules to remain the same for some time - it didn't change between Unity 3 and 4.

        rmm_file_path = os.path.join(ios_path, "Libraries/RegisterMonoModules.cpp")
        print "Patching RegisterMonoModules.cpp so it'll work in the simulator, location: " + rmm_file_path
        
        func_lines = None
        with open(rmm_file_path, "rb") as rmm_reader:
            func_lines = rmm_reader.readlines()

        # The method that registers our functions, mono_dl_register_symbol, is itself declared within
        # an '#if !(TARGET_IPHONE_SIMULATOR)' directive. We thus have to do a two-pass run on the file -
        # in the first run, move that declaration outside of the #if. In the second run, move the #endif
        # that masks the list of calls to mono_dl_register_symbol (that register our functions) above
        # the calls themselves.
        if func_lines is not None:
            # First pass: find the declaration of mono_dl_register_symbol and move it to safety.
            for ix in range(len(func_lines)):
                line = func_lines[ix]
                if "mono_dl_register_symbol" in line:
                    # Save the declaration:
                    register_symbol_line = line
                    # Comment out the original declaration, so that it won't be declared twice:
                    func_lines[ix] = "//" + line
                elif "#endif" in line:
                    # Insert the declaration we found above right after the #endif:
                    func_lines.insert(ix+1, register_symbol_line)
                    break

            # Second pass: un-mask the calls to mono_dl_register_symbol.
            added_endif = False
            for ix in range(len(func_lines)):
                line = func_lines[ix]
                if not added_endif and 'mono_dl_register_symbol("_NP' in line:
                    # Insert an #endif above the first call to mono_dl_register_symbol
                    func_lines.insert(ix, "#endif" + os.linesep)
                    added_endif = True
                elif added_endif:
                    if "#endif" in line:
                        # Delete the next #endif we find (the one we inserted above takes its place)
                        del func_lines[ix]
                        break

            with open(rmm_file_path, "wb") as rmm_writer:
                rmm_writer.writelines(func_lines)

    # Mark the build as patched by creating an empty file to serve as a flag:
    with open(os.path.join(ios_path, NP_DID_RUN_FILE), "wb"):
        pass


def _find_in_list(func, lst):
    for ix, line in enumerate(lst):
        if func(line):
            return ix

    return -1

def load_config(config_file_path):
    if not os.path.exists(config_file_path):
        return {}
    
    with open(config_file_path, "rb") as config_file:
        return json.load(config_file)
    


if __name__ == '__main__':
    try:
        main()
    except Exception as e:
        print "ERROR: %s" % e
        raise
