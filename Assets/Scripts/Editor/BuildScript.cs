using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildScript {
	[MenuItem("Build/Windows/Windows")]
	public static void BuildWindows() {
		build(BuildTarget.StandaloneWindows, false, false);
	}
	[MenuItem("Build/Windows/WindowsDebug")]
	public static void BuildWindowsDebug() {
		build(BuildTarget.StandaloneWindows, false, true);
	}
	[MenuItem("Build/Windows/WindowsHeadless")]
	public static void BuildWindowsHeadless() {
		build(BuildTarget.StandaloneWindows, true, false);
	}

	[MenuItem("Build/MacOS/MacOS")]
	public static void BuildMacOS() {
		build(BuildTarget.StandaloneOSX, false, false);
	}
	[MenuItem("Build/MacOS/MacOSDebug")]
	public static void BuildMacOSDebug() {
		build(BuildTarget.StandaloneOSX, false, true);
	}
	[MenuItem("Build/MacOS/MacOSHeadless")]
	public static void BuildMacOSHeadless() {
		build(BuildTarget.StandaloneOSX, true, false);
	}


	private static void build(BuildTarget target, bool isHeadless, bool isDevelopment) {
		BuildPlayerOptions options = new BuildPlayerOptions();
		BuildOptions buildOptions = BuildOptions.None;

		if (isHeadless) {
			buildOptions |= BuildOptions.EnableHeadlessMode;
			options.scenes = new[] { "Assets/Scenes/UltraMainScene.unity" };
		} else {
			options.scenes = new[] { "Assets/Scenes/LandonSetup0.unity", "Assets/Scenes/UltraMainScene.unity" };
		}
		if (isDevelopment) {
			buildOptions |= BuildOptions.Development;
		}

		if (target == BuildTarget.StandaloneOSX) {
			options.locationPathName = "./Builds/OSX/test";
		}

		options.target = target;
		BuildReport report = BuildPipeline.BuildPlayer(options);
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded) {
			Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
		}

		if (summary.result == BuildResult.Failed) {
			Debug.Log("Build failed");
		}
	}
}
