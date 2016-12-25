# qtbot

Do you want this bot on your own server? You need to host it yourself, on either Windows, Linux, or Mac OSX Server.  
.Net Core 1.1 needs to be installed to run. When in the project folder, just run `dotnet restore && dotnet run` to run the bot.  

To make the bot *work* you need a ProgramInfo.json in the LocalFiles folder. This folder needs to be located where you run the bot,  
this is most likely the location that Program.cs is in.

Here is the template of ProgramInfo.json:

```json
{
	"id":"bot id here",
	"DevID":"bot owner id here",
	"google_cx_code":"Here goes the google cx key, this is needed for the /img command. So it's optional",
	"google_key_code":"google secret code I think, if you set up a custom search you should just find these and replace them.",
	
	"bot_secret":"bot secret you get from discord (optional)",
	"bot_id":"bot id here (Why is this here twice?) (optional)",
	"bot_token":"This is acutally needed, the bot token is used for login. It's REQUIRED",
	
	"anilist_id":"You can get these from anilist, this is needed so you can find users.",
	"anilist_client_secret":"same from anilist, just make the app from there."
}

```

After this you can run the bot, sorry it's quite messy but to explain it a bit easier:  
All that's REQUIRED is the `bot_token`.  
To make `/img` work you need the `google_cx_code` and `google_key_code` which you can get when you make a custom search engine.  
To make anilist commands work, like `/anime` and `/al` you need the `anilist_id` and `anilist_client_secret` which you can easily get when you make an app on the Anilist servers.


have fun>?
