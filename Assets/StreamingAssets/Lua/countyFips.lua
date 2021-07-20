--By Carson Rueber
--NOT for use with unity, this is just to pre-process some data

local file = io.open("2020_Gaz_counties_national.txt", "r");

--Skip the first line
file:read("*l");

for line in file:lines() do
	print(line);
end

file:close()