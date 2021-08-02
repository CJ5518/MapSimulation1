--By Carson Rueber
--Handles some stuff with data, in lua5.1

local dataFolder = "F:\\UnityProjects\\MapSimulation1\\Assets\\StreamingAssets\\Data";


--Functions for airport geojson
--Removes useless airports from the airport data
--leaves a comma on the end of the last thingy tho, so the json isn't valid unless you remove it
local function trimAirportData(inputPath, outputPath)
	local file = io.open(inputPath, "r");
	local outputFile = io.open(outputPath, "w");
	for line in file:lines() do
		local firstChars = line:sub(1,3);
		local writeThisLine = true;
		if firstChars == '{ "' then
			if not line:find('Commercial%_Ops%"%: %"') then
				writeThisLine = false;
			end
		end
		if writeThisLine then
			outputFile:write(line .. "\n");
		end
	end
end

--Sorts the table by Commercial_Ops
--Despite my very limited efforts the json isn't formatted properly
--You have to remove the extra ] and the comma on the last entry
local function sortAirportData(inputPath, outputPath)
	local file = io.open(inputPath, "r");
	print(file);
	local outputFile = io.open(outputPath, "w");
	local outputLines = {};
	local haveGottenToImportantStuff = false;
	for line in file:lines() do
		local firstChars = line:sub(1,3);
		local writeThisLine = true;
		if firstChars == '{ "' then
			haveGottenToImportantStuff = true;
			outputLines[#outputLines+1] = line;
		else
			if haveGottenToImportantStuff then
				--Sort the table
				local function getNum(a)
					return tonumber(a:sub(a:find("Commercial_Ops\": \"%d+\"")):match(".-(%d+)."));
				end
				table.sort(
					outputLines, function(a, b)
						return getNum(a) > getNum(b)
				end)
				for q = 1, #outputLines do
					outLine = outputLines[q];
					if q == #outputLines then
						--Remove the comma, apparently doesn't work but meh
						if outLine:sub(outLine:len(), outLine:len()) == "," then
							outLine = line:sub(1, outLine:len()-1);
						end
					else
						--Add the comma
						if outLine:sub(outLine:len(), outLine:len()) ~= "," then
							outLine = outLine .. ",";
						end
					end
					outputFile:write(outLine .. "\n");
				end
				outputLines = {};
				outputFile:write(line);
			else
				outputFile:write(line .. "\n");
			end
		end
	end
end

--Returns a table of all the 3 letter codes in the airport data
--in the form returnVal[ATL] = "ATL";
--So it's a lookup table
local function getAirportCodes()
	local pattern = "Loc_Id\": \"(...)\"";

	local inputFilename = dataFolder .. "/Airports_Sorted.geojson";
	local inFile = io.open(inputFilename, "r");

	local ret = {};

	for line in inFile:lines() do
		local code = line:match(pattern);
		if code then
			ret[code] = code;
		end
	end

	inFile:close();
	return ret;
end
--Functions for dd.db28dm (passenger data)


local function passengerData()
	local inputFilename = dataFolder .. "/dd.db28dm.201901.201912.asc";
	local outputFilename = dataFolder .. "/AirportMatrix.txt";
	local inFile = io.open(inputFilename, "r");
	local outFile = io.open(outputFilename, "w");

	local pattern = "(%d%d%d%d)|(%d%d)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)|(.-)";
	--Only use the airport codes of airports that we have
	local codes = getAirportCodes();

	local count = 0;
	local tab = {};

	--Generate the tab of origin/dest pairs
	for line in inFile:lines() do
		local year, month, originCode,_,_,_, destCode,_,_,_,_,_,_, distance,_,passengerCount = line:match(pattern);
		--You can also check for month/year here
		if codes[originCode] and codes[destCode] and passengerCount ~= 0 then
			if not tab[originCode] then tab[originCode] = {}; end
			if not tab[originCode][destCode] then tab[originCode][destCode] = 0; end
			tab[originCode][destCode] = tab[originCode][destCode] + tonumber(passengerCount);
		end

	end
	
	--Output the matrix
	local order = {};
	outFile:write("X");
	for i, v in pairs(tab) do
		outFile:write(" " .. i);
		--So as to have an order for these, as pairs is not guaranteed to go in the same order each time
		order[#order+1] = i;
	end
	outFile:write("\n");

	for q = 1, #order do
		outFile:write(order[q]);
		for i = 1, #order do
			local rowCode = order[q];
			local columnCode = order[i];
			local data = tab[rowCode][columnCode] or 0;
			outFile:write(" " .. tostring(data));
		end
		outFile:write("\n");
	end

	print(tab["ATL"]["LAX"]);

	outFile:close();
	inFile:close();
end

passengerData();