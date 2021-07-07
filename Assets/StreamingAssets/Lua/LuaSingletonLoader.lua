--By Carson Rueber
--The first Lua file run by LuaSingleton.cs
--Opens other important library files and does some important setup

import("UnityEngine");
import("Assembly-CSharp"); --Our very own assembly

--Set the package path
package.path = Application.streamingAssetsPath .. "/Lua/?.lua";


--Add your files to this table to run them and register them with the global state on init
local files = {
	"RasterUtilities.lua"
}

--Runs the files
for i, v in pairs(files) do
	dofile(LuaSingleton.luaFolderPath .. "/" .. v);
end

