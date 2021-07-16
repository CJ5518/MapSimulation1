--By Carson Rueber

--A collection of functions that really only needed to be run once
--Honestly a lot of this is just for archival/if it needs to be run again one day


--Returns two arrays
--1 - a list of files to download (tif's)
--2 - a list of files to generate (vrt's)
--These files are the base data files, which are likely useless because they're huge
--and people can just be sent the warped data
local function getMissingFiles()
	local file = io.open(LuaSingleton.luaFolderPath .. "/fileModel.json");
	local modelTable = json.decode(file:read("a"));
	file:close();

	local filesToDownload = {};
	local filesToGenerate = {};

	local function recursive(folder, tab)
		folder = folder or tifFolder;
		tab = tab or modelTable;
		for i, v in pairs(tab) do
			if type(v) == "table" then
				local newRoot = folder .. "/" .. i;
				recursive(newRoot, v);
			else
				--So this is a filename from the model
				local filename = folder .. "/" .. v;
				if not File.Exists(filename) then
					--Decide what ought to be done about this file
					local extension = Path.GetExtension(filename);
					if extension == ".vrt" then
						filesToGenerate[#filesToGenerate+1] = filename;
					end
					if extension == ".tif" then
						filesToDownload[#filesToDownload+1] = filename;
					end
				end
			end
		end
	end
	recursive();
	return filesToDownload, filesToGenerate
end


--Used on a selection of the table from the data source page
local function writePopulationDownloadLinks()
	local lineList = "";
	local filename = LuaSingleton.luaFolderPath .. "/populationDataWebsiteStuff.xml"
	local file = io.open(filename, "r");
	local outputFile = io.open(LuaSingleton.luaFolderPath .. "/populationDownloadLinks.txt", "w");
	for line in file:lines() do
		if line:find("href%=") then
			local firstQuoteIdx = line:find("\"");
			local secondQuoteIdx = line:find("\"", firstQuoteIdx + 2);
			outputFile:write("https://data.humdata.org");
			outputFile:write(line:sub(firstQuoteIdx + 1, secondQuoteIdx - 1));
			outputFile:write("\n");
		end
	end
	file:close();
	outputFile:close();
end


local fileModelTable = {};

--Creates a model of the data folder
local function createFileModelTable(folder, level)
	--Tif folder to start with
	folder = folder or tifFolder;

	local rootTable = fileModelTable;

	--Iterate over the folder names
	local str = tostring(folder):gsub("\\", " ");
	str = str:gsub("/", " ");
	local pastTifFolder = false;
	for match in str:gmatch("[^%s]+") do
		if pastTifFolder then
			rootTable = rootTable[match]
		end

		if match == "tif" then pastTifFolder = true end
	end

	--For each dir
	for path in luanet.each(Directory.GetDirectories(folder)) do
		print(path, "is in", folder);
		name = DirectoryInfo(path).Name;
		rootTable[name] = {};
		print("added", name, "to root table");
		createFileModelTable(path);
	end

	--For each file
	for path in luanet.each(Directory.GetFiles(folder)) do
		rootTable[#rootTable+1] = DirectoryInfo(path).Name;
	end
end
--Outputs the above table to a file
local function outputModelTable()
	local outputFilename = LuaSingleton.luaFolderPath .. "/fileModel.json";
	local file = io.open(outputFilename, "w");
	file:write(json.encode(fileModelTable));
	file:close();
end

