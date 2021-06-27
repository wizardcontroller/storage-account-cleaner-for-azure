write-output "running post build"
xcopy /s ".\dist\dashboard\" "..\..\com.ataxlab.functions.table.retention.dashboard\wwwroot\angular"  /Y
xcopy /s ".\dist\dashboard\assets\app-root.html" "..\..\com.ataxlab.functions.table.retention.dashboard\wwwroot\angular" /Y
