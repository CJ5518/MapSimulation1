--By Carson Rueber
--For use with LuaSingleton
--Contains functions for dealing with rasters

--Calling functions which take rasterIdentifiers information:
--if rasterIdentifierMinor is nil, major must be a full name like "Population.Women"
--To use both parameters, major would be "Population" and minor would be "Women"

import("System");
import("System.IO");
import("System.Reflection");
import("UnityEngine");
import("gdal_csharp"); --Gdal needs to be loaded by this
import("OSGeo.GDAL");
import("SimpleFileBrowser");

--Xml parser things
local xml2lua = require("XML.xml2lua");
local handler = require("XML.tree");

--TODO:
--[[
More Lua and features for downloading/saving the datasets
Edit warpVrt to just be warpDataset and then use the file extension to decide how to warp
]]

--Public
RasterUtilities = {};
--Private
RasterPrivate = {};


--The Data folder, which contains all things raster
RasterDataFolderLocation = "F:\\Data";

--returns a bool, true if we need to prompt the user for a data folder, false if not
function RasterUtilities.needDataFolder()
	return not Directory.Exists(RasterDataFolderLocation);
end

--Creates the required folders in the data folder based upon the enum
function RasterUtilities.createDataDirectoryStructure()
	local tifFolder = RasterDataFolderLocation .. "/tif";

	Directory.CreateDirectory(tifFolder);

	--Iterate over every enum item
	for major = 0, LuaSingleton.castToInt(RasterType.RasterTypeCount) - 1 do
		local minorName = luanet.enum(RasterType, major):ToString();
		--Yummy hacks
		for minor = 0, LuaSingleton.castToInt(_G[minorName][minorName .. "Count"]) - 1 do
			local name = luanet.enum(_G[minorName], minor):ToString();
			Directory.CreateDirectory(tifFolder .. "/" .. name);
		end
	end
end

local tifFolder = RasterDataFolderLocation .. "/tif";

if not Directory.Exists(tifFolder) then
	
end
---------------------------------
-- High Level Raster Interface --
---------------------------------


function RasterUtilities.getFilenames(major, minor)
	local majorName = major:ToString();
	local minorName = luanet.enum(_G[majorName], minor):ToString();
	local inputFilename = 
		RasterDataFolderLocation .. "/tif/" .. majorName .. "/" .. minorName .. "/" .. minorName .. ".vrt";
	local outputFilename = RasterDataFolderLocation .. "/tif/Warped/" .. majorName .. "_" .. minorName .. ".tif";
	return inputFilename, outputFilename;
end

--if rasterIdentifierMinor is nil, major must be a full name like "Population.Women"
--To use both parameters, major would be "Population" and minor would be "Women"
function RasterUtilities.preprocessRaster(rasterIdentifierMajor, rasterIdentifierMinor)
	
end


-----------------------
-- Dataset Functions --
-----------------------


--Checks if the dataset is already warped
--string/Dataset dataset, if string, must be an absolute file path to the dataset
function RasterUtilities.checkIfDatasetIsWarped(dataset)
	local isString = type(dataset) == "string";
	if isString then
		if not File.Exists(dataset) then return false end
		dataset = Gdal.Open(dataset, Access.GA_ReadOnly);
	end
	--Create the array of size 6 to get argout
	local argout = luanet.make_array(Double, {1,2,3,4,5,6});
	dataset:GetGeoTransform(argout);

	local sizeX = argout[1];
	local sizeY = argout[5];

	local correctSize = Projection.getPixelSizeInLatLong();

	--Checks if the values are close enough for government work
	local function isCloseEnough(x,y)
		return math.floor((math.abs(x) * 1000) + 0.5) == math.floor((math.abs(y) * 1000) + 0.5)
	end

	if isCloseEnough(sizeX, correctSize.x) and isCloseEnough(sizeY, correctSize.y) then
		ret = true;
	end

	--Not sure if this needs to run, but better safe than sorry
	if isString then
		dataset:Dispose();
	end

	return ret;
end


-------------
-- Warping --
-------------

--Creates the suggested warp options based upon some editable factors
--string algorithm, see warpVrt for more info
function RasterUtilities.buildSuggestedWarpOptionsString(pixelSizeX, pixelSizeY, algorithm)
	--Stolen from unmodified women population data, it works so we don't touch it
	local extentMin = Vector2Double(-152.5876388888889039, 24.4918631200275598);
	local extentMax = Vector2Double(-66.9981474405200288, 62.5851388888888920);

	return RasterUtilities.genWarpOptionsFromString(string.format(
		"-tr %s %s -r %s -te %s %s %s %s " ..
		"-wm 500 -multi -overwrite -tap -et 0 -co \"TILED=YES\" -co \"COMPRESS=LZW\" -wo \"INIT_DEST=NO_DATA\"",
		tostring(pixelSizeX), tostring(pixelSizeY), algorithm,
		tostring(extentMin.x), tostring(extentMin.y), tostring(extentMax.x), tostring(extentMax.y)
	));
end

--Take an options string and converts it to a GDALWarpAppOptions object
--string should be in the format "-tr 1 4 -override," as in how one
--would pass these options on the command line
function RasterUtilities.genWarpOptionsFromString(options)
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
--returns the warped dataset
function RasterUtilities.warpVrt(inputVrtFilename, outputTifFilename, algorithm)
	local datasets = {};

	local tempFilePath = Application.temporaryCachePath .. "/temp.tif";

	--parse the xml
	local xmlHandlerObj = handler:new();
	local parser = xml2lua.parser(xmlHandlerObj);
	parser:parse(xml2lua.loadFile(inputVrtFilename));
	for i, v in pairs(xmlHandlerObj.root.VRTDataset.VRTRasterBand.ComplexSource) do
		--Found the filename, but need to make it absolute
		--Also here's where we assume that the tif is relative to the vrt
		local filename = Directory.GetParent(inputVrtFilename).FullName .. "/" .. v.SourceFilename[1];

		datasets[#datasets + 1]	= Gdal.Open(filename, Access.GA_ReadOnly);
	end

	--Warp drive

	local worldPixelSize = Projection.getPixelSizeInLatLong();

	local options = RasterUtilities.buildSuggestedWarpOptionsString(
		worldPixelSize.x / 5, worldPixelSize.y / 5, "sum"
	);

	--First warp
	local intermediate = Gdal.Warp(
		tempFilePath,
		luanet.make_array(Dataset, datasets), options, nil, nil
	);

	--Redo the options
	options = RasterUtilities.buildSuggestedWarpOptionsString(
		worldPixelSize.x, worldPixelSize.y, "sum"
	);
	
	--Second warp
	local ret = Gdal.Warp(outputTifFilename, luanet.make_array(Dataset, {intermediate}), options, nil, nil);
	
	--Clean up the temp file
	intermediate:Dispose();
	File.Delete(tempFilePath);
	
	return ret;
end