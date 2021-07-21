--By Carson Rueber
--NOT for use with unity, this is just to pre-process some data

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