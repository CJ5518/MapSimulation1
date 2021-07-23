--By Carson Rueber
--For use with LuaSingleton-ish
--Another one-time utils file, code here is for archival purposes

import("ogr_csharp"); --Ogr needs to be loaded like this
import("OSGeo.OGR");
luanet.load_assembly("System")

local json = require("json");


--Adds some stuff to a shape file
local function editShapeFile()
	local dataSource = Ogr.Open("F:\\Data\\shp\\county\\county_reprojected.shp", 1);
	local layer = dataSource:GetLayerByIndex(0);

	layer:DeleteField(11);
	--Create a field
	local fieldDef = FieldDefn("VaccRate", FieldType.OFTReal);
	fieldDef:SetPrecision(6);
	layer:CreateField(fieldDef, 0);

	layer:ResetReading();
	local feature = nil;
	repeat
		feature = layer:GetNextFeature();
		if feature ~= nil then
			local fips = feature:GetFieldAsString(4);
			local filePath = "F:\\Data\\countyCovid\\" .. fips .. ".json";
			if not File.Exists(filePath) then
				print(fips, "did not exist, big sad");
			else
				--local file = io.open(filePath, "r");
				--local decoded = json.decode(file:read("a"));
				--file:close();
				--feature:SetField("Population", tonumber(decoded["population"]));
				--local x = decoded["actuals"]["vaccinationsInitiated"];
				--if not x then print(fips, "has no vaccine data???"); x = -1; end
				--feature:SetField("VaccBegan", x);
				local pop = feature:GetFieldAsInteger("Population");
				local vacc = feature:GetFieldAsInteger("VaccBegan");
				--Gotta get the right overload, we got the integer overload without this
				local method = luanet.get_method_bysig(feature, "SetField", "System.String", "System.Double");
				method("VaccRate", vacc / pop);
			end
			layer:SetFeature(feature);
		end
	until not feature;


	layer:Dispose();
	dataSource:Dispose();
end

--Runs in lua 5.1
local function downloadCovidData()
	local file = io.open("2020_Gaz_counties_national.txt", "r");

	local apiKey = "8226aa8901b34f6093d5021ee473d023";

	--Skip the first line
	file:read("*l");

	for line in file:lines() do
		local state, fips, longCode, name, aland, awater, _, __, lat, long = 
		line:match("(..).(.....).(%d+).([%w%s%D]-).(%d+).(%d+).([%d%p]+).([%d%p]+).([%d%p]+).([%d%p]+)");
		print("Doing " .. fips .. ": " .. name);
		pcall(function()
			os.execute(string.format(
				'curl "https://api.covidactnow.org/v2/county/%s.json?apiKey=%s" -o "F:/Data/countyCovid/%s.json"',
				fips, apiKey, fips
			));
			print("DID " .. fips .. ": " .. name);
		end)
		
	end

	file:close()
end