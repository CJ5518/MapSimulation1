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
	[MenuItem("Build/DoThing")]
	public static void doThing() {
		var x = BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
		BuildReport report = BuildPipeline.BuildPlayer(x);
		//https://answers.unity.com/questions/1642506/getting-the-current-buildoptions.html
		//BuildBuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded) {
			Logger.Log("Build succeeded: " + summary.totalSize + " bytes");
		}

		if (summary.result == BuildResult.Failed) {
			Logger.Log("Build failed");
		}
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
			buildOptions |= BuildOptions.Development | BuildOptions.AllowDebugging;
		}

		if (target == BuildTarget.StandaloneOSX) {
			options.locationPathName = "./Builds/OSX/test";
		} else if (target == BuildTarget.StandaloneWindows) {
			options.locationPathName = "./Builds/Windows/MapSimulation1.exe";
		}
		
		options.target = target;
		BuildReport report = BuildPipeline.BuildPlayer(options);
		//https://answers.unity.com/questions/1642506/getting-the-current-buildoptions.html
		//BuildBuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions());
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded) {
			Logger.Log("Build succeeded: " + summary.totalSize + " bytes");
			Logger.Log(buildOptions);
		}

		if (summary.result == BuildResult.Failed) {
			Logger.Log("Build failed");
		}
	}
}
