using PuppeteerSharp;

void PrintLineWithColor(string text, ConsoleColor color)
{
	var temp = Console.ForegroundColor;
	Console.ForegroundColor = color;
	Console.WriteLine(text);
	Console.ForegroundColor = temp;
}

string GetInput(string question)
{
	Console.Write(question);
	return Console.ReadLine() ?? GetInput(question);
}

Console.WriteLine("Finding or Downloading Required Files");
var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync(); //Download Required Files
Console.WriteLine("Required Files Finded or Downloaded");

while (true)
{
	bool errorAccoured = false;

	void ErrorAccoured()
	{
		errorAccoured = true;
	}

	void PrintErrorState()
	{
		Console.WriteLine($"ForOutputRegexing-Error-Accoured-State:{errorAccoured}:");
	}

	var target = GetInput("Please Enter Target Website Url: ");

	await using (var browser = await Puppeteer.LaunchAsync(
		new LaunchOptions { Headless = true })) //Launch browser
	{
		await using (var page = await browser.NewPageAsync()) //Create new page in browser
		{
			page.Error += (sender, e) =>
			{
				//ErrorAccoured();
				PrintLineWithColor($"Internal System Error => {e.Error}", ConsoleColor.Red);
			};

			page.PageError += (sender, e) =>
			{
				ErrorAccoured();
				PrintLineWithColor($"Page Error => {e.Message}", ConsoleColor.Red);
			};

			page.Console += (sender, e) =>
			{
				if (e.Message.Type == ConsoleType.Error)
				{
					ErrorAccoured();
					PrintLineWithColor($"Console Error => {e.Message.Text}", ConsoleColor.Red);
				}
			};

			Console.WriteLine("Going to Target Website");
			var rep = await page.GoToAsync(target, new NavigationOptions()
			{
				Timeout = TimeSpan.FromSeconds(60).Milliseconds,
				WaitUntil = new[]{
				WaitUntilNavigation.Networkidle0
			}
			});

			if (rep.Status != System.Net.HttpStatusCode.OK)
			{
				ErrorAccoured();
				Console.WriteLine($"Status Code is not 200 (OK), it's {rep.Status.ToString()} (${(int)rep.Status})");
			}

			if (!errorAccoured)
			{
				PrintLineWithColor("No Error Accoured", ConsoleColor.Green);
			}
			else
			{
				PrintLineWithColor("Error Accoured", ConsoleColor.Red);
			}

			PrintErrorState();
		}
	}

	var res = GetInput("Do you want to exit (0) or continue (any other value)?");
	if(res.Trim() == "0")
	{
		break;
	}
}