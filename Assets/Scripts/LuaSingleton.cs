//By Carson Rueber

using NLua;
using UnityEngine;

//A Lua state for all your static Lua needs
//Useful for registering functions/configuration data
//Also used to help glue lua and c# together, when something is too difficult in Nlua
//Try not to pollute the global namespace too much
public class LuaSingleton {
	//The state
	public static Lua lua;

	//Path to the Lua folder
	public static string luaFolderPath = Application.streamingAssetsPath + @"\Lua\";

	public static void initLua() {
		lua = new Lua();
		lua.LoadCLRPackage();
		lua.DoFile(luaFolderPath + "LuaSingletonLoader.lua");
	}


	//Functions designed to be called from Lua
	public static Color color32ToColor(Color32 c) {
		return c;
	}
}
