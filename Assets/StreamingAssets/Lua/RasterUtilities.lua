--By Carson Rueber

import("System");
import("System.IO");
import("System.Reflection");
import("UnityEngine");
import("gdal_csharp"); --Gdal needs to be loaded by this
import("OSGeo.GDAL");
import("Assembly-CSharp"); --Our very own assembly

Char = luanet.import_type("System.Char");

LUA_PATH = Application.streamingAssetsPath .. "/Lua/?.lua";
package.path = LUA_PATH;

--Xml parser things
local xml2lua = require("XML.xml2lua")
local handler = require("XML.tree")
local parser = xml2lua.parser(handler)




--TODO:
--[[
So there's the disease equations things, that should probably be top priority
More Lua and features for downloading/saving the datasets
]]



--Checks if the outputTifFilename is already warped
--string outputTifFilename should be an absolute file path
--int pixelSize - size of a pixel in screen space
function checkIfDatasetIsReady(outputTifFilename, pixelSize)
	if File.Exists(outputTifFilename) then
		local dataset = Gdal.Open(outputTifFilename);

	end
end


-------------
-- Warping --
-------------

function buildSuggestedWarpOptionsString(pixelSizeX, pixelSizeY, algorithm)
	--Stolen from unmodified women population data, it works so we don't touch it
	local extentMin = Vector2Double(-152.5876388888889039, 24.4918631200275598);
	local extentMax = Vector2Double(-66.9981474405200288, 62.5851388888888920);

	return string.format(
		"-tr %s %s -r %s -te %s %s %s %s " ..
		"-wm 500 -multi -overwrite -tap -et 0 -co \"TILED=YES\" -co \"COMPRESS=LZW\" -wo \"INIT_DEST=NO_DATA\"",
		tostring(pixelSizeX), tostring(pixelSizeY), algorithm,
		tostring(extentMin.x), tostring(extentMin.y), tostring(extentMax.x), tostring(extentMax.y)
	)
end

--Take an options string and converts it to a GDALWarpAppOptions object
--string should be in the format "-tr 1 4 -override," as in how one
--would pass these options on the command line
function genWarpOptionsFromString(options)
	--Because I couldn't get c#'s String.Split function to work
	--https://stackoverflow.com/questions/1426954/split-string-in-lua
	local t = {};
	for str in string.gmatch(options, "([^".. " " .."]+)") do
		table.insert(t, str);
	end
	
	return GDALWarpAppOptions(luanet.make_array(String, t));
end

--For whatever reason warping is faster if you do it on a list of the datasets and not on the actual vrt
--Warps a dataset
--strings Input/output names should be absolute file paths
--int pixelSize - size of a pixel in screen space
--string algorithm - the algorithm used, eg "sum" or "average" see gdalwarp docs for more
function warpVrt(inputVrtFilename, outputTifFilename, algorithm)
	local datasets = {};

	--parse the xml
	parser:parse(xml2lua.loadFile(inputVrtFilename));
	for i, v in pairs(handler.root.VRTDataset.VRTRasterBand.ComplexSource) do
		--Found the filename, but need to make it absolute
		--Also here's where we assume that the tif is relative to the vrt
		local filename = Directory.GetParent(inputVrtFilename).FullName .. "/" .. v.SourceFilename[1];

		datasets[#datasets + 1]	= Gdal.Open(filename, Access.GA_ReadOnly);
	end

	--Warp drive

	local worldPixelSize = Projection.getPixelSizeInLatLong();

	local options = genWarpOptionsFromString(buildSuggestedWarpOptionsString(
		worldPixelSize.x / 5, worldPixelSize.y / 5, "sum"
	));

	--First warp
	local intermediate = Gdal.Warp(
		Application.temporaryCachePath .. "/temp.tif",
		luanet.make_array(Dataset, datasets), options, nil, nil
	);

	--Redo the options
	options = genWarpOptionsFromString(buildSuggestedWarpOptionsString(
		worldPixelSize.x, worldPixelSize.y, "sum"
	));
	
	--Second warp
	local ret = Gdal.Warp(outputTifFilename, luanet.make_array(Dataset, {intermediate}), options, nil, nil);
	
	intermediate:Dispose();
	
	return ret;
end