--By Carson Rueber
--Handles some stuff with data, in lua5.1


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