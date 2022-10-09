help:
	echo make copy - copy files to UnityProject

copy:
	cp -R BakingSheet/Src/* UnityProject/Packages/com.cathei.bakingsheet/Runtime/Core
	cp BakingSheet.Converters.Excel/*.cs UnityProject/Packages/com.cathei.bakingsheet/Runtime/Converters/Excel
	cp BakingSheet.Converters.Google/*.cs UnityProject/Packages/com.cathei.bakingsheet/Runtime/Converters/Google
	cp BakingSheet.Converters.Csv/*.cs UnityProject/Packages/com.cathei.bakingsheet/Runtime/Converters/Csv
	cp BakingSheet.Converters.Json/*.cs UnityProject/Packages/com.cathei.bakingsheet/Runtime/Converters/Json
	cp *.md UnityProject/Packages/com.cathei.bakingsheet/

.PHONY: copy
