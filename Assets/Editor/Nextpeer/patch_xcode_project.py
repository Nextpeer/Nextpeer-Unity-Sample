#!/usr/bin/python
import os
import sys
import glob
import shutil
import plistlib
from distutils.version import LooseVersion

from mod_pbxproj import XcodeProject


NP_DID_RUN_FILE = ".npdidrun"


def main():
    script_path = os.path.abspath(os.path.dirname(__file__))
    ios_path = os.path.abspath(sys.argv[1])
    unity_assets_path = os.path.abspath(sys.argv[2])
    np_resources_bundle = sys.argv[3]
    ios_sdk_version = sys.argv[4]
    unity_version = sys.argv[5]
    np_xcode_path = os.path.normpath(os.path.join(script_path, "XcodeFiles"))

    fb_sso_url_scheme = None
    if len(sys.argv) >= 7:
        fb_sso_url_scheme = sys.argv[6]
    
    print 'Nextpeer path: ' + script_path
    print 'Xcode project path: ' + ios_path
    print 'Unity Assets path: ' + unity_assets_path
    print "NP Resources Bundle: " + np_resources_bundle
    print "iOS SDK Version: " + ios_sdk_version
    print "Unity version: " + unity_version
    print "Nextpeer XcodeFiles path: " + np_xcode_path
    
    # Have we already patched this build?
    if os.path.exists(os.path.join(ios_path, NP_DID_RUN_FILE)):
        print "Build was already patched, doing nothing. Enjoy Nextpeer!"
        return

    # Extract Nextpeer SDK into iOS project path
    np_sdk_zip = os.path.join(script_path, "NextpeerSDK.zip")
    if not os.path.exists(np_sdk_zip):
        raise Exception("NextpeerSDK zip did not exist - unable to add Nextpeer to Xcode project (expected %s)" % np_sdk_zip)
    cmd = 'unzip -o -qq "' + os.path.join(np_sdk_zip) + '" -d "' + ios_path + '"'
    os.system(cmd)
    
    # Find the path of the resources bundle to use.
    selected_bundle_path = None
    for bundle in glob.glob(os.path.join(ios_path, "NextpeerSDK/Resources/*.bundle")):
        if bundle.endswith(np_resources_bundle):
            selected_bundle_path = os.path.join(ios_path, "NextpeerSDK/Resources", os.path.basename(bundle))
    
    if selected_bundle_path == None:
        raise Exception("Requested bundle was not found - unable to add Nextpeer to Xcode project (expected %s)" % np_resources_bundle)
    
    # Open the xcode project
    xcode_project_path = os.path.join(ios_path, 'Unity-iPhone.xcodeproj/project.pbxproj')
    print 'Opening: ' + xcode_project_path
    project = XcodeProject.Load(xcode_project_path)

    # Create a group for Nextpeer
    print "Creating Nextpeer group"
    parent = project.get_or_create_group("Nextpeer")
    
    # Add the Nextpeer framework and resources bundle
    print "Adding Nextpeer framwork and resources bundle"
    project.add_folder(os.path.join(ios_path, "NextpeerSDK/Nextpeer.framework"), parent=parent)
    project.add_folder(selected_bundle_path, parent=parent)
    
    # Add system frameworks
    print "Adding system frameworks"
    system_frameworks_path = 'System/Library/Frameworks'
    frameworks_parent = project.get_or_create_group("Frameworks", parent=parent)
    
    # Required
    for f in ("CoreText", "OpenGLES", "Security", "MobileCoreServices", "StoreKit", "SystemConfiguration", "CFNetwork", "MessageUI", "QuartzCore", "UIKit", "CoreGraphics", "Foundation"):
        project.add_file(os.path.join(system_frameworks_path, f+'.framework'), parent=frameworks_parent, tree='SDKROOT', weak=False)

    # Optional
    for f in ("AdSupport", ):
        project.add_file(os.path.join(system_frameworks_path, f+'.framework'), parent=frameworks_parent, tree='SDKROOT', weak=True)    
    
    # Add libs
    print "Adding system libs"
    system_libs_path = 'usr/lib'
    libs_parent = project.get_or_create_group('Libraries', parent=parent)
    for l in ["libz.dylib", "libsqlite3.dylib"]:
        project.add_file(os.path.join(system_libs_path, l), parent=libs_parent, tree='SDKROOT')    
    
    # Add all source files from Classes directory
    print "Adding Nextpeer sources"
    for f in os.listdir(os.path.join(np_xcode_path, "Classes")):
        if f.endswith((".h", ".mm")) and not f.startswith("AppController"):
            project.add_file(os.path.join(np_xcode_path, "Classes", f), parent=parent, tree='SOURCE_ROOT')
    
    # Add -ObjC
    print "Adding -ObjC linker flag"
    project.add_other_ldflags('-ObjC')
        
    print 'Saving: ' + xcode_project_path
    project.saveFormat3_2()
    
    # Populate NPDynamicDefines.h:
    dynamic_defines_path = os.path.join(np_xcode_path, "Classes", "NPDynamicDefines.h")

    # But first, clear the file:
    with open(dynamic_defines_path, "wb") as dd_out:
        dd_out.write("\n")

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


    # If we're compiling for the simulator, patch RegisterMonoModules.cpp so we can run in the simulator:
    # NB: currently, we'll always perform this patch, because the dev may switch the SDK type and perform an append.
    if True: #ios_sdk_version == "SimulatorSDK":
        # We assume the format of RegisterMonoModules to remain the same for some time. It didn't change between Unity 3 and 4...

        rmm_file_path = os.path.join(ios_path, "Libraries/RegisterMonoModules.cpp")
        print "Patching RegisterMonoModules.cpp so it'll work in the simulator, location: " + rmm_file_path
        
        func_lines = None
        with open(rmm_file_path, "rb") as rmm_reader:
            func_lines = rmm_reader.readlines()

        if func_lines is not None:
            for ix in range(len(func_lines)):
                line = func_lines[ix]
                if "mono_dl_register_symbol" in line:
                    register_symbol_line = line
                    func_lines[ix] = "//" + line
                elif "#endif" in line:
                    func_lines.insert(ix+1, register_symbol_line)
                    break

            added_endif = False
            for ix in range(len(func_lines)):
                line = func_lines[ix]
                if not added_endif and 'mono_dl_register_symbol("_NP' in line:
                    func_lines.insert(ix, "#endif" + os.linesep)
                    added_endif = True
                elif added_endif:
                    if "#endif" in line:
                        del func_lines[ix]
                        break

            with open(rmm_file_path, "wb") as rmm_writer:
                rmm_writer.writelines(func_lines)

    # Patching Info.plist to add the facebook SSO URL scheme if required:
    if fb_sso_url_scheme is not None:
        print "Adding Facebook SSO URL Scheme to Info.plist"
        info_plist_path = os.path.join(ios_path, "Info.plist")
        info_plist = plistlib.readPlist(info_plist_path)

        url_scheme_exists = False
        if 'CFBundleURLTypes' not in info_plist:
            info_plist['CFBundleURLTypes'] = []

        url_types = info_plist['CFBundleURLTypes']
        for url_type_dict in url_types:
            if 'CFBundleURLSchemes' not in url_type_dict:
                continue
            if fb_sso_url_scheme in url_type_dict['CFBundleURLSchemes']:
                break
        else:
            url_types.append({'CFBundleURLSchemes': [fb_sso_url_scheme]})
            plistlib.writePlist(info_plist, info_plist_path)
        

    # Mark the build as patched with an empty file:
    with open(os.path.join(ios_path, NP_DID_RUN_FILE), "wb"):
        pass


def _find_in_list(func, lst):
    for ix, line in enumerate(lst):
        if func(line):
            return ix

    return -1


if __name__ == '__main__':
    try:
        main()
    except Exception as e:
        print "ERROR: %s" % e
        raise
