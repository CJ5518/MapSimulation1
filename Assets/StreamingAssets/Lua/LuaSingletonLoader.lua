--By Carson Rueber
--The first Lua file run by LuaSingleton.cs
--Opens other important library files and does some important setup

import("UnityEngine");
import("Assembly-CSharp"); --Our very own assembly

--Set the package path
LUA_PATH = Application.streamingAssetsPath .. "/Lua/?.lua";
package.path = LUA_PATH;


--Add your files to this table to run them and register them with the global state on init
local files = {
	"RasterUtilities.lua"
}

--Runs the files
for i, v in pairs(files) do
	dofile(LuaSingleton.luaFolderPath .. v);
end

