# qtbot

Do you want this bot on your own server? ..No. 

Here are a couple reasons why you don't want to:
	1. it sucks
	2. it sucks some more
	3. hard to maintain
	4. stupid fucking commands that others do better
	5. setup is dumb as fuck and fuck you

Here are a couple reasons why you do want em:
	1. made by me
	
This bot is unmaintained now. It's a shell of what mess it was that made it fun, and it's just boring now.  
It does have the /magick command! But the setup of that one is stupid and requires some fiddling a round in the system files. Also I lost the C library files that it used so haha jokes on you. Go to my other website instead:

http://stepperman.me/imagefun <- pretty cool site if I do say so myself.

So you unmaintained bot, code is spaghetti and it's pretty buggy as well. Goodbye.

~~You need to host it yourself, on either Windows, Linux, or Mac OSX Server.  
.Net Core 1.1 needs to be installed to run. When in the project folder, just run `dotnet restore && dotnet run` to run the bot.~~  

~~To make the bot *work* you need a ProgramInfo.json in the LocalFiles folder. This folder needs to be located where you run the bot,  
this is most likely the location that Program.cs is in.~~

~~Here is the template of ProgramInfo.json:~~

~~```json
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
}```~~

~~After this you can run the bot, sorry it's quite messy but to explain it a bit easier:  
All that's REQUIRED is the `bot_token`.  
To make `/img` work you need the `google_cx_code` and `google_key_code` which you can get when you make a custom search engine.  
To make anilist commands work, like `/anime` and `/al` you need the `anilist_id` and `anilist_client_secret` which you can easily get when you make an app on the Anilist servers.~~


~~have fun>?~~

~~##Fortune Cookie~~

~~If you want Fortune Cookie to work, just drop [this](http://i.imgur.com/JwXl4h0.png) image in the LocalFiles folder and call it `fortunecookie.png`  
Then in there, make a text file called `fortunecookie.txt`. In that text file the first two lines are for the font size and font family. Then just add all the fortunes you want in there.~~
