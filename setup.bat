@echo off
git config --global merge.tool unityyamlmerge
git config --global mergetool.unityyamlmerge.cmd "'C:/Program Files/Unity/Hub/Editor/6000.5.5f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p \"$BASE\" \"$REMOTE\" \"$LOCAL\" \"$MERGED\""
git config --global mergetool.unityyamlmerge.trustExitCode false
echo Done!
pause