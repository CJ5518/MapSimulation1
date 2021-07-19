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

local json = require("json");
local inspect = require("inspect");

--Public
RasterUtilities = {};

--The Data folder, which contains the base data
--Very likely to only exist on my computer
RasterDataFolderLocation = "F:\\Data";
local tifFolder = RasterDataFolderLocation .. "/tif";
local warpedFolder = Application.streamingAssetsPath .. "/Warped";

---------------------------------
-- High Level Raster Interface --
---------------------------------

--Resolves enums to names, 
--if they're string they are assumed to be names
--ints are assumed to be enums but in integer form
--enums are obvious enough
--Other userdatum will likely cause strange errors so please don't do it
--Returns resovledMajor, resolvedMinor
function RasterUtilities.resolveEnumsToNames(major, minor)
	local retMajor, retMinor;

	--Resolve major
	if type(major) == "string" then
		retMajor = major;
	elseif type(major) == "number" then
		retMajor = luanet.enum(RasterType, major):ToString();
	elseif type(major) == "userdata" then --Userdata is assumed to be an enum here
		retMajor = major:ToString();
	else
		error("Cannot resolve major enum", major, "of type", type(major));
	end

	--Resolve minor
	if type(minor) == "string" then
		retMinor = minor;
	elseif type(minor) == "number" then
		retMinor = luanet.enum(_G[retMajor], minor):ToString();
	elseif type(minor) == "userdata" then --Userdata is assumed to be an enum here
		retMinor = minor:ToString();
	elseif type(minor) == "nil" then --Don't error on this being nil
		retMinor = nil;
	else
		error("Cannot resolve minor enum", minor, "of type", type(minor));
	end
	return retMajor, retMinor
end


--Returns the base data input filename and the warped output filename
function RasterUtilities.getWarpedFilename(major, minor)
	local majorName, minorName = RasterUtilities.resolveEnumsToNames(major,minor);
	local outputFilename;
	if not minor then
		outputFilename = warpedFolder .. "/" .. majorName .. ".tif";
	else
		outputFilename = warpedFolder .. "/" .. majorName .. "_" .. minorName .. ".tif";
	end
	return outputFilename;
end


-----------------------
-- Dataset Functions --
-----------------------


--Checks if the dataset is already warped
--string/Dataset dataset, if string, must be an absolute file path to the dataset
function RasterUtilities.checkIfDatasetIsWarped(dataset)
	return File.Exists(dataset);
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
		worldPixelSize.x / 5, worldPixelSize.y / 5, algorithm
	);

	--First warp
	local intermediate = Gdal.Warp(
		tempFilePath,
		luanet.make_array(Dataset, datasets), options, nil, nil
	);

	--Redo the options
	options = RasterUtilities.buildSuggestedWarpOptionsString(
		worldPixelSize.x, worldPixelSize.y, algorithm
	);
	
	--Second warp
	local ret = Gdal.Warp(outputTifFilename, luanet.make_array(Dataset, {intermediate}), options, nil, nil);
	
	--Clean up the temp file
	intermediate:Dispose();
	File.Delete(tempFilePath);
	
	return ret;
end

function RasterUtilities.warpTif(inputVrtFilename, outputTifFilename, algorithm)
	local tempFilePath = Application.temporaryCachePath .. "/temp.tif";

	--Warp drive

	local worldPixelSize = Projection.getPixelSizeInLatLong();

	local options = RasterUtilities.buildSuggestedWarpOptionsString(
		worldPixelSize.x / 5, worldPixelSize.y / 5, algorithm
	);

	--First warp
	local intermediate = Gdal.Warp(
		tempFilePath,
		luanet.make_array(Dataset, {Gdal.Open(inputVrtFilename, Access.GA_ReadOnly)}), options, nil, nil
	);

	--Redo the options
	options = RasterUtilities.buildSuggestedWarpOptionsString(
		worldPixelSize.x, worldPixelSize.y, algorithm
	);
	
	--Second warp
	local ret = Gdal.Warp(outputTifFilename, luanet.make_array(Dataset, {intermediate}), options, nil, nil);
	
	--Clean up the temp file
	intermediate:Dispose();
	File.Delete(tempFilePath);
	
	return ret;
end