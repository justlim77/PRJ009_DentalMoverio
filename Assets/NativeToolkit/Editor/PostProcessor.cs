using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PostProcessor {

	// tells Xcode to automatically @include frameworks
	// credit : https://github.com/scarlet/ScarletPostProcessor

    [PostProcessBuild(1500)]
	public static void OnPostProcessBuild(BuildTarget target, string path)
	{
		#if UNITY_IPHONE
		if (target != BuildTarget.iOS) {
			return;
		}

		string pbxproj = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

		string insertKeyword = "buildSettings = {";
		string foundKeyword = "CLANG_ENABLE_MODULES";
		string modulesFlag = "				CLANG_ENABLE_MODULES = YES;";

		List<string> lines = new List<string>();
			
		foreach (string str in File.ReadAllLines(pbxproj)) {
			if (!str.Contains(foundKeyword)) { 
				lines.Add(str);
			}
			if (str.Contains(insertKeyword)) {
				lines.Add(modulesFlag);
			}
		}
	
		using (File.Create(pbxproj)) {}

		foreach (string str in lines) {
			File.AppendAllText(pbxproj, str + Environment.NewLine);
		}

		#endif
	}	
}