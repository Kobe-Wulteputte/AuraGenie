# Default to local sql server db
$connectionString = "Data Source=c:\Users\KWLT\Development\_Private_Projects\AuraGenie\aura.sqlite;"


dotnet tool restore

# Run tool on localhost sql server db
dotnet efcpt $connectionString sqlite


# Stupid readme file...
if (Test-Path "efcpt-readme.md")
{
    Remove-Item "efcpt-readme.md"
}