--By Carson Rueber
--For use with LuaSingleton
--Contains functions for dealing with rasters

import("System");
import("System.IO");
import("System.Reflection");
import("UnityEngine");
import("gdal_csharp"); --Gdal needs to be loaded by this
import("OSGeo.GDAL");

--Xml parser things
local xml2lua = require("XML.xml2lua");
local handler = require("XML.tree");

--TODO:
--[[
More Lua and features for downloading/saving the datasets
Edit warpVrt to just be warpDataset and then use the file extension to decide how to warp
]]

RasterUtilities = {};

------------------------
-- Raster Definitions --
------------------------


--The Data folder, which contains all things raster
RasterDataFolderLocation = "F:\\Data";

--A specific raster type (lowest level item) contains the following fields:
--inputPath - File path to the input data, relative to the Data folder

--The following fields are added at runtime:
--ID - unique (among a subset) integer identifier

--The following fields are added at runtime under certain conditions/after running certain functions
--outputPath - File path to the output (warped) data
RasterTypes = {
	Population = {
		ChildrenUnderFive = {
			inputPath = "\\tif\\ChildrenUnderFive\\ChildrenUnderFive.vrt"
		},
		ElderlySixtyPlus = {
			inputPath = "\\tif\\ElderlySixtyPlus\\ElderlySixtyPlus.vrt";
		},
		Men = {
			inputPath = "\\tif\\Men\\Men.vrt";
		},
		FullPopulation = {
			inputPath = "\\tif\\FullPopulation\\population_usa_2019-07-01.vrt";
		},
		Women = {
			inputPath = "\\tif\\Women\\Women.vrt";
		},
		WomenOfReproductiveAge = {
			inputPath = "\\tif\\WomenOfReproductiveAge\\WomenOfReproductiveAge.vrt";
		},
		Youth15To24 = {
			inputPath = "\\tif\\Youth15To24\\Youth15To24.vrt";
		}
	};
};


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

--Convers a warped raster into a texture of width and height
--Returns the newly created Raster
--Dataset must be an opened dataset
function RasterUtilities.loadRasterToTexture(dataset, width, height)
	--The population data only has 1 band
	rasterBand = dataset:GetRasterBand(1);

	local texture = Texture2D(width, height, TextureFormat.RGBA32, false);

	--Create the array of size 6 to get argout
	local argout = luanet.make_array(Double, {1,2,3,4,5,6});
	dataset:GetGeoTransform(argout);

	--Array of size 1
	local rasterDataArray = luanet.make_array(Double, {1});

	--Get the corner coords of the screen
	local screenCoords = Vector2Double(0, 0);
	local projectedCoords = Projection.renderSpaceToProjection(screenCoords);
	local worldCoords = Projection.projectionToLatLongs(projectedCoords);
	local originalRasterCoords = Projection.worldToRasterSpace(worldCoords, dataset);
	local rasterCoords = originalRasterCoords;



	for x = 0, width - 1 do
		rasterCoords.x = rasterCoords.x + 1;
		rasterCoords.y = originalRasterCoords.y;
		for y = 0, height - 1 do
			--Default texture color
			local c = LuaSingleton.color32ToColor(Simulation.floatToColor(0));
			texture:SetPixel(x, y, c);
			rasterCoords.y = rasterCoords.y - 1;

			--Check if it's in bounds
			if rasterCoords.x >= 0 and rasterCoords.y >= 0 and
			rasterCoords.x < dataset.RasterXSize and rasterCoords.y < dataset.RasterYSize then
				--Read in the raster data
				rasterBand:ReadRaster(
					math.floor(rasterCoords.x + 0.5), math.floor(rasterCoords.y + 0.5),
					1, 1, rasterDataArray, 1, 1, 0, 0
				);
				local numberOfPeople = not Double.IsNaN(rasterDataArray[0]) and rasterDataArray[0] or 0.0;
				--Output the color
				local color = Simulation.floatToColor(numberOfPeople);

				--We set the color in this manner just to be sure
				texture:SetPixels32(x, y, 1, 1, luanet.make_array(Color32, {color}));
			end
		end
	end

	Debug.Log(os.clock() - startTime);

	texture:Apply()
	return texture;
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