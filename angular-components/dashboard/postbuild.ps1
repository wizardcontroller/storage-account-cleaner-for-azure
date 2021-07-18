write-output "running post build"

write-output "copying /dist/dashboard"
xcopy /s ".\dist\dashboard\" "..\..\com.ataxlab.functions.table.retention.dashboard\wwwroot\angular"  /Y
#xcopy /s ".\dist\dashboard\assets\app-root.html" "..\..\com.ataxlab.functions.table.retention.dashboard\wwwroot\angular" /Y

write-output "copying app-root.html"
xcopy /s ".\src\assets\app-root.html" "..\..\com.ataxlab.functions.table.retention.dashboard\wwwroot\angular" /Y
