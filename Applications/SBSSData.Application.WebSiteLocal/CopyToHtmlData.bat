robocopy "E:\VS2022 Projects\SBSSDataBuilder\Applications\SBSSData.Application.WebSiteLocal" J:\SBSSDataStore\HtmlData *.* /S /A /XF *.csproj *.bat LogSessions.html DataStoreInfo.html /XO /XD PlayerPhotos bin obj
pause Press any key 
exit 0